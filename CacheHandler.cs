using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    public abstract class CacheHandler<T> : MonoBehaviour
        where T : new()
    {

        protected abstract string FilePath { get; }
        public static T Cache;

        static bool isDirty;

        protected virtual void Start()
        {
            Init();
        }
        protected void Init()
        {
            Cache = LoadCache();
            StartCoroutine(AutoSave());
        }
        protected void SaveToDisk()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Cache, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MEGME] Failed to save {typeof(T).Name}: {e}");
            }
        }
        protected virtual T LoadCache()
        {
            try
            {
                var json = File.ReadAllText(FilePath);
                var data = JsonConvert.DeserializeObject<T>(json);
                return data ?? new T();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MEGME] Failed to load {typeof(T).Name}: {e}");
                return new T();
            }
        }
        protected IEnumerator AutoSave()
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
        public static void MarkDirty() => isDirty = true;
    }
}
