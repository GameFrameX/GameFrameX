// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using Animancer.Units;
using UnityEngine;

namespace Animancer
{
    /// <summary>Various string constants used throughout <see cref="Animancer"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/Strings
    /// 
    public static class Strings
    {
        /************************************************************************************************************************/

        /// <summary>The name of this product.</summary>
        public const string ProductName = nameof(Animancer);

        /// <summary>The standard prefix for <see cref="AddComponentMenu"/>.</summary>
        public const string MenuPrefix = ProductName + "/";

        /// <summary>The standard prefix for the asset creation menu (for <see cref="UnityEditor.MenuItem"/>).</summary>
        public const string CreateMenuPrefix = "Assets/Create/" + MenuPrefix;

        /// <summary>The standard prefix for <see cref="AddComponentMenu"/> for the examples.</summary>
        public const string ExamplesMenuPrefix = MenuPrefix + "Examples/";

        /// <summary>The menu path of the <see cref="Editor.Tools.AnimancerToolsWindow"/>.</summary>
        public const string AnimancerToolsMenuPath = "Window/Animation/Animancer Tools";

        /// <summary>
        /// The base value for <see cref="CreateAssetMenuAttribute.order"/> to group
        /// "Assets/Create/Animancer/..." menu items just under "Avatar Mask".
        /// </summary>
        public const int AssetMenuOrder = 410;

        /************************************************************************************************************************/

        /// <summary>The conditional compilation symbol for Editor-Only code.</summary>
        public const string UnityEditor = "UNITY_EDITOR";

        /// <summary>The conditional compilation symbol for assertions.</summary>
        public const string Assertions = "UNITY_ASSERTIONS";

        /************************************************************************************************************************/

        /// <summary>4 spaces for indentation.</summary>
        public const string Indent = "    ";

        /// <summary>A prefix for tooltips on Pro-Only features.</summary>
        /// <remarks><c>"[Pro-Only] "</c> in Animancer Lite or <c>""</c> in Animancer Pro.</remarks>
        public const string ProOnlyTag = "";

        /************************************************************************************************************************/

        /// <summary>An error message for when <see cref="AnimancerUtilities.IsFinite(float)"/> fails.</summary>
        public const string MustBeFinite = "must not be NaN or Infinity";

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] A message for <see cref="OptionalWarning.AnimatorDisabled"/>.</summary>
        public const string AnimatorDisabledMessage
            = "The " + nameof(Animator) + " is disabled so Animancer won't be able to play animations.";
#endif

        /************************************************************************************************************************/

        /// <summary>URLs of various documentation pages.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/DocsURLs
        /// 
        public static class DocsURLs
        {
            /************************************************************************************************************************/

            /// <summary>The URL of the website where the Animancer documentation is hosted.</summary>
            public const string Documentation = "https://kybernetik.com.au/animancer";

            /// <summary>The URL of the website where the Animancer API documentation is hosted.</summary>
            public const string APIDocumentation = Documentation + "/api/" + nameof(Animancer);

            /// <summary>The URL of the website where the Animancer API documentation is hosted.</summary>
            public const string ExampleAPIDocumentation = APIDocumentation + ".Examples.";

            /// <summary>The email address which handles support for Animancer.</summary>
            public const string DeveloperEmail = "animancer@kybernetik.com.au";

            /// <summary>The URL of the file which lists Animancer's latest version.</summary>
            public const string LatestVersion = Documentation + "/latest-version.txt";

            /************************************************************************************************************************/

            public const string OptionalWarning = APIDocumentation + "/" + nameof(Animancer.OptionalWarning);

            /************************************************************************************************************************/
#if UNITY_ASSERTIONS
            /************************************************************************************************************************/

            public const string Docs = Documentation + "/docs/";

            public const string AnimancerEvents = Docs + "manual/events/animancer";
            public const string ClearAutomatically = AnimancerEvents + "/behaviour#clear-automatically";
            public const string SharedEventSequences = AnimancerEvents + "/shared";
            public const string AnimatorControllers = Docs + "manual/animator-controllers";
            public const string AnimatorControllersNative = AnimatorControllers + "#native";
            public const string Fading = Docs + "manual/blending/fading";
            public const string FadeModes = Fading + "/modes";

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
#if UNITY_EDITOR
            /************************************************************************************************************************/

            public const string KeepChildrenConnected = APIDocumentation + "/" + nameof(AnimancerPlayable) + "/FEFD6C35";

            public const string Examples = Docs + "examples";
            public const string UnevenGround = Docs + "examples/ik/uneven-ground";
            public const string OdinInspector = Docs + "examples/integration/odin-inspector";

            public const string AnimancerTools = Docs + "manual/tools";
            public const string PackTextures = AnimancerTools + "/pack-textures";
            public const string ModifySprites = AnimancerTools + "/modify-sprites";
            public const string RenameSprites = AnimancerTools + "/rename-sprites";
            public const string GenerateSpriteAnimations = AnimancerTools + "/generate-sprite-animations";
            public const string RemapSpriteAnimation = AnimancerTools + "/remap-sprite-animation";
            public const string RemapAnimationBindings = AnimancerTools + "/remap-animation-bindings";

            public const string Inspector = Docs + "manual/playing/inspector";
            public const string States = Docs + "manual/playing/states";

            public const string Layers = Docs + "manual/blending/layers";

            public const string EndEvents = Docs + "manual/events/end";

            public const string TransitionPreviews = Docs + "manual/transitions/previews";

            public const string UpdateModes = Docs + "bugs/update-modes";

            public const string ChangeLogPrefix = Docs + "changes/animancer-";

            public const string Forum = "https://forum.unity.com/threads/566452";

            public const string Issues = "https://github.com/KybernetikGames/animancer/issues";

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>Tooltips for various fields.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Tooltips
        /// 
        public static class Tooltips
        {
            /************************************************************************************************************************/

            public const string MiddleClickReset =
                "\n• Middle Click = reset to default value";

            public const string FadeDuration = ProOnlyTag +
                "The amount of time the transition will take, e.g:" +
                "\n• 0s = Instant" +
                "\n• 0.25s = quarter of a second (Default)" +
                "\n• 0.25x = quarter of the animation length" +
                "\n• " + AnimationTimeAttribute.Tooltip +
                MiddleClickReset;

            public const string Speed = ProOnlyTag +
                "How fast the animation will play, e.g:" +
                "\n• 0x = paused" +
                "\n• 1x = normal speed" +
                "\n• -2x = double speed backwards";

            public const string OptionalSpeed = Speed +
                "\n• Disabled = keep previous speed" +
                MiddleClickReset;

            public const string NormalizedStartTime = ProOnlyTag +
                "• Enabled = use " + nameof(FadeMode) + "." + nameof(FadeMode.FromStart) +
                " and always restart at this time." +
                "\n• Disabled = use " + nameof(FadeMode) + "." + nameof(FadeMode.FixedSpeed) +
                " and continue from the current time if already playing." +
                "\n• " + AnimationTimeAttribute.Tooltip;

            public const string EndTime = ProOnlyTag +
                "The time when the End Callback will be triggered." +
                "\n• " + AnimationTimeAttribute.Tooltip +
                "\n\nDisabling the toggle automates the value:" +
                "\n• Speed >= 0 ends at 1x" +
                "\n• Speed < 0 ends at 0x";

            public const string CallbackTime = ProOnlyTag +
                "The time when the Event Callback will be triggered." +
                "\n• " + AnimationTimeAttribute.Tooltip;

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

