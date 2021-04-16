using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HDV.UIBinding
{
    //TODO: Need to be Generic!
    [DefaultExecutionOrder(100)]
    public static class UIBindProxy
    {
        /// <summary>
        /// For thread safe update
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        struct PendingData<T>
        {
            public Object OwnedObject;
            public string DataName;
            public T NewValue;

            public PendingData(Object ownedObject, string dataName, T newValue)
            {
                OwnedObject = ownedObject;
                DataName = dataName;
                NewValue = newValue;
            }
        }

        private static Queue<PendingData<object>> _objectPending = new Queue<PendingData<object>>();
        private static Queue<PendingData<float>> _floatPending = new Queue<PendingData<float>>();
        private static Queue<PendingData<int>> _intPending = new Queue<PendingData<int>>();

        private static Dictionary<string, Action<float>> _floatDic = new Dictionary<string, Action<float>>();
        private static Dictionary<string, Action<int>> _intDic = new Dictionary<string, Action<int>>();
        private static Dictionary<string, Action<object>> _objectDic = new Dictionary<string, Action<object>>();

        static UIBindProxy()
        {
            Update();
        }

        /// <summary>
        /// Check is pending data from another thread
        /// </summary>
        private async static void Update()
        {
            while (Application.isPlaying)
            {
                await Task.Yield();

                while (_objectPending.Count > 0)
                {
                    var data = _objectPending.Dequeue();
                    UpdateObjectValue(data.OwnedObject, data.DataName, data.NewValue);
                }

                while (_floatPending.Count > 0)
                {
                    var data = _floatPending.Dequeue();
                    UpdateFloatValue(data.OwnedObject, data.DataName, data.NewValue);
                }

                while (_intPending.Count > 0)
                {
                    var data = _intPending.Dequeue();
                    UpdateIntValue(data.OwnedObject, data.DataName, data.NewValue);
                }
            }
        }

        public static void UpdateFloatValue(Object ownedObject, string dataName, float newValue)
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                _floatPending.Enqueue(new PendingData<float>(ownedObject, dataName, newValue));
                return;
            }

            if (_floatDic.TryGetValue(ownedObject.GetInstanceID().ToString() + '.' + dataName, out Action<float> del))
            {
                del?.Invoke(newValue);
            }
        }

        public static void UpdateIntValue(Object ownedObject, string dataName, int newValue)
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                _intPending.Enqueue(new PendingData<int>(ownedObject, dataName, newValue));
                return;
            }

            if (_intDic.TryGetValue(ownedObject.GetInstanceID().ToString() + '.' + dataName, out Action<int> del))
            {
                del?.Invoke(newValue);
            }
        }

        public static void UpdateObjectValue(Object ownedObject, string dataName, object newValue)
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                _objectPending.Enqueue(new PendingData<object>(ownedObject, dataName, newValue));
                return;
            }

            if (_objectDic.TryGetValue(ownedObject.GetInstanceID().ToString() + '.' + dataName, out Action<object> del))
            {
                del?.Invoke(newValue);
            }
        }

        public static void BindFloatField(string key, Action<float> listener)
        {
            if(_floatDic.TryGetValue(key, out var del))
            {
                _floatDic[key] = del + listener;
            }
            else
            {
                _floatDic[key] = listener;
            }
        }
        public static void BindIntField(string key, Action<int> listener)
        {
            if (_intDic.TryGetValue(key, out var del))
            {
                _intDic[key] = del + listener;
            }
            else
            {
                _intDic[key] = listener;
            }
        }

        public static void BindObjectField(string key, Action<object> listener)
        {
            if (_objectDic.TryGetValue(key, out var del))
            {
                _objectDic[key] = del + listener;
            }
            else
            {
                _objectDic[key] = listener;
            }
        }

        public static void ReleaseFloatField(string key, Action<float> listener)
        {
            if (_floatDic.TryGetValue(key, out var del))
            {
                _floatDic[key] = del - listener;
            }
        }

        public static void ReleaseIntField(string key, Action<int> listener)
        {
            if (_intDic.TryGetValue(key, out var del))
            {
                _intDic[key] = del - listener;
            }
        }

        public static void ReleaseObjectField(string key, Action<object> listener)
        {
            if (_objectDic.TryGetValue(key, out var del))
            {
                _objectDic[key] = del - listener;
            }
        }
    }
}

