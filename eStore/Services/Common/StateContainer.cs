// eStore/Services/StateContainer.cs
namespace eStore.Services.Common
{
    public class StateContainer
    {
        public string AuthToken { get; private set; }
        public long ExpirationTime { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(AuthToken);

        public event Action OnChange;

        public void SetAuthData(string token, long expirationTime)
        {
            Console.WriteLine($"Setting auth data. Token length: {token?.Length ?? 0}, ExpirationTime: {expirationTime}");
            AuthToken = token;
            ExpirationTime = expirationTime;
            Console.WriteLine($"Auth data set. IsAuthenticated: {IsAuthenticated}");
            NotifyStateChanged();
        }

        public void ClearAuthData()
        {
            Console.WriteLine("Clearing auth data");
            AuthToken = null;
            ExpirationTime = 0;
            Console.WriteLine("Auth data cleared");
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            Console.WriteLine("Notifying state change");
            OnChange?.Invoke();
        }
    }
}
