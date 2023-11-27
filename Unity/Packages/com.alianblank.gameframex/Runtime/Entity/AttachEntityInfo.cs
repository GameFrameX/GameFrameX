//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX;
using UnityEngine;

namespace GameFrameX.Runtime
{
    internal sealed class AttachEntityInfo : IReference
    {
        private Transform _parentTransform;
        private object _userData;

        public AttachEntityInfo()
        {
            _parentTransform = null;
            _userData = null;
        }

        /// <summary>
        /// 父级对象
        /// </summary>
        public Transform ParentTransform
        {
            get { return _parentTransform; }
        }

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object UserData
        {
            get { return _userData; }
        }

        public static AttachEntityInfo Create(Transform parentTransform, object userData)
        {
            AttachEntityInfo attachEntityInfo = ReferencePool.Acquire<AttachEntityInfo>();
            attachEntityInfo._parentTransform = parentTransform;
            attachEntityInfo._userData = userData;
            return attachEntityInfo;
        }

        public void Clear()
        {
            _parentTransform = null;
            _userData = null;
        }
    }
}