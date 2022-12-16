using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobey.SnapBuilder;
internal class ControlHint
{
    private static string NoInputAssigned => Language.main.Get("NoInputAssigned");

    private static string FormatButton(Toggle toggle) => $"<color=#ADF8FFFF>{GetDisplayText(toggle)}</color>";

    private static string GetDisplayText(KeyCode keyCode) => Utils.KeyCode.ToString(keyCode) switch
    {
        string name when !string.IsNullOrEmpty(name) => uGUI.GetDisplayTextForBinding(name),
        _ => NoInputAssigned
    };
    private static string GetDisplayText(IEnumerable<KeyCode> keyCodes) => string.Join(" + ", keyCodes.Select(keyCode => GetDisplayText(keyCode)));
    private static string GetDisplayText(Toggle toggle) => toggle switch
    {
        { Shortcut.MainKey: KeyCode.None } => NoInputAssigned,
        _ => GetDisplayText(new HashSet<KeyCode>(toggle.Shortcut.Modifiers).Prepend(toggle.Shortcut.MainKey).Where(keyCode => keyCode != KeyCode.None))
    };

    public static string Get(string hint, Toggle toggle) => $"{hint} ({FormatButton(toggle)})";
    public static string Get(string hint, GameInput.Button button) => $"{hint} ({uGUI.FormatButton(button, true, ", ", false)})";

    public static void Show(string hint, Toggle toggle) => ErrorMessage.AddMessage(Get(hint, toggle));
    public static void Show(string hint, GameInput.Button button) => ErrorMessage.AddMessage(Get(hint, button));
}
