using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityGameFramework.Runtime
{
    public static class CameraHelper
    {
        /// <summary>
        /// 获取相机快照
        /// </summary>
        /// <param name="main">相机</param>
        /// <param name="scale">缩放比</param>
        public static Texture2D GetCaptureScreenshot(Camera main, float scale = 0.5f)
        {
            Rect rect = new Rect(0, 0, Screen.width * scale, Screen.height * scale);
            string name = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            RenderTexture renderTexture = RenderTexture.GetTemporary((int) rect.width, (int) rect.height, 0);
            renderTexture.name = SceneManager.GetActiveScene().name + "_" + renderTexture.width + "_" + renderTexture.height + "_" + name;
            main.targetTexture = renderTexture;
            main.Render();

            RenderTexture.active = renderTexture;
            Texture2D screenShot = new Texture2D((int) rect.width, (int) rect.height, TextureFormat.RGB24, false)
            {
                name = renderTexture.name
            };
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();
            main.targetTexture = null;
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            // icon.texture = new NTexture(screenShot);
            // BlurFilter blurFilter = new BlurFilter();
            // icon.filter = blurFilter;
            // icon.alpha = 0;
            // icon.TweenFade(1, 0.3f);
            return screenShot;
        }
    }
}