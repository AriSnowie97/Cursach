async function loadOrders() {
    const list = document.getElementById('ordersList');
    try {
        // Твоє реальне посилання з протоколом https://
        const response = await fetch('https://cursach-production.up.railway.app/api/orders');
        const orders = await response.json();
        
        list.innerHTML = ''; 
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
        list.innerHTML = `<p style="color:red">Не вдалося підключитися до сервера. Перевір консоль!</p>`;
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
loadOrders();

// Функція для оновлення навігації залежно від статусу входу
function updateNavigation() {
    const userName = localStorage.getItem('userName');
    const authLink = document.querySelector('nav ul li:last-child a');

    if (userName && authLink) {
        // Якщо Аріна ввійшла, показуємо вітання та кнопку виходу
        authLink.innerHTML = `👋 Привіт, ${userName} (Вийти)`;
        authLink.href = "#"; // Щоб сторінка не перезавантажувалася просто так
        
        authLink.onclick = (e) => {
            e.preventDefault();
            localStorage.removeItem('userName'); // Видаляємо ім'я з пам'яті
            alert('Ви вийшли з акаунту. До зустрічі!');
            window.location.href = 'index.html'; // Повертаємо на головну
        };
    }
}

// ОБРОБКА РЕЄСТРАЦІЇ
const registerForm = document.getElementById('registerForm');
if (registerForm) {
    registerForm.onsubmit = (e) => {
        e.preventDefault();
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

        // Дістаємо дані з пам'яті
        const savedEmail = localStorage.getItem('savedEmail');
        const savedPassword = localStorage.getItem('savedPassword');
        const savedName = localStorage.getItem('savedName') || 'Користувач';

        // Якщо людина ще не реєструвалася (для тесту), пускаємо як "Аріна"
        if (!savedEmail) {
            localStorage.setItem('userName', 'Аріна'); 
            alert('Успішний вхід! Вітаємо, Аріно.');
            window.location.href = 'index.html';
        } 
        // Перевіряємо, чи збігаються пошта і пароль з тими, що ввели при реєстрації
        else if (email === savedEmail && password === savedPassword) {
            localStorage.setItem('userName', savedName); // Беремо ім'я, яке ввели при реєстрації
            alert(`Успішний вхід! Вітаємо, ${savedName}.`);
            window.location.href = 'index.html';
        } else {
            alert('Невірний логін або пароль!');
        }
    };
}

// Викликаємо перевірку при завантаженні будь-якої сторінки
document.addEventListener('DOMContentLoaded', updateNavigation);