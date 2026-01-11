using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace ToursDULICH.Controllers
{
    public class ChatRequest
    {
        public string Message { get; set; }
    }

    public class ChatbotController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;

        public ChatbotController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _connectionString = _configuration.GetConnectionString("Harmic");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> GetResponse([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
                return BadRequest("Vui lòng nhập tin nhắn");

            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return StatusCode(500, "Chưa cấu hình API Key");

            // =================================================================
            // 1. LẤY DỮ LIỆU TỔNG HỢP TỪ 4 BẢNG QUAN TRỌNG NHẤT
            // =================================================================
            StringBuilder contextData = new StringBuilder();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // --- A. LẤY TOUR (Kèm tên thành phố) ---
                    //
                    string sqlTour = @"
                        SELECT TOP 10 t.Name, t.Price, t.SalePrice, c.CityName 
                        FROM Tours t 
                        LEFT JOIN Cities c ON t.CityId = c.CityId 
                        ORDER BY t.TourId DESC";
                    var tours = await conn.QueryAsync(sqlTour);

                    contextData.AppendLine("--- DANH SÁCH TOUR ---");
                    foreach (var t in tours)
                    {
                        decimal finalPrice = t.SalePrice > 0 ? t.SalePrice : t.Price;
                        string city = t.CityName ?? "Nhiều điểm đến";
                        contextData.AppendLine($"- Tour: {t.Name} ({city}) | Giá: {finalPrice:N0}đ");
                    }

                    // --- B. LẤY KHÁCH SẠN (Kèm đánh giá sao) ---
                    //
                    string sqlHotel = @"
                        SELECT TOP 10 h.Name, h.BasePrice, h.Rating, c.CityName 
                        FROM Hotels h 
                        LEFT JOIN Cities c ON h.CityId = c.CityId 
                        ORDER BY h.HotelId DESC";
                    var hotels = await conn.QueryAsync(sqlHotel);

                    contextData.AppendLine("\n--- KHÁCH SẠN NỔI BẬT ---");
                    foreach (var h in hotels)
                    {
                        string stars = new string('⭐', h.Rating ?? 3);
                        contextData.AppendLine($"- KS: {h.Name} ({h.CityName}) | {stars} | Giá từ: {Convert.ToDecimal(h.BasePrice):N0}đ");
                    }

                    // --- C. LẤY GIÁ PHÒNG CHI TIẾT ---
                    // - Cột trong DB là RoomType, PricePerNight
                    string sqlRoom = @"
                        SELECT TOP 10 r.RoomType, r.PricePerNight, h.Name as HotelName 
                        FROM Rooms r 
                        JOIN Hotels h ON r.HotelId = h.HotelId";
                    var rooms = await conn.QueryAsync(sqlRoom);

                    contextData.AppendLine("\n--- GIÁ PHÒNG ---");
                    foreach (var r in rooms)
                    {
                        contextData.AppendLine($"- {r.HotelName}: {r.RoomType} giá {Convert.ToDecimal(r.PricePerNight):N0}đ/đêm");
                    }

                    // --- D. LẤY TIN TỨC MỚI ---
                    //
                    string sqlBlog = "SELECT TOP 5 Title FROM BlogPosts ORDER BY CreatedAt DESC";
                    var blogs = await conn.QueryAsync(sqlBlog);

                    contextData.AppendLine("\n--- TIN TỨC MỚI ---");
                    foreach (var b in blogs)
                    {
                        contextData.AppendLine($"- Bài viết: {b.Title}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu lỗi, in ra để bạn biết, còn Bot sẽ trả lời xã giao
                contextData.Clear();
                contextData.AppendLine($"LỖI HỆ THỐNG DB: {ex.Message}");
            }

            // =================================================================
            // 2. GỬI DỮ LIỆU CHO AI (PROMPT)
            // =================================================================
            string MODEL_NAME = "gemini-2.5-flash-lite"; // Bản nhanh, ổn định
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{MODEL_NAME}:generateContent?key={apiKey}";

            var prompt = $@"Bạn là Trợ lý ảo của ToursDULICH.
            
            DỮ LIỆU TỪ HỆ THỐNG CỦA CÔNG TY:
            {contextData}
            
            YÊU CẦU:
            1. Dựa vào dữ liệu trên để trả lời khách.
            2. Nếu khách hỏi Tour/KS: Báo tên, địa điểm và giá.
            3. Dùng emoji phong phú (✈️🏨🌊). Trình bày danh sách bằng gạch đầu dòng.
            4. Nếu dữ liệu báo lỗi, hãy thành thật xin lỗi khách.

            Khách hỏi: ""{request.Message}""";

            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseString);
                    var reply = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return Ok(new { reply = reply?.Replace("\n", "<br>") });
                }
                return StatusCode((int)response.StatusCode, "Lỗi Gemini: " + responseString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi Server: " + ex.Message);
            }
        }
    }
}