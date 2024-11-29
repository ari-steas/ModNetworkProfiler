using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using VRage.FileSystem;
using VRage.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace ModNetworkProfiler.Profiling
{
    /// <summary>
    /// Stores logging data to a single file, located in %AppData%\Roaming\Space Engineers\ModNetworkProfiler_[name].csv
    /// </summary>
    internal class ProfilingLogger
    {
        private ushort[] _ids = Array.Empty<ushort>();
        private string _path;
        private Dictionary<ushort, int> _dataBuffer = new Dictionary<ushort, int>();
        private StringBuilder _lineBuffer = new StringBuilder();
        private int _ticks = 0;

        public ProfilingLogger(string name)
        {
            _path = Path.Combine(MyFileSystem.UserDataPath, $"ModNetworkProfiler_{name}.csv");
        }

        public void AddHandler(ushort id)
        {
            if (_ids.Contains(id))
                return;

            _ids = _ids.AddToArray(id);
            _dataBuffer[id] = 0;

            try
            {
                // stupid bandaid logic
                if (_ids.Length == 1)
                    File.WriteAllText(_path, "");

                string existingText = File.ReadAllText(_path);
                if (existingText.Contains("\n"))
                    existingText = existingText.Remove(0, existingText.IndexOf('\n')+1);

                File.WriteAllText(_path,
                    existingText.Insert(0,
                        $"Tick,{string.Join(",", _ids.Select(theId => Plugin.Instance.Tracker.GetNetworkIdName(theId)))}\n"));
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole("[ModNetworkProfiler] I/O exception occured in ProfilingLogger.AddHandler!\n" + ex);
            }
        }

        public void QueueData(ushort id, int size)
        {
            if (!_dataBuffer.ContainsKey(id))
                AddHandler(id);

            _dataBuffer[id] += size;
        }

        public void OnTick()
        {
            bool hadNonZeroValue = false;
            _lineBuffer.Append(_ticks + ",");

            foreach (var id in _ids)
            {
                _lineBuffer.Append(_dataBuffer[id] + ",");
                if (_dataBuffer[id] != 0)
                    hadNonZeroValue = true;
                _dataBuffer[id] = 0;
            }

            if (_dataBuffer.Count > 0)
                _lineBuffer.TrimEnd(1);

            if (hadNonZeroValue)
            {
                try
                {
                    File.AppendAllText(_path, _lineBuffer.Append('\n').ToString());
                }
                catch (Exception ex)
                {
                    MyLog.Default.WriteLineAndConsole("[ModNetworkProfiler] Write exception occured in ProfilingLogger.OnTick!\n" + ex);
                }
            }

            _lineBuffer.Clear();
            _ticks++;
        }

        public void Close()
        {
            _ids = Array.Empty<ushort>();
            _dataBuffer.Clear();
            _ticks = 0;
            _lineBuffer.Clear();
            MyLog.Default.WriteLineAndConsole($"[ModNetworkProfiler] Closing logging file {_path}.");
        }
    }
}
