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