using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using vietlabs.fr2;

public class FR2_CSV
{
    private const string SEPARATOR = ",";
    public static string GetCSVRow(FR2_Ref r, params string[] suffixes)
    {
        var asset = r.asset;
        var sr = r.isSceneRef ? (FR2_SceneRef)r : null;
        var go = (GameObject)null;

        if (sr != null)
        {
            if (sr.component is Component)
            {
                go = ((Component)sr.component).gameObject;
            }

            if (sr.component is GameObject)
            {
                go = (GameObject)sr.component;
            }
        }

        var sb = new StringBuilder();

        sb.Append(r.depth);
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? sr.component.name : asset.assetName);
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? FR2_SceneRef.FindUsageScene(new[] { go }, false).Count : asset.UsageCount());
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? string.Empty : asset.extension);
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? "0" : asset.fileSize.ToString());
        sb.Append(SEPARATOR);

        var type = r.isSceneRef ? "SceneObject" : "(missing)";
        if (!r.isSceneRef)
        {
            var obj = AssetDatabase.GetMainAssetTypeAtPath(asset.assetPath);
            if (obj != null) type = obj.ToString();

            if (type.StartsWith("UnityEngine.") || type.StartsWith("UnityEditor."))
            {
                var idx = type.LastIndexOf(".", StringComparison.Ordinal) + 1;
                type = type.Substring(idx, type.Length - idx);
            }
        }

        sb.Append(type);
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? string.Empty : asset.guid);
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? string.Empty : asset.AtlasName);
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? string.Empty : asset.AssetBundleName);
        sb.Append(SEPARATOR);

        sb.Append(r.group);
        sb.Append(SEPARATOR);

        sb.Append(r.isSceneRef ? sr.sceneFullPath : asset.assetPath);

        foreach (var t in suffixes)
        {
            sb.Append(SEPARATOR);
            sb.Append(t);
        }

        return sb.ToString();
    }

    public static string GetCSVTitle()
    {
        var sb = new StringBuilder();

        sb.Append("depth");
        sb.Append(SEPARATOR);

        sb.Append("name");
        sb.Append(SEPARATOR);

        sb.Append("usage count");
        sb.Append(SEPARATOR);

        sb.Append("extension");
        sb.Append(SEPARATOR);

        sb.Append("size");
        sb.Append(SEPARATOR);

        sb.Append("type");
        sb.Append(SEPARATOR);

        sb.Append("guid");
        sb.Append(SEPARATOR);

        sb.Append("atlas");
        sb.Append(SEPARATOR);

        sb.Append("assetbundle");
        sb.Append(SEPARATOR);

        sb.Append("group");
        sb.Append(SEPARATOR);

        sb.Append("full path");

        return sb.ToString();
    }

    public static string GetCSVRows(FR2_Ref[] source)
    {
        if (source == null)
        {
            //Debug.LogWarning("source should not be null!");
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine(GetCSVTitle());
        foreach (var s in source)
        {
            if (s == null) continue;
            sb.AppendLine(GetCSVRow(s));
        }

        return sb.ToString();
    }
}
