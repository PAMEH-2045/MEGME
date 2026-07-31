using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static BlackStartX.GestureManager.SettingsCacheHandler;

namespace BlackStartX.GestureManager
{
    public class SettingsCacheHandler : CacheHandler<SettingsCache>
    {

        protected override string FilePath => Path.Combine(Application.persistentDataPath, "megme_settings.json");

        [Serializable]
        public class SettingsCache : Dictionary<string, JToken>
        {
            public SettingsCache() { }
            public SettingsCache(Dictionary<string, JToken> source) : base(source) { }
        }
    }
}