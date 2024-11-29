using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Utils;

namespace ModNetworkProfiler.Profiling
{
    /// <summary>
    /// Tracks and logs profiling data.
    /// </summary>
    public class ProfilingTracker
    {
        public long LoggedInterval = 60 * TimeSpan.TicksPerSecond;
        public readonly Dictionary<ushort, Type> DeclaringTypeMap = new Dictionary<ushort, Type>();
        public readonly Dictionary<ushort, Queue<Message>> OutgoingMessagesTick = new Dictionary<ushort, Queue<Message>>();
        public readonly Dictionary<ushort, Queue<Message>> IncomingMessagesTick = new Dictionary<ushort, Queue<Message>>();
        public long CurrentInterval = 0;

        private bool IsPaused = false; // Play/Pause state
        private ProfilingLogger _downLogger = new ProfilingLogger("Down");
        private ProfilingLogger _upLogger = new ProfilingLogger("Up");

        #region Tracking Methods

        public void RegisterNetworkHandler(ushort networkId, Type declaringType)
        {
            DeclaringTypeMap[networkId] = declaringType;
            IncomingMessagesTick[networkId] = new Queue<Message>();
            Plugin.Window?.RegisterDownHandler(GetNetworkIdName(networkId), networkId);
            MyLog.Default.WriteLineAndConsole("[ModNetworkProfiler] Registered network handler " + networkId);
            _downLogger.AddHandler(networkId);
        }

        public bool UnregisterNetworkHandler(ushort networkId)
        {
            if (!DeclaringTypeMap.ContainsKey(networkId))
                return false;
            Plugin.Window?.UnregisterDownHandler(GetNetworkIdName(networkId));
            IncomingMessagesTick.Remove(networkId);
            DeclaringTypeMap.Remove(networkId);
            MyLog.Default.WriteLineAndConsole("[ModNetworkProfiler] Unregistered network handler " + networkId);
            return true;
        }

        public void LogSendMessage(ushort networkId, int messageSize)
        {
            if (!OutgoingMessagesTick.ContainsKey(networkId))
            {
                OutgoingMessagesTick[networkId] = new Queue<Message>();
                Plugin.Window.RegisterUpHandler(networkId);
            }
            OutgoingMessagesTick[networkId].Enqueue(new Message(messageSize));
            _upLogger.AddHandler(networkId);
            _upLogger.QueueData(networkId, messageSize);
        }

        public void LogReceiveMessage(ushort networkId, int messageSize)
        {
            if (!IncomingMessagesTick.ContainsKey(networkId))
                IncomingMessagesTick[networkId] = new Queue<Message>();

            // Lock the queue before modifying it
            lock (IncomingMessagesTick[networkId])
            {
                IncomingMessagesTick[networkId].Enqueue(new Message(messageSize));
            }
            _downLogger.QueueData(networkId, messageSize);
        }


        //private StreamWriter si = File.CreateText(@"C:\Users\jnick\Downloads\in.txt");
        //private StreamWriter so = File.CreateText(@"C:\Users\jnick\Downloads\out.txt");
        public void LogSerialize(object obj, long length)
        {
            //so.WriteLine($"S{length}: " + obj.GetType().FullName);
            //so.Flush();
        }

        public void LogDeserialize(object obj, long length)
        {
            //si.WriteLine($"D: {length}: " + obj.GetType().FullName);
            //si.Flush();
        }

        public void UnregisterAll()
        {
            Plugin.Window?.UnregisterAll();
            MyLog.Default.WriteLineAndConsole($"[ModNetworkProfiler] Unregistering {DeclaringTypeMap.Count} network handlers.");
            IncomingMessagesTick.Clear();
            OutgoingMessagesTick.Clear();
            DeclaringTypeMap.Clear();

            _upLogger.Close();
            _downLogger.Close();
        }

        #endregion

        public int GetNetworkLoadDown(ushort networkId, out int packetCount)
        {
            packetCount = 0;
            Queue<Message> queue = IncomingMessagesTick.GetValueOrDefault(networkId, null);
            if (queue == null)
                return 0;

            int total = 0;

            // Lock the queue to prevent modification while enumerating
            lock (queue)
            {
                // Create a copy of the queue to iterate over to prevent modification during enumeration
                Message[] queueSnapshot = queue.ToArray();  // Make a safe copy of the queue

                foreach (var item in queueSnapshot)
                {
                    total += item.Size;
                }

                packetCount = queueSnapshot.Length;
            }

            return total;
        }

        public int GetNetworkLoadUp(ushort networkId, out int packetCount)
        {
            packetCount = 0;
            Queue<Message> queue = OutgoingMessagesTick.GetValueOrDefault(networkId, null);
            if (queue == null)
                return 0;

            int total = 0;

            lock (queue)
            {
                foreach (var item in queue)
                {
                    total += item.Size;
                }

                packetCount = queue.Count;
            }

            return total;
        }

        public Message[] GetAllPacketsDown(ushort networkId)
        {
            return IncomingMessagesTick.GetValueOrDefault(networkId)?.ToArray();
        }

        // Pause or resume update
        public void Pause()
        {
            IsPaused = true;
        }

        public void Play()
        {
            IsPaused = false;
        }

        public bool IsPlaying()
        {
            return !IsPaused;
        }

        public void Update()
        {
            if (IsPaused)
            {
                return; // Do nothing if paused
            }

            long ticksNow = DateTime.Now.Ticks;
            CurrentInterval = 0;

            // Removes old data from outgoing messages
            foreach (var queue in OutgoingMessagesTick.Values.ToList())
            {
                lock (queue)
                {
                    List<Message> toRemove = new List<Message>();
                    foreach (var message in queue)
                    {
                        if (message.Timestamp < ticksNow - LoggedInterval)
                        {
                            toRemove.Add(message);
                        }
                    }

                    foreach (var message in toRemove)
                    {
                        queue.Dequeue();
                    }

                    if (queue.Count == 0)
                        continue;

                    long queueInterval = ticksNow - queue.Peek().Timestamp;
                    if (queueInterval > CurrentInterval)
                        CurrentInterval = queueInterval;
                }
            }

            // Removes old data from incoming messages
            foreach (var queue in IncomingMessagesTick.Values.ToList())
            {
                lock (queue)
                {
                    List<Message> toRemove = new List<Message>();
                    foreach (var message in queue)
                    {
                        if (message.Timestamp < ticksNow - LoggedInterval)
                        {
                            toRemove.Add(message);
                        }
                    }

                    foreach (var message in toRemove)
                    {
                        queue.Dequeue();
                    }

                    if (queue.Count == 0)
                        continue;

                    long queueInterval = ticksNow - queue.Peek().Timestamp;
                    if (queueInterval > CurrentInterval)
                        CurrentInterval = queueInterval;
                }
            }

            _downLogger.OnTick();
        }

        public string GetNetworkIdName(ushort networkId)
        {
            var type = DeclaringTypeMap.GetValueOrDefault(networkId, null);
            return type?.FullName + "::" + networkId;
        }

        public class Message
        {
            public long Timestamp;
            public int Size;

            public Message(int size)
            {
                Size = size;
                Timestamp = DateTime.Now.Ticks;
            }
        }
    }
}
