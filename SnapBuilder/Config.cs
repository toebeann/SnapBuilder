using SMLHelper.V2.Json;
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
        public float SnapRounding { get; set; } = 0.25f;
        public float FineSnapRounding { get; set; } = 0.05f;
        public int RotationRounding { get; set; } = 45;
        public int FineRotationRounding { get; set; } = 5;

        [JsonIgnore]
        public Toggle Snapping;
        [JsonIgnore]
        public Toggle FineSnapping;
        [JsonIgnore]
        public Toggle FineRotation;

        internal void ResetToggle(ref Toggle toggle, KeyCode keyCode, Toggle.Mode mode, bool enabled)
        {
            toggle = new Toggle(keyCode, mode, enabled);
        }
        
        internal void SetupToggles()
        {
            ResetToggle(ref Snapping, ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);
            ResetToggle(ref FineSnapping, FineSnappingKey, FineSnappingMode, false);
            ResetToggle(ref FineRotation, FineRotationKey, FineRotationMode, false);
        }
    }
}
