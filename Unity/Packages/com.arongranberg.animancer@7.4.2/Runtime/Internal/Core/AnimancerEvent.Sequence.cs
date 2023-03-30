// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    partial struct AnimancerEvent
    {
        /// <summary>
        /// A variable-size list of <see cref="AnimancerEvent"/>s which keeps itself sorted according to their
        /// <see cref="normalizedTime"/>.
        /// </summary>
        /// <remarks>
        /// <em>Animancer Lite does not allow events (except for <see cref="OnEnd"/>) in runtime builds.</em>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Sequence
        /// 
        public partial class Sequence : IEnumerable<AnimancerEvent>, ICopyable<Sequence>
        {
            /************************************************************************************************************************/
            #region Fields and Properties
            /************************************************************************************************************************/

            internal const string
                IndexOutOfRangeError = "index must be within the range of 0 <= index < " + nameof(Count);

#if UNITY_ASSERTIONS
            private const string
                NullCallbackError = nameof(AnimancerEvent) + " callbacks can't be null (except for End Events)." +
                " The " + nameof(AnimancerEvent) + "." + nameof(DummyCallback) + " can be assigned to make an event do nothing.";
#endif

            /************************************************************************************************************************/

            /// <summary>All of the <see cref="AnimancerEvent"/>s in this sequence (excluding the <see cref="EndEvent"/>).</summary>
            /// <remarks>This field should never be null. It should use <see cref="Array.Empty{T}"/> instead.</remarks>
            private AnimancerEvent[] _Events;

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] The number of events in this sequence (excluding the <see cref="EndEvent"/>).</summary>
            public int Count { get; private set; }

            /************************************************************************************************************************/

            /// <summary>Indicates whether the sequence has any events in it (including the <see cref="EndEvent"/>).</summary>
            public bool IsEmpty
            {
                get
                {
                    return
                        _EndEvent.callback == null &&
                        float.IsNaN(_EndEvent.normalizedTime) &&
                        Count == 0;
                }
            }

            /************************************************************************************************************************/

            /// <summary>The initial <see cref="Capacity"/> that will be used if another value is not specified.</summary>
            public const int DefaultCapacity = 8;

            /// <summary>[Pro-Only] The size of the internal array used to hold events.</summary>
            /// <remarks>
            /// When set, the array is reallocated to the given size.
            /// <para></para>
            /// By default, the <see cref="Capacity"/> starts at 0 and increases to the <see cref="DefaultCapacity"/>
            /// when the first event is added.
            /// </remarks>
            public int Capacity
            {
                get => _Events.Length;
                set
                {
                    if (value < Count)
                        throw new ArgumentOutOfRangeException(nameof(value),
                            $"{nameof(Capacity)} cannot be set lower than {nameof(Count)}");

                    if (value == _Events.Length)
                        return;

                    if (value > 0)
                    {
                        var newEvents = new AnimancerEvent[value];
                        if (Count > 0)
                            Array.Copy(_Events, 0, newEvents, 0, Count);
                        _Events = newEvents;
                    }
                    else
                    {
                        _Events = Array.Empty<AnimancerEvent>();
                    }
                }
            }

            /************************************************************************************************************************/
            #region Modification Detection
            /************************************************************************************************************************/

            private int _Version;

            /// <summary>[Pro-Only]
            /// The number of times the contents of this sequence have been modified. This applies to general events,
            /// but not the <see cref="EndEvent"/>.
            /// </summary>
            public int Version
            {
                get => _Version;
                private set
                {
                    _Version = value;
                    OnSequenceModified();
                }
            }

            /************************************************************************************************************************/

#if UNITY_ASSERTIONS
            /// <summary>[Assert-Only]
            /// If this property is set, any attempt to modify this sequence will trigger
            /// <see cref="OptionalWarning.LockedEvents"/> (which will include this value in its message).
            /// </summary>
            /// <remarks>This value can be set by <see cref="SetShouldNotModifyReason"/>.</remarks>
            public string ShouldNotModifyReason { get; private set; }
#endif

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Sets the <see cref="ShouldNotModifyReason"/> for <see cref="OptionalWarning.LockedEvents"/>.
            /// </summary>
            /// <remarks>
            /// If the warning is triggered, the message is formatted as:
            /// "The <see cref="Sequence"/> being modified should not be modified because " + 
            /// <see cref="ShouldNotModifyReason"/>.
            /// </remarks>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void SetShouldNotModifyReason(string reason)
            {
#if UNITY_ASSERTIONS
                ShouldNotModifyReason = reason;
#endif
            }

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional] Logs <see cref="OptionalWarning.LockedEvents"/> if necessary.</summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void OnSequenceModified()
            {
#if UNITY_ASSERTIONS
                if (string.IsNullOrEmpty(ShouldNotModifyReason) ||
                    OptionalWarning.LockedEvents.IsDisabled())
                    return;

                OptionalWarning.LockedEvents.Log(
                    $"The {nameof(AnimancerEvent)}.{nameof(Sequence)} being modified should not be modified because " +
                    ShouldNotModifyReason +
                    $"\n\nThis warning can be prevented by calling state.Events.{nameof(SetShouldNotModifyReason)}(null);" +
                    $" before making any modifications.");

                // Clear the reason so it doesn't trigger this warning again. No point in wasting performance.
                ShouldNotModifyReason = null;
#endif
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region End Event
            /************************************************************************************************************************/

            private AnimancerEvent _EndEvent = new AnimancerEvent(float.NaN, null);

            /// <summary>
            /// A <see cref="callback "/> that will be triggered every frame after the <see cref="normalizedTime"/> has
            /// passed. If you want it to only get triggered once, you can either have the event clear itself or just
            /// use a regular event instead.
            /// </summary>
            ///
            /// <example><code>
            /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
            /// {
            ///     var state = animancer.Play(clip);
            ///     state.Events.NormalizedEndTime = 0.75f;
            ///     state.Events.OnEnd = OnAnimationEnd;
            ///
            ///     // Or set the time and callback at the same time:
            ///     state.Events.EndEvent = new AnimancerEvent(0.75f, OnAnimationEnd);
            /// }
            ///
            /// void OnAnimationEnd()
            /// {
            ///     Debug.Log("Animation ended");
            /// }
            /// </code></example>
            ///
            /// <remarks>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">End Events</see>
            /// <para></para>
            /// Interrupting the animation does not trigger this event.
            /// <para></para>
            /// By default, the <see cref="normalizedTime"/> will be <see cref="float.NaN"/> so that it can choose the
            /// correct value based on the current play direction: forwards ends at 1 and backwards ends at 0.
            /// <para></para>
            /// <em>Animancer Lite does not allow the <see cref="normalizedTime"/> to be changed in Runtime Builds.</em>
            /// </remarks>
            /// 
            /// <seealso cref="OnEnd"/>
            /// <seealso cref="NormalizedEndTime"/>
            public AnimancerEvent EndEvent
            {
                get => _EndEvent;
                set
                {
                    _EndEvent = value;
                    OnSequenceModified();
                }
            }

            /************************************************************************************************************************/

            /// <summary>Shorthand for the <c>EndEvent.callback</c>.</summary>
            /// <seealso cref="EndEvent"/>
            /// <seealso cref="NormalizedEndTime"/>
            public Action OnEnd
            {
                get => _EndEvent.callback;
                set
                {
                    _EndEvent.callback = value;
                    OnSequenceModified();
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Shorthand for <c>EndEvent.normalizedTime</c>.</summary>
            /// <remarks>
            /// This value is <see cref="float.NaN"/> by default so that the actual time can be determined based on the
            /// <see cref="AnimancerNode.EffectiveSpeed"/>: positive speed ends at 1 and negative speed ends at 0.
            /// <para></para>
            /// Use <see cref="AnimancerState.NormalizedEndTime"/> to access that value.
            /// </remarks>
            /// <seealso cref="EndEvent"/>
            /// <seealso cref="OnEnd"/>
            public float NormalizedEndTime
            {
                get => _EndEvent.normalizedTime;
                set
                {
                    _EndEvent.normalizedTime = value;
                    OnSequenceModified();
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// The default <see cref="AnimancerState.NormalizedTime"/> for an animation to start at when playing
            /// forwards is 0 (the start of the animation) and when playing backwards is 1 (the end of the animation).
            /// <para></para>
            /// `speed` 0 or <see cref="float.NaN"/> will also return 0.
            /// </summary>
            /// <remarks>
            /// This method has nothing to do with events, so it is only here because of
            /// <see cref="GetDefaultNormalizedEndTime"/>.
            /// </remarks>
            public static float GetDefaultNormalizedStartTime(float speed) => speed < 0 ? 1 : 0;

            /// <summary>
            /// The default <see cref="normalizedTime"/> for an <see cref="EndEvent"/> when playing forwards is 1 (the
            /// end of the animation) and when playing backwards is 0 (the start of the animation).
            /// <para></para>
            /// `speed` 0 or <see cref="float.NaN"/> will also return 1.
            /// </summary>
            public static float GetDefaultNormalizedEndTime(float speed) => speed < 0 ? 0 : 1;

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Names
            /************************************************************************************************************************/

            private string[] _Names;

            /// <summary>[Pro-Only] The names of the events (excluding the <see cref="EndEvent"/>).</summary>
            /// <remarks>This array can be <c>null</c>.</remarks>
            public ref string[] Names => ref _Names;

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Returns the name of the event at the specified `index` or <c>null</c> if it is outside of the
            /// <see cref="Names"/> array.
            /// </summary>
            public string GetName(int index)
            {
                if (_Names == null ||
                    _Names.Length <= index)
                    return null;
                else
                    return _Names[index];
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Sets the name of the event at the specified `index`. If the <see cref="Names"/> did not previously
            /// include that `index` it will be resized with a size equal to the <see cref="Count"/>.
            /// </summary>
            public void SetName(int index, string name)
            {
                AnimancerUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);

                // Capacity can't be 0 at this point.

                if (_Names == null)
                {
                    _Names = new string[Capacity];
                }
                else if (_Names.Length <= index)
                {
                    var names = new string[Capacity];
                    Array.Copy(_Names, names, _Names.Length);
                    _Names = names;
                }

                _Names[index] = name;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Returns the index of the event with the specified `name` or <c>-1</c> if there is no such event.
            /// </summary>
            /// <seealso cref="Names"/>
            /// <seealso cref="GetName"/>
            /// <seealso cref="SetName"/>
            /// <seealso cref="IndexOfRequired(string, int)"/>
            public int IndexOf(string name, int startIndex = 0)
            {
                if (_Names == null)
                    return -1;

                var count = Mathf.Min(Count, _Names.Length);
                for (; startIndex < count; startIndex++)
                    if (_Names[startIndex] == name)
                        return startIndex;

                return -1;
            }

            /// <summary>[Pro-Only] Returns the index of the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no such event.</exception>
            /// <seealso cref="IndexOf(string, int)"/>
            public int IndexOfRequired(string name, int startIndex = 0)
            {
                startIndex = IndexOf(name, startIndex);
                if (startIndex >= 0)
                    return startIndex;

                throw new ArgumentException($"No event exists with the name '{name}'.");
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Constructors
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Sequence"/> which starts at 0 <see cref="Capacity"/>.
            /// <para></para>
            /// Adding anything to the sequence will set the <see cref="Capacity"/> = <see cref="DefaultCapacity"/>
            /// and then double it whenever the <see cref="Count"/> would exceed the <see cref="Capacity"/>.
            /// </summary>
            public Sequence()
            {
                _Events = Array.Empty<AnimancerEvent>();
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Creates a new <see cref="Sequence"/> which starts with the specified <see cref="Capacity"/>. It will be
            /// initially empty, but will have room for the given number of elements before any reallocations are
            /// required.
            /// </summary>
            public Sequence(int capacity)
            {
                _Events = capacity > 0 ? new AnimancerEvent[capacity] : Array.Empty<AnimancerEvent>();
            }

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Sequence"/> and copies the contents of `copyFrom` into it.</summary>
            /// <remarks>To copy into an existing sequence, use <see cref="CopyFrom"/> instead.</remarks>
            public Sequence(Sequence copyFrom)
            {
                _Events = Array.Empty<AnimancerEvent>();
                if (copyFrom != null)
                    CopyFrom(copyFrom);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Iteration
            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Returns the event at the specified `index`.</summary>
            public AnimancerEvent this[int index]
            {
                get
                {
                    AnimancerUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                    return _Events[index];
                }
            }

            /// <summary>[Pro-Only] Returns the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            public AnimancerEvent this[string name] => this[IndexOfRequired(name)];

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Throws an <see cref="ArgumentOutOfRangeException"/> if the <see cref="normalizedTime"/> of any events
            /// is less than 0 or greater than or equal to 1.
            /// <para></para>
            /// This does not include the <see cref="EndEvent"/> since it works differently to other events.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(AnimancerState state)
            {
                if (Count == 0 ||
                    (_Events[0].normalizedTime >= 0 && _Events[Count - 1].normalizedTime < 1))
                    return;

                throw new ArgumentOutOfRangeException(nameof(normalizedTime),
                    "Events on looping animations are triggered every loop and must be" +
                    $" within the range of 0 <= {nameof(normalizedTime)} < 1.\n{state}\n{DeepToString()}");
            }

            /// <summary>[Assert-Conditional]
            /// Calls <see cref="AssertNormalizedTimes(AnimancerState)"/> if `isLooping` is true.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(AnimancerState state, bool isLooping)
            {
                if (isLooping)
                    AssertNormalizedTimes(state);
            }

            /************************************************************************************************************************/

            /// <summary>Returns a string containing the details of all events in this sequence.</summary>
            public string DeepToString(bool multiLine = true)
            {
                var text = ObjectPool.AcquireStringBuilder()
                    .Append(ToString())
                    .Append('[')
                    .Append(Count)
                    .Append(']');

                text.Append(multiLine ? "\n{" : " {");

                for (int i = 0; i < Count; i++)
                {
                    if (multiLine)
                        text.Append("\n   ");
                    else if (i > 0)
                        text.Append(',');

                    text.Append(" [");

                    text.Append(i)
                        .Append("] ");

                    this[i].AppendDetails(text);

                    var name = GetName(i);
                    if (name != null)
                    {
                        text.Append(", Name: '")
                            .Append(name)
                            .Append('\'');
                    }
                }

                if (multiLine)
                {
                    text.Append("\n    [End] ");
                }
                else
                {
                    if (Count > 0)
                        text.Append(',');
                    text.Append(" [End] ");
                }
                _EndEvent.AppendDetails(text);

                if (multiLine)
                    text.Append("\n}\n");
                else
                    text.Append(" }");

                return text.ReleaseToString();
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Returns a <see cref="FastEnumerator{T}"/> for the events in this sequence excluding the
            /// <see cref="EndEvent"/>.
            /// </summary>
            public FastEnumerator<AnimancerEvent> GetEnumerator()
                => new FastEnumerator<AnimancerEvent>(_Events, Count);

            IEnumerator<AnimancerEvent> IEnumerable<AnimancerEvent>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent` or <c>-1</c> if there is no such event.</summary>
            /// <seealso cref="IndexOfRequired(int, AnimancerEvent)"/>
            public int IndexOf(AnimancerEvent animancerEvent) => IndexOf(Count / 2, animancerEvent);

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent`.</summary>
            /// <exception cref="ArgumentException">There is no such event.</exception>
            /// <seealso cref="IndexOf(AnimancerEvent)"/>
            public int IndexOfRequired(AnimancerEvent animancerEvent) => IndexOfRequired(Count / 2, animancerEvent);

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent` or <c>-1</c> if there is no such event.</summary>
            /// <seealso cref="IndexOfRequired(int, AnimancerEvent)"/>
            public int IndexOf(int indexHint, AnimancerEvent animancerEvent)
            {
                if (Count == 0)
                    return -1;

                if (indexHint >= Count)
                    indexHint = Count - 1;

                var otherEvent = _Events[indexHint];
                if (otherEvent == animancerEvent)
                    return indexHint;

                if (otherEvent.normalizedTime > animancerEvent.normalizedTime)
                {
                    while (--indexHint >= 0)
                    {
                        otherEvent = _Events[indexHint];
                        if (otherEvent.normalizedTime < animancerEvent.normalizedTime)
                            return -1;
                        else if (otherEvent.normalizedTime == animancerEvent.normalizedTime)
                            if (otherEvent.callback == animancerEvent.callback)
                                return indexHint;
                    }
                }
                else
                {
                    while (otherEvent.normalizedTime == animancerEvent.normalizedTime)
                    {
                        indexHint--;
                        if (indexHint < 0)
                            break;

                        otherEvent = _Events[indexHint];
                    }

                    while (++indexHint < Count)
                    {
                        otherEvent = _Events[indexHint];
                        if (otherEvent.normalizedTime > animancerEvent.normalizedTime)
                            return -1;
                        else if (otherEvent.normalizedTime == animancerEvent.normalizedTime)
                            if (otherEvent.callback == animancerEvent.callback)
                                return indexHint;
                    }
                }

                return -1;
            }

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent`.</summary>
            /// <exception cref="ArgumentException">There is no such event.</exception>
            /// <seealso cref="IndexOf(int, AnimancerEvent)"/>
            public int IndexOfRequired(int indexHint, AnimancerEvent animancerEvent)
            {
                indexHint = IndexOf(indexHint, animancerEvent);
                if (indexHint >= 0)
                    return indexHint;

                throw new ArgumentException($"Event not found in {nameof(Sequence)} '{animancerEvent}'.");
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Modification
            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <exception cref="ArgumentNullException">Use the <see cref="DummyCallback"/> instead of <c>null</c>.</exception>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(AnimancerEvent animancerEvent)
            {
#if UNITY_ASSERTIONS
                if (animancerEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(AnimancerEvent)}.{nameof(callback)}", NullCallbackError);

#endif
                var index = Insert(animancerEvent.normalizedTime);
                AssertEventUniqueness(index, animancerEvent);
                _Events[index] = animancerEvent;
                return index;
            }

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(float normalizedTime, Action callback)
                => Add(new AnimancerEvent(normalizedTime, callback));

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <exception cref="ArgumentNullException">Use the <see cref="DummyCallback"/> instead of <c>null</c>.</exception>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(int indexHint, AnimancerEvent animancerEvent)
            {
#if UNITY_ASSERTIONS
                if (animancerEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(AnimancerEvent)}.{nameof(callback)}", NullCallbackError);

#endif
                indexHint = Insert(indexHint, animancerEvent.normalizedTime);
                AssertEventUniqueness(indexHint, animancerEvent);
                _Events[indexHint] = animancerEvent;
                return indexHint;
            }

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(int indexHint, float normalizedTime, Action callback)
                => Add(indexHint, new AnimancerEvent(normalizedTime, callback));

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Adds every event in the `enumerable` to this sequence. The <see cref="Count"/> is increased by one and if
            /// required, the <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void AddRange(IEnumerable<AnimancerEvent> enumerable)
            {
                foreach (var item in enumerable)
                    Add(item);
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Adds the specified `callback` to the event at the specified `index`.</summary>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void AddCallback(int index, Action callback)
            {
                ref var animancerEvent = ref _Events[index];
                AssertCallbackUniqueness(animancerEvent.callback, callback, $"{nameof(callback)} being added");
                animancerEvent.callback += callback;
                Version++;
            }

            /// <summary>[Pro-Only] Adds the specified `callback` to the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired(string, int)"/>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void AddCallback(string name, Action callback) => AddCallback(IndexOfRequired(name), callback);

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Removes the specified `callback` from the event at the specified `index`.</summary>
            /// <remarks>
            /// If the <see cref="callback"/> would become null, it is instead set to the <see cref="DummyCallback"/>
            /// since they are not allowed to be null.
            /// </remarks>
            public void RemoveCallback(int index, Action callback)
            {
                ref var animancerEvent = ref _Events[index];
                animancerEvent.callback -= callback;
                if (animancerEvent.callback == null)
                    animancerEvent.callback = DummyCallback;
                Version++;
            }

            /// <summary>[Pro-Only] Removes the specified `callback` from the event with the specified `name`.</summary>
            /// <remarks>
            /// If the <see cref="callback"/> would become null, it is instead set to the <see cref="DummyCallback"/>
            /// since they are not allowed to be null.
            /// </remarks>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired(string, int)"/>
            public void RemoveCallback(string name, Action callback) => RemoveCallback(IndexOfRequired(name), callback);

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Replaces the <see cref="callback"/> of the event at the specified `index`.</summary>
            /// <exception cref="ArgumentNullException">Use the <see cref="DummyCallback"/> instead of <c>null</c>.</exception>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void SetCallback(int index, Action callback)
            {
#if UNITY_ASSERTIONS
                if (callback == null)
                    throw new ArgumentNullException(nameof(callback), NullCallbackError);
#endif

                ref var animancerEvent = ref _Events[index];
                AssertCallbackUniqueness(animancerEvent.callback, callback, $"{nameof(callback)} being assigned");
                animancerEvent.callback = callback;
                Version++;
            }

            /// <summary>[Pro-Only] Replaces the <see cref="callback"/> of the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired(string, int)"/>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void SetCallback(string name, Action callback) => SetCallback(IndexOfRequired(name), callback);

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Logs <see cref="OptionalWarning.DuplicateEvent"/> if the `oldCallback` is identical to the
            /// `newCallback` or just has the same <see cref="Delegate.Method"/>.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private static void AssertCallbackUniqueness(Action oldCallback, Action newCallback, string target)
            {
#if UNITY_ASSERTIONS
                if (oldCallback == DummyCallback ||
                    OptionalWarning.DuplicateEvent.IsDisabled())
                    return;

                if (oldCallback == newCallback)
                {
                    OptionalWarning.DuplicateEvent.Log($"The {target}" +
                        " is identical to an existing event in the sequence" +
                        " which may mean that it is being unintentionally added multiple times." +
                        $" If the {nameof(AnimancerEvent)}.{nameof(Sequence)} is owned by a Transition then it will not" +
                        " be cleared each time it is played so the events should only be initialized once on startup." +
                        $" See the documentation for more information: {Strings.DocsURLs.ClearAutomatically}");
                }
                else if (oldCallback?.Method == newCallback?.Method)
                {
                    OptionalWarning.DuplicateEvent.Log($"The {target}" +
                        " is identical to an existing event in the sequence except for the target object." +
                        " This often happens when a Transition is shared by multiple objects," +
                        " in which case it can be avoided by giving each object its own" +
                        $" {nameof(AnimancerEvent)}.{nameof(Sequence)} as explained in the documentation:" +
                        $" {Strings.DocsURLs.SharedEventSequences}");
                }
#endif
            }

            /// <summary>[Assert-Conditional]
            /// Logs <see cref="OptionalWarning.DuplicateEvent"/> if the event at the specified `index` is identical to
            /// the `newEvent`.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private void AssertEventUniqueness(int index, AnimancerEvent newEvent)
            {
#if UNITY_ASSERTIONS
                if (OptionalWarning.DuplicateEvent.IsDisabled() || index == 0)
                    return;

                var previousEvent = _Events[index - 1];
                if (previousEvent.normalizedTime != newEvent.normalizedTime)
                    return;

                AssertCallbackUniqueness(previousEvent.callback, newEvent.callback, $"{nameof(AnimancerEvent)} being added");
#endif
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Sets the <see cref="normalizedTime"/> of the event at the specified `index`.</summary>
            /// <remarks>
            /// If multiple events have the same <see cref="normalizedTime"/>, this method will avoid re-arranging them
            /// where calling <see cref="Remove(int)"/> then <see cref="Add(AnimancerEvent)"/> would always re-add the
            /// moved event as the last one with that time.
            /// </remarks>
            public int SetNormalizedTime(int index, float normalizedTime)
            {
#if UNITY_ASSERTIONS
                if (!normalizedTime.IsFinite())
                    throw new ArgumentOutOfRangeException(nameof(normalizedTime), normalizedTime,
                        $"{nameof(normalizedTime)} {Strings.MustBeFinite}");
#endif

                var animancerEvent = _Events[index];
                if (animancerEvent.normalizedTime == normalizedTime)
                    return index;

                var moveTo = index;
                if (animancerEvent.normalizedTime < normalizedTime)
                {
                    while (moveTo < Count - 1)
                    {
                        if (_Events[moveTo + 1].normalizedTime >= normalizedTime)
                            break;
                        else
                            moveTo++;
                    }
                }
                else
                {
                    while (moveTo > 0)
                    {
                        if (_Events[moveTo - 1].normalizedTime <= normalizedTime)
                            break;
                        else
                            moveTo--;
                    }
                }

                if (index != moveTo)
                {
                    var name = GetName(index);
                    Remove(index);

                    index = moveTo;

                    Insert(index);
                    if (!string.IsNullOrEmpty(name))
                        SetName(index, name);
                }

                animancerEvent.normalizedTime = normalizedTime;
                _Events[index] = animancerEvent;

                Version++;

                return index;
            }

            /// <summary>[Pro-Only] Sets the <see cref="normalizedTime"/> of the event with the specified `name`.</summary>
            /// <remarks>
            /// If multiple events have the same <see cref="normalizedTime"/>, this method will avoid re-arranging them
            /// where calling <see cref="Remove(int)"/> then <see cref="Add(AnimancerEvent)"/> would always re-add the
            /// moved event as the last one with that time.
            /// </remarks>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired(string, int)"/>
            public int SetNormalizedTime(string name, float normalizedTime)
            => SetNormalizedTime(IndexOfRequired(name), normalizedTime);

            /// <summary>[Pro-Only] Sets the <see cref="normalizedTime"/> of the matching `animancerEvent`.</summary>
            /// <remarks>
            /// If multiple events have the same <see cref="normalizedTime"/>, this method will avoid re-arranging them
            /// where calling <see cref="Remove(int)"/> then <see cref="Add(AnimancerEvent)"/> would always re-add the
            /// moved event as the last one with that time.
            /// </remarks>
            /// <exception cref="ArgumentException">There is no event matching the `animancerEvent`.</exception>
            /// <seealso cref="IndexOfRequired(AnimancerEvent)"/>
            public int SetNormalizedTime(AnimancerEvent animancerEvent, float normalizedTime)
                => SetNormalizedTime(IndexOfRequired(animancerEvent), normalizedTime);

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Determines the index where a new event with the specified `normalizedTime` should be added in order to
            /// keep this sequence sorted, increases the <see cref="Count"/> by one, doubles the <see cref="Capacity"/>
            /// if required, moves any existing events to open up the chosen index, and returns that index.
            /// <para></para>
            /// This overload starts searching for the desired index from the end of the sequence, using the assumption
            /// that elements will usually be added in order.
            /// </summary>
            private int Insert(float normalizedTime)
            {
                var index = Count;
                while (index > 0 && _Events[index - 1].normalizedTime > normalizedTime)
                    index--;
                Insert(index);
                return index;
            }

            /// <summary>[Pro-Only]
            /// Determines the index where a new event with the specified `normalizedTime` should be added in order to
            /// keep this sequence sorted, increases the <see cref="Count"/> by one, doubles the <see cref="Capacity"/>
            /// if required, moves any existing events to open up the chosen index, and returns that index.
            /// <para></para>
            /// This overload starts searching for the desired index from the `hint`.
            /// </summary>
            private int Insert(int indexHint, float normalizedTime)
            {
                if (Count == 0)
                {
                    Count = 0;
                }
                else
                {
                    if (indexHint >= Count)
                        indexHint = Count - 1;

                    if (_Events[indexHint].normalizedTime > normalizedTime)
                    {
                        while (indexHint > 0 && _Events[indexHint - 1].normalizedTime > normalizedTime)
                            indexHint--;
                    }
                    else
                    {
                        while (indexHint < Count && _Events[indexHint].normalizedTime <= normalizedTime)
                            indexHint++;
                    }
                }

                Insert(indexHint);
                return indexHint;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Increases the <see cref="Count"/> by one, doubles the <see cref="Capacity"/> if required, and moves any
            /// existing events to open up the `index`.
            /// </summary>
            private void Insert(int index)
            {
                AnimancerUtilities.Assert((uint)index <= (uint)Count, IndexOutOfRangeError);

                var capacity = _Events.Length;
                if (Count == capacity)
                {
                    if (capacity == 0)
                    {
                        capacity = DefaultCapacity;
                        _Events = new AnimancerEvent[DefaultCapacity];
                    }
                    else
                    {
                        capacity *= 2;
                        if (capacity < DefaultCapacity)
                            capacity = DefaultCapacity;

                        var events = new AnimancerEvent[capacity];

                        Array.Copy(_Events, 0, events, 0, index);
                        if (Count > index)
                            Array.Copy(_Events, index, events, index + 1, Count - index);

                        _Events = events;
                    }
                }
                else if (Count > index)
                {
                    Array.Copy(_Events, index, _Events, index + 1, Count - index);
                }

                if (_Names != null)
                {
                    if (_Names.Length < capacity)
                    {
                        var names = new string[capacity];

                        Array.Copy(_Names, 0, names, 0, Math.Min(_Names.Length, index));
                        if (index <= Count &&
                            index < _Names.Length)
                            Array.Copy(_Names, index, names, index + 1, Count - index);

                        _Names = names;
                    }
                    else
                    {
                        if (Count > index)
                            Array.Copy(_Names, index, _Names, index + 1, Count - index);

                        _Names[index] = null;
                    }
                }

                Count++;
                Version++;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Removes the event at the specified `index` from this sequence by decrementing the <see cref="Count"/>
            /// and copying all events after the removed one down one place.
            /// </summary>
            public void Remove(int index)
            {
                AnimancerUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                Count--;
                if (index < Count)
                {
                    Array.Copy(_Events, index + 1, _Events, index, Count - index);

                    if (_Names != null)
                    {
                        var nameCount = Mathf.Min(Count + 1, _Names.Length);
                        if (index + 1 < nameCount)
                            Array.Copy(_Names, index + 1, _Names, index, nameCount - index - 1);

                        _Names[nameCount - 1] = default;
                    }
                }
                else if (_Names != null && index < _Names.Length)
                {
                    _Names[index] = default;
                }

                _Events[Count] = default;
                Version++;
            }

            /// <summary>[Pro-Only]
            /// Removes the event with the specified `name` from this sequence by decrementing the <see cref="Count"/>
            /// and copying all events after the removed one down one place. Returns true if the event was found and
            /// removed.
            /// </summary>
            public bool Remove(string name)
            {
                var index = IndexOf(name);
                if (index >= 0)
                {
                    Remove(index);
                    return true;
                }
                else return false;
            }

            /// <summary>[Pro-Only]
            /// Removes the `animancerEvent` from this sequence by decrementing the <see cref="Count"/> and copying all
            /// events after the removed one down one place. Returns true if the event was found and removed.
            /// </summary>
            public bool Remove(AnimancerEvent animancerEvent)
            {
                var index = IndexOf(animancerEvent);
                if (index >= 0)
                {
                    Remove(index);
                    return true;
                }
                else return false;
            }

            /************************************************************************************************************************/

            /// <summary>Removes all events, including the <see cref="EndEvent"/>.</summary>
            public void Clear()
            {
                if (_Names != null)
                    Array.Clear(_Names, 0, _Names.Length);

                Array.Clear(_Events, 0, Count);
                Count = 0;
                Version++;

                _EndEvent = new AnimancerEvent(float.NaN, null);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Copying
            /************************************************************************************************************************/

            /// <inheritdoc/>
            public void CopyFrom(Sequence copyFrom)
            {
                if (copyFrom == null)
                {
                    if (_Names != null)
                        Array.Clear(_Names, 0, _Names.Length);

                    Array.Clear(_Events, 0, Count);
                    Count = 0;
                    Capacity = 0;
                    _EndEvent = default;
                    return;
                }

                AnimancerUtilities.CopyExactArray(copyFrom._Names, ref _Names);

                var sourceCount = copyFrom.Count;

                if (Count > sourceCount)
                    Array.Clear(_Events, Count, sourceCount - Count);
                else if (_Events.Length < sourceCount)
                    Capacity = sourceCount;

                Count = sourceCount;

                Array.Copy(copyFrom._Events, 0, _Events, 0, sourceCount);

                _EndEvent = copyFrom._EndEvent;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Copies the <see cref="AnimationClip.events"/> into this <see cref="Sequence"/>.</summary>
            /// <remarks>
            /// The <see cref="callback"/> of the new events will be empty and can be set by
            /// <see cref="SetCallback(string, Action)"/>.
            /// <para></para>
            /// If you are going to play the `animation`, consider disabling <see cref="Animator.fireEvents"/> so that
            /// the events copied by this method are not triggered as <see cref="AnimationEvent"/>s. Otherwise they
            /// would still trigger in addition to the <see cref="AnimancerEvent"/>s copied here.
            /// </remarks>
            public void AddAllEvents(AnimationClip animation)
            {
                if (animation == null)
                    return;

                var length = animation.length;

                var animationEvents = animation.events;
                Capacity += animationEvents.Length;

                var index = -1;
                for (int i = 0; i < animationEvents.Length; i++)
                {
                    var animationEvent = animationEvents[i];
                    index = Add(index + 1, new AnimancerEvent(animationEvent.time / length, DummyCallback));
                    SetName(index, animationEvent.functionName);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="ICollection{T}"/>] [Pro-Only]
            /// Copies all the events from this sequence into the `array`, starting at the `index`.
            /// </summary>
            public void CopyTo(AnimancerEvent[] array, int index)
            {
                Array.Copy(_Events, 0, array, index, Count);
            }

            /************************************************************************************************************************/

            /// <summary>Are all events in this sequence identical to the ones in the `other` sequence?</summary>
            public bool ContentsAreEqual(Sequence other)
            {
                if (_EndEvent != other._EndEvent)
                    return false;

                if (Count != other.Count)
                    return false;

                for (int i = Count - 1; i >= 0; i--)
                    if (this[i] != other[i])
                        return false;

                return true;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

