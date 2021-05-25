using SMLHelper.V2.Utility;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    using SMLHelper;

    internal static class ControlHint
    {
        private static string FormatButton(Toggle toggle)
        {
            string displayText = null;
            if (toggle.KeyCode == KeyCode.None)
            {
                displayText = Language.Get("NoInputAssigned");
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
                    displayText = Language.Get("NoInputAssigned");
                }
            }
            return $"<color=#ADF8FFFF>{displayText}</color>{(toggle.KeyMode == Toggle.Mode.Hold ? " (Hold)" : string.Empty)}";
        }

        public static string Get(string hintId, Toggle toggle) => $"{Language.Get(hintId)} ({FormatButton(toggle)})";

        public static string Get(string hintId, GameInput.Button button) 
            => $"{Language.Get(hintId)} ({uGUI.FormatButton(button, true, ", ", false)})";

        public static void Show(string hintId, Toggle toggle) => ErrorMessage.AddMessage(Get(hintId, toggle));

        public static void Show(string hintId, GameInput.Button button) => ErrorMessage.AddMessage(Get(hintId, button));
    }
}
