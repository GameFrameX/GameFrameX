using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using GameFrameX.ProtoExport;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// 资源 key 完整性守护：ProtoExport 与 GUI 的中英两套 resx 必须保持同一 key 集合，
/// 防止新增文案只写了一侧、运行时回退成 key 本身（<see cref="Loc.Get"/> 的兜底行为）。
/// </summary>
public class LocalizationParityTests
{
    public static IEnumerable<object[]> ResourceManifests()
    {
        yield return new object[] { "GameFrameX.ProtoExport.Strings", typeof(Loc).Assembly };
        yield return new object[] { "ProtoExporterGUI.Resources.Strings", typeof(ProtoExporterGUI.Models.LockModuleRow).Assembly };
    }

    /// <summary>
    /// 中英资源 key 集合一致
    /// </summary>
    [Theory]
    [MemberData(nameof(ResourceManifests))]
    public void ChineseAndEnglishResourceKeys_HaveIdenticalSets(string baseName, System.Reflection.Assembly assembly)
    {
        var manager = new ResourceManager(baseName, assembly);

        var neutralKeys = new HashSet<string>(StringComparer.Ordinal);
        using (var set = manager.GetResourceSet(CultureInfo.InvariantCulture, true, true))
        {
            foreach (System.Collections.DictionaryEntry entry in set)
            {
                neutralKeys.Add((string)entry.Key);
            }
        }

        var enKeys = new HashSet<string>(StringComparer.Ordinal);
        using (var set = manager.GetResourceSet(CultureInfo.GetCultureInfo("en"), true, true))
        {
            foreach (System.Collections.DictionaryEntry entry in set)
            {
                enKeys.Add((string)entry.Key);
            }
        }

        Assert.True(neutralKeys.SetEquals(enKeys),
            $"resx key 不一致 [{baseName}]：仅中文有 {{{string.Join(", ", neutralKeys.Except(enKeys))}}}，仅英文有 {{{string.Join(", ", enKeys.Except(neutralKeys))}}}");
    }

    /// <summary>
    /// 中英资源的复合格式占位符逐 key 一致（引用的参数编号集合）。
    /// 语序随语言重排是合法翻译（如 zh "{0} 中的 {1}" / en "{1} in {0}"），不比较出现顺序；
    /// 但两侧引用的编号集合必须一致且从 0 连续无跳号——漏写一个 {0} 只会在运行时
    /// string.Format 抛 FormatException 或静默丢参数，此处在静态层面拦截。
    /// </summary>
    [Theory]
    [MemberData(nameof(ResourceManifests))]
    public void ChineseAndEnglishPlaceholders_ReferenceSameArgumentsPerKey(string baseName, System.Reflection.Assembly assembly)
    {
        var manager = new ResourceManager(baseName, assembly);
        var neutral = BuildResourceMap(manager, CultureInfo.InvariantCulture);
        var en = BuildResourceMap(manager, CultureInfo.GetCultureInfo("en"));

        foreach (var kv in neutral)
        {
            var zhSorted = ExtractPlaceholderIndices(kv.Value).OrderBy(x => x).ToArray();
            var enSorted = ExtractPlaceholderIndices(en[kv.Key]).OrderBy(x => x).ToArray();

            Assert.True(zhSorted.SequenceEqual(enSorted),
                $"占位符不一致 [{baseName}] key={kv.Key}：zh={DescribeIndices(zhSorted)} en={DescribeIndices(enSorted)}");

            // 引用的编号必须恰好是 0..n-1 的排列，防止 {0},{2} 这类跳号笔误
            for (var i = 0; i < zhSorted.Length; i++)
            {
                Assert.True(zhSorted[i] == i,
                    $"占位符跳号 [{baseName}] key={kv.Key}：引用了 {{{zhSorted[i]}}}，期望第 {i} 个引用为 {{{i}}}");
            }
        }
    }

    private static Dictionary<string, string> BuildResourceMap(ResourceManager manager, CultureInfo culture)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        using (var set = manager.GetResourceSet(culture, true, true))
        {
            foreach (System.Collections.DictionaryEntry entry in set)
            {
                map[(string)entry.Key] = entry.Value?.ToString() ?? string.Empty;
            }
        }

        return map;
    }

    private static int[] ExtractPlaceholderIndices(string value)
    {
        var pattern = new System.Text.RegularExpressions.Regex(@"\{(\d+)\}");
        var indices = new List<int>();
        foreach (System.Text.RegularExpressions.Match match in pattern.Matches(value))
        {
            indices.Add(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        }

        return indices.ToArray();
    }

    private static string DescribeIndices(int[] indices)
    {
        return "[" + string.Join(", ", indices.Select(x => "{" + x + "}")) + "]";
    }
}
