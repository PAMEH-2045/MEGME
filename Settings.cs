using HarmonyLib;
using System;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    class Settings
    {
        static readonly Func<object> AvatarGravityController = () => CurrentModel.AvatarGravityControllerProxy.Inst;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SetupSettingsMenu()
        {
            RadialMenuController.RegisterSettingsMenu(
                ModSettings.SubMenu(
                    name: "MEGME",
                    icon: EResources.Load<Texture2D>("Icon"),
                    ModSettings.Radial(
                        name: "SpringBone inertia power",
                        radialField: ModSettings.ValueRef.From(
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
                        radialField: ModSettings.ValueRef.From(
                            null,
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
