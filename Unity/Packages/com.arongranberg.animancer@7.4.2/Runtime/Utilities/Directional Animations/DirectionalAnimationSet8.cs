// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>A set of up/right/down/left animations with diagonals as well.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/DirectionalAnimationSet8
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Directional Animation Set/8 Directions", order = Strings.AssetMenuOrder + 11)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(DirectionalAnimationSet8))]
    public class DirectionalAnimationSet8 : DirectionalAnimationSet
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _UpRight;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally up-right ~(0.7, 0.7).</summary>
        /// <exception cref="ArgumentException"><see cref="AllowSetClips"/> was not called before setting this value.</exception>
        public AnimationClip UpRight
        {
            get => _UpRight;
            set
            {
                AssertCanSetClips();
                _UpRight = value;
                AnimancerUtilities.SetDirty(this);
            }
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _DownRight;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally down-right ~(0.7, -0.7).</summary>
        /// <exception cref="ArgumentException"><see cref="AllowSetClips"/> was not called before setting this value.</exception>
        public AnimationClip DownRight
        {
            get => _DownRight;
            set
            {
                AssertCanSetClips();
                _DownRight = value;
                AnimancerUtilities.SetDirty(this);
            }
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _DownLeft;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally down-left ~(-0.7, -0.7).</summary>
        /// <exception cref="ArgumentException"><see cref="AllowSetClips"/> was not called before setting this value.</exception>
        public AnimationClip DownLeft
        {
            get => _DownLeft;
            set
            {
                AssertCanSetClips();
                _DownLeft = value;
                AnimancerUtilities.SetDirty(this);
            }
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _UpLeft;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally up-left ~(-0.7, 0.7).</summary>
        /// <exception cref="ArgumentException"><see cref="AllowSetClips"/> was not called before setting this value.</exception>
        public AnimationClip UpLeft
        {
            get => _UpLeft;
            set
            {
                AssertCanSetClips();
                _UpLeft = value;
                AnimancerUtilities.SetDirty(this);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the animation closest to the specified `direction`.</summary>
        public override AnimationClip GetClip(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x);
            var octant = Mathf.RoundToInt(8 * angle / (2 * Mathf.PI) + 8) % 8;
            switch (octant)
            {
                case 0: return Right;
                case 1: return _UpRight;
                case 2: return Up;
                case 3: return _UpLeft;
                case 4: return Left;
                case 5: return _DownLeft;
                case 6: return Down;
                case 7: return _DownRight;
                default: throw new ArgumentOutOfRangeException("Invalid octant");
            }
        }

        /************************************************************************************************************************/
        #region Directions
        /************************************************************************************************************************/

        /// <summary>Constants for each of the diagonal directions.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Diagonals
        /// 
        public static class Diagonals
        {
            /************************************************************************************************************************/

            /// <summary>1 / (Square Root of 2).</summary>
            public const float OneOverSqrt2 = 0.70710678118f;

            /// <summary>A vector with a magnitude of 1 pointing up to the right.</summary>
            /// <remarks>The value is approximately (0.7, 0.7).</remarks>
            public static Vector2 UpRight => new Vector2(OneOverSqrt2, OneOverSqrt2);

            /// <summary>A vector with a magnitude of 1 pointing down to the right.</summary>
            /// <remarks>The value is approximately (0.7, -0.7).</remarks>
            public static Vector2 DownRight => new Vector2(OneOverSqrt2, -OneOverSqrt2);

            /// <summary>A vector with a magnitude of 1 pointing down to the left.</summary>
            /// <remarks>The value is approximately (-0.7, -0.7).</remarks>
            public static Vector2 DownLeft => new Vector2(-OneOverSqrt2, -OneOverSqrt2);

            /// <summary>A vector with a magnitude of 1 pointing up to the left.</summary>
            /// <remarks>The value is approximately (-0.707, 0.707).</remarks>
            public static Vector2 UpLeft => new Vector2(-OneOverSqrt2, OneOverSqrt2);

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        public override int ClipCount => 8;

        /************************************************************************************************************************/

        /// <summary>Up, Right, Down, Left, or their diagonals.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Direction
        /// 
        public new enum Direction
        {
            /// <summary><see cref="Vector2.up"/>.</summary>
            Up,

            /// <summary><see cref="Vector2.right"/>.</summary>
            Right,

            /// <summary><see cref="Vector2.down"/>.</summary>
            Down,

            /// <summary><see cref="Vector2.left"/>.</summary>
            Left,

            /// <summary><see cref="Vector2"/>(0.7..., 0.7...).</summary>
            UpRight,

            /// <summary><see cref="Vector2"/>(0.7..., -0.7...).</summary>
            DownRight,

            /// <summary><see cref="Vector2"/>(-0.7..., -0.7...).</summary>
            DownLeft,

            /// <summary><see cref="Vector2"/>(-0.7..., 0.7...).</summary>
            UpLeft,
        }

        /************************************************************************************************************************/

        protected override string GetDirectionName(int direction) => ((Direction)direction).ToString();

        /************************************************************************************************************************/

        /// <summary>Returns the animation associated with the specified `direction`.</summary>
        public AnimationClip GetClip(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Up;
                case Direction.Right: return Right;
                case Direction.Down: return Down;
                case Direction.Left: return Left;
                case Direction.UpRight: return _UpRight;
                case Direction.DownRight: return _DownRight;
                case Direction.DownLeft: return _DownLeft;
                case Direction.UpLeft: return _UpLeft;
                default: throw AnimancerUtilities.CreateUnsupportedArgumentException(direction);
            }
        }

        public override AnimationClip GetClip(int direction) => GetClip((Direction)direction);

        /************************************************************************************************************************/

        /// <summary>Sets the animation associated with the specified `direction`.</summary>
        public void SetClip(Direction direction, AnimationClip clip)
        {
            switch (direction)
            {
                case Direction.Up: Up = clip; break;
                case Direction.Right: Right = clip; break;
                case Direction.Down: Down = clip; break;
                case Direction.Left: Left = clip; break;
                case Direction.UpRight: UpRight = clip; break;
                case Direction.DownRight: DownRight = clip; break;
                case Direction.DownLeft: DownLeft = clip; break;
                case Direction.UpLeft: UpLeft = clip; break;
                default: throw AnimancerUtilities.CreateUnsupportedArgumentException(direction);
            }
        }

        public override void SetClip(int direction, AnimationClip clip) => SetClip((Direction)direction, clip);

        /************************************************************************************************************************/

        /// <summary>Returns a vector representing the specified `direction`.</summary>
        public static Vector2 DirectionToVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Right: return Vector2.right;
                case Direction.Down: return Vector2.down;
                case Direction.Left: return Vector2.left;
                case Direction.UpRight: return Diagonals.UpRight;
                case Direction.DownRight: return Diagonals.DownRight;
                case Direction.DownLeft: return Diagonals.DownLeft;
                case Direction.UpLeft: return Diagonals.UpLeft;
                default: throw AnimancerUtilities.CreateUnsupportedArgumentException(direction);
            }
        }

        public override Vector2 GetDirection(int direction) => DirectionToVector((Direction)direction);

        /************************************************************************************************************************/

        /// <summary>Returns the direction closest to the specified `vector`.</summary>
        public new static Direction VectorToDirection(Vector2 vector)
        {
            var angle = Mathf.Atan2(vector.y, vector.x);
            var octant = Mathf.RoundToInt(8 * angle / (2 * Mathf.PI) + 8) % 8;
            switch (octant)
            {
                case 0: return Direction.Right;
                case 1: return Direction.UpRight;
                case 2: return Direction.Up;
                case 3: return Direction.UpLeft;
                case 4: return Direction.Left;
                case 5: return Direction.DownLeft;
                case 6: return Direction.Down;
                case 7: return Direction.DownRight;
                default: throw new ArgumentOutOfRangeException("Invalid octant");
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns a copy of the `vector` pointing in the closest direction this set type has an animation for.</summary>
        public new static Vector2 SnapVectorToDirection(Vector2 vector)
        {
            var magnitude = vector.magnitude;
            var direction = VectorToDirection(vector);
            vector = DirectionToVector(direction) * magnitude;
            return vector;
        }

        public override Vector2 Snap(Vector2 vector) => SnapVectorToDirection(vector);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Name Based Operations
        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        public override int SetClipByName(AnimationClip clip)
        {
            var name = clip.name;

            var directionCount = ClipCount;
            for (int i = directionCount - 1; i >= 0; i--)
            {
                if (name.Contains(GetDirectionName(i)))
                {
                    SetClip(i, clip);
                    return i;
                }
            }

            return -1;
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}
