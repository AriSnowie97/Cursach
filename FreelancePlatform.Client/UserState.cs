using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace FreelancePlatform.Client
{
    public static class UserState
    {
        public static bool IsLoggedIn { get; set; } = false;
        public static string Name { get; set; } = "";
        public static string LastName { get; set; } = "";
        public static string Role { get; set; } = ""; // "Customer" або "Freelancer"

        public static event Action? OnChange;
        public static void NotifyStateChanged() => OnChange?.Invoke();

        public static async Task SaveToCookies(IJSRuntime js)
        {
            var data = JsonSerializer.Serialize(new { Name, LastName, Role, IsLoggedIn });
            // Зберігаємо на 7 днів
            await js.InvokeVoidAsync("cookieFunctions.setCookie", "user_session", data, 7);
        }

        public static async Task LoadFromCookies(IJSRuntime js)
        {
            try
            {
                var data = await js.InvokeAsync<string>("cookieFunctions.getCookie", "user_session");
                if (!string.IsNullOrEmpty(data))
                {
                    var session = JsonSerializer.Deserialize<UserSession>(data);
                    if (session != null)
                    {
                        IsLoggedIn = session.IsLoggedIn;
                        Name = session.Name;
                        LastName = session.LastName;
                        Role = session.Role;
                        NotifyStateChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження кукі: {ex.Message}");
            }
        }

        // Внутрішній клас для десеріалізації
        private class UserSession
        {
            public bool IsLoggedIn { get; set; }
            public string Name { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Role { get; set; } = "";
        }
    }
}