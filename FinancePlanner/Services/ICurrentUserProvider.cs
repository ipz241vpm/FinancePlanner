namespace FinancePlanner.Services
{
    /// <summary>
    /// Абстракція для отримання ідентифікатора або ім'я поточного авторизованого користувача.
    /// </summary>
    public interface ICurrentUserProvider
    {
        int GetCurrentUserId();
        string GetCurrentUsername();
    }
}
