using System;
using System.Collections.Generic;
using SMLHelper.V2.Options;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal class Options : ModOptions
    {
        public Options() : base("SnapBuilder")
        {
            InitEvents();
            InitLanguage();
        }

        private void InitEvents()
        {
            ToggleChanged += OnToggleChanged;
            KeybindChanged += OnKeybindChanged;
            ChoiceChanged += OnChoiceChanged;
            SliderChanged += OnSliderChanged;
        }

        private void InitLanguage()
        {
            foreach (var entry in new Dictionary<string, string>()
        {
            { "Options.SnappingEnabledByDefault", "Snapping enabled by default" },
            { "Options.ToggleSnappingKey", "Toggle snapping button" },
            { "Options.ToggleSnappingMode", "Toggle snapping mode" },
            { "Options.FineSnappingKey", "Fine snapping button" },
            { "Options.FineSnappingMode", "Fine snapping mode" },
            { "Options.FineRotationKey", "Fine rotation button" },
            { "Options.FineRotationMode", "Fine rotation mode" },
            { "Options.SnapRounding", "Snap rounding" },
            { "Options.FineSnapRounding", "Fine snap rounding" },
            { "Options.RotationRounding", "Rotation rounding (degrees)" },
            { "Options.FineRotationRounding", "Fine rotation rounding (degrees)" }
        })
            {
                SnapBuilder.SetLanguage(entry.Key, entry.Value);
            }
        }

        public override void BuildModOptions()
        {
            AddToggleOption("enabledByDefault", SnapBuilder.GetLanguage("Options.SnappingEnabledByDefault"),
                SnapBuilder.Config.EnabledByDefault);
            AddKeybindOption("toggle", SnapBuilder.GetLanguage("Options.ToggleSnappingKey"), GameInput.GetPrimaryDevice(),
                SnapBuilder.Config.ToggleSnappingKey);
            AddChoiceOption("toggleMode", SnapBuilder.GetLanguage("Options.ToggleSnappingMode"),
                SnapBuilder.Config.ToggleSnappingMode);
            AddKeybindOption("fineSnap", SnapBuilder.GetLanguage("Options.FineSnappingKey"), GameInput.GetPrimaryDevice(),
                SnapBuilder.Config.FineSnappingKey);
            AddChoiceOption("fineSnapMode", SnapBuilder.GetLanguage("Options.FineSnappingMode"),
                SnapBuilder.Config.FineSnappingMode);
            AddKeybindOption("fineRotate", SnapBuilder.GetLanguage("Options.FineRotationKey"), GameInput.GetPrimaryDevice(),
                SnapBuilder.Config.FineRotationKey);
            AddChoiceOption("fineRotateMode", SnapBuilder.GetLanguage("Options.FineRotationMode"),
                SnapBuilder.Config.FineRotationMode);
            AddSliderOption("snapRounding", SnapBuilder.GetLanguage("Options.SnapRounding"), 0, 1,
                SnapBuilder.Config.SnapRounding);
            AddSliderOption("fineSnapRounding", SnapBuilder.GetLanguage("Options.FineSnapRounding"), 0, 1,
                SnapBuilder.Config.FineSnapRounding * 2);
            AddSliderOption("rotationRounding", SnapBuilder.GetLanguage("Options.RotationRounding"), 0, 90,
                SnapBuilder.Config.RotationRounding);
            AddSliderOption("fineRotationRounding", SnapBuilder.GetLanguage("Options.FineRotationRounding"), 0, 45,
                SnapBuilder.Config.FineRotationRounding);
        }

        public void OnToggleChanged(object sender, ToggleChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "enabledByDefault":
                    SnapBuilder.Config.Snapping.EnabledByDefault = SnapBuilder.Config.EnabledByDefault = eventArgs.Value;
                    break;
            }
            SnapBuilder.Config.Save();
        }

        public void OnKeybindChanged(object sender, KeybindChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "toggle":
                    SnapBuilder.Config.Snapping.KeyCode = SnapBuilder.Config.ToggleSnappingKey = eventArgs.Key;
                    break;
                case "fineSnap":
                    SnapBuilder.Config.FineSnapping.KeyCode = SnapBuilder.Config.FineSnappingKey = eventArgs.Key;
                    break;
                case "fineRotate":
                    SnapBuilder.Config.FineRotation.KeyCode = SnapBuilder.Config.FineRotationKey = eventArgs.Key;
                    break;
            }
            SnapBuilder.Config.Save();
        }

        public void OnChoiceChanged(object sender, ChoiceChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "toggleMode":
                    SnapBuilder.Config.Snapping.KeyMode = SnapBuilder.Config.ToggleSnappingMode
                        = (Toggle.Mode)eventArgs.Index;
                    break;
                case "fineSnapMode":
                    SnapBuilder.Config.FineSnapping.KeyMode = SnapBuilder.Config.FineSnappingMode
                        = (Toggle.Mode)eventArgs.Index;
                    break;
                case "fineRotateMode":
                    SnapBuilder.Config.FineRotation.KeyMode = SnapBuilder.Config.FineRotationMode
                        = (Toggle.Mode)eventArgs.Index;
                    break;
            }
            SnapBuilder.Config.Save();
        }

        public void OnSliderChanged(object sender, SliderChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "snapRounding":
                    SnapBuilder.Config.SnapRounding = Mathf.Max(eventArgs.Value, 0.01f);
                    break;
                case "fineSnapRounding":
                    SnapBuilder.Config.FineSnapRounding = Mathf.Max(eventArgs.Value / 2, 0.01f);
                    break;
                case "rotationRounding":
                    SnapBuilder.Config.RotationRounding = eventArgs.IntegerValue;
                    break;
                case "fineRotationRounding":
                    SnapBuilder.Config.FineRotationRounding = eventArgs.IntegerValue;
                    break;
            }
            SnapBuilder.Config.Save();
        }
    }
}
