using System.Text.Json;

namespace AbcRetailApp.Services
{
    // Helper for storing/retrieving objects (like the cart) in Session as JSON,
    // since ISession only natively supports strings and byte arrays.
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}