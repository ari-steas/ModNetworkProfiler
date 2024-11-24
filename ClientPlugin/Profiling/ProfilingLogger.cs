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
        private TextWriter _outputFile;
        private ushort[] _ids = Array.Empty<ushort>();
        private string _path;
        private Dictionary<ushort, int> _dataBuffer = new Dictionary<ushort, int>();
        private StringBuilder _lineBuffer = new StringBuilder();
        private int _ticks = 0;
        private bool _wasClosed = false;

        public ProfilingLogger(string name)
        {
            _path = Path.Combine(MyFileSystem.UserDataPath, $"ModNetworkProfiler_{name}.csv");
            _outputFile = CreateFile();
            _outputFile.WriteLine("Tick");
        }

        public void AddHandler(ushort id)
        {
            if (_ids.Contains(id))
                return;

            _outputFile.Close();
            string existingText = File.ReadAllText(_path);
            existingText = existingText.Insert(existingText.IndexOf('\n')-1, "," + id);
            _outputFile = CreateFile();
            Write(existingText);
            
            _ids = _ids.AddToArray(id);
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
            _outputFile.Close();
            _ids = Array.Empty<ushort>();
            _dataBuffer.Clear();
            _wasClosed = true;
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
                _outputFile?.Close(); // double-check
                _wasClosed = false;
                _outputFile = CreateFile();
                _outputFile.WriteLine("Tick");
            }

            _outputFile.WriteLine(text);
            _outputFile.Flush();
        }

        private void Write(object text)
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
