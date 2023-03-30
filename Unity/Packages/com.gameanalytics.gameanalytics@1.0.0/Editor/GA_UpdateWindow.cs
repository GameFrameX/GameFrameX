using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using GameAnalyticsSDK.Setup;

namespace GameAnalyticsSDK.Editor
{
	public class GA_UpdateWindow : EditorWindow
	{
		private GUIContent _close					= new GUIContent("Skip", "Skip this version.");
		private GUIContent _download				= new GUIContent("Download Page", "Open the GameAnalytics download support page.");
		//private GUIContent _assetStore				= new GUIContent("AssetStore", "Open Unity Asset Store page in a browser window.");

		private Vector2 _scrollPos;
		
		void OnGUI ()
		{
			GUILayout.BeginHorizontal();
			
			GUILayout.Label(GameAnalytics.SettingsGA.Logo);
			
			GUILayout.BeginVertical();
			
			GUILayout.Label("A new version of the GameAnalytics Unity SDK is available");
			
			EditorGUILayout.Space();
			
			GUILayout.Label("Currently installed version: " + GameAnalyticsSDK.Setup.Settings.VERSION);
			GUILayout.Label("Latest version: " + GameAnalytics.SettingsGA.NewVersion);
			
			EditorGUILayout.Space();
			
			GUILayout.Label("Changes:");
			
			EditorGUILayout.Space();
			
			//EditorGUILayout.BeginVertical();
			
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width (320), GUILayout.Height (160));
			GUILayout.Label(new GUIContent(GameAnalytics.SettingsGA.Changes), EditorStyles.wordWrappedLabel);
			EditorGUILayout.EndScrollView();
			
			//EditorGUILayout.EndVertical();
			
			EditorGUILayout.Space();
			
			GUILayout.Label("You can download the latest version from the GameAnalytics download support page.", EditorStyles.wordWrappedMiniLabel, GUILayout.MaxWidth(320));
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			
			/*if (GUILayout.Button(_assetStore, GUILayout.MaxWidth(115)))
			{
				Application.OpenURL("https://www.assetstore.unity3d.com/#/content/6755");
			}*/
			
			if (GUILayout.Button(_download, GUILayout.MaxWidth(115)))
			{
				Application.OpenURL("http://download.gameanalytics.com/unity/GA_SDK_UNITY.unitypackage");
			}
			
			if (GUILayout.Button(_close, GUILayout.MaxWidth(72)))
			{
				EditorPrefs.SetString("ga_skip_version"+"-"+Application.dataPath, GameAnalytics.SettingsGA.NewVersion);
				Close();
			}
			
			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
	    }
		
		public static void SetNewVersion (string newVersion)
		{
			if (!string.IsNullOrEmpty(newVersion))
			{
				GameAnalytics.SettingsGA.NewVersion = newVersion;
			}
		}
		
		public static string GetNewVersion ()
		{
			return GameAnalytics.SettingsGA.NewVersion;
		}
		
		public static void SetChanges (string changes)
		{
			if (!string.IsNullOrEmpty(changes))
			{
				GameAnalytics.SettingsGA.Changes = changes;
			}
		}
		
		public static string UpdateStatus (string currentVersion)
		{
			try {
				int newV = int.Parse(GameAnalytics.SettingsGA.NewVersion.Replace(".",""));
				int oldV = int.Parse(currentVersion.Replace(".",""));

				if (newV > oldV)
					return "New update";
				else
					return "";
			} catch {
				return "";
			}
		}
	}
}