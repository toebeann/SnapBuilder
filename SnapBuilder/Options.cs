using System;
using System.IO;
using System.Reflection;
using System.Text;
using LitJson;
using SMLHelper.V2.Options;
using UnityEngine;

namespace SnapBuilder
{
    internal struct OptionsObject
    {
        public bool EnabledByDefault { get; set; }
        public KeyCode ToggleSnappingKey { get; set; }
        public Toggle.Mode ToggleSnappingMode { get; set; }
        public KeyCode FineSnappingKey { get; set; }
        public Toggle.Mode FineSnappingMode { get; set; }
        public KeyCode FineRotationKey { get; set; }
        public Toggle.Mode FineRotationMode { get; set; }
        public double SnapRounding { get; set; }
        public double FineSnapRounding { get; set; }
        public int RotationRounding { get; set; }
        public int FineRotationRounding { get; set; }
    }

    internal class Options : SMLHelper.V2.Options.ModOptions
    {
        public bool EnabledByDefault = true;
        public KeyCode ToggleSnappingKey = KeyCode.Mouse2;
        public Toggle.Mode ToggleSnappingMode = Toggle.Mode.Press;
        public KeyCode FineSnappingKey = KeyCode.LeftControl;
        public Toggle.Mode FineSnappingMode = Toggle.Mode.Hold;
        public KeyCode FineRotationKey = KeyCode.LeftAlt;
        public Toggle.Mode FineRotationMode = Toggle.Mode.Hold;
        public float SnapRounding = 0.25f;
        public float FineSnapRounding = 0.05f;
        public int RotationRounding = 45;
        public int FineRotationRounding = 5;

        public Toggle.Key Snapping;
        public Toggle.Key FineSnapping;
        public Toggle.Key FineRotation;

        private string ConfigPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");

        public Options() : base("SnapBuilder")
        {
            ToggleChanged += OnToggleChanged;
            KeybindChanged += OnKeybindChanged;
            ChoiceChanged += OnChoiceChanged;
            SliderChanged += OnSliderChanged;

            if (!File.Exists(ConfigPath))
            {
                UpdateJSON();
            }
            else
            {
                ReadOptionsFromJSON();
            }

            Snapping = new Toggle.Key(ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);
            FineSnapping = new Toggle.Key(FineSnappingKey, FineSnappingMode, false);
            FineRotation = new Toggle.Key(FineRotationKey, FineRotationMode, false);
        }

        public override void BuildModOptions()
        {
            AddToggleOption("enabledByDefault", "Snapping enabled by default", EnabledByDefault);
            AddKeybindOption("toggle", "Toggle snapping button", GameInput.GetPrimaryDevice(), ToggleSnappingKey);
            AddChoiceOption<Toggle.Mode>("toggleMode", "Toggle snapping mode", ToggleSnappingMode);
            AddKeybindOption("fineSnap", "Fine snapping button", GameInput.GetPrimaryDevice(), FineSnappingKey);
            AddChoiceOption<Toggle.Mode>("fineSnapMode", "Fine snapping mode", FineSnappingMode);
            AddKeybindOption("fineRotate", "Fine rotation button", GameInput.GetPrimaryDevice(), FineRotationKey);
            AddChoiceOption<Toggle.Mode>("fineRotateMode", "Fine rotation mode", FineRotationMode);
            AddSliderOption("snapRounding", "Snap rounding", 0, 1, SnapRounding);
            AddSliderOption("fineSnapRounding", "Fine snap rounding", 0, 1, FineSnapRounding * 2);
            AddSliderOption("rotationRounding", "Rotation rounding (degrees)", 0, 90, RotationRounding);
            AddSliderOption("fineRotationRounding", "Fine rotation rounding (degrees)", 0, 45, FineRotationRounding);
        }

        public void OnToggleChanged(object sender, ToggleChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "enabledByDefault":
                    EnabledByDefault = eventArgs.Value;
                    Snapping = new Toggle.Key(ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);
                    break;
            }
            UpdateJSON();
        }

