using System;
using System.IO;
using HarmonyLib;
using ProtoBuf;
using ProtoBuf.Meta;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace ModNetworkProfiler.Patches
{
    [HarmonyPatch(typeof(TypeModel))]
    class SerializeToBinaryPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ProtoBuf.Meta.TypeModel.Serialize), typeof(Stream), typeof(object))]
        public static void SerializePostfix(Stream dest, object value)
        {
            Plugin.Instance.Tracker.LogSerialize(value, dest.Length);
            
            //return true; // Skip the original method
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ProtoBuf.Meta.TypeModel.Deserialize), typeof(Stream), typeof(object), typeof(Type), typeof(SerializationContext))]
        public static void DeserializePostfix(Stream source, object value, Type type, SerializationContext context, ref object __result)
        {
            Plugin.Instance.Tracker.LogDeserialize(__result, source.Length);
            
            //return true; // Skip the original method
        }
    }
}
