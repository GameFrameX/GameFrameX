// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Various utilities used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerEditorUtilities
    /// 
    public static partial class AnimancerEditorUtilities
    {
        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/

        /// <summary>Commonly used <see cref="BindingFlags"/> combinations.</summary>
        public const BindingFlags
            AnyAccessBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
            InstanceBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            StaticBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Editor-Only]
        /// Returns the first <typeparamref name="TAttribute"/> attribute on the `member` or <c>null</c> if there is none.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider member, bool inherit = false)
            where TAttribute : class
        {
            var type = typeof(TAttribute);
            if (member.IsDefined(type, inherit))
                return (TAttribute)member.GetCustomAttributes(type, inherit)[0];
            else
                return null;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Editor-Only] Is the <see cref="Vector2.x"/> or <see cref="Vector2.y"/> NaN?</summary>
        public static bool IsNaN(this Vector2 vector) => float.IsNaN(vector.x) || float.IsNaN(vector.y);

        /// <summary>[Animancer Extension] [Editor-Only] Is the <see cref="Vector3.x"/>, <see cref="Vector3.y"/>, or <see cref="Vector3.z"/> NaN?</summary>
        public static bool IsNaN(this Vector3 vector) => float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);

        /************************************************************************************************************************/

        /// <summary>Finds an asset of the specified type anywhere in the project.</summary>
        public static T FindAssetOfType<T>() where T : Object
        {
            var filter = typeof(Component).IsAssignableFrom(typeof(T))
                ? $"t:{nameof(GameObject)}"
                : $"t:{typeof(T).Name}";

            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length == 0)
                return null;

            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    return asset;
            }

            return null;
        }

        /************************************************************************************************************************/

        // The "g" format gives a lower case 'e' for exponentials instead of upper case 'E'.
        private static readonly ConversionCache<float, string>
            FloatToString = new ConversionCache<float, string>((value) => $"{value:g}");

        /// <summary>[Animancer Extension]
        /// Calls <see cref="float.ToString(string)"/> using <c>"g"</c> as the format and caches the result.
        /// </summary>
        public static string ToStringCached(this float value) => FloatToString.Convert(value);

        /************************************************************************************************************************/

        /// <summary>The most recent <see cref="PlayModeStateChange"/>.</summary>
        public static PlayModeStateChange PlayModeState { get; private set; }

        /// <summary>Is the Unity Editor is currently changing between Play Mode and Edit Mode?</summary>
        public static bool IsChangingPlayMode =>
            PlayModeState == PlayModeStateChange.ExitingEditMode ||
            PlayModeState == PlayModeStateChange.ExitingPlayMode;

        [InitializeOnLoadMethod]
        private static void WatchForPlayModeChanges()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                PlayModeState = EditorApplication.isPlaying ?
                    PlayModeStateChange.EnteredPlayMode :
                    PlayModeStateChange.ExitingEditMode;

            EditorApplication.playModeStateChanged += (change) => PlayModeState = change;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Collections
        /************************************************************************************************************************/

        /// <summary>Adds default items or removes items to make the <see cref="List{T}.Count"/> equal to the `count`.</summary>
        public static void SetCount<T>(List<T> list, int count)
        {
            if (list.Count < count)
            {
                while (list.Count < count)
                    list.Add(default);
            }
            else
            {
                list.RemoveRange(count, list.Count - count);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Removes any items from the `list` that are <c>null</c> and items that appear multiple times.
        /// Returns true if the `list` was modified.
        /// </summary>
        public static bool RemoveMissingAndDuplicates(ref List<GameObject> list)
        {
            if (list == null)
            {
                list = new List<GameObject>();
                return false;
            }

            var modified = false;

            using (ObjectPool.Disposable.AcquireSet<Object>(out var previousItems))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var item = list[i];
                    if (item == null || previousItems.Contains(item))
                    {
                        list.RemoveAt(i);
                        modified = true;
                    }
                    else
                    {
                        previousItems.Add(item);
                    }
                }
            }

            return modified;
        }

        /************************************************************************************************************************/

        /// <summary>Removes any items from the `dictionary` that use destroyed objects as their key.</summary>
        public static void RemoveDestroyedObjects<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : Object
        {
            using (ObjectPool.Disposable.AcquireList<TKey>(out var oldObjects))
            {
                foreach (var obj in dictionary.Keys)
                {
                    if (obj == null)
                        oldObjects.Add(obj);
                }

                for (int i = 0; i < oldObjects.Count; i++)
                {
                    dictionary.Remove(oldObjects[i]);
                }
            }
        }

        /// <summary>
        /// Creates a new dictionary and returns true if it was null or calls <see cref="RemoveDestroyedObjects"/> and
        /// returns false if it wasn't.
        /// </summary>
        public static bool InitializeCleanDictionary<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary) where TKey : Object
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<TKey, TValue>();
                return true;
            }
            else
            {
                RemoveDestroyedObjects(dictionary);
                return false;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Context Menus
        /************************************************************************************************************************/

        /// <summary>
        /// Adds a menu function which passes the result of <see cref="CalculateEditorFadeDuration"/> into `startFade`.
        /// </summary>
        public static void AddFadeFunction(GenericMenu menu, string label, bool isEnabled, AnimancerNode node, Action<float> startFade)
        {
            // Fade functions need to be delayed twice since the context menu itself causes the next frame delta
            // time to be unreasonably high (which would skip the start of the fade).
            menu.AddFunction(label, isEnabled,
                () => EditorApplication.delayCall +=
                () => EditorApplication.delayCall +=
                () =>
                {
                    startFade(node.CalculateEditorFadeDuration());
                });
        }

        /// <summary>[Animancer Extension] [Editor-Only]
        /// Returns the duration of the `node`s current fade (if any), otherwise returns the `defaultDuration`.
        /// </summary>
        public static float CalculateEditorFadeDuration(this AnimancerNode node, float defaultDuration = 1)
            => node.FadeSpeed > 0 ? 1 / node.FadeSpeed : defaultDuration;

        /************************************************************************************************************************/

        /// <summary>
        /// Adds a menu function to open a web page. If the `linkSuffix` starts with a '/' then it will be relative to
        /// the <see cref="Strings.DocsURLs.Documentation"/>.
        /// </summary>
        public static void AddDocumentationLink(GenericMenu menu, string label, string linkSuffix)
        {
            if (linkSuffix[0] == '/')
                linkSuffix = Strings.DocsURLs.Documentation + linkSuffix;

            menu.AddItem(new GUIContent(label), false, () =>
            {
                EditorUtility.OpenWithDefaultApp(linkSuffix);
            });
        }

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="MenuCommand.context"/> editable?</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Looping", validate = true)]
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Legacy", validate = true)]
        private static bool ValidateEditable(MenuCommand command)
        {
            return (command.context.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable;
        }

        /************************************************************************************************************************/

        /// <summary>Toggles the <see cref="Motion.isLooping"/> flag between true and false.</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Looping")]
        private static void ToggleLooping(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            SetLooping(clip, !clip.isLooping);
        }

        /// <summary>Sets the <see cref="Motion.isLooping"/> flag.</summary>
        public static void SetLooping(AnimationClip clip, bool looping)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = looping;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Debug.Log($"Set {clip.name} to be {(looping ? "Looping" : "Not Looping")}." +
                " Note that you may need to restart Unity for this change to take effect.", clip);

            // None of these let us avoid the need to restart Unity.
            //EditorUtility.SetDirty(clip);
            //AssetDatabase.SaveAssets();

            //var path = AssetDatabase.GetAssetPath(clip);
            //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        /************************************************************************************************************************/

        /// <summary>Swaps the <see cref="AnimationClip.legacy"/> flag between true and false.</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Legacy")]
        private static void ToggleLegacy(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            clip.legacy = !clip.legacy;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Animator.Rebind"/>.</summary>
        [MenuItem("CONTEXT/" + nameof(Animator) + "/Restore Bind Pose", priority = 110)]
        private static void RestoreBindPose(MenuCommand command)
        {
            var animator = (Animator)command.context;

            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Restore bind pose");

            const string TypeName = "UnityEditor.AvatarSetupTool, UnityEditor";
            var type = Type.GetType(TypeName);
            if (type == null)
                throw new TypeLoadException($"Unable to find the type '{TypeName}'");

            const string MethodName = "SampleBindPose";
            var method = type.GetMethod(MethodName, StaticBindings);
            if (method == null)
                throw new MissingMethodException($"Unable to find the method '{MethodName}'");

            method.Invoke(null, new object[] { animator.gameObject });
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Type Names
        /************************************************************************************************************************/

        private static readonly Dictionary<Type, string>
            TypeNames = new Dictionary<Type, string>
            {
                { typeof(object), "object" },
                { typeof(void), "void" },
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(char), "char" },
                { typeof(string), "string" },
                { typeof(short), "short" },
                { typeof(int), "int" },
                { typeof(long), "long" },
                { typeof(ushort), "ushort" },
                { typeof(uint), "uint" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
            };

        private static readonly Dictionary<Type, string>
            FullTypeNames = new Dictionary<Type, string>(TypeNames);

        /************************************************************************************************************************/

        /// <summary>Returns the name of a `type` as it would appear in C# code.</summary>
        /// <remarks>Returned values are stored in a dictionary to speed up repeated use.</remarks>
        /// <example>
        /// <c>typeof(List&lt;float&gt;).FullName</c> would give you:
        /// <c>System.Collections.Generic.List`1[[System.Single, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</c>
        /// <para></para>
        /// This method would instead return <c>System.Collections.Generic.List&lt;float&gt;</c> if `fullName` is <c>true</c>, or
        /// just <c>List&lt;float&gt;</c> if it is <c>false</c>.
        /// </example>
        public static string GetNameCS(this Type type, bool fullName = true)
        {
            if (type == null)
                return "null";

            // Check if we have already got the name for that type.
            var names = fullName ? FullTypeNames : TypeNames;

            if (names.TryGetValue(type, out var name))
                return name;

            var text = ObjectPool.AcquireStringBuilder();

            if (type.IsArray)// Array = TypeName[].
            {
                text.Append(type.GetElementType().GetNameCS(fullName));

                text.Append('[');
                var dimensions = type.GetArrayRank();
                while (dimensions-- > 1)
                    text.Append(',');
                text.Append(']');

                goto Return;
            }

            if (type.IsPointer)// Pointer = TypeName*.
            {
                text.Append(type.GetElementType().GetNameCS(fullName));
                text.Append('*');

                goto Return;
            }

            if (type.IsGenericParameter)// Generic Parameter = TypeName (for unspecified generic parameters).
            {
                text.Append(type.Name);
                goto Return;
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)// Nullable = TypeName != null ?
            {
                text.Append(underlyingType.GetNameCS(fullName));
                text.Append('?');

                goto Return;
            }

            // Other Type = Namespace.NestedTypes.TypeName<GenericArguments>.

            if (fullName && type.Namespace != null)// Namespace.
            {
                text.Append(type.Namespace);
                text.Append('.');
            }

            var genericArguments = 0;

            if (type.DeclaringType != null)// Account for Nested Types.
            {
                // Count the nesting level.
                var nesting = 1;
                var declaringType = type.DeclaringType;
                while (declaringType.DeclaringType != null)
                {
                    declaringType = declaringType.DeclaringType;
                    nesting++;
                }

                // Append the name of each outer type, starting from the outside.
                while (nesting-- > 0)
                {
                    // Walk out to the current nesting level.
                    // This avoids the need to make a list of types in the nest or to insert type names instead of appending them.
                    declaringType = type;
                    for (int i = nesting; i >= 0; i--)
                        declaringType = declaringType.DeclaringType;

                    // Nested Type Name.
                    genericArguments = AppendNameAndGenericArguments(text, declaringType, fullName, genericArguments);
                    text.Append('.');
                }
            }

            // Type Name.
            AppendNameAndGenericArguments(text, type, fullName, genericArguments);

            Return:// Remember and return the name.
            name = text.ReleaseToString();
            names.Add(type, name);
            return name;
        }

        /************************************************************************************************************************/

        /// <summary>Appends the generic arguments of `type` (after skipping the specified number).</summary>
        public static int AppendNameAndGenericArguments(StringBuilder text, Type type, bool fullName = true, int skipGenericArguments = 0)
        {
            var name = type.Name;
            text.Append(name);

            if (type.IsGenericType)
            {
                var backQuote = name.IndexOf('`');
                if (backQuote >= 0)
                {
                    text.Length -= name.Length - backQuote;

                    var genericArguments = type.GetGenericArguments();
                    if (skipGenericArguments < genericArguments.Length)
                    {
                        text.Append('<');

                        var firstArgument = genericArguments[skipGenericArguments];
                        skipGenericArguments++;

                        if (firstArgument.IsGenericParameter)
                        {
                            while (skipGenericArguments < genericArguments.Length)
                            {
                                text.Append(',');
                                skipGenericArguments++;
                            }
                        }
                        else
                        {
                            text.Append(firstArgument.GetNameCS(fullName));

                            while (skipGenericArguments < genericArguments.Length)
                            {
                                text.Append(", ");
                                text.Append(genericArguments[skipGenericArguments].GetNameCS(fullName));
                                skipGenericArguments++;
                            }
                        }

                        text.Append('>');
                    }
                }
            }

            return skipGenericArguments;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Dummy Animancer Component
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// An <see cref="IAnimancerComponent"/> which is not actually a <see cref="Component"/>.
        /// </summary>
        public class DummyAnimancerComponent : IAnimancerComponent
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="DummyAnimancerComponent"/>.</summary>
            public DummyAnimancerComponent(Animator animator, AnimancerPlayable playable)
            {
                Animator = animator;
                Playable = playable;
                InitialUpdateMode = animator.updateMode;
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns true.</summary>
            public bool enabled => true;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns the <see cref="Animator"/>'s <see cref="GameObject"/>.</summary>
            public GameObject gameObject => Animator.gameObject;

            /// <summary>[<see cref="IAnimancerComponent"/>] The target <see cref="UnityEngine.Animator"/>.</summary>
            public Animator Animator { get; set; }

            /// <summary>[<see cref="IAnimancerComponent"/>] The target <see cref="AnimancerPlayable"/>.</summary>
            public AnimancerPlayable Playable { get; private set; }

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns true.</summary>
            public bool IsPlayableInitialized => true;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns false.</summary>
            public bool ResetOnDisable => false;

            /// <summary>[<see cref="IAnimancerComponent"/>] The <see cref="Animator.updateMode"/>.</summary>
            public AnimatorUpdateMode UpdateMode
            {
                get => Animator.updateMode;
                set => Animator.updateMode = value;
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns the `clip`.</summary>
            public object GetKey(AnimationClip clip) => clip;

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns null.</summary>
            public string AnimatorFieldName => null;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns null.</summary>
            public string ActionOnDisableFieldName => null;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns the <see cref="Animator.updateMode"/> from when this object was created.</summary>
            public AnimatorUpdateMode? InitialUpdateMode { get; private set; }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

