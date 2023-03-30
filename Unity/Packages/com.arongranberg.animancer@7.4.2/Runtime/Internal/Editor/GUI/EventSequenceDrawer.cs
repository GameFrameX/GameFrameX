// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Sequence = Animancer.AnimancerEvent.Sequence;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Sequence"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/EventSequenceDrawer
    ///
    public class EventSequenceDrawer
    {
        /************************************************************************************************************************/

        private static readonly ConditionalWeakTable<Sequence, EventSequenceDrawer>
            SequenceToDrawer = new ConditionalWeakTable<Sequence, EventSequenceDrawer>();

        /// <summary>Returns a cached <see cref="EventSequenceDrawer"/> for the `events`.</summary>
        /// <remarks>
        /// The cache uses a <see cref="ConditionalWeakTable{TKey, TValue}"/> so it doesn't prevent the `events`
        /// from being garbage collected.
        /// </remarks>
        public static EventSequenceDrawer Get(Sequence events)
        {
            if (events == null)
                return null;

            if (!SequenceToDrawer.TryGetValue(events, out var drawer))
                SequenceToDrawer.Add(events, drawer = new EventSequenceDrawer());

            return drawer;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the number of vertical pixels required to draw the specified `lineCount` using the
        /// <see cref="AnimancerGUI.LineHeight"/> and <see cref="AnimancerGUI.StandardSpacing"/>.
        /// </summary>
        public static float CalculateHeight(int lineCount)
            => lineCount == 0 ? 0 :
                AnimancerGUI.LineHeight * lineCount +
                AnimancerGUI.StandardSpacing * (lineCount - 1);

        /************************************************************************************************************************/

        /// <summary>Calculates the number of vertical pixels required to draw the contents of the `events`.</summary>
        public float CalculateHeight(Sequence events)
            => CalculateHeight(CalculateLineCount(events));

        /// <summary>Calculates the number of lines required to draw the contents of the `events`.</summary>
        public int CalculateLineCount(Sequence events)
        {
            if (events == null)
                return 0;

            if (!_IsExpanded)
                return 1;

            var count = 1;

            for (int i = 0; i < events.Count; i++)
            {
                count++;
                count += CalculateLineCount(events[i].callback);
            }

            count++;
            count += CalculateLineCount(events.EndEvent.callback);

            return count;
        }

        /************************************************************************************************************************/

        private bool _IsExpanded;

        private static ConversionCache<int, string> _EventNumberCache;

        private static float _LogButtonWidth = float.NaN;

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the `events`.</summary>
        public void Draw(ref Rect area, Sequence events, GUIContent label)
        {
            if (events == null)
                return;

            area.height = AnimancerGUI.LineHeight;

            var headerArea = area;

            const string LogLabel = "Log";
            if (float.IsNaN(_LogButtonWidth))
                _LogButtonWidth = EditorStyles.miniButton.CalculateWidth(LogLabel);
            var logArea = AnimancerGUI.StealFromRight(ref headerArea, _LogButtonWidth);
            if (GUI.Button(logArea, LogLabel, EditorStyles.miniButton))
                Debug.Log(events.DeepToString());

            _IsExpanded = EditorGUI.Foldout(headerArea, _IsExpanded, GUIContent.none, true);
            using (ObjectPool.Disposable.AcquireContent(out var summary, GetSummary(events)))
                EditorGUI.LabelField(headerArea, label, summary);

            AnimancerGUI.NextVerticalArea(ref area);

            if (!_IsExpanded)
                return;

            var enabled = GUI.enabled;
            GUI.enabled = false;

            EditorGUI.indentLevel++;

            for (int i = 0; i < events.Count; i++)
            {
                var name = events.GetName(i);
                if (string.IsNullOrEmpty(name))
                {
                    if (_EventNumberCache == null)
                        _EventNumberCache = new ConversionCache<int, string>((index) => $"Event {index}");

                    name = _EventNumberCache.Convert(i);
                }

                Draw(ref area, name, events[i]);
            }

            Draw(ref area, "End Event", events.EndEvent);

            EditorGUI.indentLevel--;

            GUI.enabled = enabled;
        }

        /************************************************************************************************************************/

        private static readonly ConversionCache<int, string>
            SummaryCache = new ConversionCache<int, string>((count) => $"[{count}]"),
            EndSummaryCache = new ConversionCache<int, string>((count) => $"[{count}] + End");

        /// <summary>Returns a summary of the `events`.</summary>
        public static string GetSummary(Sequence events)
        {
            var cache =
                float.IsNaN(events.NormalizedEndTime) &&
                AnimancerEvent.IsNullOrDummy(events.OnEnd)
                ? SummaryCache : EndSummaryCache;
            return cache.Convert(events.Count);
        }

        /************************************************************************************************************************/

        private static ConversionCache<float, string> _EventTimeCache;

        /// <summary>Draws the GUI for the `animancerEvent`.</summary>
        public static void Draw(ref Rect area, string name, AnimancerEvent animancerEvent)
        {
            area.height = AnimancerGUI.LineHeight;

            if (_EventTimeCache == null)
                _EventTimeCache = new ConversionCache<float, string>((time)
                    => float.IsNaN(time) ? "Time = Auto" : $"Time = {time.ToStringCached()}x");

            EditorGUI.LabelField(area, name, _EventTimeCache.Convert(animancerEvent.normalizedTime));

            AnimancerGUI.NextVerticalArea(ref area);

            EditorGUI.indentLevel++;
            DrawInvocationList(ref area, animancerEvent.callback);
            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the number of vertical pixels required to draw the specified <see cref="Delegate"/>.</summary>
        public static float CalculateHeight(MulticastDelegate del)
            => CalculateHeight(CalculateLineCount(del));

        /// <summary>Calculates the number of lines required to draw the specified <see cref="Delegate"/>.</summary>
        public static int CalculateLineCount(MulticastDelegate del)
        {
            if (del == null)
                return 1;

            var delegates = GetInvocationListIfMulticast(del);
            return delegates == null ? 2 : delegates.Length * 2;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the target and name of the specified <see cref="Delegate"/>.</summary>
        public static void DrawInvocationList(ref Rect area, MulticastDelegate del)
        {
            if (del == null)
            {
                EditorGUI.LabelField(area, "Delegate", "Null");
                AnimancerGUI.NextVerticalArea(ref area);
                return;
            }

            var delegates = GetInvocationListIfMulticast(del);
            if (delegates == null)
            {
                Draw(ref area, del);
            }
            else
            {
                for (int i = 0; i < delegates.Length; i++)
                    Draw(ref area, delegates[i]);
            }
        }

        /************************************************************************************************************************/

        private static Delegate[] GetInvocationListIfMulticast(MulticastDelegate del)
            => AnimancerUtilities.TryGetInvocationListNonAlloc(del, out var delegates) ? delegates : del.GetInvocationList();

        /************************************************************************************************************************/

        /// <summary>Draws the target and name of the specified <see cref="Delegate"/>.</summary>
        public static void Draw(ref Rect area, Delegate del)
        {
            area.height = AnimancerGUI.LineHeight;

            if (del == null)
            {
                EditorGUI.LabelField(area, "Callback", "Null");
                AnimancerGUI.NextVerticalArea(ref area);
                return;
            }

            var method = del.Method;
            EditorGUI.LabelField(area, "Method", method.Name);

            AnimancerGUI.NextVerticalArea(ref area);

            var target = del.Target;
            if (target is Object obj)
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;

                EditorGUI.ObjectField(area, "Target", obj, obj.GetType(), true);

                GUI.enabled = enabled;
            }
            else if (target != null)
            {
                EditorGUI.LabelField(area, "Target", target.ToString());
            }
            else
            {
                EditorGUI.LabelField(area, "Declaring Type", method.DeclaringType.GetNameCS());
            }

            AnimancerGUI.NextVerticalArea(ref area);
        }

        /************************************************************************************************************************/
    }
}

#endif

