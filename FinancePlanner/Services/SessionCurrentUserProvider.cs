namespace FinancePlanner.Services
{
    /// <summary>
    /// Реалізація ICurrentUserProvider, що делегує до SessionManager.
    /// </summary>
    public class SessionCurrentUserProvider : ICurrentUserProvider
    {
        public int GetCurrentUserId()
        {
            return SessionManager.Instance.CurrentUser?.Id ?? 0;
        }

        public string GetCurrentUsername()
        {
            return SessionManager.Instance.CurrentUser?.Username;
        }
    }
}
