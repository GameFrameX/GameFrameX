# UICommon 包结构

> 由 gencode 插件自动生成（发布时更新），请勿手改。
> 标注说明：`Wrapper·FGUI类型·mode`；`→ varName` 为生成的 C# 属性名（m_ 前缀）；`[无绑定]` 表示该节点未生成 C# 属性（无名称或 n+数字 自动名）；`[pkg=X]` 跨包引用；`← 资源名` 图片资源；`"文本"` 文案；`(↺)` 循环引用已省略。

## UICommonGoodItem (UICommonGoodItem·GButton, 160x160) [exported]

```text
Controllers: button(up, down)
├─ bg (GLoader, 160x160) → m_bg
├─ gift (GLoader, 123x99) → m_gift
└─ number (GTextField, 120x42) → m_number "100"
```

## UICommonYellowProgressBar (UICommonYellowProgressBar·GProgressBar, 182x70) [exported]

```text
├─ n0 (GImage, 182x70) [无绑定] ← bg
└─ bar (GImage, 182x68) [无绑定] ← bar
```

## UICommonYellowCheckButton (UICommonYellowCheckButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 120x76) [无绑定] ← 图层1_698
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonCloseButton (UICommonCloseButton·GButton, 80x80) [exported]

```text
Controllers: button(up, down, over, selectedOver)
└─ n0 (GImage, 80x80) [无绑定] ← 图层1_409
```

## UICommonSwitchButton (UICommonSwitchButton·GButton, 144x60) [exported]

```text
Controllers: button(up, down)
├─ n0 (GImage, 144x60) [无绑定] ← 图层1_131
├─ n1 (GImage, 144x60) [无绑定] ← 图层1_124
├─ icon (GLoader, 144x60) [无绑定]
├─ n3 (GImage, 81x53) [无绑定] ← switch_no
└─ n4 (GImage, 85x54) [无绑定] ← switch_yes
```

## UICommonDialogMessageBox (UICommonDialogMessageBox·GComponent, 1080x2340) [exported]

```text
├─ n8 (GGraph, 1080x2340) [无绑定]
├─ n3 (GImage, 842x532) [无绑定] ← dialog_bg
├─ enter_button (UICommonYellowCheckButton·GButton·Check, 196x76) → m_enter_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ n0 (GImage, 120x76) ← 图层1_698
│  ├─ title (GTextField, 120x76)
│  └─ icon (GLoader, 120x76)
├─ cancel_button (UICommonWhiteCheckButton·GButton·Check, 196x76) → m_cancel_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ n0 (GImage, 124x77) ← 图层1_636
│  ├─ title (GTextField, 120x76)
│  └─ icon (GLoader, 120x76)
├─ n5 (GGroup, 599x76) [无绑定]
├─ content (GRichTextField, 657x239) → m_content "较高冷淡开召开C…"
└─ n7 (GGroup, 842x532) [无绑定]
```

## UICommonWhiteCheckButton (UICommonWhiteCheckButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 124x77) [无绑定] ← 图层1_636
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonBlueCheckButton (UICommonBlueCheckButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 132x72) [无绑定] ← 图层1_697
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonYellowSingleButton (UICommonYellowSingleButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 120x76) [无绑定] ← 图层1_698
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonYellowNormalButton (UICommonYellowNormalButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 120x76) [无绑定] ← 图层1_698
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonBlueSingleButton (UICommonBlueSingleButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 132x72) [无绑定] ← 图层1_697
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonBlueNormalButton (UICommonBlueNormalButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 132x72) [无绑定] ← 图层1_697
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonWhiteSingleButton (UICommonWhiteSingleButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 124x77) [无绑定] ← 图层1_636
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonWhiteNormalButton (UICommonWhiteNormalButton·GButton, 120x76) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n0 (GImage, 124x77) [无绑定] ← 图层1_636
├─ title (GTextField, 120x76) [无绑定]
└─ icon (GLoader, 120x76) [无绑定]
```

## UICommonBackButton (UICommonBackButton·GButton, 200x200) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ n2 (GGraph, 200x200) [无绑定]
└─ n0 (GImage, 174x142) [无绑定] ← back
```

## UICommonDialogWithoutButton (UICommonDialogWithoutButton·GComponent, 1080x1920) [exported]

```text
├─ n2 (GLoader, 1080x1920) [无绑定]
└─ close_icon (UICommonCloseButton·GButton·Check, 80x80) → m_close_icon
   Controllers: button(up, down, over, selectedOver)
   └─ n0 (GImage, 80x80) ← 图层1_409
```
