using HarmonyLib;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using VRage.Game.ModAPI;
using ProtoBuf.Meta;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace ClientPlugin.Patches
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
        [HarmonyPatch(nameof(ProtoBuf.Meta.TypeModel.Deserialize), typeof(Stream), typeof(object), typeof(Type))]
        public static void DeserializePostfix(Stream source, object value, Type type, ref object __result)
        {
            Plugin.Instance.Tracker.LogDeserialize(__result, source.Length);
            
            //return true; // Skip the original method
        }
    }
}
