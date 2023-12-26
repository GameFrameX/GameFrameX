//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFrameX
{
    public static partial class ReferencePool
    {
        /// <summary>
        /// 引用集合
        /// </summary>
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> _references;
            private readonly Type _referenceType;
            private int _usingReferenceCount;
            private int _acquireReferenceCount;
            private int _releaseReferenceCount;
            private int _addReferenceCount;
            private int _removeReferenceCount;

            public ReferenceCollection(Type referenceType)
            {
                _references = new Queue<IReference>();
                _referenceType = referenceType;
                _usingReferenceCount = 0;
                _acquireReferenceCount = 0;
                _releaseReferenceCount = 0;
                _addReferenceCount = 0;
                _removeReferenceCount = 0;
            }

            /// <summary>
            /// 引用类型
            /// </summary>
            public Type ReferenceType
            {
                get { return _referenceType; }
            }

            /// <summary>
            /// 未使用的引用计数。
            /// </summary>
            public int UnusedReferenceCount
            {
                get { return _references.Count; }
            }

            /// <summary>
            /// 正在使用的引用计数。
            /// </summary>
            public int UsingReferenceCount
            {
                get { return _usingReferenceCount; }
            }

            /// <summary>
            /// 获取引用的次数。
            /// </summary>
            public int AcquireReferenceCount
            {
                get { return _acquireReferenceCount; }
            }

            /// <summary>
            /// 归还引用的次数。
            /// </summary>
            public int ReleaseReferenceCount
            {
                get { return _releaseReferenceCount; }
            }

            /// <summary>
            /// 添加引用的次数。
            /// </summary>
            public int AddReferenceCount
            {
                get { return _addReferenceCount; }
            }

            /// <summary>
            /// 移除引用的次数。
            /// </summary>
            public int RemoveReferenceCount
            {
                get { return _removeReferenceCount; }
            }

            /// <summary>
            /// 从引用池获取引用。
            /// </summary>
            /// <typeparam name="T">引用类型。</typeparam>
            /// <returns>引用。</returns>
            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return (T)_references.Dequeue();
                    }
                }

                _addReferenceCount++;
                return new T();
            }

            /// <summary>
            /// 从引用池获取引用。
            /// </summary>
            /// <returns>引用。</returns>
            public IReference Acquire()
            {
                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return _references.Dequeue();
                    }
                }

                _addReferenceCount++;
                return (IReference)Activator.CreateInstance(_referenceType);
            }

            /// <summary>
            /// 释放一个引用对象。
            /// </summary>
            /// <param name="reference">要释放的引用对象。</param>
            public void Release(IReference reference)
            {
                reference.Clear();
                lock (_references)
                {
                    if (m_EnableStrictCheck && _references.Contains(reference))
                    {
                        GameFrameworkLog.Error("Reference has been released!=>{0}", reference.GetType().FullName);
                        return;
                    }

                    _references.Enqueue(reference);
                }

                _releaseReferenceCount++;
                _usingReferenceCount--;
            }

            /// <summary>
            /// 添加指定类型的引用对象到引用池中。
            /// </summary>
            /// <typeparam name="T">要添加的引用对象类型。</typeparam>
            /// <param name="count">要添加的引用对象数量。</param>
            /// <exception cref="GameFrameworkException">类型无效。</exception>
            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                lock (_references)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Enqueue(new T());
                    }
                }
            }

            /// <summary>
            /// 向引用池中添加指定数量的引用。
            /// </summary>
            /// <param name="count">要添加的引用数量。</param>
            public void Add(int count)
            {
                lock (_references)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Enqueue((IReference)Activator.CreateInstance(_referenceType));
                    }
                }
            }

            /// <summary>
            /// 从引用池中移除指定数量的引用。
            /// </summary>
            /// <param name="count">要移除的引用数量。</param>
            public void Remove(int count)
            {
                lock (_references)
                {
                    if (count > _references.Count)
                    {
                        count = _references.Count;
                    }

                    _removeReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Dequeue();
                    }
                }
            }

            /// <summary>
            /// 从引用池中移除所有的引用。
            /// </summary>
            public void RemoveAll()
            {
                lock (_references)
                {
                    _removeReferenceCount += _references.Count;
                    _references.Clear();
                }
            }
        }
    }
}