using System;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal static class SnapBuilder
    {
        public static Config Config = new Config();
        public static float LastButtonHeldTime = -1f;
        public static GameInput.Button LastButton;

        public static void Initialise()
        {
            Config.Load();
            OptionsPanelHandler.RegisterModOptions(new Options());
            InitLanguage();
        }

        public static void InitLanguage()
        {
            SetLanguage("GhostToggleSnappingHint", "Toggle snapping");
            SetLanguage("GhostToggleFineSnappingHint", "Toggle fine snapping");
            SetLanguage("GhostToggleFineRotationHint", "Toggle fine rotation");
        }

        public static string FormatButton(Toggle toggle)
        {
            string displayText = null;
            if (toggle.KeyCode == KeyCode.None)
            {
                displayText = GetLanguage("NoInputAssigned");
            }
            else
            {
                string bindingName = KeyCodeUtils.KeyCodeToString(toggle.KeyCode);
                if (!string.IsNullOrEmpty(bindingName))
                {
                    displayText = uGUI.GetDisplayTextForBinding(bindingName);
                }
                if (string.IsNullOrEmpty(displayText))
                {
                    displayText = GetLanguage("NoInputAssigned");
                }
            }
            return $"<color=#ADF8FFFF>{displayText}</color>{(toggle.KeyMode == Toggle.Mode.Hold ? " (Hold)" : string.Empty)}";
        }

        public static float RoundToNearest(float x, float y) => y * Mathf.Round(x / y);
        public static double RoundToNearest(double x, double y) => y * Math.Round(x / y);
        public static float FloorToNearest(float x, float y) => y * Mathf.Floor(x / y);
        public static double FloorToNearest(double x, double y) => y * Math.Floor(x / y);

        public static string GetLanguage(string id) => Language.main.Get(id);
        public static void SetLanguage(string id, string value) => SMLHelper.V2.Handlers.LanguageHandler.SetLanguageLine(id, value);
    }
}
