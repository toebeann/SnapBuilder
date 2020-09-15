using System;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    [QModCore]
    [Obsolete("Should not be used!", true)]
    public static class Main
    {
        [QModPatch]
        [Obsolete("Should not be used!", true)]
        public static void Patch() => SnapBuilder.Initialise();
    }
}
