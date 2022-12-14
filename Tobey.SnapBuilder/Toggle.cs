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

    private readonly ConfigEntry<KeyboardShortcut> shortcutConfigEntry;
    private readonly ConfigEntry<ToggleMode> modeConfigEntry;
    private readonly ConfigEntry<bool> enabledByDefaultConfigEntry;
    private readonly bool enabledByDefault;

    public KeyboardShortcut Shortcut => shortcutConfigEntry.Value;
    public ToggleMode Mode => modeConfigEntry.Value;
    public bool EnabledByDefault => enabledByDefaultConfigEntry?.Value ?? enabledByDefault;

    private bool enabled;
    private int lastFrame = -1;
    private bool disposed;

    public bool IsEnabled
    {
        get
        {
            switch (modeConfigEntry.Value)
            {
                case ToggleMode.Press:
                    int currentFrame = Time.frameCount;
                    if (shortcutConfigEntry.Value.IsDown() && currentFrame > lastFrame)
                    {
                        lastFrame = currentFrame;
                        enabled = !enabled;
                    }
                    break;
                case ToggleMode.Hold:
                    if (shortcutConfigEntry.Value.IsDown())
                    {
                        enabled = !EnabledByDefault;
                    }
                    else if (shortcutConfigEntry.Value.IsUp())
                    {
                        enabled = EnabledByDefault;
                    }
                    break;
            }
            return enabled;
        }
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
        shortcutConfigEntry.SettingChanged += Reset;
        modeConfigEntry.SettingChanged += Reset;
        if (enabledByDefaultConfigEntry is not null)
        {
            enabledByDefaultConfigEntry.SettingChanged += Reset;
        }
    }

    public void Unbind()
    {
        shortcutConfigEntry.SettingChanged -= Reset;
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
