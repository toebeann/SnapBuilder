using SMLHelper.V2.Json;
using SMLHelper.V2.Json.Converters;
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
    internal class Config : ConfigFile
    {
        public bool EnabledByDefault { get; set; } = true;
        public KeyCode ToggleSnappingKey { get; set; } = KeyCode.Mouse2;
        [JsonConverter(typeof(StringEnumConverter))]
        public Toggle.Mode ToggleSnappingMode { get; set; } = Toggle.Mode.Press;
        public KeyCode FineSnappingKey { get; set; } = KeyCode.LeftControl;
        [JsonConverter(typeof(StringEnumConverter))]
        public Toggle.Mode FineSnappingMode { get; set; } = Toggle.Mode.Hold;
        public KeyCode FineRotationKey { get; set; } = KeyCode.LeftAlt;
        [JsonConverter(typeof(StringEnumConverter))]
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
