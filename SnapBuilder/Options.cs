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
        public KeyCode FineSnappingKey { get; set; }
        public KeyCode FineRotationKey { get; set; }
        public double SnapRounding { get; set; }
        public double FineSnapRounding { get; set; }
        public int RotationRounding { get; set; }
        public int FineRotationRounding { get; set; }
    }

    internal class Options : SMLHelper.V2.Options.ModOptions
    {
        public bool EnabledByDefault = true;
        public KeyCode ToggleSnappingKey = KeyCode.Mouse2;
        public KeyCode FineSnappingKey = KeyCode.LeftControl;
        public KeyCode FineRotationKey = KeyCode.LeftAlt;
        public float SnapRounding = 0.25f;
        public float FineSnapRounding = 0.05f;
        public int RotationRounding = 45;
        public int FineRotationRounding = 10;

        private string ConfigPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");

        public Options() : base("SnapBuilder")
        {
            ToggleChanged += OnToggleChanged;
            KeybindChanged += OnKeybindChanged;
            SliderChanged += OnSliderChanged;

            if (!File.Exists(ConfigPath))
            {
                UpdateJSON();
            }
            else
            {
                ReadOptionsFromJSON();
            }
        }

        public override void BuildModOptions()
        {
            AddKeybindOption("toggle", "Toggle snapping", GameInput.GetPrimaryDevice(), ToggleSnappingKey);
            AddKeybindOption("fineSnap", "Fine snapping (hold)", GameInput.GetPrimaryDevice(), FineSnappingKey);
            AddKeybindOption("fineRotate", "Fine rotation (hold)", GameInput.GetPrimaryDevice(), FineRotationKey);
            AddToggleOption("enabledByDefault", "Snapping enabled by default", EnabledByDefault);
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
                    break;
                case "fineSnap":
                    FineSnappingKey = eventArgs.Key;
                    break;
                case "fineRotate":
                    FineRotationKey = eventArgs.Key;
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
                FineSnappingKey = FineSnappingKey,
                FineRotationKey = FineRotationKey,
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
                EnabledByDefault = options.EnabledByDefault;
                ToggleSnappingKey = options.ToggleSnappingKey;
                FineSnappingKey = options.FineSnappingKey;
                FineRotationKey = options.FineRotationKey;
                SnapRounding = (float)options.SnapRounding;
                FineSnapRounding = (float)options.FineSnapRounding;
                RotationRounding = options.RotationRounding;
                FineRotationRounding = options.FineRotationRounding;
                JsonData data = JsonMapper.ToObject(optionsJSON);
                if (!data.ContainsKey("EnabledByDefault") || !data.ContainsKey("InvertSnappingKey") ||
                    !data.ContainsKey("FineSnappingKey") || !data.ContainsKey("FineRotationKey") ||
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
