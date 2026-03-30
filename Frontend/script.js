async function loadOrders() {
    const list = document.getElementById('ordersList');
    
    try {
        // Стукаємо до нашого API (переконайся, що порт 5245 вірний)
        const response = await fetch('http://localhost:5245/api/orders');
        const orders = await response.json();
        
        list.innerHTML = ''; // Очищуємо старе

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
        list.innerHTML = `<p style="color:red">Не вдалося підключитися до сервера. Перевір, чи запущено бекенд!</p>`;
    }
}

// Завантажуємо дані відразу при відкритті сторінки
loadOrders();