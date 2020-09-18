﻿using SMLHelper.V2.Json;
using SMLHelper.V2.Json.Converters;
using SMLHelper.V2.Options.Attributes;
using SMLHelper.V2.Options;
#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
#elif BELOWZERO
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    [Menu("SnapBuilder", LoadOn = MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened)]
    internal class Config : ConfigFile
    {
        [JsonIgnore]
        private Toggle snapping;

        [JsonIgnore]
        public Toggle Snapping => snapping ??= new Toggle(ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);

        [JsonIgnore]
        private Toggle fineSnapping;

        [JsonIgnore]
        public Toggle FineSnapping => fineSnapping ??= new Toggle(FineSnappingKey, FineSnappingMode, false);

        [JsonIgnore]
        private Toggle fineRotation;
        [JsonIgnore]
        public Toggle FineRotation => fineRotation ??= new Toggle(FineRotationKey, FineRotationMode, false);

        [JsonIgnore]
        private Toggle toggleRotation;
        [JsonIgnore]
        public Toggle ToggleRotation => toggleRotation ??= new Toggle(ToggleRotationKey, ToggleRotationMode, false);

        [Toggle(LabelLanguageId = Lang.Option.DEFAULT_SNAPPING_ENABLED), OnChange(nameof(EnabledByDefaultChanged))]
        public bool EnabledByDefault { get; set; } = true;
        private void EnabledByDefaultChanged(ToggleChangedEventArgs e)
            => Snapping.EnabledByDefault = e.Value;

        [Keybind(LabelLanguageId = Lang.Option.TOGGLE_SNAPPING_KEY), OnChange(nameof(ToggleSnappingKeyChanged))]
        public KeyCode ToggleSnappingKey { get; set; } = KeyCode.Mouse2;
        private void ToggleSnappingKeyChanged(KeybindChangedEventArgs e)
            => Snapping.KeyCode = e.Key;

        [JsonConverter(typeof(StringEnumConverter))]
        [Choice(LabelLanguageId = Lang.Option.TOGGLE_SNAPPING_MODE), OnChange(nameof(ToggleSnappingModeChanged))]
        public Toggle.Mode ToggleSnappingMode { get; set; } = Toggle.Mode.Press;
        private void ToggleSnappingModeChanged(ChoiceChangedEventArgs e)
            => Snapping.KeyMode = (Toggle.Mode)e.Index;

        [Keybind(LabelLanguageId = Lang.Option.FINE_SNAPPING_KEY), OnChange(nameof(FineSnappingKeyChanged))]
        public KeyCode FineSnappingKey { get; set; } = KeyCode.LeftControl;
        private void FineSnappingKeyChanged(KeybindChangedEventArgs e)
            => FineSnapping.KeyCode = e.Key;

        [JsonConverter(typeof(StringEnumConverter))]
        [Choice(LabelLanguageId = Lang.Option.FINE_SNAPPING_MODE), OnChange(nameof(FineSnappingModeChanged))]
        public Toggle.Mode FineSnappingMode { get; set; } = Toggle.Mode.Hold;
        private void FineSnappingModeChanged(ChoiceChangedEventArgs e)
            => FineSnapping.KeyMode = (Toggle.Mode)e.Index;

        [Keybind(LabelLanguageId = Lang.Option.FINE_ROTATION_KEY), OnChange(nameof(FineRotationKeyChanged))]
        public KeyCode FineRotationKey { get; set; } = KeyCode.LeftAlt;
        private void FineRotationKeyChanged(KeybindChangedEventArgs e)
            => FineRotation.KeyCode = e.Key;

        [JsonConverter(typeof(StringEnumConverter))]
        [Choice(LabelLanguageId = Lang.Option.FINE_ROTATION_MODE), OnChange(nameof(FineRotationModeChanged))]
        public Toggle.Mode FineRotationMode { get; set; } = Toggle.Mode.Hold;
        private void FineRotationModeChanged(ChoiceChangedEventArgs e)
            => FineRotation.KeyMode = (Toggle.Mode)e.Index;

        [Keybind(LabelLanguageId = Lang.Option.TOGGLE_ROTATION_KEY), OnChange(nameof(EnableRotationKeyChanged))]
        public KeyCode ToggleRotationKey { get; set; } = KeyCode.Q;
        private void EnableRotationKeyChanged(KeybindChangedEventArgs e)
            => ToggleRotation.KeyCode = e.Key;

        [Choice(LabelLanguageId = Lang.Option.TOGGLE_ROTATION_MODE), OnChange(nameof(EnableRotationModeChanged))]
        public Toggle.Mode ToggleRotationMode { get; set; } = Toggle.Mode.Hold;
        private void EnableRotationModeChanged(ChoiceChangedEventArgs e)
            => ToggleRotation.KeyMode = (Toggle.Mode)e.Index;

        [JsonConverter(typeof(FloatConverter), 2)]
        [Slider(0.01f, 1, LabelLanguageId = Lang.Option.SNAP_ROUNDING, Step = 0.01f, Format = "{0:##0%}", DefaultValue = 0.5f)]
        public float SnapRounding { get; set; } = 0.5f;

        [JsonConverter(typeof(FloatConverter), 2)]
        [Slider(0.01f, 1, LabelLanguageId = Lang.Option.FINE_SNAP_ROUNDING, Step = 0.01f, Format = "{0:##0%}", DefaultValue = 0.2f)]
        public float FineSnapRounding { get; set; } = 0.2f;

        [Slider(0, 90, LabelLanguageId = Lang.Option.ROTATION_ROUNDING, DefaultValue = 45)]
        public int RotationRounding { get; set; } = 45;

        [Slider(0, 45, LabelLanguageId = Lang.Option.FINE_ROTATION_ROUNDING, DefaultValue = 5)]
        public int FineRotationRounding { get; set; } = 5;

        public bool HasUpgraded = false;

        public void Initialise()
        {
            Upgrade();
        }

        public void ResetToggles()
        {
            Snapping.Reset();
            FineSnapping.Reset();
            FineRotation.Reset();
            ToggleRotation.Reset();
        }

        private void Upgrade()
        {
            if (HasUpgraded)
                return;

            HasUpgraded = true;
            Save();

            if (!Main.PreviousConfigFileExists)
                return;

            FineSnapRounding *= 2;
            Save();
        }
    }
}
