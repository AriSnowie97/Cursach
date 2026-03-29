using FreelancePlatformApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключаем нашу базу данных PostgreSQL к проекту
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Добавляем поддержку контроллеров (они нам понадобятся на следующем шаге)
builder.Services.AddControllers();

// === ВСЕ НАСТРОЙКИ ДОЛЖНЫ БЫТЬ ВЫШЕ ЭТОЙ СТРОКИ ===
var app = builder.Build();
// ===================================================

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();