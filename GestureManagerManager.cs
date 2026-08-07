using BlackStartX.GestureManager;
using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using BlackStartX.GestureManager.Modules;
using MEGME.Settings;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UniVRM10;
using VRC.SDK3.Avatars.Components;
using VRM;
using static VRC.SDKBase.VRC_AvatarDescriptor;

namespace MEGME
{
    [DefaultExecutionOrder(+10)] // Should run after all possible MEManipulators
    public class GestureManagerManager : MonoBehaviour
    {
        [SerializeField] private GestureManager Manager;
        [SerializeField] private RadialMenuController radialMenuController;

        [SerializeField] private bool bindMEController = true;

        bool? bigScreenWasActive;

        void OnEnable()
        {
            CurrentModel.OnAvatarSwitch += SettingsManager.OnAvatarSwitch;
            CurrentModel.OnAvatarSwitch += OnAvatarSwitch;
            CurrentModel.OnAvatarSwitch += radialMenuController.OnAvatarSwitch;
        }
        void Start()
        {
            CurrentModel.OnStart();

            Manager.settings = new ModuleSettings();
        }
        void Update()
        {
            CurrentModel.OnUpdate();
            SettingsManager.OnUpdate();
            radialMenuController.OnUpdate();

            if (Manager == null || Manager.Module == null) return;

            if (bigScreenWasActive != (bigScreenWasActive = CurrentModel.IsBigScreenActive))
            {
                Manager.Module.AvatarTools.ContactsClickable.BlockExecution = !bigScreenWasActive.Value;
            }
        }
        void OnDisable()
        {
            CurrentModel.OnAvatarSwitch -= SettingsManager.OnAvatarSwitch;
            CurrentModel.OnAvatarSwitch -= OnAvatarSwitch;
            CurrentModel.OnAvatarSwitch -= radialMenuController.OnAvatarSwitch;
        }
        private void OnAvatarSwitch()
        {
            var descriptor = CurrentModel.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                Manager.UnlinkModule();
                return;
            }

            var module = new ModuleVrc3(descriptor);

            Manager.SetModule(module);

            var isBlendShapesConfigured = CurrentModel.GetComponent<Vrm10Instance>()?.Runtime.Expression.ExpressionKeys.Count > 0
                || CurrentModel.GetComponent<VRMBlendShapeProxy>()?.BlendShapeAvatar.Clips.Count > 0;
            if (!isBlendShapesConfigured)
            {
                TrySetupBlendshapes();
            }
        }

