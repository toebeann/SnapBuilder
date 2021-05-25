using SMLHelper.V2.Utility;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    using Language = SMLHelper.Language;

    internal static class ControlHint
    {
        private static string FormatButton(Toggle toggle)
        {
            string displayText = null;
            if (toggle.KeyCode == KeyCode.None)
            {
                displayText = SMLHelper.Language.Get("NoInputAssigned");
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
                    displayText = SMLHelper.Language.Get("NoInputAssigned");
                }
            }
            return $"<color=#ADF8FFFF>{displayText}</color>{(toggle.KeyMode == Toggle.Mode.Hold ? " (Hold)" : string.Empty)}";
        }

        public static void Show(string hintId, Toggle toggle)
        {
            ErrorMessage.AddMessage($"{Language.Get(hintId)} ({FormatButton(toggle)})");
        }

        public static void Show(string hintId, GameInput.Button button)
        {
            ErrorMessage.AddMessage($"{Language.Get(hintId)} ({uGUI.FormatButton(button, true, ", ", false)})");
        }
    }
}
