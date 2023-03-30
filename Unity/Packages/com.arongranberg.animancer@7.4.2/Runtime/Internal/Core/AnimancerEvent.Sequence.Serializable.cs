// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

//#define ANIMANCER_ULT_EVENTS

// If you edit this file to change the callback type to something other than UltEvents, you will need to change this
// alias as well as the HasPersistentCalls method below.
#if ANIMANCER_ULT_EVENTS
using SerializableCallback = UltEvents.UltEvent;
#else
using SerializableCallback = UnityEngine.Events.UnityEvent;
#endif

using UnityEngine;
using System;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// https://kybernetik.com.au/animancer/api/Animancer/Sequence
        partial class Sequence
        {
            /// <summary>
            /// An <see cref="Sequence"/> that can be serialized and uses <see cref="SerializableCallback"/>s to define
            /// the <see cref="callback"/>s.
            /// </summary>
            /// <remarks>
            /// If you have Animancer Pro you can replace <see cref="SerializableCallback"/>s with
            /// <see href="https://kybernetik.com.au/ultevents">UltEvents</see> using the following procedure:
            /// <list type="number">
            /// <item>Select the <c>Assets/Plugins/Animancer/Animancer.asmdef</c> and add a Reference to the
            /// <c>UltEvents</c> Assembly Definition.</item>
            /// <item>Go into the Player Settings of your project and add <c>ANIMANCER_ULT_EVENTS</c> as a Scripting
            /// Define Symbol. Or you can simply edit this script to change the event type (it is located at
            /// <c>Assets/Plugins/Animancer/Internal/Core/AnimancerEvent.Sequence.Serializable.cs</c> by default.</item>
            /// </list>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
            /// </remarks>
            /// https://kybernetik.com.au/animancer/api/Animancer/Serializable
            /// 
            [Serializable]
            public class Serializable : ICopyable<Serializable>
#if UNITY_EDITOR
                , ISerializationCallbackReceiver
#endif
            {
                /************************************************************************************************************************/

                [SerializeField]
                private float[] _NormalizedTimes;

                /// <summary>[<see cref="SerializeField"/>] The serialized <see cref="normalizedTime"/>s.</summary>
                public ref float[] NormalizedTimes => ref _NormalizedTimes;

                /************************************************************************************************************************/

                [SerializeField]
                private SerializableCallback[] _Callbacks;

                /// <summary>[<see cref="SerializeField"/>] The serialized <see cref="callback"/>s.</summary>
                /// <remarks>
                /// This array only needs to be large enough to hold the last event that actually contains any calls.
                /// Any empty or missing elements will simply use the <see cref="DummyCallback"/> at runtime.
                /// </remarks>
                public ref SerializableCallback[] Callbacks => ref _Callbacks;

                /************************************************************************************************************************/

                [SerializeField]
                private string[] _Names;

                /// <summary>[<see cref="SerializeField"/>] The serialized <see cref="Sequence.Names"/>.</summary>
                public ref string[] Names => ref _Names;

                /************************************************************************************************************************/
#if UNITY_EDITOR
                /************************************************************************************************************************/

                /// <summary>[Editor-Only, Internal] The name of the array field which stores the <see cref="normalizedTime"/>s.</summary>
                internal const string NormalizedTimesField = nameof(_NormalizedTimes);

                /// <summary>[Editor-Only, Internal] The name of the array field which stores the serialized <see cref="callback"/>s.</summary>
                internal const string CallbacksField = nameof(_Callbacks);

                /// <summary>[Editor-Only, Internal] The name of the array field which stores the serialized <see cref="Names"/>.</summary>
                internal const string NamesField = nameof(_Names);

                /************************************************************************************************************************/
#endif
                /************************************************************************************************************************/

                private Sequence _Events;

                /// <summary>
                /// The runtime <see cref="Sequence"/> compiled from this <see cref="Serializable"/>.
                /// Each call after the first will return the same reference.
                /// </summary>
                /// <remarks>
                /// Unlike <see cref="GetEventsOptional"/>, this property will create an empty
                /// <see cref="Sequence"/> instead of returning null if there are no events.
                /// </remarks>
                public Sequence Events
                {
                    get
                    {
                        if (_Events == null)
                        {
                            GetEventsOptional();
                            if (_Events == null)
                                _Events = new Sequence();
                        }

                        return _Events;
                    }
                    set => _Events = value;
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Returns the runtime <see cref="Sequence"/> compiled from this <see cref="Serializable"/>.
                /// Each call after the first will return the same reference.
                /// </summary>
                /// <remarks>
                /// This method returns null if the sequence would be empty anyway and is used by the implicit
                /// conversion from <see cref="Serializable"/> to <see cref="Sequence"/>.
                /// </remarks>
                public Sequence GetEventsOptional()
                {
                    if (_Events != null ||
                        _NormalizedTimes == null)
                        return _Events;

                    var timeCount = _NormalizedTimes.Length;
                    if (timeCount == 0)
                        return null;

                    var callbackCount = _Callbacks != null ? _Callbacks.Length : 0;

                    var callback = callbackCount >= timeCount-- ?
                        GetInvoker(_Callbacks[timeCount]) :
                        null;
                    var endEvent = new AnimancerEvent(_NormalizedTimes[timeCount], callback);

                    _Events = new Sequence(timeCount)
                    {
                        EndEvent = endEvent,
                        Count = timeCount,
                        _Names = _Names,
                    };

                    for (int i = 0; i < timeCount; i++)
                    {
                        callback = i < callbackCount ? GetInvoker(_Callbacks[i]) : DummyCallback;
                        _Events._Events[i] = new AnimancerEvent(_NormalizedTimes[i], callback);
                    }

                    return _Events;
                }

                /// <summary>Calls <see cref="GetEventsOptional"/>.</summary>
                public static implicit operator Sequence(Serializable serializable) => serializable?.GetEventsOptional();

                /************************************************************************************************************************/

                /// <summary>Returns the <see cref="Events"/> or <c>null</c> if it wasn't yet initialized.</summary>
                internal Sequence InitializedEvents => _Events;

                /************************************************************************************************************************/

                /// <summary>
                /// If the `callback` has any persistent calls, this method returns a delegate to call its
                /// <see cref="SerializableCallback.Invoke"/> method. Otherwise it returns the
                /// <see cref="DummyCallback"/>.
                /// </summary>
                public static Action GetInvoker(SerializableCallback callback)
                    => HasPersistentCalls(callback) ? callback.Invoke : DummyCallback;

#if UNITY_EDITOR
                /// <summary>[Editor-Only]
                /// Casts the `callback` and calls <see cref="GetInvoker(SerializableCallback)"/>.
                /// </summary>
                public static Action GetInvoker(object callback)
                    => GetInvoker((SerializableCallback)callback);
#endif

                /************************************************************************************************************************/

                /// <summary>
                /// Determines if the `callback` contains any method calls that will be serialized (otherwise the
                /// <see cref="DummyCallback"/> can be used instead of creating a new delegate to invoke the empty
                /// `callback`).
                /// </summary>
                public static bool HasPersistentCalls(SerializableCallback callback)
                {
                    if (callback == null)
                        return false;

                    // UnityEvents do not allow us to check if any dynamic calls are present.
                    // But we are not giving runtime access to the events so it does not really matter.
                    // UltEvents does allow it (via the HasCalls property), but we might as well be consistent.

#if ANIMANCER_ULT_EVENTS
                    var calls = callback.PersistentCallsList;
                    return calls != null && calls.Count > 0;
#else
                    return callback.GetPersistentEventCount() > 0;
#endif
                }

#if UNITY_EDITOR
                /// <summary>[Editor-Only]
                /// Casts the `callback` and calls <see cref="HasPersistentCalls(SerializableCallback)"/>.
                /// </summary>
                public static bool HasPersistentCalls(object callback) => HasPersistentCalls((SerializableCallback)callback);
#endif

                /************************************************************************************************************************/

                /// <summary>Returns the <see cref="normalizedTime"/> of the <see cref="EndEvent"/>.</summary>
                /// <remarks>If the value is not set, the value is determined by <see cref="GetDefaultNormalizedEndTime"/>.</remarks>
                public float GetNormalizedEndTime(float speed = 1)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        return GetDefaultNormalizedEndTime(speed);
                    else
                        return _NormalizedTimes[_NormalizedTimes.Length - 1];
                }

                /************************************************************************************************************************/

                /// <summary>Sets the <see cref="normalizedTime"/> of the <see cref="EndEvent"/>.</summary>
                public void SetNormalizedEndTime(float normalizedTime)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        _NormalizedTimes = new float[] { normalizedTime };
                    else
                        _NormalizedTimes[_NormalizedTimes.Length - 1] = normalizedTime;
                }

                /************************************************************************************************************************/

                /// <inheritdoc/>
                public void CopyFrom(Serializable copyFrom)
                {
                    if (copyFrom == null)
                    {
                        _NormalizedTimes = default;
                        _Callbacks = default;
                        _Names = default;
                        return;
                    }

                    AnimancerUtilities.CopyExactArray(copyFrom._NormalizedTimes, ref _NormalizedTimes);
                    AnimancerUtilities.CopyExactArray(copyFrom._Callbacks, ref _Callbacks);
                    AnimancerUtilities.CopyExactArray(copyFrom._Names, ref _Names);
                }

                /************************************************************************************************************************/
#if UNITY_EDITOR
                /************************************************************************************************************************/

                /// <summary>[Editor-Only] Does nothing.</summary>
                /// <remarks>
                /// Keeping the runtime <see cref="Events"/> in sync with the serialized data is handled by
                /// <see cref="Editor.SerializableEventSequenceDrawer"/>.
                /// </remarks>
                void ISerializationCallbackReceiver.OnAfterDeserialize() { }

                /************************************************************************************************************************/

                /// <summary>[Editor-Only] Ensures that the events are sorted by time (excluding the end event).</summary>
                void ISerializationCallbackReceiver.OnBeforeSerialize()
                {
                    if (_NormalizedTimes == null ||
                        _NormalizedTimes.Length <= 2)
                    {
                        CompactArrays();
                        return;
                    }

                    var eventContext = Editor.SerializableEventSequenceDrawer.Context.Current;
                    var selectedEvent = eventContext?.Property != null ? eventContext.SelectedEvent : -1;

                    var timeCount = _NormalizedTimes.Length - 1;

                    var previousTime = _NormalizedTimes[0];

                    // Bubble Sort based on the normalized times.
                    for (int i = 1; i < timeCount; i++)
                    {
                        var time = _NormalizedTimes[i];
                        if (time >= previousTime)
                        {
                            previousTime = time;
                            continue;
                        }

                        _NormalizedTimes.Swap(i, i - 1);
                        DynamicSwap(ref _Callbacks, i);
                        DynamicSwap(ref _Names, i);

                        if (selectedEvent == i)
                            selectedEvent = i - 1;
                        else if (selectedEvent == i - 1)
                            selectedEvent = i;

                        if (i == 1)
                        {
                            i = 0;
                            previousTime = float.NegativeInfinity;
                        }
                        else
                        {
                            i -= 2;
                            previousTime = _NormalizedTimes[i];
                        }
                    }

                    // If the current animation is looping, clamp all times within the 0-1 range.
                    var transitionContext = Editor.TransitionDrawer.Context;
                    if (transitionContext != null &&
                        transitionContext.Transition != null &&
                        transitionContext.Transition.IsLooping)
                    {
                        for (int i = _NormalizedTimes.Length - 1; i >= 0; i--)
                        {
                            var time = _NormalizedTimes[i];
                            if (time < 0)
                                _NormalizedTimes[i] = 0;
                            else if (time > AlmostOne)
                                _NormalizedTimes[i] = AlmostOne;
                        }
                    }

                    // If the selected event was moved adjust the selection.
                    if (eventContext?.Property != null && eventContext.SelectedEvent != selectedEvent)
                    {
                        eventContext.SelectedEvent = selectedEvent;
                        Editor.TransitionPreviewWindow.PreviewNormalizedTime = _NormalizedTimes[selectedEvent];
                    }

                    CompactArrays();
                }

                /************************************************************************************************************************/

                /// <summary>[Editor-Only]
                /// Swaps <c>array[index]</c> with <c>array[index - 1]</c> while accounting for the possibility of the
                /// `index` being beyond the bounds of the `array`.
                /// </summary>
                private static void DynamicSwap<T>(ref T[] array, int index)
                {
                    var count = array != null ? array.Length : 0;

                    if (index == count)
                        Array.Resize(ref array, ++count);

                    if (index < count)
                        array.Swap(index, index - 1);
                }

                /************************************************************************************************************************/

                /// <summary>[Internal]
                /// Should the arrays be prevented from reducing their size when their last elements are unused?
                /// </summary>
                internal static bool DisableCompactArrays { get; set; }

                /// <summary>[Editor-Only]
                /// Removes empty data from the ends of the arrays to reduce the serialized data size.
                /// </summary>
                private void CompactArrays()
                {
                    if (DisableCompactArrays)
                        return;

                    // If there is only one time and it is NaN, we don't need to store anything.
                    if (_NormalizedTimes == null ||
                        (_NormalizedTimes.Length == 1 &&
                        (_Callbacks == null || _Callbacks.Length == 0) &&
                        (_Names == null || _Names.Length == 0) &&
                        float.IsNaN(_NormalizedTimes[0])))
                    {
                        _NormalizedTimes = Array.Empty<float>();
                        _Callbacks = Array.Empty<SerializableCallback>();
                        _Names = Array.Empty<string>();
                        return;
                    }

                    Trim(ref _Callbacks, _NormalizedTimes.Length, (callback) => HasPersistentCalls(callback));
                    Trim(ref _Names, _NormalizedTimes.Length, (name) => !string.IsNullOrEmpty(name));
                }

                /************************************************************************************************************************/

                /// <summary>[Editor-Only] Removes unimportant values from the end of the `array`.</summary>
                private static void Trim<T>(ref T[] array, int maxLength, Func<T, bool> isImportant)
                {
                    if (array == null)
                        return;

                    var count = Math.Min(array.Length, maxLength);

                    while (count >= 1)
                    {
                        var item = array[count - 1];
                        if (isImportant(item))
                            break;
                        else
                            count--;
                    }

                    Array.Resize(ref array, count);
                }

                /************************************************************************************************************************/
#endif
                /************************************************************************************************************************/
            }
        }
    }
}

/************************************************************************************************************************/
#if UNITY_EDITOR
/************************************************************************************************************************/

namespace Animancer.Editor
{
    /// <summary>[Editor-Only, Internal]
    /// A serializable container which holds a <see cref="SerializableCallback"/> in a field named "_Callback".
    /// </summary>
    /// <remarks>
    /// <see cref="DummySerializableCallback"/> needs to be in a file with the same name as it (otherwise it can't
    /// draw the callback properly) and this class needs to be in the same file as
    /// <see cref="AnimancerEvent.Sequence.Serializable"/> to use the <see cref="SerializableCallback"/> alias.
    /// </remarks>
    [Serializable]
    internal sealed class SerializableCallbackHolder
    {
#pragma warning disable CS0169 // Field is never used.
        [SerializeField]
        private SerializableCallback _Callback;
#pragma warning restore CS0169 // Field is never used.

        /// <summary>The name of the field which stores the <see cref="SerializableCallback"/>.</summary>
        internal const string CallbackField = nameof(_Callback);
    }
}

/************************************************************************************************************************/
#endif
/************************************************************************************************************************/

