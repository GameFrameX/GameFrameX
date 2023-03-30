using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using BestHTTP.Examples.Helpers;

namespace BestHTTP.Examples
{
    public class SampleRoot : MonoBehaviour
    {
#pragma warning disable 0649, 0169
        [Header("Common Properties")]
        public string BaseURL = "https://besthttpwebgldemo.azurewebsites.net";

        [Header("References")]

        [SerializeField]
        private Text _pluginVersion;

        [SerializeField]
        private Dropdown _logLevelDropdown;

        [SerializeField]
        private Text _proxyLabel;

        [SerializeField]
        private InputField _proxyInputField;

#pragma warning restore

        [SerializeField]
        public List<SampleBase> samples = new List<SampleBase>();

        [HideInInspector]
        public SampleBase selectedExamplePrefab;

        private void Start()
        {
            Application.runInBackground = true;

            this._pluginVersion.text = "Version: " + HTTPManager.UserAgent;

            int logLevel = PlayerPrefs.GetInt("BestHTTP.HTTPManager.Logger.Level", (int)HTTPManager.Logger.Level);
            this._logLevelDropdown.value = logLevel;
            HTTPManager.Logger.Level = (BestHTTP.Logger.Loglevels)logLevel;

#if (UNITY_WEBGL && !UNITY_EDITOR) || BESTHTTP_DISABLE_PROXY
            this._proxyLabel.gameObject.SetActive(false);
            this._proxyInputField.gameObject.SetActive(false);
#else
            string proxyURL = PlayerPrefs.GetString("BestHTTP.HTTPManager.Proxy", null);
            if (!string.IsNullOrEmpty(proxyURL))
            {
                try
                {
                    HTTPManager.Proxy = new HTTPProxy(new Uri(proxyURL), null, true);
#if UNITY_2019_1_OR_NEWER
                    this._proxyInputField.SetTextWithoutNotify(proxyURL);
#else
                    this._proxyInputField.onEndEdit.RemoveAllListeners();
                    this._proxyInputField.text = proxyURL;
                    this._proxyInputField.onEndEdit.AddListener(this.OnProxyEditEnd);
#endif
                }
                catch
                { }
            }
            else
                HTTPManager.Proxy = null;
#endif

#if !BESTHTTP_DISABLE_CACHING
            // Remove too old cache entries.
            BestHTTP.Caching.HTTPCacheService.BeginMaintainence(new BestHTTP.Caching.HTTPCacheMaintananceParams(TimeSpan.FromDays(30), ulong.MaxValue));
#endif
        }

        public void OnLogLevelChanged(int idx)
        {
            HTTPManager.Logger.Level = (BestHTTP.Logger.Loglevels)idx;
            PlayerPrefs.SetInt("BestHTTP.HTTPManager.Logger.Level", idx);
        }

        public void OnProxyEditEnd(string proxyURL)
        {
#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_PROXY
            try
            {
                if (string.IsNullOrEmpty(this._proxyInputField.text))
                    HTTPManager.Proxy = null;
                else
                    HTTPManager.Proxy = new HTTPProxy(new Uri(this._proxyInputField.text), null, true);

                PlayerPrefs.SetString("BestHTTP.HTTPManager.Proxy", this._proxyInputField.text);
            }
            catch
            { }
#endif
        }
    }
}
