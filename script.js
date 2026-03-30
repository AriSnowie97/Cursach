async function loadOrders() {
    const list = document.getElementById('ordersList');
    if (!list) return;

    // 1. СТАВИМ ЛОАДЕР-ПОДСКАЗКУ ПЕРЕД ЗАПРОСОМ
    list.innerHTML = `
        <div style="text-align: center; padding: 30px;">
            <h3 style="color: #1a73e8;">⏳ Завантаження замовлень...</h3>
            <p style="color: #666; font-size: 0.9em;">
                (Сервер прокидається 😴<br>Це може зайняти 15-30 секунд. Будь ласка, зачекайте!)
            </p>
        </div>
    `;

    try {
        // Скрипт будет ждать на этой строчке, пока Railway просыпается
        const response = await fetch('https://cursach-production.up.railway.app/api/orders');
        const orders = await response.json();
        
        // 2. ОТВЕТ ПРИШЕЛ! ОЧИЩАЕМ ЛОАДЕР
        list.innerHTML = ''; 
        
        // Рисуем карточки
        orders.forEach(order => {
            const card = `
                <article class="card">
                    <h3>${order.title}</h3>
                    <p>${order.description}</p>
                    <p class="budget">💰 Бюджет: $${order.budget}</p>
                    <span class="status-badge">${order.status}</span>
                </article>
            `;
            list.innerHTML += card;
        });
    } catch (error) {
        console.error("Помилка завантаження:", error);
        list.innerHTML = `<p style="color:red; text-align:center;">Не вдалося підключитися до сервера. Перевір консоль!</p>`;
    }
}

async function createOrder(event) {
    event.preventDefault();
    const newOrder = {
        title: document.getElementById('title').value,
        description: document.getElementById('desc').value,
        budget: parseFloat(document.getElementById('budget').value),
        deadline: new Date(document.getElementById('deadline').value).toISOString(),
        status: "Open",
        customerId: 1 
    };

    try {
        const response = await fetch('https://cursach-production.up.railway.app/api/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newOrder)
        });

        if (response.ok) {
            alert('Замовлення створено!');
            window.location.href = 'index.html';
        } else {
            alert('Помилка сервера.');
        }
    } catch (error) {
        alert('Немає зв\'язку з бекендом!');
    }
}

// Запускаем загрузку ордеров (если списка нет, она тихо завершится благодаря предохранителю)
loadOrders();

// Функція для оновлення навігації залежно від статусу входу
function updateNavigation() {
    const userName = localStorage.getItem('userName');
    const authLink = document.querySelector('nav ul li:last-child a');

    if (userName && authLink) {
        authLink.innerHTML = `👋 Привіт, ${userName} (Вийти)`;
        authLink.href = "#"; 
        
        authLink.onclick = (e) => {
            e.preventDefault();
            localStorage.removeItem('userName'); 
            alert('Ви вийшли з акаунту. До зустрічі!');
            window.location.href = 'index.html'; 
        };
    }
}



// НОВЕ: Фоновий будильник для сервера
function wakeUpServer() {
    fetch('https://cursach-production.up.railway.app/api/ping')
        .then(() => console.log('✅ Будильник спрацював, сервер прокидається...'))
        .catch(() => console.log('⏳ Сервер ще встає з ліжка...'));
}

// Функція для оновлення навігації залежно від статусу входу
function updateNavigation() {
    const userName = localStorage.getItem('userName');
    const authLink = document.querySelector('nav ul li:last-child a');

    if (userName && authLink) {
        authLink.innerHTML = `👋 Привіт, ${userName} (Вийти)`;
        authLink.href = "#"; 
        
        authLink.onclick = (e) => {
            e.preventDefault();
            localStorage.removeItem('userName'); 
            alert('Ви вийшли з акаунту. До зустрічі!');
            window.location.href = 'index.html'; 
        };
    }
}

// ВАЖНО: Ждем, пока загрузится весь HTML, и только потом ищем формы и вешаем обработчики
document.addEventListener('DOMContentLoaded', () => {
    
    // НОВЕ: Смикаємо бекенд одразу, як тільки завантажилась будь-яка сторінка
    wakeUpServer();

    // Оновлюємо навігацію
    updateNavigation();

    // ОБРОБКА РЕЄСТРАЦІЇ
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.onsubmit = (e) => {
            e.preventDefault(); // Теперь это сработает, и страница не перезагрузится!
            const name = document.getElementById('regName').value;
            const email = document.getElementById('regEmail').value;
            const password = document.getElementById('regPassword').value;

            // "Зберігаємо" користувача в пам'яті браузера
            localStorage.setItem('savedEmail', email);
            localStorage.setItem('savedPassword', password);
            localStorage.setItem('savedName', name);

            alert(`Чудово, ${name}! Акаунт створено. Тепер ви можете увійти.`);
            window.location.href = 'login.html'; // Перекидаємо на вхід
        };
    }

    // ОБРОБКА ВХОДУ (ЛОГІН)
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.onsubmit = (e) => {
            e.preventDefault();
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;

            const savedEmail = localStorage.getItem('savedEmail');
            const savedPassword = localStorage.getItem('savedPassword');
            const savedName = localStorage.getItem('savedName') || 'Користувач';

            if (!savedEmail) {
                localStorage.setItem('userName', 'Аріна'); 
                alert('Успішний вхід! Вітаємо, Аріно.');
                window.location.href = 'index.html';
            } 
            else if (email === savedEmail && password === savedPassword) {
                localStorage.setItem('userName', savedName); 
                alert(`Успішний вхід! Вітаємо, ${savedName}.`);
                window.location.href = 'index.html';
            } else {
                alert('Невірний логін або пароль!');
            }
        };
    }
});