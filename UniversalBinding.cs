using HarmonyLib;
using UnityEngine.Animations;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class UniversalBinding
    {
        public AccessTools.FieldRef<UniversalBlendshapes, float> universalBlendshapesBinding;
        public PropertyStreamHandle animatorBinding;
        public float value;
        public UniversalBinding(AccessTools.FieldRef<UniversalBlendshapes, float> universalBlendshapesBinding, PropertyStreamHandle animatorBinding, float value)
        {
            this.universalBlendshapesBinding = universalBlendshapesBinding;
            this.animatorBinding = animatorBinding;
            this.value = value;
        }
    }
}