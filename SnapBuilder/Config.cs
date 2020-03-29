using SMLHelper.V2.Json;
using SMLHelper.V2.Json.Converters;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal class Config : ConfigFile
    {
        public bool EnabledByDefault { get; set; } = true;
        public KeyCode ToggleSnappingKey { get; set; } = KeyCode.Mouse2;
        public Toggle.Mode ToggleSnappingMode { get; set; } = Toggle.Mode.Press;
        public KeyCode FineSnappingKey { get; set; } = KeyCode.LeftControl;
        public Toggle.Mode FineSnappingMode { get; set; } = Toggle.Mode.Hold;
        public KeyCode FineRotationKey { get; set; } = KeyCode.LeftAlt;
        public Toggle.Mode FineRotationMode { get; set; } = Toggle.Mode.Hold;
        [JsonConverter(typeof(FloatConverter), 2)]
        public float SnapRounding { get; set; } = 0.25f;
        [JsonConverter(typeof(FloatConverter), 2)]
        public float FineSnapRounding { get; set; } = 0.05f;
        public int RotationRounding { get; set; } = 45;
        public int FineRotationRounding { get; set; } = 5;

        [JsonIgnore]
        public Toggle Snapping;
        [JsonIgnore]
        public Toggle FineSnapping;
        [JsonIgnore]
        public Toggle FineRotation;

        internal void SetupToggles()
        {
            Snapping = new Toggle(ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);
            FineSnapping = new Toggle(FineSnappingKey, FineSnappingMode, false);
            FineRotation = new Toggle(FineRotationKey, FineRotationMode, false);
        }
    }
}
