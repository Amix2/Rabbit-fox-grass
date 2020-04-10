using System;
using System.Collections.Generic;

namespace World
{
    public class MultiTypeEventHandler<T1, T2, T3>
    {
        private readonly Dictionary<HistoryEventType, EventHandler<T1>> t1Handlers;
        private readonly Dictionary<HistoryEventType, EventHandler<T2>> t2Handlers;
        private readonly Dictionary<HistoryEventType, EventHandler<T3>> t3Handlers;
        private readonly Dictionary<HistoryEventType, bool> usedTypes;

        public MultiTypeEventHandler()
        {
            t1Handlers = new Dictionary<HistoryEventType, EventHandler<T1>>();
            t2Handlers = new Dictionary<HistoryEventType, EventHandler<T2>>();
            t3Handlers = new Dictionary<HistoryEventType, EventHandler<T3>>();
            usedTypes = new Dictionary<HistoryEventType, bool>();
        }

        public void Subscribe(HistoryEventType type, EventHandler<T1> action)
        {
            SubscribeToDict(t1Handlers, type, action);
        }

        public void Subscribe(HistoryEventType type, EventHandler<T2> action)
        {
            SubscribeToDict(t2Handlers, type, action);
        }

        public void Subscribe(HistoryEventType type, EventHandler<T3> action)
        {
            SubscribeToDict(t3Handlers, type, action);
        }

        private void SubscribeToDict<T>(Dictionary<HistoryEventType, EventHandler<T>> dict, HistoryEventType type, EventHandler<T> action)
        {
            if (!dict.ContainsKey(type) && usedTypes.ContainsKey(type)) throw new Exception(" MultiTypeEventHandler already contains given type with different value type");
            if (!dict.ContainsKey(type)) dict.Add(type, action);
            else dict[type] += action;
        }

        public void Invoke(object sender, HistoryEventType type, T1 value)
        {
            InvokeToDict(t1Handlers, sender, type, value);
        }

        public void Invoke(object sender, HistoryEventType type, T2 value)
        {
            InvokeToDict(t2Handlers, sender, type, value);
        }

        public void Invoke(object sender, HistoryEventType type, T3 value)
        {
            InvokeToDict(t3Handlers, sender, type, value);
        }

        private void InvokeToDict<T>(Dictionary<HistoryEventType, EventHandler<T>> dict, object sender, HistoryEventType type, T value)
        {
            if (dict.ContainsKey(type)) dict[type]?.Invoke(sender, value);
        }
    }
}