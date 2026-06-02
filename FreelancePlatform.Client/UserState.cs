using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace FreelancePlatform.Client
{
    public static class UserState
    {
        public static int Id { get; set; } = 0;
        public static string Name { get; set; } = "";
        public static string LastName { get; set; } = "";
        public static string Role { get; set; } = "";
        public static bool IsLoggedIn { get; set; } = false;
        public static string? AvatarUrl { get; set; } = null;

        public static int UserId => Id; 

        public static event Action? OnChange;

        public static void NotifyStateChanged() => OnChange?.Invoke();

        // 1. ЗБЕРЕЖЕННЯ (SaveSession)
        public static async Task SaveSession(IJSRuntime js, int id, string name, string lastName, string role, string? avatarUrl = null)
        {
            Id = id;
            Name = name;
            LastName = lastName;
            Role = role;
            IsLoggedIn = true;
            AvatarUrl = avatarUrl;

            var userData = JsonSerializer.Serialize(new { Id, Name, LastName, Role, IsLoggedIn, AvatarUrl });
            await js.InvokeVoidAsync("localStorage.setItem", "user_session", userData);
            NotifyStateChanged();
        }

        // 2. ЗАВАНТАЖЕННЯ (LoadSession)
        public static async Task LoadSession(IJSRuntime js)
        {
            try 
            {
                var jsonData = await js.InvokeAsync<string>("localStorage.getItem", "user_session");
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<UserSession>(jsonData, options);
                    
                    if (data != null)
                    {
                        Id = data.Id; 
                        Name = data.Name;
                        LastName = data.LastName;
                        Role = data.Role;
                        IsLoggedIn = data.IsLoggedIn;
                        AvatarUrl = data.AvatarUrl;
                        NotifyStateChanged();
                    }
                }
            }
            catch 
            {
                // Якщо щось зламалося, обнуляємо
            }
        }

        // Оновити аватарку в сесії
        public static async Task UpdateAvatar(IJSRuntime js, string avatarUrl)
        {
            AvatarUrl = avatarUrl;
            var userData = JsonSerializer.Serialize(new { Id, Name, LastName, Role, IsLoggedIn, AvatarUrl });
            await js.InvokeVoidAsync("localStorage.setItem", "user_session", userData);
            NotifyStateChanged();
        }

        // 3. ОЧИЩЕННЯ (ClearSession)
        public static async Task ClearSession(IJSRuntime js)
        {
            Name = "";
            LastName = "";
            Role = "";
            IsLoggedIn = false;
            AvatarUrl = null;

            await js.InvokeVoidAsync("localStorage.removeItem", "user_session");
            NotifyStateChanged();
        }

        private class UserSession
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Role { get; set; } = "";
            public bool IsLoggedIn { get; set; }
            public string? AvatarUrl { get; set; }
        }
    }
}