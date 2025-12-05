using elFinder.NetCore;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/Admin/el-finder-file-system")]
    public class FileSystemController : Controller
    {
        IWebHostEnvironment _env;
        public FileSystemController(IWebHostEnvironment env) => _env = env;
        // Thêm hàm này vào trong FileSystemController
        [Route("Index")] // Đường dẫn: /Admin/el-finder-file-system/Index
        public IActionResult Index()
        {
            return View();
        }
        [Route("connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            var result = await connector.ProcessAsync(Request);
            if (result is JsonResult)
            {
                var json = result as JsonResult;
                // Thêm ?? "application/json" vào cuối để nếu null thì lấy mặc định
                return Content(JsonSerializer.Serialize(json.Value), json.ContentType ?? "application/json");
            }
            else
            {
                return Json(result);
            }
        }

        [Route("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector()
        {
            string pathroot = "files";

            // --- LỖI CỦA ÔNG LÀ DO THIẾU DÒNG NÀY NÈ ---
            var driver = new FileSystemDriver();
            // --------------------------------------------

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            string rootDirectory = Path.Combine(_env.WebRootPath, pathroot);

            // Tự tạo thư mục nếu chưa có
            if (!Directory.Exists(rootDirectory)) Directory.CreateDirectory(rootDirectory);

            string url = $"/{pathroot}/";
            string urlthumb = $"{uri.Scheme}://Admin/el-finder-file-system/thumb/";

            var root = new RootVolume(rootDirectory, url, urlthumb)
            {
                IsReadOnly = false,
                IsLocked = false,
                Alias = "Files",
                ThumbnailSize = 100,
            };

            driver.AddRoot(root);

            return new Connector(driver)
            {
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}