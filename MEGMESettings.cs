using HarmonyLib;
using MEGME.Settings;
using System;
using UnityEngine;

namespace MEGME
{
    class MEGMESettings
    {
        static readonly Func<object> AvatarGravityController = () => CurrentModel.AvatarGravityController.Inst;
        static readonly Func<object> DynamicBoneAvatarGravityController = () => CurrentModel.DynamicBoneAvatarGravityController.Inst;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SetupSettingsMenu()
        {
            RadialMenuController.RegisterSettingsMenu(
                ModSettings.SubMenu(
                    name: "MEGME",
                    icon: EResources.Load<Texture2D>("Icon"),
                    ModSettings.Radial(
                        name: "SpringBone inertia power",
                        setting: Setting<float>.From(
                            AvatarGravityController,
                            AccessTools.Field(typeof(AvatarGravityController), "impactMultiplier")
                        ),
                        min: 0f,
                        max: 7.5f,
                        checkpoint: 1.5f,
                        displayType: ModSettings.DisplayType.Percentage
                    ),
                    ModSettings.Radial(
                        name: "DynamicBone inertia power",
                        setting: Setting<float>.From(
                            DynamicBoneAvatarGravityController,
                            AccessTools.Field(typeof(DynamicBoneAvatarGravityController), "outputRange")
                        ),
                        min: 0f,
                        max: 0.65f,
                        checkpoint: 0.13f,
                        displayType: ModSettings.DisplayType.Percentage
                    )
                )
            );
        }

    }
}
