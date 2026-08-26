using System.Net;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameFrameX;
using GameFrameX.Event.Runtime;
using GameFrameX.Localization.Runtime;
#if ENABLE_UI_FAIRYGUI
using GameFrameX.UI.FairyGUI.Runtime;
#endif
using GameFrameX.Network.Runtime;
using Hotfix.Proto;
using SimpleJSON;
using UnityEngine;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Hotfix.Config;
using Hotfix.Config.Local;
using Hotfix.Config.Tables;
using Hotfix.UI;
#if ENABLE_BINARY_CONFIG
using LuBan.Runtime;
#endif

namespace Hotfix
{
    public static class HotfixLauncher
    {
        public static void Main()
        {
            Log.Info("Hello World HybridCLR");
            ProtoMessageIdHandler.Init(HotfixProtoHandler.CurrentAssembly);
            LoadConfig();
            LoadUI();
        }

        private static async void LoadUI()
        {
#if ENABLE_UI_FAIRYGUI
            GameApp.FUIPackage.AddPackageAsync(Utility.Asset.Path.GetUIPackagePath(FUIPackage.UICommon));
            GameApp.FUIPackage.AddPackageAsync(Utility.Asset.Path.GetUIPackagePath(FUIPackage.UICommonAvatar));
            GameApp.FUIPackage.AddPackageAsync(Utility.Asset.Path.GetUIPackagePath(FUIPackage.UIRoom));
#endif
            await GameApp.UI.OpenAsync<UILogin>();
            var item = GameApp.Config.GetConfig<TbSoundsConfig>().FirstOrDefault;
            Log.Info(item);
        }

        static async void LoadConfig()
        {
            _tablesComponent= new TablesComponent();
            _tablesComponent.Init(GameApp.Config);
#if ENABLE_BINARY_CONFIG
            // 使用二进制配置表
            await _tablesComponent.LoadAsync(ConfigBufferLoader);
#else
            // 使用JSON配置表
            await _tablesComponent.LoadAsync(ConfigLoader);
#endif
            TranslateText();
        }
        static TablesComponent _tablesComponent;

        /// <summary>
        /// 翻译本地化
        /// </summary>
        private static void TranslateText()
        {
            // 1. 删除源数据
            GameApp.Localization.RemoveAllRawStrings();
            // 2. 从配置表中加载数据
            var tbLocalization = GameApp.Config.GetConfig<TbLocalization>();
            // 3. 装填数据信息
            tbLocalization.ForEach(localization =>
            {
                if (localization.Key.IsNullOrWhiteSpace())
                {
                    return;
                }

                var value = string.Empty;
                // 根据不同的语言读取不同的值，这个地方可以自行优化
                switch (GameApp.Localization.Language)
                {
                    case LocalizationCode.ChineseSimplified:
                        value = localization.ChineseSimplified;
                        break;
                    case LocalizationCode.ChineseTraditionalTW:
                        value = localization.ChineseTraditional;
                        break;
                    case LocalizationCode.ChineseTraditionalHK:
                        value = localization.ChineseTraditional;
                        break;
                    case LocalizationCode.English:
                        value = localization.English;
                        break;
                    case LocalizationCode.French:
                        value = localization.French;
                        break;
                    case LocalizationCode.German:
                        value = localization.German;
                        break;
                    case LocalizationCode.Italian:
                        value = localization.Italian;
                        break;
                    case LocalizationCode.Japanese:
                        value = localization.Japanese;
                        break;
                    case LocalizationCode.Korean:
                        value = localization.Korean;
                        break;
                    case LocalizationCode.Portuguese:
                    case LocalizationCode.PortugueseBR:
                    case LocalizationCode.PortugueseAO:
                    case LocalizationCode.PortugueseCV:
                    case LocalizationCode.PortugueseGW:
                    case LocalizationCode.PortugueseMZ:
                    case LocalizationCode.PortugueseST:
                    case LocalizationCode.PortugueseTL:
                        value = localization.PortuguesePortugal;
                        break;
                    case LocalizationCode.SpanishAR:
                    case LocalizationCode.SpanishBO:
                    case LocalizationCode.SpanishCL:
                    case LocalizationCode.SpanishCO:
                    case LocalizationCode.SpanishCR:
                    case LocalizationCode.SpanishDO:
                    case LocalizationCode.SpanishEC:
                    case LocalizationCode.SpanishGQ:
                    case LocalizationCode.SpanishGT:
                    case LocalizationCode.SpanishHN:
                    case LocalizationCode.SpanishMX:
                    case LocalizationCode.SpanishNI:
                    case LocalizationCode.SpanishPA:
                    case LocalizationCode.SpanishPE:
                    case LocalizationCode.SpanishPR:
                    case LocalizationCode.SpanishSV:
                    case LocalizationCode.SpanishUY:
                    case LocalizationCode.SpanishVE:
                        value = localization.Spanish;
                        break;
                    case LocalizationCode.Thai:
                        value = localization.Thai;
                        break;
                    case LocalizationCode.Vietnamese:
                        value = localization.Vietnamese;
                        break;
                    case LocalizationCode.Russian:
                        value = localization.Russian;
                        break;
                    case LocalizationCode.Indonesian:
                        value = localization.Indonesian;
                        break;
                    default:
                    {
                        if (GameApp.Localization.DefaultLanguage == LocalizationCode.English)
                        {
                            value = localization.English;
                        }
                    }
                        break;
                }

                GameApp.Localization.AddRawString(localization.Key, value);
            });

            // 刷新Luban配置表中的本地化内容
            _tablesComponent.SetTranslateText((key, y) =>
            {
                if (key.IsNullOrWhiteSpace())
                {
                    return string.Empty;
                }

                return GameApp.Localization.GetString(key);
            });
        }


#if ENABLE_BINARY_CONFIG
        /// <summary>
        /// 加载二进制配置表
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static async Task<ByteBuf> ConfigBufferLoader(string file)
        {
            var assetHandle = await GameApp.Asset.LoadAssetAsync<TextAsset>(Utility.Asset.Path.GetConfigPath(file, Utility.Const.FileNameSuffix.Binary));
            return ByteBuf.Wrap(assetHandle.GetAssetObject<TextAsset>().bytes);
        }
#else
        /// <summary>
        /// 加载json配置表
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static async Task<JSONNode> ConfigLoader(string file)
        {
            var assetHandle = await GameApp.Asset.LoadAssetAsync<TextAsset>(Utility.Asset.Path.GetConfigPath(file, Utility.Const.FileNameSuffix.Json));
            return JSON.Parse(assetHandle.GetAssetObject<TextAsset>().text);
        }
#endif
    }
}
