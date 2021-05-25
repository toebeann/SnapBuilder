using System;
using System.IO;
using QModManager.API;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    [QModCore]
    public static class Main
    {
        [QModPatch]
        [Obsolete("Should not be used!", true)]
        public static void Patch() => SnapBuilder.Initialise();

        internal static readonly bool PreviousConfigFileExists = File.Exists(
            Path.Combine(Path.GetDirectoryName(QModServices.Main.GetMyMod().LoadedAssembly.Location), "config.json"));
    }
}
