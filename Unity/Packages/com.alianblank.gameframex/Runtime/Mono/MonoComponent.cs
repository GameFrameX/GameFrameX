using System;
using GameFrameX.Mono;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Mono 组件
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Mono")]
    public class MonoComponent : GameFrameworkComponent
    {
        private MonoManager _monoManager;

        protected override void Awake()
        {
            base.Awake();
            new MonoManager();
            _monoManager = GameFrameworkEntry.GetModule<MonoManager>();
            if (_monoManager == null)
            {
                Log.Fatal("Mono manager is invalid.");
                return;
            }
        }


        private void FixedUpdate()
        {
            _monoManager.FixedUpdate();
        }

        private void LateUpdate()
        {
            _monoManager.LateUpdate();
        }

        private void OnDestroy()
        {
            _monoManager.OnDestroy();
        }

        private void OnDrawGizmos()
        {
            _monoManager.OnDrawGizmos();
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            _monoManager.OnApplicationFocus(focusStatus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _monoManager.OnApplicationPause(pauseStatus);
        }

        public void AddLateUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddLateUpdateListener(fun);
        }

        public void RemoveLateUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveLateUpdateListener(fun);
        }

        public void AddFixedUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddFixedUpdateListener(fun);
        }

        public void RemoveFixedUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveFixedUpdateListener(fun);
        }

        public void AddUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddUpdateListener(fun);
        }

        public void RemoveUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveUpdateListener(fun);
        }

        public void AddDestroyListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddDestroyListener(fun);
        }

        public void RemoveDestroyListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveDestroyListener(fun);
        }

        public void AddOnDrawGizmosListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddOnDrawGizmosListener(fun);
        }

        public void RemoveOnDrawGizmosListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveOnDrawGizmosListener(fun);
        }

        public void AddOnApplicationPauseListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddOnApplicationPauseListener(fun);
        }

        public void RemoveOnApplicationPauseListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveOnApplicationPauseListener(fun);
        }

        public void AddOnApplicationFocusListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddOnApplicationFocusListener(fun);
        }

        public void RemoveOnApplicationFocusListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveOnApplicationFocusListener(fun);
        }
    }
}