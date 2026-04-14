using System.Collections.Generic;
using UnityEngine.Animations;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public struct ReadUniversalBSJob : IAnimationJob
    {
        List<UniversalBinding> universalBindings;
        public void ProcessAnimation(AnimationStream stream)
        {
            universalBindings.ForEach(b =>
            {
                b.value = b.animatorBinding.GetFloat(stream);
            });
        }

        public void ProcessRootMotion(AnimationStream stream) { }

        public ReadUniversalBSJob(List<UniversalBinding> universalBindings)
        {
            this.universalBindings = universalBindings;
        }
    }
}