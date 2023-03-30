// Based on https://github.com/nickgravelyn/UnityToolbag/blob/master/Future/Future.cs
/*
 * The MIT License (MIT)

Copyright (c) 2017, Nick Gravelyn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 * */

using System;
using System.Collections.Generic;

namespace BestHTTP.Futures
{
    /// <summary>
    /// Describes the state of a future.
    /// </summary>
    public enum FutureState
    {
        /// <summary>
        /// The future hasn't begun to resolve a value.
        /// </summary>
        Pending,

        /// <summary>
        /// The future is working on resolving a value.
        /// </summary>
        Processing,

        /// <summary>
        /// The future has a value ready.
        /// </summary>
        Success,

        /// <summary>
        /// The future failed to resolve a value.
        /// </summary>
        Error
    }

    /// <summary>
    /// Defines the interface of an object that can be used to track a future value.
    /// </summary>
    /// <typeparam name="T">The type of object being retrieved.</typeparam>
    public interface IFuture<T>
    {
        /// <summary>
        /// Gets the state of the future.
        /// </summary>
        FutureState state { get; }

        /// <summary>
        /// Gets the value if the State is Success.
        /// </summary>
        T value { get; }

        /// <summary>
        /// Gets the failure exception if the State is Error.
        /// </summary>
        Exception error { get; }

        /// <summary>
        /// Adds a new callback to invoke when an intermediate result is known.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The future so additional calls can be chained together.</returns>
        IFuture<T> OnItem(FutureValueCallback<T> callback);

        /// <summary>
        /// Adds a new callback to invoke if the future value is retrieved successfully.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The future so additional calls can be chained together.</returns>
        IFuture<T> OnSuccess(FutureValueCallback<T> callback);

        /// <summary>
        /// Adds a new callback to invoke if the future has an error.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The future so additional calls can be chained together.</returns>
        IFuture<T> OnError(FutureErrorCallback callback);

        /// <summary>
        /// Adds a new callback to invoke if the future value is retrieved successfully or has an error.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The future so additional calls can be chained together.</returns>
        IFuture<T> OnComplete(FutureCallback<T> callback);
    }
    
    /// <summary>
    /// Defines the signature for callbacks used by the future.
    /// </summary>
    /// <param name="future">The future.</param>
    public delegate void FutureCallback<T>(IFuture<T> future);

    public delegate void FutureValueCallback<T>(T value);
    public delegate void FutureErrorCallback(Exception error);

    /// <summary>
    /// An implementation of <see cref="IFuture{T}"/> that can be used internally by methods that return futures.
    /// </summary>
    /// <remarks>
    /// Methods should always return the <see cref="IFuture{T}"/> interface when calling code requests a future.
    /// This class is intended to be constructed internally in the method to provide a simple implementation of
    /// the interface. By returning the interface instead of the class it ensures the implementation can change
    /// later on if requirements change, without affecting the calling code.
    /// </remarks>
    /// <typeparam name="T">The type of object being retrieved.</typeparam>
    public class Future<T> : IFuture<T>
    {
        private volatile FutureState _state;
        private T _value;
        private Exception _error;
        private Func<T> _processFunc;

        private readonly List<FutureValueCallback<T>> _itemCallbacks = new List<FutureValueCallback<T>>();
        private readonly List<FutureValueCallback<T>> _successCallbacks = new List<FutureValueCallback<T>>();
        private readonly List<FutureErrorCallback> _errorCallbacks = new List<FutureErrorCallback>();
        private readonly List<FutureCallback<T>> _complationCallbacks = new List<FutureCallback<T>>();

        /// <summary>
        /// Gets the state of the future.
        /// </summary>
        public FutureState state { get { return _state; } }

        /// <summary>
        /// Gets the value if the State is Success.
        /// </summary>
        public T value
        {
            get
            {
                if (_state != FutureState.Success && _state != FutureState.Processing)
                {
                    throw new InvalidOperationException("value is not available unless state is Success or Processing.");
                }

                return _value;
            }
        }

