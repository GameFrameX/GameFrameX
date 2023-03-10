using UnityEngine;

namespace Framework
{
    [DefaultExecutionOrder(-200)]
    public sealed class FrameworkStartup : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}