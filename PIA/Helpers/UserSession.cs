using System.Threading.Tasks;
using Xamarin.Essentials;

namespace PIA.Helpers
{
    public static class UserSession
    {
        public static async Task<string> GetCurrentUserIdAsync()
        {
            return await SecureStorage.GetAsync("userId");
        }

        public static async Task SetCurrentUserIdAsync(string userId)
        {
            await SecureStorage.SetAsync("userId", userId);
        }
    }
}