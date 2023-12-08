// #define ENABLE_LUBAN_TEST

// using Sirenix.OdinInspector;
using UnityEngine;

// #if ENABLE_LUBAN_TEST
namespace Luban.Editor
{
    // [LabelText("测试/生成前")]
    public class TestBeforeGen : IBeforeGen
    {
        public void Process()
        {
            Debug.Log("Before");
        }
    }

    // [LabelText("测试/生成后")]
    public class TestAfterGen : IAfterGen
    {
        public void Process()
        {
            Debug.Log("After");
        }
    }
}

// #endif