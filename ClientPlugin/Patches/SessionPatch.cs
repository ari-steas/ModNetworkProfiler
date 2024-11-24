using HarmonyLib;
using ModNetworkProfiler.Profiling;
using Sandbox.Game.World;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace ModNetworkProfiler.Patches
{
    [HarmonyPatch(typeof(MySession))]
    internal class SessionPatch
    {
        private static ProfilingTracker Tracker => Plugin.Instance?.Tracker;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MySession.Unload))]
        public static void UnloadPrefix()
        {
            Tracker?.UnregisterAll();
        }
    }
}
