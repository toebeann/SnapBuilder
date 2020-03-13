using System;
using UnityEngine;

namespace SnapBuilder
{
    internal class SnapBuilder
    {
        public static Options Options = new Options();
        public static float LastButtonHeldTime = -1f;
        public static GameInput.Button LastButton;

        public static void Initialise()
        {
            SMLHelper.V2.Handlers.OptionsPanelHandler.RegisterModOptions(Options);
        }

        public static float RoundToNearest(float x, float y) => y * Mathf.Round(x / y);
        public static double RoundToNearest(double x, double y) => y * Math.Round(x / y);
        public static float FloorToNearest(float x, float y) => y * Mathf.Floor(x / y);
        public static double FloorToNearest(double x, double y) => y * Math.Floor(x / y);
    }
}
