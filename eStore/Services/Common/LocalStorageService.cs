// eStore/Services/LocalStorageService.cs
using Microsoft.JSInterop;
using System.Text.Json;

namespace eStore.Services.Common
{
    public class LocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly StateContainer _stateContainer;

        public LocalStorageService(IJSRuntime jsRuntime, StateContainer stateContainer)
        {
            _jsRuntime = jsRuntime;
            _stateContainer = stateContainer;
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            // Handle auth tokens specially to avoid prerendering issues
            if (key == "authToken")
            {
                _stateContainer.SetAuthData(value.ToString(), 0);
            }
            else if (key == "expirationTime" && long.TryParse(value.ToString(), out long expTime))
            {
                _stateContainer.SetAuthData(_stateContainer.AuthToken, expTime);
            }

            // Store in localStorage after component is rendered
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            // For auth tokens, use state container during prerendering
            if (key == "authToken" && typeof(T) == typeof(string))
            {
                return (T)(object)_stateContainer.AuthToken;
            }

            // Use JS interop for other cases
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task RemoveItemAsync(string key)
        {
            // Handle auth tokens specially
            if (key == "authToken" || key == "expirationTime")
            {
                _stateContainer.ClearAuthData();
            }

            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}
