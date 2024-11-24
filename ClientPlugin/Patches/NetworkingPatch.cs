using System;
using System.Collections.Generic;
using HarmonyLib;
using Sandbox.Game.World;
using Sandbox.ModAPI;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyModAPIHelper.MyMultiplayer))]
    public static class NetworkingPatch
    {
        private static ProfilingTracker Tracker => Plugin.Instance?.Tracker;
        private static Dictionary<ushort, Action<ushort, byte[], ulong, bool>> SecureMessageActions = new Dictionary<ushort, Action<ushort, byte[], ulong, bool>>();
        private static Dictionary<ushort, Action<byte[]>> MessageActions = new Dictionary<ushort, Action<byte[]>>();

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MyModAPIHelper.MyMultiplayer.RegisterSecureMessageHandler))]
        public static void RegisterSecureMessageHandlerPrefix(ushort id,
            ref Action<ushort, byte[], ulong, bool> messageHandler)
        {
            Tracker?.RegisterNetworkHandler(id, messageHandler.Method.DeclaringType);
            SecureMessageActions[id] = (ushort msgid, byte[] msgdata, ulong msgsender, bool d) =>
                Tracker.LogReceiveMessage(id, msgdata.Length);
            messageHandler += SecureMessageActions[id];
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MyModAPIHelper.MyMultiplayer.RegisterMessageHandler))]
        public static void RegisterMessageHandlerPrefix(ushort id, ref Action<byte[]> messageHandler)
        {
            Tracker?.RegisterNetworkHandler(id, messageHandler.Method.DeclaringType);
            MessageActions[id] = (byte[] msgdata) => Tracker.LogReceiveMessage(id, msgdata.Length);
            messageHandler += MessageActions[id];
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MyModAPIHelper.MyMultiplayer.UnregisterSecureMessageHandler))]
        public static void UnregisterSecureMessageHandlerPrefix(ushort id,
            ref Action<ushort, byte[], ulong, bool> messageHandler)
        {
            if (!Tracker?.UnregisterNetworkHandler(id) ?? true)
                return;
            //messageHandler -= SecureMessageActions[id];
            SecureMessageActions.Remove(id);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MyModAPIHelper.MyMultiplayer.UnregisterMessageHandler))]
        public static void UnregisterMessageHandlerPrefix(ushort id, ref Action<byte[]> messageHandler)
        {
            if (!Tracker?.UnregisterNetworkHandler(id) ?? true)
                return;
            //messageHandler -= MessageActions[id];
            MessageActions.Remove(id);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MyModAPIHelper.MyMultiplayer.SendMessageToServer))]
        public static void SendMessageToServerPrefix(ushort id, byte[] message, bool reliable)
        {
            Tracker?.LogSendMessage(id, message.Length);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MyModAPIHelper.MyMultiplayer.SendMessageToOthers))]
        public static void SendMessageToOthersPrefix(ushort id, byte[] message, bool reliable)
        {
            Tracker?.LogSendMessage(id, message.Length);
            //MyAPIGateway.Utilities.ShowMessage("ModNetworkProfile.SMO", $"{id}: Message sent! Size: {message.Length} bytes");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MyModAPIHelper.MyMultiplayer.SendMessageTo))]
        public static void SendMessageToPrefix(ushort id, byte[] message, ulong recipient, bool reliable)
        {
            Tracker?.LogSendMessage(id, message.Length);
            //MyAPIGateway.Utilities.ShowMessage("ModNetworkProfile.SMT", $"{id}: Message sent! Size: {message.Length} bytes");
        }
    }
}
