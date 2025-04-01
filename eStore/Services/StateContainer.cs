// eStore/Services/StateContainer.cs
namespace eStore.Services
{
    public class StateContainer
    {
        public string AuthToken { get; private set; }
        public long ExpirationTime { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(AuthToken);

        public event Action OnChange;

        public void SetAuthData(string token, long expirationTime)
        {
            AuthToken = token;
            ExpirationTime = expirationTime;
            NotifyStateChanged();
        }

        public void ClearAuthData()
        {
            AuthToken = null;
            ExpirationTime = 0;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