        /// <summary>
        /// Gets the failure exception if the State is Error.
        /// </summary>
        public Exception error
        {
            get
            {
                if (_state != FutureState.Error)
                {
                    throw new InvalidOperationException("error is not available unless state is Error.");
                }

                return _error;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Future{T}"/> class.
        /// </summary>
        public Future()
        {
            _state = FutureState.Pending;
        }

        public IFuture<T> OnItem(FutureValueCallback<T> callback)
        {
            if (_state < FutureState.Success && !_itemCallbacks.Contains(callback))
              _itemCallbacks.Add(callback);

            return this;
        }

        /// <summary>
        /// Adds a new callback to invoke if the future value is retrieved successfully.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The future so additional calls can be chained together.</returns>
        public IFuture<T> OnSuccess(FutureValueCallback<T> callback)
        {
            if (_state == FutureState.Success)
            {
                callback(this.value);
            }
            else if (_state != FutureState.Error && !_successCallbacks.Contains(callback))
            {
                _successCallbacks.Add(callback);
            }

            return this;
        }

        /// <summary>
        /// Adds a new callback to invoke if the future has an error.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The future so additional calls can be chained together.</returns>
        public IFuture<T> OnError(FutureErrorCallback callback)
        {
            if (_state == FutureState.Error)
            {
                callback(this.error);
            }
            else if (_state != FutureState.Success && !_errorCallbacks.Contains(callback))
            {
                _errorCallbacks.Add(callback);
            }

            return this;
        }

        /// <summary>
        /// Adds a new callback to invoke if the future value is retrieved successfully or has an error.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The future so additional calls can be chained together.</returns>
        public IFuture<T> OnComplete(FutureCallback<T> callback)
        {
            if (_state == FutureState.Success || _state == FutureState.Error)
            {
                callback(this);
            }
            else
            {
                if (!_complationCallbacks.Contains(callback))
                    _complationCallbacks.Add(callback);
            }

            return this;
        }

#pragma warning disable 1998

        /// <summary>
        /// Begins running a given function on a background thread to resolve the future's value, as long
        /// as it is still in the Pending state.
        /// </summary>
        /// <param name="func">The function that will retrieve the desired value.</param>
        public IFuture<T> Process(Func<T> func)
        {
            if (_state != FutureState.Pending)
            {
                throw new InvalidOperationException("Cannot process a future that isn't in the Pending state.");
            }

            BeginProcess();
            _processFunc = func;

#if NETFX_CORE
#pragma warning disable 4014
            Windows.System.Threading.ThreadPool.RunAsync(ThreadFunc);
#pragma warning restore 4014
#else
            System.Threading.ThreadPool.QueueUserWorkItem(ThreadFunc);
#endif

            return this;
        }

        private
#if NETFX_CORE
            async
#endif
            void ThreadFunc(object param)
        {
            try
            {
                // Directly call the Impl version to avoid the state validation of the public method
                AssignImpl(_processFunc());
            }
            catch (Exception e)
            {
                // Directly call the Impl version to avoid the state validation of the public method
                FailImpl(e);
            }
            finally
            {
                _processFunc = null;
            }
        }

#pragma warning restore 1998

        /// <summary>
        /// Allows manually assigning a value to a future, as long as it is still in the pending state.
        /// </summary>
        /// <remarks>
        /// There are times where you may not need to do background processing for a value. For example,
        /// you may have a cache of values and can just hand one out. In those cases you still want to
        /// return a future for the method signature, but can just call this method to fill in the future.
        /// </remarks>
        /// <param name="value">The value to assign the future.</param>
        public void Assign(T value)
        {
            if (_state != FutureState.Pending && _state != FutureState.Processing)
            {
                throw new InvalidOperationException("Cannot assign a value to a future that isn't in the Pending or Processing state.");
            }

            AssignImpl(value);
        }

        public void BeginProcess(T initialItem = default(T))
        {
            _state = FutureState.Processing;
            _value = initialItem;
        }

        public void AssignItem(T value)
        {
            _value = value;
            _error = null;

            foreach (var callback in _itemCallbacks)
                callback(this.value);
        }

        public void Finish()
        {
            _state = FutureState.Success;

            FlushSuccessCallbacks();
        }

        /// <summary>
        /// Allows manually failing a future, as long as it is still in the pending state.
        /// </summary>
        /// <remarks>
        /// As with the Assign method, there are times where you may know a future value is a failure without
        /// doing any background work. In those cases you can simply fail the future manually and return it.
        /// </remarks>
        /// <param name="error">The exception to use to fail the future.</param>
        public void Fail(Exception error)
        {
            if (_state != FutureState.Pending && _state != FutureState.Processing)
            {
                throw new InvalidOperationException("Cannot fail future that isn't in the Pending or Processing state.");
            }

            FailImpl(error);
        }

        private void AssignImpl(T value)
        {
            _value = value;
            _error = null;
            _state = FutureState.Success;

            FlushSuccessCallbacks();
        }

        private void FailImpl(Exception error)
        {
            _value = default(T);
            _error = error;
            _state = FutureState.Error;

            FlushErrorCallbacks();
        }

        private void FlushSuccessCallbacks()
        {
            foreach (var callback in _successCallbacks)
                callback(this.value);

            FlushComplationCallbacks();
        }

        private void FlushErrorCallbacks()
        {
            foreach (var callback in _errorCallbacks)
                callback(this.error);

            FlushComplationCallbacks();
        }

        private void FlushComplationCallbacks()
        {
            foreach (var callback in _complationCallbacks)
                callback(this);
            ClearCallbacks();
        }

        private void ClearCallbacks()
        {
            _itemCallbacks.Clear();
            _successCallbacks.Clear();
            _errorCallbacks.Clear();
            _complationCallbacks.Clear();
        }
    }
}