using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;
using VRage.FileSystem;

namespace ModNetworkProfiler.Profiling
{
    internal class ProfilingLogger
    {
        private ushort[] _ids = Array.Empty<ushort>();
        private string _path;
        private Dictionary<ushort, int> _dataBuffer = new Dictionary<ushort, int>();
        private StringBuilder _lineBuffer = new StringBuilder();
        private int _ticks = 0;
        private bool _wasClosed = true;

        public ProfilingLogger(string name)
        {
            _path = Path.Combine(MyFileSystem.UserDataPath, $"ModNetworkProfiler_{name}.csv");
            WriteLine("");
        }

        public void AddHandler(ushort id)
        {
            if (_ids.Contains(id))
                return;

            string existingText = File.ReadAllText(_path);
            if (existingText.Contains("\n"))
                existingText = existingText.Insert(existingText.IndexOf('\n') - 1, "," + id);
            else
                existingText = $"Tick,{id}";
            Write(existingText);
            
            _ids = _ids.AddToArray(id);
            _dataBuffer[id] = 0;
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
                WriteLine(_lineBuffer);
            }

            _lineBuffer.Clear();
            _ticks++;
        }

        public void Close()
        {
            _ids = Array.Empty<ushort>();
            _dataBuffer.Clear();
            _wasClosed = true;
        }

        private TextWriter CreateFile(bool overwrite = false)
        {
            string path = Path.Combine(MyFileSystem.UserDataPath, _path);
            Stream stream = MyFileSystem.OpenWrite(path, overwrite ? FileMode.Create : FileMode.OpenOrCreate);
            if (stream != null)
            {
                TextWriter writer = new StreamWriter(stream);
                if (overwrite)
                    writer.WriteLine("Tick");
                return writer;
            }

            throw new FileNotFoundException();
        }

        private void WriteLine(object text)
        {
            using (TextWriter outputFile = CreateFile(_wasClosed))
            {
                _wasClosed = false;
                outputFile.WriteLine(text);
                outputFile.Flush();
            }
        }

        private void Write(object text)
        {
            using (TextWriter outputFile = CreateFile(_wasClosed))
            {
                _wasClosed = false;
                outputFile.Write(text);
                outputFile.Flush();
            }
        }
    }
}
