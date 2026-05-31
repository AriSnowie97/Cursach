using FreelancePlatformApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// 1. Налаштовуємо CORS (дозволяємо фронтенду стукатися до нас)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGitHubPages",
        policy => policy.WithOrigins("https://arisnowie97.github.io")
                        .AllowAnyMethod()
                        .AllowAnyHeader());

    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

app.UseCors("AllowAll");

// --- ДОБАВЛЯЕМ АВТО-ОБНОВЛЕНИЕ БАЗЫ ДАННЫХ ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); 
}
// ----------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5245";
app.Run($"http://0.0.0.0:{port}");