using System;
using System.Reflection;
using System.Threading;
using ClientPlugin.Window;
using HarmonyLib;
using Sandbox.ModAPI;
using VRage.Plugins;

namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin, IDisposable
    {
        public const string Name = "ModNetworkProfiler";
        public static Plugin Instance { get; private set; }

        public static ModNetworkProfiler_Window Window;
        public ProfilingTracker Tracker = new ProfilingTracker();
        public Harmony Harmony;


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Instance = this;

            // TODO: Put your one time initialization code here.
            Harmony = new Harmony(Name);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Pass the ProfilingTracker instance (Tracker) to the window
            Window = new ModNetworkProfiler_Window(Tracker);

            new Thread(() =>
            {
                Window.ShowDialog();
            }).Start();
        }

        public void Dispose()
        {
            // TODO: Save state and close resources here, called when the game exits (not guaranteed!)
            // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.

            Window?.Close();
            Window?.Dispose();
            Window = null;
            Instance = null;
        }

        public void Update()
        {
            Tracker.Update();
            Window.UpdateData();
        }
    }
}