        private void TrySetupBlendshapes()
        {
            var descriptor = CurrentModel.GetComponent<VRCAvatarDescriptor>();

            var visemeMesh = descriptor.VisemeSkinnedMesh;
            var eyelidMesh = descriptor.customEyeLookSettings.eyelidsSkinnedMesh;

            CurrentModel.UniversalBlendshapes.proxy0 = CurrentModel.AddComponent<VRMBlendShapeProxy>();
            var blendShapeAvatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();

            foreach (var (preset, entry) in VRC_VRM)
            {
                string match = null;

                var (targetMesh, targetIndex) = entry.Source switch
                {
                    BlendShapeSourceType.VisemeSkinnedMesh => FromViseme(entry.Viseme),
                    BlendShapeSourceType.EyelidsSkinnedMesh => FromEyelid(entry.EyelidState),
                    _ => (null, -1)
                };

                if (targetMesh == null && entry.SearchNames != null)
                {
                    (targetMesh, targetIndex, match) = Search(entry.SearchNames);
                }
                if (targetMesh != null && targetIndex >= 0)
                {
                    var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                    clip.Preset = preset;
                    clip.Values = [
                        new BlendShapeBinding
                        {
                            RelativePath = RelativePathFrom(targetMesh.transform, CurrentModel.transform),
                            Index = targetIndex,
                            Weight = 100f
                        }
                    ];

                    blendShapeAvatar.Clips.Add(clip);

                    Debug.Log($"[MEGME] Preset {preset}:{match ?? entry.Source.ToString()}, {targetMesh.name}[{targetIndex}]" +
                        $"->{targetMesh.sharedMesh.GetBlendShapeName(targetIndex)}");
                }
            }

            CurrentModel.UniversalBlendshapes.proxy0.BlendShapeAvatar = blendShapeAvatar;

            (SkinnedMeshRenderer targetMesh, int targetIndex) FromViseme(Viseme viseme)
            {
                if (visemeMesh == null) return (null, -1);

                int visemeIndex = (int)viseme;

                if (visemeIndex >= descriptor.VisemeBlendShapes.Length) return (null, -1);

                var blendShapeName = descriptor.VisemeBlendShapes[visemeIndex];
                var index = visemeMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);

                if (index <= 0) return (null, -1);

                return (visemeMesh, index);
            }
            (SkinnedMeshRenderer targetMesh, int targetIndex) FromEyelid(EyelidState state)
            {
                if (eyelidMesh == null) return (null, -1);

                int eyelidIndex = (int)state;

                if (eyelidIndex >= descriptor.customEyeLookSettings.eyelidsBlendshapes.Length) return (null, -1);

                var index = descriptor.customEyeLookSettings.eyelidsBlendshapes[eyelidIndex];
                if (index <= 0) return (null, -1);

                return (eyelidMesh, index);
            }
            (SkinnedMeshRenderer targetMesh, int targetIndex, string match) Search(string[] SearchNames)
            {
                foreach (var mesh in new[] { eyelidMesh, visemeMesh })
                {
                    if (mesh == null) continue;

                    for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
                    {
                        var blendShapeName = mesh.sharedMesh.GetBlendShapeName(i);
                        foreach (var search in SearchNames)
                        {
                            if (StringToRegex(search).IsMatch(blendShapeName))
                                return (mesh, i, search);
                        }
                    }
                }

                return (null, -1, null);
            }
        }

        public static Regex StringToRegex(string s)
        {
            var escaped = Regex.Escape(s);
            var deGlob = escaped.Replace("\\*", ".*");
            return new Regex("^" + deGlob + "$", RegexOptions.IgnoreCase);
        }

        // MEX3.3.0 UniVRM-0.128.3 UniGLTF.UnityExtensions.RelativePathFrom NonExtension variant
        public static string RelativePathFrom(Transform self, Transform root)
        {
            var path = new List<String>();
            for (var current = self; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return String.Join("/", path.ToArray());
                }

                path.Insert(0, current.name);
            }

            throw new Exception("no RelativePath");
        }
        // AAO_Merged_*_BSNumber
        Dictionary<BlendShapePreset, BlendShapeMapEntry> VRC_VRM = new Dictionary<BlendShapePreset, BlendShapeMapEntry>
        {
            { BlendShapePreset.Neutral, BlendShapeMapEntry.From(Viseme.sil, "vrc.v_sil", "sil", "neutral") },
            { BlendShapePreset.A, BlendShapeMapEntry.From(Viseme.aa, "vrc.v_aa", "aa", "あ","mouth*a*") },
            { BlendShapePreset.I, BlendShapeMapEntry.From(Viseme.ih, "vrc.v_ih", "ih", "い", "mouth*i*") },
            { BlendShapePreset.U, BlendShapeMapEntry.From(Viseme.ou, "vrc.v_ou", "ou", "う", "mouth*u*") },
            { BlendShapePreset.E, BlendShapeMapEntry.From(Viseme.E, "vrc.v_e", "e", "え", "mouth*e*") },
            { BlendShapePreset.O, BlendShapeMapEntry.From(Viseme.oh, "vrc.v_oh", "oh", "お", "mouth*o*") },
            { BlendShapePreset.Blink, BlendShapeMapEntry.From(EyelidState.Blink, "vrc.blink", "*blink", "まばたき", "blink_double", "eyeclosed") },
            { BlendShapePreset.Joy, BlendShapeMapEntry.From("*joy", "*happy", "にっこり") },
            { BlendShapePreset.Angry, BlendShapeMapEntry.From("*angry", "怒り") },
            { BlendShapePreset.Sorrow, BlendShapeMapEntry.From("*sorrow", "*sad", "困る") },
            { BlendShapePreset.Fun, BlendShapeMapEntry.From("*fun", "ワ") },
            { BlendShapePreset.LookUp, BlendShapeMapEntry.From(EyelidState.LookingUp, "vrc.looking_up", "*look*up*", "eye_move_up", "eye_up") },
            { BlendShapePreset.LookDown, BlendShapeMapEntry.From(EyelidState.LookingDown, "vrc.looking_down", "*look*down*", "eye_move_down", "eye_down") },
            { BlendShapePreset.LookLeft, BlendShapeMapEntry.From("*look*left*", "eyes_left", "eye_left", "eye_move_l") },
            { BlendShapePreset.LookRight, BlendShapeMapEntry.From("*look*right*", "eyes_right", "eye_right", "eye_move_r") },
            { BlendShapePreset.Blink_L, BlendShapeMapEntry.From("*blink*l", "eye_close_l", "eye_close_left", "eye_wink_l", "eyeclosedleft", "ウィンク２") },
            { BlendShapePreset.Blink_R, BlendShapeMapEntry.From("*blink*r", "eye_close_r", "eye_close_right", "eye_wink_r", "eyeclosedright", "ｳｨﾝｸ２右") },
        };
        struct BlendShapeMapEntry
        {
            public BlendShapeSourceType Source;
            public Viseme Viseme;
            public EyelidState EyelidState;
            public string[] SearchNames;

            public static BlendShapeMapEntry From(Viseme viseme, params string[] names)
            {
                return new BlendShapeMapEntry() { Source = BlendShapeSourceType.VisemeSkinnedMesh, Viseme = viseme, SearchNames = names };
            }
            public static BlendShapeMapEntry From(EyelidState state, params string[] names)
            {
                return new BlendShapeMapEntry() { Source = BlendShapeSourceType.EyelidsSkinnedMesh, EyelidState = state, SearchNames = names };
            }
            public static BlendShapeMapEntry From(string name, params string[] s)
            {
                return new BlendShapeMapEntry() { Source = BlendShapeSourceType.ManualSearch, SearchNames = [name, .. s] };
            }
        }
        enum BlendShapeSourceType
        {
            VisemeSkinnedMesh,
            EyelidsSkinnedMesh,
            ManualSearch
        }
        // VRCSDKA3.9.0 com.vrchat.avatars\Editor\VRCSDK\SDK3A\Components3\VRCAvatarDescriptorEditor3EyeLook.cs:300
        enum EyelidState
        {
            Blink,
            LookingUp,
            LookingDown
        }
    }
}