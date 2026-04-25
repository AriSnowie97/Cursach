using Microsoft.JSInterop;
using System.Text.Json;

namespace FreelancePlatform.Client
{
    public static class UserState
    {
        public static string Name { get; set; } = "";
        public static string LastName { get; set; } = "";
        public static string Role { get; set; } = "";
        public static bool IsLoggedIn { get; set; } = false;

        public static event Action? OnChange;

        public static void NotifyStateChanged() => OnChange?.Invoke();

        // 1. Сохранение сессии
        public static async Task SaveSession(IJSRuntime js, string name, string lastName, string role)
        {
            Name = name;
            LastName = lastName;
            Role = role;
            IsLoggedIn = true;

            var userData = JsonSerializer.Serialize(new { Name, LastName, Role, IsLoggedIn });
            await js.InvokeVoidAsync("localStorage.setItem", "user_session", userData);
            
            NotifyStateChanged();
        }

        // 2. Загрузка сессии (вызывается при F5)
        public static async Task LoadSession(IJSRuntime js)
        {
            var jsonData = await js.InvokeAsync<string>("localStorage.getItem", "user_session");
            if (!string.IsNullOrEmpty(jsonData))
            {
                var data = JsonSerializer.Deserialize<UserSession>(jsonData);
                if (data != null)
                {
                    Name = data.Name;
                    LastName = data.LastName;
                    Role = data.Role;
                    IsLoggedIn = data.IsLoggedIn;
                    NotifyStateChanged();
                }
            }
        }

        // 3. Очистка (Выход)
        public static async Task ClearSession(IJSRuntime js)
        {
            Name = "";
            LastName = "";
            Role = "";
            IsLoggedIn = false;

            await js.InvokeVoidAsync("localStorage.removeItem", "user_session");
            NotifyStateChanged();
        }

        private class UserSession
        {
            public string Name { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Role { get; set; } = "";
            public bool IsLoggedIn { get; set; }
        }
    }
}