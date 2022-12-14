﻿using BepInEx;
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

    private bool? hasBuilderTool;
    public bool HasBuilderTool => hasBuilderTool ??= BuilderTool_GetCustomUseText_Patch.TargetMethod() is not null;

    public bool HasLargeRoom { get; } = Enum.TryParse<Base.CellType>("LargeRoom", out _);

    public ConfigEntry<int> RotationFactor => Toggles.FineRotation.IsEnabled switch
    {
        true => Snapping.FineRotationRounding,
        false => Snapping.RotationRounding
    };

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
        Toggles.Bind();
        ApplyHarmonyPatches();
    }

    private void ApplyHarmonyPatches()
    {
        Harmony.PatchAll(typeof(BuilderPatch));
        Harmony.PatchAll(typeof(PlaceToolPatch));

        if (HasBuilderTool)
        {
            Harmony.PatchAll(typeof(BuilderTool_GetCustomUseText_Patch));
        }
    }

    private void OnDisable()
    {
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
            && hitTransform.GetComponent<BaseCell>() is null
            && hitTransform.GetComponent<Base>() is null
                ? Vector3.forward
                : hitTransform.forward;

        // align the empty to face the chosen forward direction
        empty.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        // for components that are not forced upright, align the empty's up direction with the hit.normal
        if (!forceUpright)
        {
            empty.transform.up = hit.normal;
            empty.transform.rotation *= Quaternion.FromToRotation(Vector3.forward, forward);
        }

        child.transform.SetParent(empty.transform, false); // parent the child to the empty

        // Rotate the child transform to look at the player (so that the object will face the player by default, as in the original)
        child.transform.LookAt(Player.main.transform);

        // Round/snap the Y axis of the child transform's local rotation based on the user's rotation factor, after adding the additiveRotation
        child.transform.localEulerAngles
            = new Vector3(0, Math.RoundToNearest(child.transform.localEulerAngles.y + additiveRotation, RotationFactor.Value) % 360, 0);

        Quaternion rotation = child.transform.rotation; // Our final rotation

        // Clean up after ourselves
        DestroyImmediate(child);
        DestroyImmediate(empty);

        return rotation;
    }
}
