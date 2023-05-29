using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Tobey.SnapBuilder;
public class Toggle : IDisposable
{
    public enum ToggleMode
    {
        Press, Hold
    }

    private readonly ConfigEntry<KeyCode> keyCodeConfigEntry;
    private readonly ConfigEntry<KeyboardShortcut> shortcutConfigEntry;
    private readonly ConfigEntry<ToggleMode> modeConfigEntry;
    private readonly ConfigEntry<bool> enabledByDefaultConfigEntry;
    private readonly bool enabledByDefault;

    public KeyCode? KeyCode => keyCodeConfigEntry?.Value;
    public KeyboardShortcut? Shortcut => shortcutConfigEntry?.Value;
    public ToggleMode Mode => modeConfigEntry.Value;
    public bool EnabledByDefault => enabledByDefaultConfigEntry?.Value ?? enabledByDefault;

    private bool enabled;
    private bool disposed;

    public bool IsEnabled
    {
        get
        {
            switch (modeConfigEntry.Value)
            {
                case ToggleMode.Press:
                    if (shortcutConfigEntry?.Value.IsDown() ?? Input.GetKeyDown(keyCodeConfigEntry.Value))
                    {
                        enabled = !enabled;
                    }
                    break;
                case ToggleMode.Hold:
                    if (shortcutConfigEntry?.Value.IsPressed() ?? Input.GetKey(keyCodeConfigEntry.Value))
                    {
                        enabled = !EnabledByDefault;
                    }
                    else
                    {
                        enabled = EnabledByDefault;
                    }
                    break;
            }
            return enabled;
        }
    }

    public Toggle(ConfigEntry<KeyCode> keyCode, ConfigEntry<ToggleMode> mode, ConfigEntry<bool> enabledByDefault)
    {
        keyCodeConfigEntry = keyCode;
        modeConfigEntry = mode;
        enabledByDefaultConfigEntry = enabledByDefault;
        enabled = enabledByDefault.Value;
    }

    public Toggle(ConfigEntry<KeyCode> keyCode, ConfigEntry<ToggleMode> mode, bool enabledByDefault = false)
    {
        keyCodeConfigEntry = keyCode;
        modeConfigEntry = mode;
        this.enabledByDefault = enabled = enabledByDefault;
    }

    public Toggle(ConfigEntry<KeyboardShortcut> shortcut, ConfigEntry<ToggleMode> mode, ConfigEntry<bool> enabledByDefault)
    {
        shortcutConfigEntry = shortcut;
        modeConfigEntry = mode;
        enabledByDefaultConfigEntry = enabledByDefault;
        enabled = enabledByDefault.Value;
    }

    public Toggle(ConfigEntry<KeyboardShortcut> shortcut, ConfigEntry<ToggleMode> mode, bool enabledByDefault = false)
    {
        shortcutConfigEntry = shortcut;
        modeConfigEntry = mode;
        this.enabledByDefault = enabled = enabledByDefault;
    }

    public void Bind()
    {
        if (shortcutConfigEntry is not null)
        {
            shortcutConfigEntry.SettingChanged += Reset;
        }
        if (keyCodeConfigEntry is not null)
        {
            keyCodeConfigEntry.SettingChanged += Reset;
        }
        modeConfigEntry.SettingChanged += Reset;
        if (enabledByDefaultConfigEntry is not null)
        {
            enabledByDefaultConfigEntry.SettingChanged += Reset;
        }
    }

    public void Unbind()
    {
        if (shortcutConfigEntry is not null)
        {
            shortcutConfigEntry.SettingChanged -= Reset;
        }
        if (keyCodeConfigEntry is not null)
        {
            keyCodeConfigEntry.SettingChanged -= Reset;
        }
        modeConfigEntry.SettingChanged -= Reset;
        if (enabledByDefaultConfigEntry is not null)
        {
            enabledByDefaultConfigEntry.SettingChanged -= Reset;
        }
    }

    private void Reset(object _, EventArgs __) => Reset();
    public void Reset() => enabled = EnabledByDefault;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                Unbind();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
