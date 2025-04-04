// eStore/Services/StateContainer.cs
namespace eStore.Services.Common
{
    public class StateContainer
    {
        public string AuthToken { get; set; }
        public long ExpirationTime { get; set; }
        public bool IsAuthenticated { get; private set; }

        // Define the event for component notification
        public event Action OnChange;

        public void SetAuthData(string token, long expirationTime)
        {
            AuthToken = token;
            ExpirationTime = expirationTime;
            IsAuthenticated = true;
            NotifyStateChanged();
        }

        public void ClearAuthData()
        {
            AuthToken = null;
            ExpirationTime = 0;
            IsAuthenticated = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }

}
