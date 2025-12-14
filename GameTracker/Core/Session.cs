namespace GameTracker
{
    public static class Session
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string Email { get; set; }

        public static void Clear()
        {
            UserId = 0;
            Username = null;
            Email = null;
        }

        public static bool IsLoggedIn()
        {
            return UserId > 0;
        }
    }
}