        public void OnKeybindChanged(object sender, KeybindChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "toggle":
                    ToggleSnappingKey = eventArgs.Key;
                    Snapping = new Toggle.Key(ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);
                    break;
                case "fineSnap":
                    FineSnappingKey = eventArgs.Key;
                    FineSnapping = new Toggle.Key(FineSnappingKey, FineSnappingMode, false);
                    break;
                case "fineRotate":
                    FineRotationKey = eventArgs.Key;
                    FineRotation = new Toggle.Key(FineRotationKey, FineRotationMode, false);
                    break;
            }
            UpdateJSON();
        }

        public void OnChoiceChanged(object sender, ChoiceChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "toggleMode":
                    ToggleSnappingMode = (Toggle.Mode)eventArgs.Index;
                    Snapping = new Toggle.Key(ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);
                    break;
                case "fineSnapMode":
                    FineSnappingMode = (Toggle.Mode)eventArgs.Index;
                    FineSnapping = new Toggle.Key(FineSnappingKey, FineSnappingMode, false);
                    break;
                case "fineRotateMode":
                    FineRotationMode = (Toggle.Mode)eventArgs.Index;
                    FineRotation = new Toggle.Key(FineRotationKey, FineRotationMode, false);
                    break;
            }
            UpdateJSON();
        }

        public void OnSliderChanged(object sender, SliderChangedEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "snapRounding":
                    SnapRounding = Mathf.Max((float)Math.Round(eventArgs.Value, 2), 0.01f);
                    break;
                case "fineSnapRounding":
                    FineSnapRounding = Mathf.Max((float)Math.Round(eventArgs.Value / 2, 2), 0.01f);
                    break;
                case "rotationRounding":
                    RotationRounding = eventArgs.IntegerValue;
                    break;
                case "fineRotationRounding":
                    FineRotationRounding = eventArgs.IntegerValue;
                    break;
            }
            UpdateJSON();
        }

        private void UpdateJSON()
        {
            OptionsObject options = new OptionsObject
            {
                EnabledByDefault = EnabledByDefault,
                ToggleSnappingKey = ToggleSnappingKey,
                ToggleSnappingMode = ToggleSnappingMode,
                FineSnappingKey = FineSnappingKey,
                FineSnappingMode = FineSnappingMode,
                FineRotationKey = FineRotationKey,
                FineRotationMode = FineRotationMode,
                SnapRounding = Math.Round(SnapRounding, 2),
                FineSnapRounding = Math.Round(FineSnapRounding, 2),
                RotationRounding = RotationRounding,
                FineRotationRounding = FineRotationRounding
            };

            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb)
            {
                PrettyPrint = true
            };
            JsonMapper.ToJson(options, writer);

            string optionsJSON = sb.ToString();
            File.WriteAllText(ConfigPath, optionsJSON);
        }

        private void ReadOptionsFromJSON()
        {
            if (File.Exists(ConfigPath))
            {
                string optionsJSON = File.ReadAllText(ConfigPath);
                OptionsObject options = JsonMapper.ToObject<OptionsObject>(optionsJSON);
                JsonData data = JsonMapper.ToObject(optionsJSON);
                EnabledByDefault = data.ContainsKey("EnabledByDefault") ? options.EnabledByDefault : EnabledByDefault;
                ToggleSnappingKey = data.ContainsKey("ToggleSnappingKey") ? options.ToggleSnappingKey : ToggleSnappingKey;
                ToggleSnappingMode = data.ContainsKey("ToggleSnappingMode") ? options.ToggleSnappingMode : ToggleSnappingMode;
                FineSnappingKey = data.ContainsKey("FineSnappingKey") ? options.FineSnappingKey : FineSnappingKey;
                FineSnappingMode = data.ContainsKey("FineSnappingMode") ? options.FineSnappingMode : FineSnappingMode;
                FineRotationKey = data.ContainsKey("FineRotationKey") ? options.FineRotationKey : FineRotationKey;
                FineRotationMode = data.ContainsKey("FineRotationMode") ? options.FineRotationMode : FineRotationMode;
                SnapRounding = data.ContainsKey("SnapRounding") ? (float)options.SnapRounding : SnapRounding;
                FineSnapRounding = data.ContainsKey("FineSnapRounding") ? (float)options.FineSnapRounding : FineSnapRounding; ;
                RotationRounding = data.ContainsKey("RotationRounding") ? options.RotationRounding : RotationRounding;
                FineRotationRounding = data.ContainsKey("FineRotationRounding") ? options.FineRotationRounding : FineRotationRounding;
                if (!data.ContainsKey("EnabledByDefault") ||
                    !data.ContainsKey("ToggleSnappingKey") || !data.ContainsKey("ToggleSnappingMode") ||
                    !data.ContainsKey("FineSnappingKey") || !data.ContainsKey("FineSnappingMode") ||
                    !data.ContainsKey("FineRotationKey") || !data.ContainsKey("FineRotationMode") ||
                    !data.ContainsKey("SnapRounding") || !data.ContainsKey("FineSnapRounding") ||
                    !data.ContainsKey("RotationRounding") || !data.ContainsKey("FineRotationRounding"))
                {
                    UpdateJSON();
                }
            }
            else
            {
                UpdateJSON();
            }
        }
    }
}
