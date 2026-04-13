using FreelancePlatformApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Налаштовуємо CORS (дозволяємо фронтенду стукатися до нас)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Дозволяємо будь-який сайт (у тому числі GitHub)
              .AllowAnyMethod()   // Дозволяємо GET, POST тощо
              .AllowAnyHeader();  // Дозволяємо будь-які заголовки
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();
// Автоматично оновлюємо базу даних при запуску сервера
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 2. Вмикаємо CORS (обов'язково ПЕРЕД MapControllers)
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5245";
app.Run($"http://0.0.0.0:{port}");