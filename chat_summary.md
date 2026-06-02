# 💬 Реалізація чату в реальному часі — Підсумок

## 📌 Мета

Реалізувати двосторонній чат між **замовником** та **фрілансером** для курсової роботи на базі платформи фрілансу **ФРІЛАНС.ЮА**.

---

## ⚙️ Технологічний стек

| Компонент | Технологія |
|-----------|-----------|
| Бекенд | ASP.NET Core 9 (C#) |
| Реальний час | **ASP.NET Core SignalR** |
| БД | PostgreSQL (Railway) |
| ORM | Entity Framework Core |
| Фронтенд | Blazor WebAssembly |
| Хостинг API | Railway |
| Хостинг UI | GitHub Pages |

> **Чому SignalR, а не WebSocket вручну?**  
> SignalR автоматично обирає найкращий транспорт (WebSocket → Server-Sent Events → Long Polling). Він нативно підтримується в ASP.NET Core, добре інтегрується з Blazor WASM та підходить для хостингу на Railway.

---

## 🗂️ Архітектура чату

```
┌──────────────────────┐        SignalR Hub         ┌──────────────────────┐
│   Blazor (Замовник)  │ ◄──────────────────────► │  Blazor (Фрілансер)  │
│   OrderDetails.razor │        /chathub            │  OrderDetails.razor  │
└──────────┬───────────┘                            └───────────┬──────────┘
           │                                                    │
           │              ASP.NET Core API                      │
           └──────────────────┬─────────────────────────────────┘
                              │
                    ┌─────────▼──────────┐
                    │    ChatHub.cs       │
                    │  (SignalR Hub)      │
                    │                    │
                    │ OnlineUsers dict:  │
                    │ userId → connId    │
                    └─────────┬──────────┘
                              │
                    ┌─────────▼──────────┐
                    │   PostgreSQL DB     │
                    │  таблиця ChatMsg   │
                    └────────────────────┘
```

---

## 📁 Створені / змінені файли

### Бекенд (`FreelancePlatformApi/`)

#### [NEW] `Models/ChatMessage.cs`
Модель повідомлення в БД:
```csharp
public class ChatMessage {
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string MessageText { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    // Навігаційні властивості
    public Order? Order { get; set; }
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
}
```

#### [NEW] `Hubs/ChatHub.cs`
Головний SignalR хаб. Відповідає за:
- `OnConnectedAsync` — реєстрація користувача в `OnlineUsers` словнику
- `OnDisconnectedAsync` — видалення з `OnlineUsers`, оповіщення про офлайн
- `SendMessage(orderId, senderId, receiverId, text)` — збереження в БД + broadcast
- `MarkAsRead(orderId, senderId, receiverId)` — відмітка як прочитане
- `CheckUserOnline(userId)` — перевірка онлайн-статусу

#### [NEW] `Controllers/ChatController.cs`
REST API для чату:
- `GET api/chat/{orderId}/history?userId={id}` — завантаження історії (тільки для учасників замовлення)
- `PUT api/chat/{orderId}/read?userId={id}` — позначити повідомлення прочитаними

#### [MODIFY] `Program.cs`
```csharp
builder.Services.AddSignalR();
// ...
app.MapHub<ChatHub>("/chathub");
```
CORS оновлено для підтримки `credentials` (потрібно для SignalR):
```csharp
policy.SetIsOriginAllowed(origin => true)
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials()
```

#### [MODIFY] `Controllers/OrdersController.cs`
- Додано `PUT api/orders/{id}/accept-proposal/{proposalId}` — прийняти фрілансера, встановити `FreelancerId` та `Status = "InProgress"`
- Авто-виправлення для старих замовлень: якщо `FreelancerId == null` при `InProgress` — автоматично підставляється з пропозиції

#### [MODIFY] `Data/AppDbContext.cs`
```csharp
public DbSet<ChatMessage> ChatMessages { get; set; }
```

---

### Фронтенд (`FreelancePlatform.Client/`)

#### [MODIFY] `Pages/OrderDetails.razor`
Повний UI чату з:
- **Умовою відображення:** чат видно тільки якщо `order.Status == "InProgress"` і поточний користувач є замовником або фрілансером
- **Завантаженням історії** через REST при ініціалізації
- **Підключенням до SignalR хабу** (`HubConnectionBuilder`)
- **Обробниками подій:**
  - `ReceiveMessage` → додати повідомлення в список
  - `UserStatusChanged` → оновити індикатор онлайн
  - `UserStatusResponse` → перша перевірка при відкритті
  - `MessagesRead` → поставити сині галочки
- **`IAsyncDisposable`** — коректне відключення від хабу при закритті сторінки

#### [MODIFY] `FreelancePlatform.Client.csproj`
```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.7" />
```

---

## 🚀 Функціональність

| Функція | Статус |
|---------|--------|
| ✉️ Надсилання повідомлень в реальному часі | ✅ |
| 💾 Збереження в БД (PostgreSQL) | ✅ |
| 📜 Завантаження історії повідомлень | ✅ |
| 🟢 Індикатор "В мережі / Поза мережею" | ✅ |
| ✔️✔️ Одинарні/подвійні галочки (доставлено/прочитано) | ✅ |
| 🔒 Доступ лише при `Status = "InProgress"` | ✅ |
| 🔒 Тільки учасники замовлення бачать чат | ✅ |
| 📱 UI в стилі месенджера (бульбашки повідомлень) | ✅ |

---

## 🐛 Виправлені баги

### Bug #1 — Повідомлення "зникали" після відправки
**Причина:** SignalR використовує **власний JSON-серіалізатор**, незалежний від `AddControllers()`. Коли хаб передавав повний EF-об'єкт `ChatMessage` з навігаційними властивостями (`Order`, `Sender`, `Receiver`) — виникала помилка **циклічних посилань** і `SendAsync()` падав мовчки.

**Виправлення:** замінено на анонімний DTO:
```csharp
// ❌ Було (crash через кругові посилання):
await Clients.Client(id).SendAsync("ReceiveMessage", message);

// ✅ Стало (чистий DTO без навігаційних властивостей):
var messageDto = new {
    message.Id, message.OrderId, message.SenderId,
    message.ReceiverId, message.MessageText,
    message.SentAt, message.IsRead
};
await Clients.Client(id).SendAsync("ReceiveMessage", messageDto);
```

### Bug #2 — Фрілансер не бачив чат (Firefox)
**Причина:** Старі замовлення (створені до впровадження функції) мали `FreelancerId = null` у БД, тому умова `UserState.Id == order.FreelancerId` завжди давала `false`.

**Виправлення:** авто-виправлення в `GetOrder`:
```csharp
if (order.Status == "InProgress" && order.FreelancerId == null && order.Proposals.Any())
{
    order.FreelancerId = order.Proposals.First().FreelancerId;
    await _context.SaveChangesAsync();
}
```

---

## 🧪 Тестування

### Локальне тестування (localhost:5281 + localhost:5245)
- ✅ Відправка повідомлень — відображаються миттєво
- ✅ Підключення SignalR хабу
- ✅ Галочки доставки на відправлених повідомленнях
- ✅ Стара переписка завантажується при відкритті сторінки

### Продакшн тестування (arisnowie97.github.io/Cursach)
- ✅ Повідомлення "Продакшн тест - чат працює! 🎉" відправлено успішно
- ✅ Повідомлення відображається в UI без перезавантаження
- ✅ Railway (бекенд) + GitHub Pages (фронтенд) сумісні

---

## 📦 Деплой

| Сервіс | URL | Автодеплой |
|--------|-----|-----------|
| Бекенд API | `https://cursach-production.up.railway.app` | ✅ з гілки `main` (Railway) |
| Фронтенд | `https://arisnowie97.github.io/Cursach` | ✅ GitHub Actions (`deploy.yml`) |

---

## 💡 Висновок

Чат реалізовано через **ASP.NET Core SignalR** — оптимальне рішення для курсової роботи, яке:
- Добре інтегрується з існуючим ASP.NET Core бекендом
- Підтримується нативно в Blazor WASM через `Microsoft.AspNetCore.SignalR.Client`
- Масштабується на хостинг (Railway підтримує WebSocket)
- Не потребує додаткової інфраструктури (Redis, черги тощо)
