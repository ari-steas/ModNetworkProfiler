using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;

namespace ClientPlugin
{
    public class ProfilingTracker
    {
        public long LoggedInterval = 60 * TimeSpan.TicksPerSecond;

        public readonly Dictionary<ushort, Type> DeclaringTypeMap = new Dictionary<ushort, Type>();
        public readonly Dictionary<ushort, Queue<Message>> OutgoingMessagesTick = new Dictionary<ushort, Queue<Message>>();
        public readonly Dictionary<ushort, Queue<Message>> IncomingMessagesTick = new Dictionary<ushort, Queue<Message>>();

        public long CurrentInterval = 0;

        #region Tracking Methods

        public void RegisterNetworkHandler(ushort networkId, Type declaringType)
        {
            DeclaringTypeMap[networkId] = declaringType;
            IncomingMessagesTick[networkId] = new Queue<Message>();
            Plugin.Window?.RegisterDownHandler(GetNetworkIdName(networkId), networkId);
        }

        public void UnregisterNetworkHandler(ushort networkId)
        {
            if (!DeclaringTypeMap.ContainsKey(networkId))
                return;
            Plugin.Window?.UnregisterDownHandler(GetNetworkIdName(networkId));
            IncomingMessagesTick.Remove(networkId);
            DeclaringTypeMap.Remove(networkId);
        }

        public void LogSendMessage(ushort networkId, int messageSize)
        {
            if (!OutgoingMessagesTick.ContainsKey(networkId))
            {
                OutgoingMessagesTick[networkId] = new Queue<Message>();
                Plugin.Window.RegisterUpHandler(networkId);
            }
            OutgoingMessagesTick[networkId].Enqueue(new Message(messageSize));
        }

        public void LogReceiveMessage(ushort networkId, int messageSize)
        {
            if (!IncomingMessagesTick.ContainsKey(networkId))
                IncomingMessagesTick[networkId] = new Queue<Message>();
            IncomingMessagesTick[networkId].Enqueue(new Message(messageSize));
        }

        #endregion

        public int GetNetworkLoadDown(ushort networkId, out int packetCount)
        {
            packetCount = 0;
            Queue<Message> queue = IncomingMessagesTick.GetValueOrDefault(networkId, null);
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

        public void Update()
        {
            long ticksNow = DateTime.Now.Ticks;
            CurrentInterval = 0;
            // Removes old data
            foreach (var queue in OutgoingMessagesTick.Values)
            {
                lock (queue)
                {
                    while (queue.Count > 0 && queue.Peek().Timestamp < ticksNow - LoggedInterval)
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

            foreach (var queue in IncomingMessagesTick.Values)
            {
                lock (queue)
                {
                    while (queue.Count > 0 && queue.Peek().Timestamp < ticksNow - LoggedInterval)
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
