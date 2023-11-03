using System;
using System.Collections.Generic;
using GameFrameX.Timer;
using UnityEngine;

namespace GameFrameX.Timer
{
    /// <summary>
    /// 定时器管理器
    /// </summary>
    public sealed class TimerManager : GameFrameworkModule, ITimerManager
    {
        class TimerItem
        {
            public float Interval;
            public int Repeat;
            public Action<object> Callback;
            public object Param;

            public float Elapsed;
            public bool Deleted;

            public void Set(float interval, int repeat, Action<object> callback, object param)
            {
                this.Interval = interval;
                this.Repeat = repeat;
                this.Callback = callback;
                this.Param = param;
            }
        }

        private readonly Dictionary<Action<object>, TimerItem> _items = new Dictionary<Action<object>, TimerItem>();
        private readonly Dictionary<Action<object>, TimerItem> _toAdd = new Dictionary<Action<object>, TimerItem>();
        private readonly List<TimerItem> _toRemove = new List<TimerItem>();
        private readonly List<TimerItem> _pool = new List<TimerItem>(100);

        private static readonly object Locker = new object();

        /// <summary>
        /// 是否触发回调异常
        /// </summary>
        public static bool CatchCallbackExceptions = false;

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (Locker)
            {
                Dictionary<Action<object>, TimerItem>.Enumerator enumerator;

                if (_items.Count > 0)
                {
                    enumerator = _items.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        TimerItem timerItem = enumerator.Current.Value;
                        if (timerItem.Deleted)
                        {
                            _toRemove.Add(timerItem);
                            continue;
                        }

                        timerItem.Elapsed += realElapseSeconds;
                        if (timerItem.Elapsed < timerItem.Interval)
                        {
                            continue;
                        }

                        timerItem.Elapsed -= timerItem.Interval;
                        if (timerItem.Elapsed < 0 || timerItem.Elapsed > 0.03f)
                            timerItem.Elapsed = 0;

                        if (timerItem.Repeat > 0)
                        {
                            timerItem.Repeat--;
                            if (timerItem.Repeat == 0)
                            {
                                timerItem.Deleted = true;
                                _toRemove.Add(timerItem);
                            }
                        }

                        // var repeat = i.Repeat;
                        if (timerItem.Callback != null)
                        {
                            if (CatchCallbackExceptions)
                            {
                                try
                                {
                                    timerItem.Callback(timerItem.Param);
                                }
                                catch (System.Exception e)
                                {
                                    timerItem.Deleted = true;
                                    Debug.LogWarning("Timer： timer(internal=" + timerItem.Interval + ", repeat=" + timerItem.Repeat + ") callback error > " + e.Message);
                                }
                            }
                            else
                            {
                                timerItem.Callback(timerItem.Param);
                            }
                        }
                    }

                    enumerator.Dispose();
                }


                int len = _toRemove.Count;
                if (len > 0)
                {
                    for (int k = 0; k < len; k++)
                    {
                        TimerItem i = _toRemove[k];
                        if (i.Deleted && i.Callback != null)
                        {
                            _items.Remove(i.Callback);
                            ReturnToPool(i);
                        }
                    }

                    _toRemove.Clear();
                }

                if (_toAdd.Count > 0)
                {
                    enumerator = _toAdd.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        _items.Add(enumerator.Current.Key, enumerator.Current.Value);
                    }

                    enumerator.Dispose();
                    _toAdd.Clear();
                }
            }
        }

        internal override void Shutdown()
        {
            lock (Locker)
            {
                this._toRemove.Clear();
                this._toAdd.Clear();
                this._items.Clear();
            }
        }

        /// <summary>
        /// 添加一个定时调用的任务
        /// </summary>
        /// <param name="interval">间隔时间（以毫秒为单位）</param>
        /// <param name="repeat">重复次数（0 表示无限重复）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        public void Add(float interval, int repeat, Action<object> callback, object callbackParam = null)
        {
            lock (Locker)
            {
                if (callback == null)
                {
                    Debug.LogWarning("timer callback is null, " + interval + "," + repeat);
                    return;
                }


                if (_items.TryGetValue(callback, out var t))
                {
                    t.Set(interval, repeat, callback, callbackParam);
                    t.Elapsed = 0;
                    t.Deleted = false;
                    return;
                }

                if (_toAdd.TryGetValue(callback, out t))
                {
                    t.Set(interval, repeat, callback, callbackParam);
                    return;
                }

                t = GetFromPool();
                t.Interval = interval;
                t.Repeat = repeat;
                t.Callback = callback;
                t.Param = callbackParam;
                _toAdd[callback] = t;
            }
        }


        /// <summary>
        /// 添加一个只执行一次的任务
        /// </summary>
        /// <param name="interval">间隔时间（以毫秒为单位）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        public void AddOnce(float interval, Action<object> callback, object callbackParam = null)
        {
            Add(interval, 1, callback, callbackParam);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        public void AddUpdate(Action<object> callback)
        {
            Add(0.001f, 0, callback);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数</param>
        public void AddUpdate(Action<object> callback, object callbackParam)
        {
            Add(0.001f, 0, callback, callbackParam);
        }

        /// <summary>
        /// 检查指定的任务是否存在
        /// </summary>
        /// <param name="callback">要检查的回调函数</param>
        /// <returns>存在返回 true，不存在返回 false</returns>
        public bool Exists(Action<object> callback)
        {
            lock (Locker)
            {
                if (_toAdd.ContainsKey(callback))
                {
                    return true;
                }


                if (_items.TryGetValue(callback, out var at))
                {
                    return !at.Deleted;
                }

                return false;
            }
        }

        /// <summary>
        /// 移除指定的任务
        /// </summary>
        /// <param name="callback">要移除的回调函数</param>
        public void Remove(Action<object> callback)
        {
            lock (Locker)
            {
                if (_toAdd.TryGetValue(callback, out var t))
                {
                    _toAdd.Remove(callback);
                    ReturnToPool(t);
                }

                if (_items.TryGetValue(callback, out t))
                {
                    t.Deleted = true;
                }
            }
        }

        private TimerItem GetFromPool()
        {
            TimerItem t;
            int cnt = _pool.Count;
            if (cnt > 0)
            {
                t = _pool[cnt - 1];
                _pool.RemoveAt(cnt - 1);
                t.Deleted = false;
                t.Elapsed = 0;
            }
            else
            {
                t = new TimerItem();
            }

            return t;
        }

        private void ReturnToPool(TimerItem t)
        {
            t.Callback = null;
            _pool.Add(t);
        }
    }
}