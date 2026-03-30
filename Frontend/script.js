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

async function createOrder(event) {
    event.preventDefault(); // Щоб сторінка не моргала при натисканні

    const newOrder = {
        title: document.getElementById('title').value,
        description: document.getElementById('desc').value,
        budget: parseFloat(document.getElementById('budget').value),
        deadline: new Date(document.getElementById('deadline').value).toISOString(),
        status: "Open",
        customerId: 1 
    };

    try {
        const response = await fetch('http://localhost:5245/api/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newOrder)
        });

        if (response.ok) {
            document.getElementById('orderForm').reset(); // Очищаємо поля
            loadOrders(); // Одразу показуємо нове замовлення
        } else {
            alert('Помилка сервера. Замовлення не створено.');
        }
    } catch (error) {
        alert('Немає зв\'язку з бекендом!');
    }
}

// Завантажуємо дані відразу при відкритті сторінки
loadOrders();