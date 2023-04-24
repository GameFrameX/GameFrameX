//#define FR2_DEBUG

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

namespace vietlabs.fr2
{

    public class FR2_SplitView
    {
        private const float SPLIT_SIZE = 2f;


        private IWindow window;
        private bool dirty;

        public FR2_SplitView(IWindow w)
        {
            this.window = w;
        }

        [Serializable]
        public class Info
        {
            public GUIContent title;

#if FR2_DEBUG
            public Color color;
#endif

            public Rect rect;
            public float normWeight;
            public int stIndex;

            public bool visible = true;
            public float weight = 1f;
            public Action<Rect> draw;

            public void DoDraw()
            {
#if FR2_DEBUG
                GUI2.Rect(rect, Color.white, 0.1f);
#endif
                var drawRect = rect;
                if (title != null)
                {
                    var titleRect = new Rect(rect.x, rect.y, rect.width, 16f);
                    GUI2.Rect(titleRect, Color.black, 0.2f);

                    titleRect.xMin += 4f;
                    GUI.Label(titleRect, title, EditorStyles.boldLabel);
                    drawRect.yMin += 16f;
                }

                draw(drawRect);
            }
        }

        public bool isHorz;
        public List<Info> splits = new List<Info>();

        public bool isVisible
        {
            get { return _visibleCount > 0; }
        }

        private int _visibleCount;
        private Rect _rect;

        public void CalculateWeight()
        {
            _visibleCount = 0;
            var _totalWeight = 0f;

            for (var i = 0; i < splits.Count; i++)
            {
                var info = splits[i];
                if (!info.visible) continue;

                info.stIndex = _visibleCount;
                _totalWeight += info.weight;

                _visibleCount++;
            }

            if (_visibleCount == 0 || _totalWeight == 0)
            {
                //Debug.LogWarning("Nothing visible!");
                return;
            }

            var cWeight = 0f;
            for (var i = 0; i < splits.Count; i++)
            {
                var info = splits[i];
                if (!info.visible) continue;

                cWeight += info.weight;
                info.normWeight = info.weight / _totalWeight;
            }
        }

        public void Draw(Rect rect)
        {
            if (rect.width > 0 || rect.height > 0)
            {
                _rect = rect;
            }

            if (dirty)
            {
                dirty = false;
                CalculateWeight();
            }

            var sz = (_visibleCount - 1) * SPLIT_SIZE;
            var dx = _rect.x;
            var dy = _rect.y;

            for (var i = 0; i < splits.Count; i++)
            {
                var info = splits[i];
                if (!info.visible) continue;

                var rr = new Rect
                (
                    dx, dy,
                    isHorz ? (_rect.width - sz) * info.normWeight : _rect.width,
                    isHorz ? _rect.height : (_rect.height - sz) * info.normWeight
                );

                if (rr.width > 0 && rr.height > 0)
                {
                    info.rect = rr;
                }

                if (info.draw != null) info.DoDraw();

                if (info.stIndex < _visibleCount - 1)
                {
                    DrawSpliter(i, isHorz ? info.rect.xMax : info.rect.yMax);
                }

                if (isHorz)
                {
                    dx += info.rect.width + SPLIT_SIZE;
                }
                else
                {
                    dy += info.rect.height + SPLIT_SIZE;
                }
            }
        }

        public void DrawLayout()
        {
            var rect = StartLayout(isHorz);
            {
                Draw(rect);
            }
            EndLayout(isHorz);
        }

        private int resizeIndex = -1;


        void RefreshSpliterPos(int index, float px)
        {
            var sp1 = splits[index];
            var sp2 = splits[index + 1];

            var r1 = sp1.rect;
            var r2 = sp2.rect;

            var w1 = sp1.weight;
            var w2 = sp2.weight;
            var tt = w1 + w2;

            var dd = isHorz ? (r2.xMax - r1.xMin) : (r2.yMax - r1.yMin) - SPLIT_SIZE;
            var m = isHorz ? (Event.current.mousePosition.x - r1.x) : (Event.current.mousePosition.y - r1.y);
            var pct = Mathf.Min(0.9f, Mathf.Max(0.1f, m / dd));

            sp1.weight = tt * pct;
            sp2.weight = tt * (1 - pct);

            dirty = true;
            if (window != null) window.WillRepaint = true;
        }

        void DrawSpliter(int index, float px)
        {
            var dRect = _rect;

            if (isHorz)
            {
                dRect.x = px + SPLIT_SIZE;
                dRect.width = SPLIT_SIZE;
            }
            else
            {
                dRect.y = px;
                dRect.height = SPLIT_SIZE;
            }

            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseMove)
            {
                GUI2.Rect(dRect, Color.black, 0.4f);
            }

            var dRect2 = GUI2.Padding(dRect, -2f, -2f);

            EditorGUIUtility.AddCursorRect(dRect2, isHorz ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && dRect2.Contains(Event.current.mousePosition))
            {
                resizeIndex = index;
                RefreshSpliterPos(index, px);
            }

            if (resizeIndex == index)
            {
                RefreshSpliterPos(index, px);
            }

            if (Event.current.type == EventType.MouseUp)
            {
                resizeIndex = -1;
            }
        }

        Rect StartLayout(bool horz)
        {
            return horz
                ? EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))
                : EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }

        void EndLayout(bool horz)
        {
            if (horz)
            {
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.EndVertical();
            }
        }
    }
}