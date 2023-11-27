//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Entity;
using UnityEngine;
using YooAsset;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 默认实体辅助器。
    /// </summary>
    public class DefaultEntityHelper : EntityHelperBase
    {
        private IAssetManager _assetManager = null;

        private AssetOperationHandle _assetOperationHandle;

        /// <summary>
        /// 实例化实体。
        /// </summary>
        /// <param name="entityAsset">要实例化的实体资源。</param>
        /// <returns>实例化后的实体。</returns>
        public override object InstantiateEntity(object entityAsset)
        {
            _assetOperationHandle = entityAsset as AssetOperationHandle;
            if (_assetOperationHandle != null)
            {
                return _assetOperationHandle.InstantiateSync();
            }
            else
            {
                Log.Error("entityAsset is AssetOperationHandle invalid.");
                return null;
            }
        }

        /// <summary>
        /// 创建实体。
        /// </summary>
        /// <param name="entityInstance">实体实例。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>实体。</returns>
        public override IEntity CreateEntity(object entityInstance, IEntityGroup entityGroup, object userData)
        {
            GameObject gameObject = entityInstance as GameObject;
            if (gameObject == null)
            {
                Log.Error("Entity instance is invalid.");
                return null;
            }

            Transform transform = gameObject.transform;
            transform.SetParent(((MonoBehaviour)entityGroup.Helper).transform);

            return gameObject.GetOrAddComponent<Entity>();
        }

        /// <summary>
        /// 释放实体。
        /// </summary>
        /// <param name="entityAsset">要释放的实体资源。</param>
        /// <param name="entityInstance">要释放的实体实例。</param>
        public override void ReleaseEntity(object entityAsset, object entityInstance)
        {
            AssetOperationHandle assetOperationHandle = entityAsset as AssetOperationHandle;
            if (assetOperationHandle == null)
            {
                Log.Error("entityAsset is AssetOperationHandle invalid.");
                return;
            }

            assetOperationHandle.Release();
            Destroy((Object)entityInstance);
        }

        private void Start()
        {
            _assetManager = GameFrameworkEntry.GetModule<IAssetManager>();
            if (_assetManager == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }
    }
}