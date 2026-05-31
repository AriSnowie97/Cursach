using FreelancePlatformApi.Data;
using FreelancePlatformApi.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// 1. Налаштовуємо CORS (дозволяємо фронтенду стукатися до нас)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGitHubPages",
        policy => policy.WithOrigins("https://arisnowie97.github.io")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());

    options.AddPolicy("AllowAll",
        policy => policy.SetIsOriginAllowed(origin => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

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
app.MapHub<ChatHub>("/chathub");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5245";
app.Run($"http://0.0.0.0:{port}");