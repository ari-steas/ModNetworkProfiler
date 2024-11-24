﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRage.FileSystem;
using VRage.Private;

namespace ClientPlugin.Profiling
{
    internal class ProfilingLogger
    {
        private TextWriter _outputFile;
        private ushort[] _ids = Array.Empty<ushort>();
        private string _path;
        private Dictionary<ushort, int> _dataBuffer = new Dictionary<ushort, int>();
        private StringBuilder _lineBuffer = new StringBuilder();
        private int _ticks = 0;
        private bool _wasClosed = true;

        public ProfilingLogger(string name)
        {
            _path = Path.Combine(MyFileSystem.UserDataPath, $"ModNetworkProfiler_{name}.csv");
        }

        public void AddHandler(ushort id)
        {
            if (_ids.Contains(id))
                return;

            string existingText = File.ReadAllText(_path);
            existingText = existingText.Insert(existingText.IndexOf('\n'), "," + id);
            WriteLine(existingText);
            _dataBuffer[id] = 0;
        }

        public void QueueData(ushort id, int size)
        {
            _dataBuffer[id] += size;
        }

        public void OnTick()
        {
            bool hadNonZeroValue = false;
            _lineBuffer.Append(_ticks + ",");

            foreach (var id in _ids)
            {
                _lineBuffer.Append(_dataBuffer[id] + ",");
                if (_dataBuffer[id] == 0)
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
            _outputFile.Close();
            _ids = Array.Empty<ushort>();
            _dataBuffer.Clear();
        }

        private TextWriter CreateFile()
        {
            string path = Path.Combine(MyFileSystem.UserDataPath, _path);
            Stream stream = MyFileSystem.OpenWrite(path, FileMode.Create);
            if (stream != null)
            {
                return new StreamWriter(stream);
            }

            throw new FileNotFoundException();
        }

        private void WriteLine(object text)
        {
            if (_wasClosed)
            {
                _wasClosed = false;
                _outputFile = CreateFile();
                _outputFile.WriteLine("Tick");
            }

            _outputFile.Write(text);
            _outputFile.Flush();
        }
    }
}
