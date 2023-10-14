using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 管理所有UI 包
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/FUIPackage")]
    public sealed class FUIPackageComponent : GameFrameworkComponent
    {
        private readonly Dictionary<string, FairyGUI.UIPackage> _uiPackages = new Dictionary<string, UIPackage>(32);

        public void AddPackage(string descFilePath)
        {
            if (!_uiPackages.TryGetValue(descFilePath, out var package))
            {
                package = UIPackage.AddPackage(descFilePath);
                package.LoadAllAssets();
                _uiPackages.Add(descFilePath, package);
            }
        }

        public void RemovePackage(string descFilePath)
        {
            if (_uiPackages.TryGetValue(descFilePath, out var package))
            {
                UIPackage.RemovePackage(descFilePath);
                _uiPackages.Remove(descFilePath);
            }
        }

        public void RemoveAllPackages()
        {
            UIPackage.RemoveAllPackages();
            _uiPackages.Clear();
        }


        public bool Has(string uiPackageName)
        {
            return Get(uiPackageName) != null;
        }

        public UIPackage Get(string uiPackageName)
        {
            if (_uiPackages.TryGetValue(uiPackageName, out var package))
            {
                return package;
            }

            return null;
        }

        protected override void Awake()
        {
            base.Awake();
            // UIPackage.LoadResource
        }
    }
}