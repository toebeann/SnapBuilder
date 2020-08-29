using System;
using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal static class SnapBuilder
    {
        public static Config Config = new Config();
        public static float LastButtonHeldTime = -1f;
        public static GameInput.Button LastButton;

        public static void Initialise()
        {
            Config.Load();
            Config.Initialise();
            OptionsPanelHandler.RegisterModOptions(new Options());
            InitLanguage();
        }

        public static void InitLanguage()
        {
            SetLanguage("GhostToggleSnappingHint", "Toggle snapping");
            SetLanguage("GhostToggleFineSnappingHint", "Toggle fine snapping");
            SetLanguage("GhostToggleFineRotationHint", "Toggle fine rotation");
        }

        public static string FormatButton(Toggle toggle)
        {
            string displayText = null;
            if (toggle.KeyCode == KeyCode.None)
            {
                displayText = GetLanguage("NoInputAssigned");
            }
            else
            {
                string bindingName = KeyCodeUtils.KeyCodeToString(toggle.KeyCode);
                if (!string.IsNullOrEmpty(bindingName))
                {
                    displayText = uGUI.GetDisplayTextForBinding(bindingName);
                }
                if (string.IsNullOrEmpty(displayText))
                {
                    displayText = GetLanguage("NoInputAssigned");
                }
            }
            return $"<color=#ADF8FFFF>{displayText}</color>{(toggle.KeyMode == Toggle.Mode.Hold ? " (Hold)" : string.Empty)}";
        }

        public static float RoundToNearest(float x, float y) => y * Mathf.Round(x / y);
        public static double RoundToNearest(double x, double y) => y * Math.Round(x / y);
        public static float FloorToNearest(float x, float y) => y * Mathf.Floor(x / y);
        public static double FloorToNearest(double x, double y) => y * Math.Floor(x / y);

        public static string GetLanguage(string id) => Language.main.Get(id);
        public static void SetLanguage(string id, string value) => SMLHelper.V2.Handlers.LanguageHandler.SetLanguageLine(id, value);

        public static void ShowSnappingHint(bool shouldShow = true)
        {
            if (shouldShow)
            {
                ErrorMessage.AddError($"{GetLanguage("GhostToggleSnappingHint")}" +
                        $" ({FormatButton(Config.Snapping)})");
                ErrorMessage.AddError($"{GetLanguage("GhostToggleFineSnappingHint")}" +
                    $" ({FormatButton(Config.FineSnapping)})");
            }
        }

        public static void ShowRotationHint(bool shouldShow = true)
        {
            if (shouldShow)
            {
                ErrorMessage.AddError($"{GetLanguage("GhostToggleFineRotationHint")}" +
                    $" ({FormatButton(Config.FineRotation)})");
            }
        }

        public static bool TryGetSnappedHitPoint(LayerMask layerMask, ref RaycastHit hit,
            out Vector3 snappedHitPoint, out Vector3 snappedHitNormal, float maxDistance = 5f)
        {
            Transform aimTransform = Builder.GetAimTransform();

            Vector3 localPoint = hit.transform.InverseTransformPoint(hit.point); // Get the hit point localised relative to the hit transform
            Vector3 localNormal = hit.transform.InverseTransformDirection(hit.normal).normalized; // Get the hit normal localised to the hit transform

            // Set the localised normal to absolute values for comparison
            localNormal.x = Mathf.Abs(localNormal.x);
            localNormal.y = Mathf.Abs(localNormal.y);
            localNormal.z = Mathf.Abs(localNormal.z);
            localNormal = localNormal.normalized; // For sanity's sake, make sure the normal is normalised

            // Get the rounding factor from user options based on whether the fine snapping key is held or not
            float roundFactor = Config.FineSnapping.Enabled ? Config.FineSnapRounding : Config.SnapRounding;

            // Round (snap) the localised hit point coords only on axes where the corresponding normal axis is less than 1
            if (localNormal.x < 1)
            {
                localPoint.x = RoundToNearest(localPoint.x, roundFactor);
            }
            if (localNormal.y < 1)
            {
                localPoint.y = RoundToNearest(localPoint.y, roundFactor);
            }
            if (localNormal.z < 1)
            {
                localPoint.z = RoundToNearest(localPoint.z, roundFactor);
            }

            // Now, perform a new raycast so that we can get the normal of the new position
            if (!Physics.Raycast(aimTransform.position,
                hit.transform.TransformPoint(localPoint) - aimTransform.position, // direction from the aim transform to the new world space position of the rounded/snapped position
                out hit, // overwrite hit
                maxDistance,
                layerMask,
                QueryTriggerInteraction.Ignore))
            {
                snappedHitPoint = Vector3.zero;
                snappedHitNormal = Vector3.zero;
                return false;
            }

            snappedHitPoint = hit.point;
            snappedHitNormal = hit.normal; // Store the hit.normal as we may need to change this in certain circumstances

            // If the hit.collider is a MeshCollider and has a sharedMesh, it is a surface like the ground or the roof of a multipurpose room,
            // in which case we want a more accurate normal where possible
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider?.sharedMesh != null)
            {
                // Set up the offsets for raycasts around the point
                Vector3[] offsets = new Vector3[]
                {
                    Vector3.forward,
                    Vector3.back,
                    Vector3.left,
                    Vector3.right,
                    (Vector3.forward + Vector3.right) * .707f,
                    (Vector3.forward + Vector3.left) * .707f,
                    (Vector3.back + Vector3.right) * .707f,
                    (Vector3.back + Vector3.left) * .707f
                };

                List<Vector3> normals = new List<Vector3>();
                // Perform a raycast from each offset, pointing down toward the surface
                foreach (Vector3 offset in offsets)
                {
                    Physics.Raycast(snappedHitPoint + Vector3.up * .1f + offset * .2f,
                        Vector3.down,
                        out RaycastHit offsetHit,
                        Builder.placeMaxDistance,
                        Builder.placeLayerMask.value,
                        QueryTriggerInteraction.Ignore);
                    if (offsetHit.transform == hit.transform) // If we hit the same object, add the hit normal
                    {
                        normals.Add(offsetHit.normal);
                    }
                }

                foreach (Vector3 normal in normals)
                {
                    if (normal != hit.normal)
                    {   // If the normal is not the same as the original, add it to the normal and average
                        snappedHitNormal += normal / 2;
                    }
                }

                // For sanity's sake, make sure the normal is normalised after summing & averaging
                snappedHitNormal = snappedHitNormal.normalized;
            }

            return true;
        }

        public static void ApplyAdditiveRotation(ref float additiveRotation, out float rotationFactor)
        {
            // Get the rotation factor from user options based on whether the fine snapping key is held or not
            rotationFactor = Config.FineRotation.Enabled ? Config.FineRotationRounding : Config.RotationRounding;

            // If the user is rotating, apply the additive rotation
            if (GameInput.GetButtonHeld(Builder.buttonRotateCW)) // Clockwise
            {
                if (LastButton != Builder.buttonRotateCW)
                {   // Clear previous rotation held time
                    LastButton = Builder.buttonRotateCW;
                    LastButtonHeldTime = -1f;
                }

                float buttonHeldTime = FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCW), 0.15f);
                if (buttonHeldTime > LastButtonHeldTime)
                {   // Store rotation held time
                    LastButtonHeldTime = buttonHeldTime;
                    additiveRotation -= rotationFactor; // Decrement rotation
                }
            }
            else if (GameInput.GetButtonHeld(Builder.buttonRotateCCW)) // Counter-clockwise
            {
                if (LastButton != Builder.buttonRotateCCW)
                {   // Clear previous rotation held time
                    LastButton = Builder.buttonRotateCCW;
                    LastButtonHeldTime = -1f;
                }

                float buttonHeldTime = FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCCW), 0.15f);
                if (buttonHeldTime > LastButtonHeldTime)
                {   // Store rotation held time
                    LastButtonHeldTime = buttonHeldTime;
                    additiveRotation += rotationFactor; // Increment rotation
                }
            }
            else if (GameInput.GetButtonUp(Builder.buttonRotateCW) || GameInput.GetButtonUp(Builder.buttonRotateCCW))
            {   // User is not rotating, clear rotation held time
                LastButtonHeldTime = -1f;
            }

            // Round to the nearest rotation factor for rotation snapping
            additiveRotation = RoundToNearest(additiveRotation, rotationFactor) % 360;
        }

        public static Quaternion CalculateRotation(ref float additiveRotation, RaycastHit hit,
            Vector3 snappedHitPoint, Vector3 snappedHitNormal, bool forceUpright)
        {
            ApplyAdditiveRotation(ref additiveRotation, out float rotationFactor);

            Transform hitTransform = hit.transform;
            if (!Player.main.IsInside())
            {   // If the player is outside, get the root transform if there is one, otherwise default to the original
                hitTransform = UWE.Utils.GetEntityRoot(hit.transform.gameObject)?.transform ?? hit.transform;
            }

            // Instantiate empty game objects for applying rotations
            GameObject empty = new GameObject();
            GameObject child = new GameObject();
            child.transform.parent = empty.transform; // parent the child to the empty
            child.transform.localPosition = Vector3.zero; // Make sure the child's local position is Vector3.zero
            empty.transform.position = snappedHitPoint; // Set the parent transform's position to our chosen position

#if BELOWZERO
                if (Builder.constructableTechType != TechType.Hoverpad)
#endif
            {   // Stupid hoverpad, working differently to everything else in the game...
                empty.transform.forward = hitTransform.forward; // Set the parent transform's forward to match the forward of the hit.transform
            }

            if (!forceUpright)
            {   // Rotate the parent transform so that it's Y axis is aligned with the hit.normal, but only when it isn't forced upright
                empty.transform.rotation = Quaternion.FromToRotation(Vector3.up, snappedHitNormal) * empty.transform.rotation;
            }

            child.transform.LookAt(Player.main.transform); // Rotate the child transform to look at the player (so that the object will face the player by default, as in the original)
            child.transform.localEulerAngles
                = new Vector3(0,
                SnapBuilder.RoundToNearest(child.transform.localEulerAngles.y + additiveRotation, rotationFactor) % 360,
                0); // Round/snap the Y axis of the child transform's local rotation based on the user's rotation factor, after adding the additiveRotation

            Quaternion rotation = child.transform.rotation; // Our final rotation

            // Clean up after ourselves
            GameObject.DestroyImmediate(child);
            GameObject.DestroyImmediate(empty);

            return rotation;
        }
    }
}
