using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    public class ExpressionsCacheHandler : MonoBehaviour
    {
        public static ExpressionsCache Cache;
        public static SaveLoadHandler saveLoadHandler;

        public static string BaseDir;
        public static string expressionsCacheFilepath;

        public static bool isDirty;

        static AccessTools.FieldRef<AvatarLibraryMenu, List<AvatarLibraryMenu.AvatarEntry>> avatarEntries = AccessTools.FieldRefAccess<AvatarLibraryMenu, List<AvatarLibraryMenu.AvatarEntry>>("avatarEntries");

        void Start()
        {
            saveLoadHandler = GameObject.FindFirstObjectByType<SaveLoadHandler>();
            BaseDir = (string)typeof(SaveLoadHandler).GetProperty("BaseDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(saveLoadHandler);
            expressionsCacheFilepath = Path.Combine(BaseDir, "megme_expressions_cache.json");

            Cache = LoadCache();

            RemoveDeleted();

            StartCoroutine(AutoSave());
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
        public static void SaveToDisk()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Cache, Formatting.Indented);
                File.WriteAllText(expressionsCacheFilepath, json);
            }
            catch (Exception e)
            {
                Debug.LogError("[MEGME] Failed to save Expressions cache: " + e);
            }
        }
        public static ExpressionsCache LoadCache()
        {
            try
            {
                var json = File.ReadAllText(expressionsCacheFilepath);
                var data = JsonConvert.DeserializeObject<ExpressionsCache>(json);
                return data ?? new ExpressionsCache();
            }
            catch (Exception e)
            {
                try
                {
                    var json = File.ReadAllText(expressionsCacheFilepath);
                    var data = JsonConvert.DeserializeObject<ExpressionsCache_Old>(json);
                    Debug.Log("[MEGME] Migrating cache");
                    return new ExpressionsCache(data.avatarsParams);
                } catch { }

                Debug.LogError("[MEGME] Failed to load Expressions cache: " + e);
                return new ExpressionsCache();
            }
        }
        IEnumerator AutoSave()
        {
            while (true)
            {
                if (isDirty)
                {
                    SaveToDisk();
                    isDirty = false;
                }

                yield return new WaitForSeconds(10);
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