using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobey.SnapBuilder;
internal class ControlHint
{
    private static string NoInputAssigned => Language.main.Get("NoInputAssigned");

    private static string FormatButton(Toggle toggle) => $"<color=#ADF8FFFF>{GetDisplayText(toggle)}</color>";
    private static string FormatButton(KeyCode keyCode) => $"<color=#ADF8FFFF>{GetDisplayText(keyCode)}</color>";
    private static string FormatButton(IEnumerable<KeyCode> keyCodes) => string.Join(" + ", keyCodes.Select(keyCode => $"<color=#ADF8FFFF>{GetDisplayText(keyCode)}</color>"));

    private static string GetDisplayText(KeyCode keyCode) => Utils.KeyCode.ToString(keyCode) switch
    {
        string name when !string.IsNullOrEmpty(name) => uGUI.GetDisplayTextForBinding(name),
        _ => NoInputAssigned
    };
    private static string GetDisplayText(IEnumerable<KeyCode> keyCodes) => string.Join(" + ", keyCodes.Select(keyCode => GetDisplayText(keyCode)));
    private static string GetDisplayText(Toggle toggle) => toggle switch
    {
        { Shortcut: null, KeyCode: KeyCode keyCode } when keyCode != KeyCode.None => GetDisplayText(keyCode),
        { Shortcut: KeyboardShortcut shortcut } when shortcut.MainKey != KeyCode.None => GetDisplayText(new HashSet<KeyCode>(shortcut.Modifiers).Prepend(shortcut.MainKey).Where(keyCode => keyCode != KeyCode.None)),
        _ => NoInputAssigned,
    };

    public static string Get(string hint, Toggle toggle) => $"{hint} ({FormatButton(toggle)})";
    public static string Get(string hint, KeyCode keyCode) => $"{hint} ({FormatButton(keyCode)})";
    public static string Get(string hint, KeyCode keyCode1, KeyCode keyCode2) => $"{hint} ({FormatButton(keyCode1)} / {FormatButton(keyCode2)})";
    public static string Get(string hint, IEnumerable<KeyCode> keyCodes) => $"{hint} ({FormatButton(keyCodes)})";
    public static string Get(string hint, GameInput.Button button) => $"{hint} ({uGUI.FormatButton(button, true, ", ", false)})";

    public static void Show(string hint, Toggle toggle) => ErrorMessage.AddMessage(Get(hint, toggle));
    public static void Show(string hint, KeyCode keyCode) => ErrorMessage.AddMessage(Get(hint, keyCode));
    public static void Show(string hint, KeyCode keyCode1, KeyCode keyCode2) => ErrorMessage.AddMessage(Get(hint, keyCode1, keyCode2));
    public static void Show(string hint, IEnumerable<KeyCode> keyCodes) => ErrorMessage.AddMessage(Get(hint, keyCodes));
    public static void Show(string hint, GameInput.Button button) => ErrorMessage.AddMessage(Get(hint, button));
}
