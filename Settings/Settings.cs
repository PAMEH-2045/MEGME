using Newtonsoft.Json.Linq;

namespace MEGME.Settings
{
    public static class Settings
    {
        public static bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            if (!SettingsCacheHandler.Cache.TryGetValue(key, out var token))
                return false;

            value = token.ToObject<T>();
            return true;
        }

        public static void Set<T>(string key, T value)
        {
            SettingsCacheHandler.Cache[key] = JToken.FromObject(value);
            SettingsCacheHandler.MarkDirty();
        }

        public static T Get<T>(string key)
        {
            return TryGetValue<T>(key, out var value) ? value : default;
        }
    }
}
