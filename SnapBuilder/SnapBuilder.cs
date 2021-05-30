using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    using BepInEx.Subnautica;
    using ExtensionMethods;
    using Patches;

    internal class SnapBuilder
    {
        public static float LastButtonHeldTime = -1f;
        public static GameInput.Button LastButton;
        public static Config Config = OptionsPanelHandler.RegisterModOptions<Config>();

        private enum SupportedGame
        {
            Subnautica,
            BelowZero
        }

#if SUBNAUTICA
        private const SupportedGame TargetGame = SupportedGame.Subnautica;
#elif BELOWZERO
        private const SupportedGame TargetGame = SupportedGame.BelowZero;
#endif

        public static void Initialise()
        {
            Logger.LogInfo($"Initialising SnapBuilder {Assembly.GetExecutingAssembly().GetName().Version} for {TargetGame}...");
            var stopwatch = Stopwatch.StartNew();

            ApplyHarmonyPatches();
            Config.Initialise();
            Lang.Initialise();

            stopwatch.Stop();
            Logger.LogInfo($"Initialised in {stopwatch.ElapsedMilliseconds}ms.");
        }

        private static void ApplyHarmonyPatches()
        {
            var stopwatch = Stopwatch.StartNew();

            var harmony = new Harmony("SnapBuilder");
            harmony.PatchAll(typeof(BuilderPatch));
            harmony.PatchAll(typeof(PlaceToolPatch));
#if BELOWZERO
            harmony.PatchAll(typeof(BuilderToolPatch));
#endif

            stopwatch.Stop();
            Logger.LogInfo($"Harmony patches applied in {stopwatch.ElapsedMilliseconds}ms.");
        }

        private static void ApplyAdditiveRotation(ref float additiveRotation)
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
                    additiveRotation -= Config.RotationFactor; // Decrement rotation
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
                    additiveRotation += Config.RotationFactor; // Increment rotation
                }
            }
            else if (GameInput.GetButtonUp(Builder.buttonRotateCW) || GameInput.GetButtonUp(Builder.buttonRotateCCW))
            {   // User is not rotating, clear rotation held time
                LastButtonHeldTime = -1f;
            }

            // Round to the nearest rotation factor for rotation snapping
            additiveRotation %= 360;
        }

        public static Quaternion CalculateRotation(ref float additiveRotation, RaycastHit hit, bool forceUpright)
        {
            ApplyAdditiveRotation(ref additiveRotation);
            hit.normal = hit.AverageNormal(.2f);

            Transform hitTransform = hit.GetOptimalTransform();

            // Instantiate empty game objects for applying rotations
            GameObject empty = new GameObject();
            GameObject child = new GameObject();
            empty.transform.position = hit.point; // Set the parent transform's position to our chosen position

            // choose whether we should use the global forward, or the forward of the hitTransform
            Vector3 forward = hitTransform.forward.y != 0
                              && !Player.main.IsInsideWalkable()
                              && !Player.main.IsInSub()
                              && hitTransform.GetComponent<BaseCell>() is null
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
                = new Vector3(0, Math.RoundToNearest(child.transform.localEulerAngles.y + additiveRotation, Config.RotationFactor) % 360, 0);

            Quaternion rotation = child.transform.rotation; // Our final rotation

            // Clean up after ourselves
            Object.DestroyImmediate(child);
            Object.DestroyImmediate(empty);

            return rotation;
        }
    }
}
