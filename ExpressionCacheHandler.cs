using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BlackStartX.GestureManager.ExpressionsCacheHandler;

namespace BlackStartX.GestureManager
{
    public class ExpressionsCacheHandler : CacheHandler<ExpressionsCache>
    {
        public static SaveLoadHandler saveLoadHandler;

        protected override string FilePath => Path.Combine(BaseDir, "megme_expressions_cache.json");
        static string BaseDir;

        static AccessTools.FieldRef<AvatarLibraryMenu, List<AvatarLibraryMenu.AvatarEntry>> avatarEntries = AccessTools.FieldRefAccess<AvatarLibraryMenu, List<AvatarLibraryMenu.AvatarEntry>>("avatarEntries");

        protected override void Start()
        {
            saveLoadHandler = GameObject.FindFirstObjectByType<SaveLoadHandler>();
            BaseDir = (string)typeof(SaveLoadHandler).GetProperty("BaseDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(saveLoadHandler);

            Init();

            RemoveDeleted();
        }
        void RemoveDeleted()
        {
            var avatarLibraryMenu = GameObject.FindFirstObjectByType<AvatarLibraryMenu>();

            var avatarEntries = ExpressionsCacheHandler.avatarEntries(avatarLibraryMenu);

            var ids = new HashSet<string>(avatarEntries.Select(e => e.filePath));

            var updatedCache = Cache
                .Where(pair => ids.Contains(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (Cache.Count != updatedCache.Count)
            {
                Cache = new ExpressionsCache(updatedCache);
                SaveToDisk();
            }
        }

        protected override ExpressionsCache LoadCache()
        {
            try
            {
                var json = File.ReadAllText(FilePath);
                var data = JsonConvert.DeserializeObject<ExpressionsCache>(json);
                return data ?? new ExpressionsCache();
            }
            catch (Exception e)
            {
                try
                {
                    var json = File.ReadAllText(FilePath);
                    var data = JsonConvert.DeserializeObject<ExpressionsCache_Old>(json);
                    Debug.Log($"[MEGME] Migrating {typeof(ExpressionsCache).Name}");
                    return new ExpressionsCache(data.avatarsParams);
                }
                catch { }

                Debug.LogError($"[MEGME] Failed to load {typeof(ExpressionsCache).Name}: {e}");
                return new ExpressionsCache();
            }
        }

        [Serializable]
        public class ExpressionsCache_Old
        {
            public Dictionary<string, Dictionary<string, float>> avatarsParams = new();
        }
        [Serializable]
        public class ExpressionsCache : Dictionary<string, Dictionary<string, float>>
        {
            public ExpressionsCache() { }
            public ExpressionsCache(Dictionary<string, Dictionary<string, float>> source) : base(source) { }
        }
    }
}