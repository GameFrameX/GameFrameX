// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws a GUI box denoting a period of time.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TimelineGUI
    /// 
    public class TimelineGUI : IDisposable
    {
        /************************************************************************************************************************/
        #region Fields
        /************************************************************************************************************************/

        private static readonly ConversionCache<float, string>
            G2Cache = new ConversionCache<float, string>((value) =>
            {
                if (Math.Abs(value) <= 99)
                    return value.ToString("G2");
                else
                    return ((int)value).ToString();
            });

        private static Texture _EventIcon;

        /// <summary>The icon used for events.</summary>
        public static Texture EventIcon => _EventIcon != null ?
            _EventIcon :
            (_EventIcon = AnimancerGUI.LoadIcon("Animation.EventMarker"));

        private static readonly Color
            FadeHighlightColor = new Color(0.35f, 0.5f, 1, 0.5f),
            SelectedEventColor = new Color(0.3f, 0.55f, 0.95f),
            UnselectedEventColor = new Color(0.9f, 0.9f, 0.9f),
            PreviewTimeColor = new Color(1, 0.25f, 0.1f),
            BaseTimeColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);

        private Rect _Area;

        /// <summary>The pixel area in which this <see cref="TimelineGUI"/> is drawing.</summary>
        public Rect Area => _Area;

        private float _Speed, _Duration, _MinTime, _MaxTime, _StartTime, _FadeInEnd, _FadeOutEnd, _EndTime, _SecondsToPixels;
        private bool _HasEndTime;

        private readonly List<float>
            EventTimes = new List<float>();

        /// <summary>The height of the time ticks.</summary>
        public float TickHeight { get; private set; }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Conversions
        /************************************************************************************************************************/

        /// <summary>Converts a number of seconds to a horizontal pixel position along the ruler.</summary>
        /// <remarks>The value is rounded to the nearest integer.</remarks>
        public float SecondsToPixels(float seconds) => AnimancerUtilities.Round((seconds - _MinTime) * _SecondsToPixels);

        /// <summary>Converts a horizontal pixel position along the ruler to a number of seconds.</summary>
        public float PixelsToSeconds(float pixels) => (pixels / _SecondsToPixels) + _MinTime;

        /// <summary>Converts a number of seconds to a normalized time value.</summary>
        public float SecondsToNormalized(float seconds) => seconds / _Duration;

        /// <summary>Converts a normalized time value to a number of seconds.</summary>
        public float NormalizedToSeconds(float normalized) => normalized * _Duration;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        private TimelineGUI() { }
        private static readonly TimelineGUI Instance = new TimelineGUI();

        /// <summary>The currently drawing <see cref="TimelineGUI"/> (or null if none is drawing).</summary>
        public static TimelineGUI Current { get; private set; }

        /// <summary>Ends the area started by <see cref="BeginGUI"/>.</summary>
        void IDisposable.Dispose()
        {
            Current = null;
            GUI.EndClip();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the `area` in which the ruler will be drawn and draws a <see cref="GUI.Box(Rect, string)"/> there.
        /// The returned object must have <see cref="IDisposable.Dispose"/> called on it afterwards.
        /// </summary>
        private static IDisposable BeginGUI(Rect area)
        {
            if (Current != null)
                throw new InvalidOperationException($"{nameof(TimelineGUI)} can't be used recursively.");

            if (!EditorGUIUtility.hierarchyMode)
                EditorGUI.indentLevel++;

            area = EditorGUI.IndentedRect(area);

            if (!EditorGUIUtility.hierarchyMode)
                EditorGUI.indentLevel--;

            GUI.Box(area, "");

            GUI.BeginClip(area);

            area.x = area.y = 0;
            Instance._Area = area;

            Instance.TickHeight = Mathf.Ceil(area.height * 0.3f);

            return Current = Instance;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the ruler GUI and handles input events for the specified `context`.</summary>
        public static void DoGUI(Rect area, SerializableEventSequenceDrawer.Context context, out float addEventNormalizedTime)
        {
            using (BeginGUI(area))
                Current.DoGUI(context, out addEventNormalizedTime);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the ruler GUI and handles input events for the specified `context`.</summary>
        private void DoGUI(SerializableEventSequenceDrawer.Context context, out float addEventNormalizedTime)
        {
            if (context.Property.hasMultipleDifferentValues)
            {
                GUI.Label(_Area, "Multi-editing events is not supported");
                addEventNormalizedTime = float.NaN;
                return;
            }

            var warnings = OptionalWarning.LockedEvents.DisableTemporarily();

            var transition = context.TransitionContext.Transition;

            _Speed = transition.Speed;
            if (float.IsNaN(_Speed))
                _Speed = 1;

            _Duration = context.TransitionContext.MaximumDuration;
            if (_Duration <= 0)
                _Duration = 1;

            GatherEventTimes(context);

            _StartTime = GetStartTime(transition.NormalizedStartTime, _Speed, _Duration);

            _FadeInEnd = _StartTime + transition.FadeDuration * _Speed;

            _FadeOutEnd = GetFadeOutEnd(_Speed, _EndTime, _Duration);

            _MinTime = Mathf.Min(0, _StartTime);
            _MinTime = Mathf.Min(_MinTime, _FadeOutEnd);
            _MinTime = Mathf.Min(_MinTime, EventTimes[0]);

            _MaxTime = Mathf.Max(_StartTime, _FadeOutEnd);
            if (EventTimes.Count >= 2)
                _MaxTime = Mathf.Max(_MaxTime, EventTimes[EventTimes.Count - 2]);

            if (_MaxTime < _Duration)
                _MaxTime = _Duration;

            _SecondsToPixels = _Area.width / (_MaxTime - _MinTime);

            DoFadeHighlightGUI();

            if (AnimancerUtilities.TryGetWrappedObject(transition, out ITransitionGUI gui))
                gui.OnTimelineBackgroundGUI();

            DoEventsGUI(context, out addEventNormalizedTime);
            DoRulerGUI();

            if (_Speed > 0)
            {
                if (_StartTime >= _EndTime)
                    GUI.Label(_Area, "Start Time is not before End Time");
            }
            else if (_Speed < 0)
            {
                if (_StartTime <= _EndTime)
                    GUI.Label(_Area, "Start Time is not after End Time");
            }

            if (gui != null)
                gui.OnTimelineForegroundGUI();

            warnings.Enable();
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the start time of the transition (in seconds).</summary>
        public static float GetStartTime(float normalizedStartTime, float speed, float duration)
        {
            if (float.IsNaN(normalizedStartTime))
            {
                return speed < 0 ? duration : 0;
            }
            else
            {
                return normalizedStartTime * duration;
            }
        }

        /// <summary>Calculates the end time of the fade out (in seconds).</summary>
        public static float GetFadeOutEnd(float speed, float endTime, float duration)
        {
            if (speed < 0)
                return endTime > 0 ? 0 : (endTime - AnimancerPlayable.DefaultFadeDuration) * -speed;
            else
                return endTime < duration ? duration : endTime + AnimancerPlayable.DefaultFadeDuration * speed;
        }

        /************************************************************************************************************************/

        private static readonly Vector3[] QuadVertices = new Vector3[4];

        /// <summary>Draws a polygon describing the start, end, and fade details.</summary>
        private void DoFadeHighlightGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var color = Handles.color;
            Handles.color = FadeHighlightColor;
            QuadVertices[0] = new Vector3(SecondsToPixels(_StartTime), _Area.y);
            QuadVertices[1] = new Vector3(SecondsToPixels(_FadeInEnd), _Area.yMax + 1);
            QuadVertices[2] = new Vector3(SecondsToPixels(_FadeOutEnd), _Area.yMax + 1);
            QuadVertices[3] = new Vector3(SecondsToPixels(_EndTime), _Area.y);
            Handles.DrawAAConvexPolygon(QuadVertices);
            Handles.color = color;
        }

        /************************************************************************************************************************/
        #region Events
        /************************************************************************************************************************/

        private void GatherEventTimes(SerializableEventSequenceDrawer.Context context)
        {
            EventTimes.Clear();

            if (context.Times.Count > 0)
            {
                var depth = context.Times.Property.depth;
                var time = context.Times.GetElement(0);

                while (time.depth > depth)
                {
                    EventTimes.Add(time.floatValue * _Duration);
                    time.Next(false);
                }

                _EndTime = EventTimes[EventTimes.Count - 1];
                if (!float.IsNaN(_EndTime))
                {
                    _HasEndTime = true;
                    return;
                }
            }

            _EndTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(_Speed) * _Duration;
            _HasEndTime = false;
            if (EventTimes.Count == 0)
                EventTimes.Add(_EndTime);
            else
                EventTimes[EventTimes.Count - 1] = _EndTime;
        }

        /************************************************************************************************************************/

        private static readonly int EventHash = "Event".GetHashCode();
        private static readonly List<int> EventControlIDs = new List<int>();

        /// <summary>Draws the details of the <see cref="SerializableEventSequenceDrawer.Context.Callbacks"/>.</summary>
        private void DoEventsGUI(SerializableEventSequenceDrawer.Context context, out float addEventNormalizedTime)
        {
            addEventNormalizedTime = float.NaN;
            var currentEvent = Event.current;

            EventControlIDs.Clear();
            var selectedEventControlID = -1;

            var baseControlID = GUIUtility.GetControlID(EventHash - 1, FocusType.Passive);

            for (int i = 0; i < EventTimes.Count; i++)
            {
                var controlID = GUIUtility.GetControlID(EventHash + i, FocusType.Keyboard);
                EventControlIDs.Add(controlID);
                if (context.SelectedEvent == i)
                    selectedEventControlID = controlID;
            }

            EventControlIDs.Add(baseControlID);

            switch (currentEvent.type)
            {
                case EventType.Repaint:
                    RepaintEventsGUI(context);
                    break;

                case EventType.MouseDown:
                    OnMouseDown(currentEvent, context, ref addEventNormalizedTime);
                    break;

                case EventType.MouseUp:
                    OnMouseUp(currentEvent, context);
                    break;

                case EventType.MouseDrag:
                    if (_Duration <= 0)
                        break;

                    var hotControl = GUIUtility.hotControl;
                    if (hotControl == baseControlID)
                    {
                        SetPreviewTime(context, currentEvent);
                        GUIUtility.ExitGUI();
                    }
                    else
                    {
                        for (int i = 0; i < EventTimes.Count; i++)
                        {
                            if (hotControl == EventControlIDs[i])
                            {
                                if (context.Times.Count < 1)
                                    context.Times.Count = 1;

                                var seconds = PixelsToSeconds(currentEvent.mousePosition.x);

                                if (currentEvent.control)
                                    SnapToFrameRate(context, ref seconds);

                                var timeProperty = context.Times.GetElement(i);
                                var normalizedTime = seconds / _Duration;
                                timeProperty.floatValue = normalizedTime;
                                SerializableEventSequenceDrawer.SyncEventTimeChange(context, i, normalizedTime);
                                timeProperty.serializedObject.ApplyModifiedProperties();
                                timeProperty.serializedObject.Update();

                                GUIUtility.hotControl = EventControlIDs[context.SelectedEvent];
                                GUI.changed = true;

                                SetPreviewTime(context, currentEvent);
                                GUIUtility.ExitGUI();
                            }
                        }
                    }
                    break;

                case EventType.KeyUp:
                    if (GUIUtility.keyboardControl != selectedEventControlID)
                        break;

                    var exitGUI = false;

                    switch (currentEvent.keyCode)
                    {
                        case KeyCode.Delete:
                        case KeyCode.Backspace:
                            SerializableEventSequenceDrawer.RemoveEvent(context, context.SelectedEvent);
                            exitGUI = true;
                            break;

                        case KeyCode.LeftArrow: NudgeEventTime(context, Event.current.shift ? -10 : -1); break;
                        case KeyCode.RightArrow: NudgeEventTime(context, Event.current.shift ? 10 : 1); break;

                        case KeyCode.Space: RoundEventTime(context); break;

                        default: return;// Don't call Use.
                    }

                    currentEvent.Use();
                    GUI.changed = true;

                    if (exitGUI)
                        GUIUtility.ExitGUI();
                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Snaps the `seconds` value to the nearest multiple of the <see cref="AnimationClip.frameRate"/>.</summary>
        public void SnapToFrameRate(SerializableEventSequenceDrawer.Context context, ref float seconds)
        {
            if (AnimancerUtilities.TryGetFrameRate(context.TransitionContext.Transition, out var frameRate))
            {
                seconds = AnimancerUtilities.Round(seconds, 1f / frameRate);
            }
        }

        /************************************************************************************************************************/

        private void RepaintEventsGUI(SerializableEventSequenceDrawer.Context context)
        {
            var color = GUI.color;

            for (int i = 0; i < EventTimes.Count; i++)
            {
                var currentColor = color;
                // Read Only: currentColor *= new Color(0.9f, 0.9f, 0.9f, 0.5f * alpha);
                if (context.SelectedEvent == i)
                {
                    currentColor *= SelectedEventColor;
                }
                else
                {
                    currentColor *= UnselectedEventColor;
                }

                if (i == EventTimes.Count - 1 && !_HasEndTime)
                    currentColor.a *= 0.65f;

                GUI.color = currentColor;

                var area = GetEventIconArea(i);
                GUI.DrawTexture(area, EventIcon);
            }

            GUI.color = color;
        }

        /************************************************************************************************************************/

        private void OnMouseDown(Event currentEvent, SerializableEventSequenceDrawer.Context context, ref float addEventNormalizedTime)
        {
            if (!_Area.Contains(currentEvent.mousePosition))
                return;

            var selectedEventControlID = 0;
            var selectedEvent = -1;

            for (int i = 0; i < EventControlIDs.Count; i++)
            {
                var area = i < EventTimes.Count ? GetEventIconArea(i) : _Area;

                if (area.Contains(currentEvent.mousePosition))
                {
                    selectedEventControlID = EventControlIDs[i];
                    selectedEvent = i;
                    break;
                }
            }

            GUIUtility.hotControl = GUIUtility.keyboardControl = selectedEventControlID;

            if (selectedEvent < 0 || selectedEvent >= EventTimes.Count)
            {
                SetPreviewTime(context, currentEvent);
                selectedEvent = -1;
            }

            if (currentEvent.type == EventType.MouseDown &&
                currentEvent.clickCount == 2)
            {
                addEventNormalizedTime = PixelsToSeconds(currentEvent.mousePosition.x);
                addEventNormalizedTime = SecondsToNormalized(addEventNormalizedTime);
            }

            context.SelectedEvent = selectedEvent;
            currentEvent.Use();
        }

        /************************************************************************************************************************/

        private void OnMouseUp(Event currentEvent, SerializableEventSequenceDrawer.Context context)
        {
            if (currentEvent.button == 1 &&
                _Area.Contains(currentEvent.mousePosition))
            {
                ShowContextMenu(currentEvent, context);
                currentEvent.Use();
            }
        }

        /************************************************************************************************************************/

        private void ShowContextMenu(Event currentEvent, SerializableEventSequenceDrawer.Context context)
        {
            context = context.Copy();
            var time = SecondsToNormalized(PixelsToSeconds(currentEvent.mousePosition.x));
            var hasSelectedEvent = context.SelectedEvent >= 0;

            var menu = new GenericMenu();

            AddContextFunction(menu, context, "Add Event (Double Click)", true,
                () => SerializableEventSequenceDrawer.AddEvent(context, time));

            AddContextFunction(menu, context, "Remove Event (Delete)", hasSelectedEvent,
                () => SerializableEventSequenceDrawer.RemoveEvent(context, context.SelectedEvent));

            const string NudgePrefix = "Nudge Event Time/";
            AddContextFunction(menu, context, NudgePrefix + "Left 1 Pixel (Left Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, -1));
            AddContextFunction(menu, context, NudgePrefix + "Left 10 Pixels (Shift + Left Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, -10));
            AddContextFunction(menu, context, NudgePrefix + "Right 1 Pixel (Right Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, 1));
            AddContextFunction(menu, context, NudgePrefix + "Right 10 Pixels (Shift + Right Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, 10));

            var canRoundTime = hasSelectedEvent;
            if (canRoundTime)
            {
                time = context.Times.GetElement(context.SelectedEvent).floatValue;
                canRoundTime = TryRoundValue(ref time);
            }

            AddContextFunction(menu, context, $"Round Event Time to {time}x (Space)", canRoundTime,
                () => RoundEventTime(context));

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        private static void AddContextFunction(
            GenericMenu menu, SerializableEventSequenceDrawer.Context context, string label, bool enabled, Action function)
        {
            menu.AddFunction(label, enabled, () =>
            {
                using (context.SetAsCurrent())
                {
                    function();
                    GUI.changed = true;
                }
            });
        }

        /************************************************************************************************************************/

        private void SetPreviewTime(SerializableEventSequenceDrawer.Context context, Event currentEvent)
        {
            if (_Duration > 0)
            {
                var seconds = PixelsToSeconds(currentEvent.mousePosition.x);

                if (currentEvent.control)
                    SnapToFrameRate(context, ref seconds);

                TransitionPreviewWindow.PreviewNormalizedTime = seconds / _Duration;
            }
        }

        /************************************************************************************************************************/

        private Rect GetEventIconArea(int index)
        {
            var width = EventIcon.width;

            var x = SecondsToPixels(EventTimes[index]) - width * 0.5f;
            x = Mathf.Clamp(x, 0, _Area.width - width);

            return new Rect(x, _Area.y, width, EventIcon.height);
        }

        /************************************************************************************************************************/

        private void NudgeEventTime(SerializableEventSequenceDrawer.Context context, float offsetPixels)
        {
            var index = context.SelectedEvent;
            var time = context.Times.GetElement(index);

            var value = time.floatValue;
            value = NormalizedToSeconds(value);
            value = SecondsToPixels(value);

            value += offsetPixels;

            value = PixelsToSeconds(value);
            value = SecondsToNormalized(value);
            time.floatValue = value;

            SerializableEventSequenceDrawer.SyncEventTimeChange(context, index, value);
        }

        /************************************************************************************************************************/

        private static void RoundEventTime(SerializableEventSequenceDrawer.Context context)
        {
            var index = context.SelectedEvent;
            var time = context.Times.GetElement(index);
            var value = time.floatValue;

            if (TryRoundValue(ref value))
            {
                time.floatValue = value;
                SerializableEventSequenceDrawer.SyncEventTimeChange(context, index, value);
            }
        }

        private static bool TryRoundValue(ref float value)
        {
            var format = System.Globalization.NumberFormatInfo.InvariantInfo;
            var text = value.ToString(format);
            var dot = text.IndexOf('.');
            if (dot < 0)
                return false;

            Round:
            var newValue = (float)Math.Round(value, text.Length - dot - 2, MidpointRounding.AwayFromZero);
            if (newValue == value)
            {
                dot--;
                if (dot > 0)
                    goto Round;
            }

            if (value != newValue)
            {
                value = newValue;
                return true;
            }
            else return false;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Ticks
        /************************************************************************************************************************/

        private static readonly List<float> TickTimes = new List<float>();

        /// <summary>Draws ticks and labels for important times throughout the area.</summary>
        private void DoRulerGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var area = new Rect(SecondsToPixels(0), _Area.yMax - TickHeight, 0, TickHeight)
            {
                xMax = SecondsToPixels(_Duration)
            };

            EditorGUI.DrawRect(area, BaseTimeColor);

            TickTimes.Clear();
            TickTimes.Add(0);
            TickTimes.Add(_StartTime);
            TickTimes.Add(_FadeInEnd);
            TickTimes.Add(_Duration);
            TickTimes.AddRange(EventTimes);
            TickTimes.Sort();

            var previousTime = float.NaN;
            area.x = float.NegativeInfinity;

            for (int i = 0; i < TickTimes.Count; i++)
            {
                var time = TickTimes[i];
                if (previousTime != time)
                {
                    previousTime = time;
                    DoRulerLabelGUI(ref area, time);
                }
            }

            DrawPreviewTime();
        }

        /************************************************************************************************************************/

        private void DrawPreviewTime()
        {
            var state = TransitionPreviewWindow.GetCurrentState();
            if (state == null)
                return;

            var normalizedTime = TransitionPreviewWindow.PreviewNormalizedTime;
            DrawPreviewTime(normalizedTime, alpha: 1);

            // Looping states show faded indicators at every other multiple of the loop.
            if (!state.IsLooping)
                return;

            // Make sure the area is actually wide enough for it to not just be a solid bar.
            if ((int)SecondsToPixels(0) > (int)SecondsToPixels(_Duration) - 4)
                return;

            // Go back to the first visible increment.
            while (normalizedTime * _Duration >= _MinTime + _Duration)
                normalizedTime -= 1;

            // Draw every visible increment from there on.
            while (normalizedTime * _Duration <= _MaxTime)
            {
                DrawPreviewTime(normalizedTime, alpha: 0.2f);
                normalizedTime += 1;
            }
        }

        private void DrawPreviewTime(float normalizedTime, float alpha)
        {
            var time = NormalizedToSeconds(normalizedTime);
            var x = SecondsToPixels(time);
            if (x >= 0 && x <= _Area.width)
            {
                var color = PreviewTimeColor;
                color.a = alpha;
                EditorGUI.DrawRect(new Rect(x - 1, _Area.y, 2, _Area.height), color);
            }
        }

        /************************************************************************************************************************/

        private static GUIStyle _RulerLabelStyle;
        private static ConversionCache<string, float> _TimeLabelWidthCache;

        private void DoRulerLabelGUI(ref Rect previousArea, float time)
        {
            if (_RulerLabelStyle == null)
                _RulerLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(),
                    contentOffset = new Vector2(0, -2),
                    alignment = TextAnchor.UpperLeft,
                    fontSize = Mathf.CeilToInt(AnimancerGUI.LineHeight * 0.6f),
                };

            var text = G2Cache.Convert(time);

            if (_TimeLabelWidthCache == null)
                _TimeLabelWidthCache = AnimancerGUI.CreateWidthCache(_RulerLabelStyle);

            var area = new Rect(
                SecondsToPixels(time),
                _Area.y,
                _TimeLabelWidthCache.Convert(text),
                _Area.height);

            if (area.x > _Area.x)
            {
                var tickY = _Area.yMax - TickHeight;
                EditorGUI.DrawRect(new Rect(area.x, tickY, 1, TickHeight), AnimancerGUI.TextColor);
            }

            if (area.xMax > _Area.xMax)
                area.x = _Area.xMax - area.width;
            if (area.x < 0)
                area.x = 0;

            if (area.x > previousArea.xMax + 2)
            {
                GUI.Label(area, text, _RulerLabelStyle);

                previousArea = area;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

