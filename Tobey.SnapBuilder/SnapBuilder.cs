using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace Tobey.SnapBuilder;

using ExtensionMethods.UnityEngine;
using Patches;
using static Config;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class SnapBuilder : BaseUnityPlugin
{
    public static SnapBuilder Instance { get; private set; }
    internal static ManualLogSource Log => Instance.Logger;

    public Harmony Harmony { get; } = new Harmony(PluginInfo.PLUGIN_GUID);

    public ConfigEntry<int> RotationFactor => Toggles.FineRotation.IsEnabled switch
    {
        true => Snapping.FineRotationRounding,
        false => Snapping.RotationRounding
    };

    public bool IsSN1 => Paths.ProcessName == "Subnautica";
    public bool HasLargeRoom => Enum.TryParse<TechType>("BaseLargeRoom", out var _);

    private void Awake()
    {
        // enforce singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    private void OnEnable()
    {
        General.Initialise();
        Snapping.Initialise();
        ExtendedBuildRange.Initialise();
        Keybinds.Initialise();
        Toggles.Initialise();
        Localisation.Initialise();

        ExtendedBuildRange.Multiplier.SettingChanged -= Multiplier_SettingChanged;
        ExtendedBuildRange.Multiplier.SettingChanged += Multiplier_SettingChanged;

        Toggles.Bind();
        ApplyHarmonyPatches();
    }

    private void Multiplier_SettingChanged(object sender, EventArgs e)
    {
        foreach (var entry in ConstructablePatch.constructableDistances)
        {
            if (Toggles.ExtendBuildRange.IsEnabled)
            {
                entry.Key.placeDefaultDistance = entry.Value.Item1 * ExtendedBuildRange.Multiplier.Value;
                entry.Key.placeMaxDistance = entry.Value.Item2 * ExtendedBuildRange.Multiplier.Value;
            }
            else
            {
                entry.Key.placeDefaultDistance = entry.Value.Item1;
                entry.Key.placeMaxDistance = entry.Value.Item2;
            }
        }
    }

    private void ApplyHarmonyPatches()
    {
        Harmony.PatchAll(typeof(BuilderPatch));
        if (!IsSN1)
        {
            Harmony.PatchAll(typeof(BuilderTool_GetCustomUseText_Patch));
        }
        Harmony.PatchAll(typeof(ConstructablePatch));
        Harmony.PatchAll(typeof(PlaceToolPatch));
        Harmony.PatchAll(typeof(PhysicsPatch));
    }

    private bool wasReset;
    private void Update()
    {
        if (Builder.isPlacing)
        {
            if (Toggles.ExtendBuildRange.IsEnabled)
            {
                if (wasReset)
                {
                    if (!Input.GetKey(Keybinds.IncreaseExtendedBuildRange.Value) && !Input.GetKey(Keybinds.DecreaseExtendedBuildRange.Value))
                    {
                        wasReset = false;
                    }
                }
                else
                {
                    if (Input.GetKey(Keybinds.IncreaseExtendedBuildRange.Value) && Input.GetKey(Keybinds.DecreaseExtendedBuildRange.Value))
                    {
                        ExtendedBuildRange.Multiplier.Value = (float)ExtendedBuildRange.Multiplier.DefaultValue;
                        wasReset = true;
                    }
                    else if (Input.GetKey(Keybinds.IncreaseExtendedBuildRange.Value))
                    {
                        ExtendedBuildRange.Multiplier.Value += .02f;
                    }
                    else if (Input.GetKey(Keybinds.DecreaseExtendedBuildRange.Value))
                    {
                        ExtendedBuildRange.Multiplier.Value -= .02f;
                    }
                }
            }

            if (Toggles.ExtendBuildRange.IsEnabled)
            {
                foreach (var constructable in ConstructablePatch.constructableDistances)
                {
                    constructable.Key.placeDefaultDistance = constructable.Value.Item1 * ExtendedBuildRange.Multiplier.Value;
                    constructable.Key.placeMaxDistance = constructable.Value.Item2 * ExtendedBuildRange.Multiplier.Value;
                }
            }
            else
            {
                foreach (var constructable in ConstructablePatch.constructableDistances)
                {
                    constructable.Key.placeDefaultDistance = constructable.Value.Item1;
                    constructable.Key.placeMaxDistance = constructable.Value.Item2;
                }
            }
        }
    }

    private void OnDisable()
    {
        ExtendedBuildRange.Multiplier.SettingChanged -= Multiplier_SettingChanged;
        Toggles.Unbind();
        Harmony.UnpatchSelf();
        Destroy(AimTransform.Instance);
    }

    private void OnDestroy()
    {
        Toggles.Dispose();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public float LastButtonHeldTime = -1f;
    public GameInput.Button LastButton;
    private void ApplyAdditiveRotation(ref float additiveRotation)
    {
        // If the user is rotating, apply the additive rotation
        if (GameInput.GetButtonHeld(Builder.buttonRotateCW)) // Clockwise
        {
            if (LastButton != Builder.buttonRotateCW)
            {   // Clear previous rotation held time
                LastButton = Builder.buttonRotateCW;
                LastButtonHeldTime = -1f;
            }

            float buttonHeldTime = Math.FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCW), 0.15f);
            if (buttonHeldTime > LastButtonHeldTime)
            {   // Store rotation held time
                LastButtonHeldTime = buttonHeldTime;
                additiveRotation -= RotationFactor.Value; // Decrement rotation
            }
        }
        else if (GameInput.GetButtonHeld(Builder.buttonRotateCCW)) // Counter-clockwise
        {
            if (LastButton != Builder.buttonRotateCCW)
            {   // Clear previous rotation held time
                LastButton = Builder.buttonRotateCCW;
                LastButtonHeldTime = -1f;
            }

            float buttonHeldTime = Math.FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCCW), 0.15f);
            if (buttonHeldTime > LastButtonHeldTime)
            {   // Store rotation held time
                LastButtonHeldTime = buttonHeldTime;
                additiveRotation += RotationFactor.Value; // Increment rotation
            }
        }
        else if (GameInput.GetButtonUp(Builder.buttonRotateCW) || GameInput.GetButtonUp(Builder.buttonRotateCCW))
        {   // User is not rotating, clear rotation held time
            LastButtonHeldTime = -1f;
        }

        // Round to the nearest rotation factor for rotation snapping
        additiveRotation %= 360;
    }

    public Quaternion CalculateRotation(ref float additiveRotation, RaycastHit hit, bool forceUpright)
    {
        ApplyAdditiveRotation(ref additiveRotation);
        hit.normal = hit.AverageNormal(.2f);

        Transform hitTransform = hit.GetOptimalTransform();

        // Instantiate empty game objects for applying rotations
        GameObject empty = new GameObject();
        GameObject child = new GameObject();
        empty.transform.position = hit.point; // Set the parent transform's position to our chosen position

        // choose whether we should use the global forward, or the forward of the hitTransform
        Vector3 forward = !Mathf.Approximately(Mathf.Abs(Vector3.Dot(Vector3.up, hitTransform.up)), 1)
            && !Player.main.IsInsideWalkable()
            && hitTransform.GetComponent<BaseCell>() == null
            && hitTransform.GetComponent<Base>() == null
                ? Vector3.forward
                : hitTransform.forward;

        // align the empty to face the chosen forward direction
        empty.transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);

        // for components that are not forced upright, align the empty's up direction with the hit.normal
        if (!forceUpright)
        {
            empty.transform.up = hit.normal;
            empty.transform.localRotation *= Quaternion.FromToRotation(Vector3.forward, forward);
        }

        child.transform.SetParent(empty.transform, false); // parent the child to the empty

        // Rotate the child transform to look at the player (so that the object will face the player by default, as in the original)
        if (Builder.GetSurfaceType(hit.normal) != SurfaceType.Wall)
        {
            child.transform.LookAt(Player.main.transform);
        }
        var final = new GameObject();
        final.transform.SetParent(child.transform, true);
        final.transform.localRotation = GetDefaultRotation();

        // Round/snap the Y axis of the child transform's local rotation based on the user's rotation factor, after adding the additiveRotation
        child.transform.localEulerAngles = Builder.GetSurfaceType(hit.normal) == SurfaceType.Wall
            ? new Vector3(Math.RoundToNearest(child.transform.localEulerAngles.y + additiveRotation, RotationFactor.Value) % 360, 0, 0)
            : new Vector3(0, Math.RoundToNearest(child.transform.localEulerAngles.y + additiveRotation, RotationFactor.Value) % 360, 0);

        Quaternion rotation = final.transform.rotation; // Our final rotation

        // Clean up after ourselves
        DestroyImmediate(final);
        DestroyImmediate(child);
        DestroyImmediate(empty);

        return rotation;
    }

    public Transform GetMetadata() => Builder.ghostModel?.transform.Find("SnapBuilder");

    public Quaternion GetDefaultRotation() => GetMetadata()?.localRotation ?? Quaternion.identity;
}
