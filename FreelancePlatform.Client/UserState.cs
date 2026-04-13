namespace FreelancePlatform.Client
{
    public static class UserState
    {
        public static bool IsLoggedIn { get; set; } = false;
        public static string Name { get; set; } = "";
        public static string LastName { get; set; } = "";
        public static string Role { get; set; } = ""; // Сюди запишемо "Customer" або "Freelancer"
        public static event Action? OnChange;
        public static void NotifyStateChanged() => OnChange?.Invoke();
    }
}