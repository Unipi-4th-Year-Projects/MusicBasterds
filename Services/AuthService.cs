using System.Text.Json;
using Microsoft.JSInterop;
using MusicBasterds.Models;

namespace MusicBasterds.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _js;
        private const string USERS_KEY = "musicbasterds_users";
        private const string CURRENT_USER_KEY = "musicbasterds_current_user";

        public AuthService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", USERS_KEY);
            return string.IsNullOrEmpty(json)
                ? new List<User>()
                : JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private async Task SaveUsersAsync(List<User> users)
        {
            var json = JsonSerializer.Serialize(users);
            await _js.InvokeVoidAsync("localStorage.setItem", USERS_KEY, json);
        }

        public async Task<bool> RegisterAsync(User user)
        {
            var users = await GetUsersAsync();

            if (users.Any(u => u.Username == user.Username || u.Email == user.Email))
                return false;

            users.Add(user);
            await SaveUsersAsync(users);
            return true;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var users = await GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                var json = JsonSerializer.Serialize(user);
                await _js.InvokeVoidAsync("localStorage.setItem", CURRENT_USER_KEY, json);
            }

            return user;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", CURRENT_USER_KEY);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<User>(json);
        }

        public async Task LogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", CURRENT_USER_KEY);
        }
    }
}
