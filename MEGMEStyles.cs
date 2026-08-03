using UnityEngine;

namespace MEGME
{
    internal static class MEGMEStyles
    {
        private static Texture2D _void;
        private static Texture2D _megmeIcon;
        private static Texture2D _alphaPicker;
        private static Texture2D _radialReverse;

        internal static Texture2D Void => _void ??= EResources.Load<Texture2D>("Void");
        internal static Texture2D Icon => _megmeIcon ??= EResources.Load<Texture2D>("Icon");
        internal static Texture2D AlphaPicker => _alphaPicker ??= EResources.Load<Texture2D>("Alpha_Picker");
        internal static Texture2D RadialReverse => _radialReverse ??= EResources.Load<Texture2D>("Radial_Reverse");

    }
}
