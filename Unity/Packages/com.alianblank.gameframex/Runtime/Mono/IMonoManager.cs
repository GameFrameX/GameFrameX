using System;

namespace GameFrameX.Mono
{
    public interface IMonoManager
    {
        void FixedUpdate();


        void LateUpdate();


        void OnDestroy();


        void OnDrawGizmos();


        void OnApplicationFocus(bool focusStatus);


        void OnApplicationPause(bool pauseStatus);


        void AddLateUpdateListener(Action fun);


        void RemoveLateUpdateListener(Action fun);


        void AddFixedUpdateListener(Action fun);


        void RemoveFixedUpdateListener(Action fun);


        void AddUpdateListener(Action fun);


        void RemoveUpdateListener(Action fun);


        void AddDestroyListener(Action fun);


        void RemoveDestroyListener(Action fun);


        void AddOnDrawGizmosListener(Action fun);


        void RemoveOnDrawGizmosListener(Action fun);


        void AddOnApplicationPauseListener(Action<bool> fun);


        void RemoveOnApplicationPauseListener(Action<bool> fun);


        void AddOnApplicationFocusListener(Action<bool> fun);


        void RemoveOnApplicationFocusListener(Action<bool> fun);
    }
}