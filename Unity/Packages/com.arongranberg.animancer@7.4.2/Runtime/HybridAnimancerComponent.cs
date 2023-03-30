// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// A <see cref="NamedAnimancerComponent"/> which plays a main <see cref="RuntimeAnimatorController"/> with the
    /// ability to play other individual <see cref="AnimationClip"/>s separately.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/animator-controllers#hybrid">Hybrid</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/HybridAnimancerComponent
    /// 
    [AddComponentMenu(Strings.MenuPrefix + "Hybrid Animancer Component")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(HybridAnimancerComponent))]
    public class HybridAnimancerComponent : NamedAnimancerComponent
    {
        /************************************************************************************************************************/
        #region Controller
        /************************************************************************************************************************/

        [SerializeField, Tooltip("The main Animator Controller that this object will play")]
        private ControllerTransition _Controller;

        /// <summary>[<see cref="SerializeField"/>]
        /// The transition containing the main <see cref="RuntimeAnimatorController"/> that this object plays.
        /// </summary>
        public ref ControllerTransition Controller => ref _Controller;

        /************************************************************************************************************************/

        /// <summary>
        /// Transitions to the <see cref="Controller"/> over its specified
        /// <see cref="AnimancerTransition{TState}.FadeDuration"/> and returns the
        /// <see cref="AnimancerTransition{TState}.State"/>.
        /// </summary>
        public ControllerState PlayController()
        {
            if (!_Controller.IsValid())
                return null;

            Play(_Controller);
            return _Controller.State;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="Playable"/> of the <see cref="ControllerState"/>.</summary>
        public AnimatorControllerPlayable ControllerPlayable
            => _Controller.State.Playable;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// Sets <see cref="PlayAutomatically"/> = false by default so that <see cref="OnEnable"/> will play the
        /// <see cref="Controller"/> instead of the first animation in the
        /// <see cref="NamedAnimancerComponent.Animations"/> array.
        /// </summary>
        /// <remarks>
        /// Called by the Unity Editor when this component is first added (in Edit Mode) and whenever the Reset command
        /// is executed from its context menu.
        /// </remarks>
        protected override void Reset()
        {
            base.Reset();

            if (Animator != null)
            {
                Controller = Animator.runtimeAnimatorController;
                Animator.runtimeAnimatorController = null;
            }

            PlayAutomatically = false;
        }
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// Plays the <see cref="Controller"/> if <see cref="PlayAutomatically"/> is false (otherwise it plays the
        /// first animation in the <see cref="NamedAnimancerComponent.Animations"/> array).
        /// </summary>
        protected override void OnEnable()
        {
            if (!TryGetAnimator())
                return;

            PlayController();
            base.OnEnable();

#if UNITY_ASSERTIONS
            if (Animator != null && Animator.runtimeAnimatorController != null)
                OptionalWarning.NativeControllerHybrid.Log($"An Animator Controller is assigned to the" +
                    $" {nameof(Animator)} component while also using a {nameof(HybridAnimancerComponent)}." +
                    $" Most likely only one of them is being used so the other should be removed." +
                    $" See the documentation for more information: {Strings.DocsURLs.AnimatorControllers}", this);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets <see cref="AnimancerPlayable.KeepChildrenConnected"/> to <c>true</c> in order to avoid some
        /// undesirable behaviours caused by disconnecting <see cref="AnimatorControllerPlayable"/>s from the graph.
        /// </summary>
        protected override void OnInitializePlayable()
        {
            base.OnInitializePlayable();
            Playable.KeepChildrenConnected = true;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            base.GatherAnimationClips(clips);
            clips.GatherFromSource(_Controller);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Animator Wrappers
        /************************************************************************************************************************/
        #region Properties
        /************************************************************************************************************************/

        /// <summary><see cref="AnimancerPlayable.Graph"/></summary>
        public PlayableGraph playableGraph => Playable.Graph;

        /// <summary><see cref="Controller"/></summary>
        public RuntimeAnimatorController runtimeAnimatorController
        {
            get => Controller.Controller;
            set => Controller.Controller = value;
        }

        /// <summary>[Warning] <see cref="HybridAnimancerComponent"/> doesn't support speed control.</summary>
        /// <remarks>
        /// If you don't need this feature, you can use <c>#pragma warning disable CS0618</c> to disable this warning.
        /// Otherwise, check https://kybernetik.com.au/animancer/docs/manual/animator-controllers for other options.
        /// </remarks>
#if UNITY_ASSERTIONS
        [Obsolete(nameof(HybridAnimancerComponent) + " doesn't support speed control." +
            " If you don't need this feature, you can use `#pragma warning disable CS0618` to disable this warning." +
            " Otherwise, check " + Strings.DocsURLs.AnimatorControllers + " for other options.")]
#endif
        public float speed
        {
            get => Animator.speed;
            set => Animator.speed = value;
        }

        /************************************************************************************************************************/
        // Root Motion.
        /************************************************************************************************************************/

        /// <summary><see cref="Animator.applyRootMotion"/></summary>
        public bool applyRootMotion
        {
            get => Animator.applyRootMotion;
            set => Animator.applyRootMotion = value;
        }

        /// <summary><see cref="Animator.bodyRotation"/></summary>
        public Quaternion bodyRotation
        {
            get => Animator.bodyRotation;
            set => Animator.bodyRotation = value;
        }

        /// <summary><see cref="Animator.bodyPosition"/></summary>
        public Vector3 bodyPosition
        {
            get => Animator.bodyPosition;
            set => Animator.bodyPosition = value;
        }

        /// <summary><see cref="Animator.gravityWeight"/></summary>
        public float gravityWeight => Animator.gravityWeight;

        /// <summary><see cref="Animator.hasRootMotion"/></summary>
        public bool hasRootMotion => Animator.hasRootMotion;

        /// <summary><see cref="Animator.layersAffectMassCenter"/></summary>
        public bool layersAffectMassCenter
        {
            get => Animator.layersAffectMassCenter;
            set => Animator.layersAffectMassCenter = value;
        }

        /// <summary><see cref="Animator.pivotPosition"/></summary>
        public Vector3 pivotPosition => Animator.pivotPosition;

        /// <summary><see cref="Animator.pivotWeight"/></summary>
        public float pivotWeight => Animator.pivotWeight;

        /// <summary><see cref="Animator.rootRotation"/></summary>
        public Quaternion rootRotation
        {
            get => Animator.rootRotation;
            set => Animator.rootRotation = value;
        }

        /// <summary><see cref="Animator.rootPosition"/></summary>
        public Vector3 rootPosition
        {
            get => Animator.rootPosition;
            set => Animator.rootPosition = value;
        }

        /// <summary><see cref="Animator.angularVelocity"/></summary>
        public Vector3 angularVelocity => Animator.angularVelocity;

        /// <summary><see cref="Animator.velocity"/></summary>
        public Vector3 velocity => Animator.velocity;

        /// <summary><see cref="Animator.deltaRotation"/></summary>
        public Quaternion deltaRotation => Animator.deltaRotation;

        /// <summary><see cref="Animator.deltaPosition"/></summary>
        public Vector3 deltaPosition => Animator.deltaPosition;

        /// <summary><see cref="Animator.ApplyBuiltinRootMotion"/></summary>
        public void ApplyBuiltinRootMotion() => Animator.ApplyBuiltinRootMotion();

        /************************************************************************************************************************/
        // Feet.
        /************************************************************************************************************************/

        /// <summary><see cref="Animator.feetPivotActive"/></summary>
        public float feetPivotActive
        {
            get => Animator.feetPivotActive;
            set => Animator.feetPivotActive = value;
        }

        /// <summary><see cref="Animator.stabilizeFeet"/></summary>
        public bool stabilizeFeet
        {
            get => Animator.stabilizeFeet;
            set => Animator.stabilizeFeet = value;
        }

        /// <summary><see cref="Animator.rightFeetBottomHeight"/></summary>
        public float rightFeetBottomHeight => Animator.rightFeetBottomHeight;

        /// <summary><see cref="Animator.leftFeetBottomHeight"/></summary>
        public float leftFeetBottomHeight => Animator.leftFeetBottomHeight;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Cross Fade
        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using normalized times.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerPlayable.DefaultFadeDuration"/>.</remarks>
        public void CrossFade(
            int stateNameHash,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);
            var controllerState = PlayController();
            controllerState.Playable.CrossFade(stateNameHash, fadeDuration, layer, normalizedTime);
        }

        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using normalized times.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerPlayable.DefaultFadeDuration"/>.</remarks>
        public AnimancerState CrossFade(
            string stateName,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);

            if (States.TryGet(name, out var state))
            {
                Play(state, fadeDuration);

                if (layer >= 0)
                    state.LayerIndex = layer;

                if (normalizedTime != float.NegativeInfinity)
                    state.NormalizedTime = normalizedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.CrossFade(stateName, fadeDuration, layer, normalizedTime);
                return controllerState;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using times in seconds.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerPlayable.DefaultFadeDuration"/>.</remarks>
        public void CrossFadeInFixedTime(
            int stateNameHash,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);
            var controllerState = PlayController();
            controllerState.Playable.CrossFadeInFixedTime(stateNameHash, fadeDuration, layer, fixedTime);
        }

        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using times in seconds.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerPlayable.DefaultFadeDuration"/>.</remarks>
        public AnimancerState CrossFadeInFixedTime(
            string stateName,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);

            if (States.TryGet(name, out var state))
            {
                Play(state, fadeDuration);

                if (layer >= 0)
                    state.LayerIndex = layer;

                state.Time = fixedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.CrossFadeInFixedTime(stateName, fadeDuration, layer, fixedTime);
                return controllerState;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play
        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular normalized time.</summary>
        public void Play(
            int stateNameHash,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            var controllerState = PlayController();
            controllerState.Playable.Play(stateNameHash, layer, normalizedTime);
        }

        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular normalized time.</summary>
        public AnimancerState Play(
            string stateName,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            if (States.TryGet(name, out var state))
            {
                Play(state);

                if (layer >= 0)
                    state.LayerIndex = layer;

                if (normalizedTime != float.NegativeInfinity)
                    state.NormalizedTime = normalizedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.Play(stateName, layer, normalizedTime);
                return controllerState;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular time (in seconds).</summary>
        public void PlayInFixedTime(
            int stateNameHash,
            int layer = -1,
            float fixedTime = 0)
        {
            var controllerState = PlayController();
            controllerState.Playable.PlayInFixedTime(stateNameHash, layer, fixedTime);
        }

        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular time (in seconds).</summary>
        public AnimancerState PlayInFixedTime(
            string stateName,
            int layer = -1,
            float fixedTime = 0)
        {
            if (States.TryGet(name, out var state))
            {
                Play(state);

                if (layer >= 0)
                    state.LayerIndex = layer;

                state.Time = fixedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.PlayInFixedTime(stateName, layer, fixedTime);
                return controllerState;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Parameters
        /************************************************************************************************************************/

        /// <summary>Gets the value of the specified boolean parameter.</summary>
        public bool GetBool(int id) => ControllerPlayable.GetBool(id);
        /// <summary>Gets the value of the specified boolean parameter.</summary>
        public bool GetBool(string name) => ControllerPlayable.GetBool(name);
        /// <summary>Sets the value of the specified boolean parameter.</summary>
        public void SetBool(int id, bool value) => ControllerPlayable.SetBool(id, value);
        /// <summary>Sets the value of the specified boolean parameter.</summary>
        public void SetBool(string name, bool value) => ControllerPlayable.SetBool(name, value);

        /// <summary>Gets the value of the specified float parameter.</summary>
        public float GetFloat(int id) => ControllerPlayable.GetFloat(id);
        /// <summary>Gets the value of the specified float parameter.</summary>
        public float GetFloat(string name) => ControllerPlayable.GetFloat(name);
        /// <summary>Sets the value of the specified float parameter.</summary>
        public void SetFloat(int id, float value) => ControllerPlayable.SetFloat(id, value);
        /// <summary>Sets the value of the specified float parameter.</summary>
        public void SetFloat(string name, float value) => ControllerPlayable.SetFloat(name, value);

        /// <summary>Sets the value of the specified float parameter with smoothing.</summary>
        public float SetFloat(string name, float value, float dampTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
            => _Controller.State.SetFloat(name, value, dampTime, deltaTime, maxSpeed);

        /// <summary>Sets the value of the specified float parameter with smoothing.</summary>
        public float SetFloat(int id, float value, float dampTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
            => _Controller.State.SetFloat(name, value, dampTime, deltaTime, maxSpeed);

        /// <summary>Gets the value of the specified integer parameter.</summary>
        public int GetInteger(int id) => ControllerPlayable.GetInteger(id);
        /// <summary>Gets the value of the specified integer parameter.</summary>
        public int GetInteger(string name) => ControllerPlayable.GetInteger(name);
        /// <summary>Sets the value of the specified integer parameter.</summary>
        public void SetInteger(int id, int value) => ControllerPlayable.SetInteger(id, value);
        /// <summary>Sets the value of the specified integer parameter.</summary>
        public void SetInteger(string name, int value) => ControllerPlayable.SetInteger(name, value);

        /// <summary>Sets the specified trigger parameter to true.</summary>
        public void SetTrigger(int id) => ControllerPlayable.SetTrigger(id);
        /// <summary>Sets the specified trigger parameter to true.</summary>
        public void SetTrigger(string name) => ControllerPlayable.SetTrigger(name);
        /// <summary>Resets the specified trigger parameter to false.</summary>
        public void ResetTrigger(int id) => ControllerPlayable.ResetTrigger(id);
        /// <summary>Resets the specified trigger parameter to false.</summary>
        public void ResetTrigger(string name) => ControllerPlayable.ResetTrigger(name);

        /// <summary>Indicates whether the specified parameter is controlled by an <see cref="AnimationClip"/>.</summary>
        public bool IsParameterControlledByCurve(int id) => ControllerPlayable.IsParameterControlledByCurve(id);
        /// <summary>Indicates whether the specified parameter is controlled by an <see cref="AnimationClip"/>.</summary>
        public bool IsParameterControlledByCurve(string name) => ControllerPlayable.IsParameterControlledByCurve(name);

        /// <summary>Gets the details of one of the <see cref="Controller"/>'s parameters.</summary>
        public AnimatorControllerParameter GetParameter(int index) => ControllerPlayable.GetParameter(index);
        /// <summary>Gets the number of parameters in the <see cref="Controller"/>.</summary>
        public int GetParameterCount() => ControllerPlayable.GetParameterCount();

        /************************************************************************************************************************/

        /// <summary>The number of parameters in the <see cref="Controller"/>.</summary>
        public int parameterCount => ControllerPlayable.GetParameterCount();

        /// <summary>The parameters in the <see cref="Controller"/>.</summary>
        /// <remarks>
        /// This property allocates a new array when first accessed. To avoid that, you can use
        /// <see cref="GetParameterCount"/> and <see cref="GetParameter"/> instead.
        /// </remarks>
        public AnimatorControllerParameter[] parameters => _Controller.State.parameters;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Other
        /************************************************************************************************************************/
        // Clips.
        /************************************************************************************************************************/

        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being played.</summary>
        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex = 0) => ControllerPlayable.GetCurrentAnimatorClipInfo(layerIndex);
        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being played.</summary>
        public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => ControllerPlayable.GetCurrentAnimatorClipInfo(layerIndex, clips);
        /// <summary>Gets the number of <see cref="AnimationClip"/>s currently being played.</summary>
        public int GetCurrentAnimatorClipInfoCount(int layerIndex = 0) => ControllerPlayable.GetCurrentAnimatorClipInfoCount(layerIndex);

        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being transitioned towards.</summary>
        public AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex = 0) => ControllerPlayable.GetNextAnimatorClipInfo(layerIndex);
        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being transitioned towards.</summary>
        public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => ControllerPlayable.GetNextAnimatorClipInfo(layerIndex, clips);
        /// <summary>Gets the number of <see cref="AnimationClip"/>s currently being transitioned towards.</summary>
        public int GetNextAnimatorClipInfoCount(int layerIndex = 0) => ControllerPlayable.GetNextAnimatorClipInfoCount(layerIndex);

        /************************************************************************************************************************/
        // Humanoid.
        /************************************************************************************************************************/

        /// <summary><see cref="Animator.humanScale"/></summary>
        public float humanScale => Animator.humanScale;

        /// <summary><see cref="Animator.isHuman"/></summary>
        public bool isHuman => Animator.isHuman;

        /// <summary><see cref="Animator.GetBoneTransform"/></summary>
        public Transform GetBoneTransform(HumanBodyBones humanBoneId) => Animator.GetBoneTransform(humanBoneId);

        /// <summary><see cref="Animator.SetBoneLocalRotation"/></summary>
        public void SetBoneLocalRotation(HumanBodyBones humanBoneId, Quaternion rotation) => Animator.SetBoneLocalRotation(humanBoneId, rotation);

        /************************************************************************************************************************/
        // Layers.
        /************************************************************************************************************************/

        /// <summary>Gets the number of layers in the <see cref="Controller"/>.</summary>
        public int GetLayerCount() => ControllerPlayable.GetLayerCount();
        /// <summary>The number of layers in the <see cref="Controller"/>.</summary>
        public int layerCount => ControllerPlayable.GetLayerCount();

        /// <summary>Gets the index of the layer with the specified name.</summary>
        public int GetLayerIndex(string layerName) => ControllerPlayable.GetLayerIndex(layerName);
        /// <summary>Gets the name of the layer with the specified index.</summary>
        public string GetLayerName(int layerIndex) => ControllerPlayable.GetLayerName(layerIndex);

        /// <summary>Gets the weight of the layer at the specified index.</summary>
        public float GetLayerWeight(int layerIndex) => ControllerPlayable.GetLayerWeight(layerIndex);
        /// <summary>Sets the weight of the layer at the specified index.</summary>
        public void SetLayerWeight(int layerIndex, float weight) => ControllerPlayable.SetLayerWeight(layerIndex, weight);

        /************************************************************************************************************************/
        // StateMachineBehaviours.
        /************************************************************************************************************************/

        /// <summary><see cref="Animator.GetBehaviour{T}()"/></summary>
        public T GetBehaviour<T>() where T : StateMachineBehaviour => Animator.GetBehaviour<T>();

        /// <summary><see cref="Animator.GetBehaviours{T}()"/></summary>
        public T[] GetBehaviours<T>() where T : StateMachineBehaviour => Animator.GetBehaviours<T>();

        /// <summary><see cref="Animator.GetBehaviours"/></summary>
        public StateMachineBehaviour[] GetBehaviours(int fullPathHash, int layerIndex) => Animator.GetBehaviours(fullPathHash, layerIndex);

        /************************************************************************************************************************/
        // States.
        /************************************************************************************************************************/

        /// <summary>Returns information about the current state.</summary>
        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0) => ControllerPlayable.GetCurrentAnimatorStateInfo(layerIndex);
        /// <summary>Returns information about the next state being transitioned towards.</summary>
        public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex = 0) => ControllerPlayable.GetNextAnimatorStateInfo(layerIndex);

        /// <summary>Indicates whether the specified layer contains the specified state.</summary>
        public bool HasState(int layerIndex, int stateID) => ControllerPlayable.HasState(layerIndex, stateID);

        /************************************************************************************************************************/
        // Transitions.
        /************************************************************************************************************************/

        /// <summary>Indicates whether the specified layer is currently executing a transition.</summary>
        public bool IsInTransition(int layerIndex = 0) => ControllerPlayable.IsInTransition(layerIndex);

        /// <summary>Gets information about the current transition.</summary>
        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex = 0) => ControllerPlayable.GetAnimatorTransitionInfo(layerIndex);

        /************************************************************************************************************************/
        // Other.
        /************************************************************************************************************************/

        /// <summary><see cref="Animator.avatar"/></summary>
        public Avatar avatar
        {
            get => Animator.avatar;
            set => Animator.avatar = value;
        }

        /// <summary><see cref="Animator.cullingMode"/></summary>
        public AnimatorCullingMode cullingMode
        {
            get => Animator.cullingMode;
            set => Animator.cullingMode = value;
        }

        /// <summary><see cref="Animator.fireEvents"/></summary>
        public bool fireEvents
        {
            get => Animator.fireEvents;
            set => Animator.fireEvents = value;
        }

        /// <summary><see cref="Animator.hasBoundPlayables"/></summary>
        public bool hasBoundPlayables => Animator.hasBoundPlayables;

        /// <summary><see cref="Animator.hasTransformHierarchy"/></summary>
        public bool hasTransformHierarchy => Animator.hasTransformHierarchy;

        /// <summary><see cref="Animator.isInitialized"/></summary>
        public bool isInitialized => Animator.isInitialized;

        /// <summary><see cref="Animator.isOptimizable"/></summary>
        public bool isOptimizable => Animator.isOptimizable;

        /// <summary><see cref="Animator.logWarnings"/></summary>
        public bool logWarnings
        {
            get => Animator.logWarnings;
            set => Animator.logWarnings = value;
        }

        /// <summary><see cref="Animator.updateMode"/></summary>
        /// <remarks>Changing this at runtime doesn't work when using the Playables API.</remarks>
        public AnimatorUpdateMode updateMode
        {
            get => Animator.updateMode;
            set => Animator.updateMode = value;
        }

        /************************************************************************************************************************/

#if UNITY_2022_2_OR_NEWER
        /// <summary><see cref="Animator.keepAnimatorStateOnDisable"/></summary>
        public bool keepAnimatorStateOnDisable
        {
            get => Animator.keepAnimatorStateOnDisable;
            set => Animator.keepAnimatorStateOnDisable = value;
        }
#else
        /// <summary><see cref="Animator.keepAnimatorControllerStateOnDisable"/></summary>
        public bool keepAnimatorControllerStateOnDisable
        {
            get => Animator.keepAnimatorControllerStateOnDisable;
            set => Animator.keepAnimatorControllerStateOnDisable = value;
        }
#endif

        /************************************************************************************************************************/

        /// <summary><see cref="Animator.Rebind"/></summary>
        public void Rebind() => Animator.Rebind();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }

    /// <summary>Extension methods for <see cref="HybridAnimancerComponent"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/HybridAnimancerComponentExtensions
    /// 
    public static class HybridAnimancerComponentExtensions
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Advances time by the specified value (in seconds) and immediately applies the current states of all
        /// animations to the animated objects.
        /// </summary>
        /// <remarks>
        /// This is an extension method to avoid being treated as a <see cref="MonoBehaviour"/> <code>Update</code>
        /// message and getting called every frame.
        /// </remarks>
        public static void Update(this HybridAnimancerComponent animancer, float deltaTime)
            => animancer.Evaluate(deltaTime);

        /************************************************************************************************************************/
    }
}
