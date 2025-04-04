// eStore/Services/StateContainer.cs
namespace eStore.Services.Common
{
    public class StateContainer
    {
        private string _authToken;
        private long _expirationTime;
        private string _userRole;

        public string AuthToken
        {
            get => _authToken;
            set
            {
                if (_authToken != value)
                {
                    _authToken = value;
                    NotifyStateChanged();
                }
            }
        }

        public long ExpirationTime
        {
            get => _expirationTime;
            set
            {
                if (_expirationTime != value)
                {
                    _expirationTime = value;
                    NotifyStateChanged();
                }
            }
        }

        public string UserRole
        {
            get => _userRole;
            private set
            {
                if (_userRole != value)
                {
                    _userRole = value;
                    NotifyStateChanged();
                }
            }
        }

        public bool IsAuthenticated
        {
            get => !string.IsNullOrEmpty(_authToken) &&
                   DateTimeOffset.UtcNow.ToUnixTimeSeconds() < _expirationTime;
        }

        public event Action OnChange;

        public void SetAuthData(string token, long expirationTime, string role = "")
        {
            bool stateChanged = false;

            if (_authToken != token)
            {
                _authToken = token;
                stateChanged = true;
            }

            if (_expirationTime != expirationTime)
            {
                _expirationTime = expirationTime;
                stateChanged = true;
            }

            if (_userRole != role)
            {
                _userRole = role;
                stateChanged = true;
            }

            if (stateChanged)
            {
                NotifyStateChanged();
            }
        }

        public void ClearAuthData()
        {
            bool stateChanged = false;

            if (_authToken != null)
            {
                _authToken = null;
                stateChanged = true;
            }

            if (_expirationTime != 0)
            {
                _expirationTime = 0;
                stateChanged = true;
            }

            if (_userRole != string.Empty)
            {
                _userRole = string.Empty;
                stateChanged = true;
            }

            if (stateChanged)
            {
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}