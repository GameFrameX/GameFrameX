# UILauncher 包结构

> 由 gencode 插件自动生成（发布时更新），请勿手改。
> 标注说明：`Wrapper·FGUI类型·mode`；`→ varName` 为生成的 C# 属性名（m_ 前缀）；`[无绑定]` 表示该节点未生成 C# 属性（无名称或 n+数字 自动名）；`[pkg=X]` 跨包引用；`← 资源名` 图片资源；`"文本"` 文案；`(↺)` 循环引用已省略。

## UILoadingMainProgressBar (GProgressBar, 1080x40)

```text
├─ n0 (GGraph, 1080x40) [无绑定]
└─ bar (GGraph, 1080x40) [无绑定]
```

## UILauncherUpgrade (GComponent, 1080x2160)

```text
├─ bg (GGraph, 1080x2160) → m_bg
├─ n29 (GImage, 972x572) [无绑定] ← Background
├─ enter_button (GButton, 480x128) → m_enter_button
│  ├─ icon (GImage, 480x128) ← UI_icon_btn_green
│  └─ title (GTextField, 104x66) "确定"
└─ text_content (GLabel, 972x640) → m_text_content
   └─ title (GRichTextField, 972x638) "更新文本更新文…"
```

## UILauncherUpgradeContent (GLabel, 972x640)

```text
└─ title (GRichTextField, 972x638) [无绑定] "更新文本更新文…"
```

## UILauncherEnterButton (GButton, 480x128)

```text
├─ icon (GImage, 480x128) [无绑定] ← UI_icon_btn_green
└─ title (GTextField, 104x66) [无绑定] "确定"
```

## UILauncher (UILauncher·GComponent, 1080x2160) [exported]

```text
Controllers: IsUpgrade(No, Yes), IsDownload(No, Yes)
├─ bg (GLoader, 1080x2160) → m_bg
├─ tip_text (GTextField, 972x50) → m_tip_text "正在下载游戏资…"
├─ progress_bar (GProgressBar, 1080x40) → m_progress_bar
│  ├─ n0 (GGraph, 1080x40)
│  └─ bar (GGraph, 1080x40)
└─ upgrade (GComponent, 1080x2160) → m_upgrade
   ├─ bg (GGraph, 1080x2160)
   ├─ n29 (GImage, 972x572) ← Background
   ├─ enter_button (GButton, 480x128)
   │  ├─ icon (GImage, 480x128) ← UI_icon_btn_green
   │  └─ title (GTextField, 104x66) "确定"
   └─ text_content (GLabel, 972x640)
      └─ title (GRichTextField, 972x638) "更新文本更新文…"
```
