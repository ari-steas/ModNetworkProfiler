using HarmonyLib;
using Sandbox.Game.World;

namespace ClientPlugin.Patches
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
