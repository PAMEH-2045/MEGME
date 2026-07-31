using System.Collections.Generic;

namespace MEGME.Settings
{
    internal class SettingsManager
    {
        static bool isActivated;

        static readonly List<ISetting> settings = new();

        public static void OnUpdate()
        {
            foreach (var set in settings)
            {
                set.SyncWithReference();
            }
        }
        public static void OnAvatarSwitch()
        {
            if (!isActivated)
                Activate();

            Reapply();
        }
        static void Activate()
        {
            foreach (var set in settings)
            {
                set.Init();
            }

            isActivated = true;
        }
        static void Reapply()
        {
            foreach (var set in settings)
            {
                set.ApplyToReference();
            }
        }
        public static void Register(ISetting setting)
        {
            if (isActivated)
                setting.Init();

            settings.Add(setting);
        }
    }
}
