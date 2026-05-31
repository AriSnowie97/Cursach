using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FreelancePlatform.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Налаштовуємо адресу API автоматично: для локальної розробки (якщо в URL є localhost/127.0.0.1) або для продакшену
var isLocal = builder.HostEnvironment.BaseAddress.Contains("localhost") || builder.HostEnvironment.BaseAddress.Contains("127.0.0.1");
var apiAddress = isLocal 
    ? "http://localhost:5245/" 
    : "https://cursach-production.up.railway.app/";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiAddress) });

await builder.Build().RunAsync();
