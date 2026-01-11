//
const chatWindow = document.getElementById('chatWindow');
const chatBody = document.getElementById('chatBody');
const userInput = document.getElementById('userInput');
let isFirstOpen = true;

function toggleChat() {
    chatWindow.classList.toggle('active');
    if (isFirstOpen && chatWindow.classList.contains('active')) {
        setTimeout(() => {
            addBotMessage("Xin chào! 👋 Mình là trợ lý ảo của <b>ToursDULICH</b>. ✈️<br><br>Bạn cần tìm tour, khách sạn hay xem tin tức? Hỏi mình nhé! 👇");
        }, 500);
        isFirstOpen = false;
    }
}

function handleEnter(e) {
    if (e.key === 'Enter') sendMessage();
}

async function sendMessage() {
    const text = userInput.value.trim();
    if (!text) return;

    // 1. Hiện tin nhắn khách
    addUserMessage(text);
    userInput.value = '';

    // 2. Hiện loading
    const loadingId = addBotLoading();

    try {
        // 3. Gọi về Controller C#
        const response = await fetch('/Chatbot/GetResponse', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Message: text })
        });

        removeMessage(loadingId);

        if (response.ok) {
            const data = await response.json();
            addBotMessage(data.reply);
        } else {
            addBotMessage("Hệ thống đang bận, bạn thử lại sau nha! 😅");
        }
    } catch (error) {
        removeMessage(loadingId);
        addBotMessage("Lỗi kết nối server rồi! 🥺");
    }
}

// Hàm phụ trợ
function addBotLoading() {
    const id = 'msg-' + Date.now();
    const div = document.createElement('div');
    div.className = 'message bot loading';
    div.id = id;
    div.innerHTML = '<span class="dot">.</span><span class="dot">.</span><span class="dot">.</span>';
    chatBody.appendChild(div);
    scrollToBottom();
    return id;
}

function removeMessage(id) {
    const el = document.getElementById(id);
    if (el) el.remove();
}

function addUserMessage(text) {
    const div = document.createElement('div');
    div.className = 'message user';
    div.innerText = text;
    chatBody.appendChild(div);
    scrollToBottom();
}

function addBotMessage(html) {
    const div = document.createElement('div');
    div.className = 'message bot';
    div.innerHTML = html;
    chatBody.appendChild(div);
    scrollToBottom();
}

function scrollToBottom() {
    chatBody.scrollTop = chatBody.scrollHeight;
}