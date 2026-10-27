# UIBag 包结构

> 由 gencode 插件自动生成（发布时更新），请勿手改。
> 标注说明：`Wrapper·FGUI类型·mode`；`→ varName` 为生成的 C# 属性名（m_ 前缀）；`[无绑定]` 表示该节点未生成 C# 属性（无名称或 n+数字 自动名）；`[pkg=X]` 跨包引用；`← 资源名` 图片资源；`"文本"` 文案；`(↺)` 循环引用已省略。

## UIBagContent (GComponent, 1000x1024)

```text
Controllers: IsSelectedItem(No, Yes)
├─ n22 (GImage, 1000x1024) [pkg=UICommon] [无绑定] ← 图层1_636
├─ n26 (GGraph, 1000x985) [无绑定]
├─ list (GList, 988x628) → m_list
│     └─ defaultItem: UIBagItem (UIBagItem·GComponent)
├─ info (GComponent, 940x200) → m_info
│  Controllers: IsCanUse(No, Yes)
│  ├─ n28 (GGraph, 940x200)
│  ├─ name_text (GTextField, 450x72) "夏日沙滩碎片"
│  ├─ desc_text (GRichTextField, 446x141) "消耗该随便可合…"
│  ├─ good_item (UICommonGoodItem·GButton, 160x160) [pkg=UICommon]
│  │  Controllers: button(up, down)
│  │  ├─ bg (GLoader, 160x160)
│  │  ├─ gift (GLoader, 123x99)
│  │  └─ number (GTextField, 120x42) "100"
│  ├─ use_button (UICommonYellowNormalButton·GButton, 242x100) [pkg=UICommon]
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ n0 (GImage, 120x76) [pkg=UICommon] ← 图层1_698
│  │  ├─ title (GTextField, 120x76)
│  │  └─ icon (GLoader, 120x76)
│  └─ get_source_button (UICommonBlueNormalButton·GButton, 242x100) [pkg=UICommon]
│     Controllers: button(up, down, over, selectedOver)
│     ├─ n0 (GImage, 132x72) [pkg=UICommon] ← 图层1_697
│     ├─ title (GTextField, 120x76)
│     └─ icon (GLoader, 120x76)
└─ type_list (GList, 960x124) → m_type_list
      └─ defaultItem: UIBagTypeItem (GComponent)
```

## UIBagItemInfo (GComponent, 940x200)

```text
Controllers: IsCanUse(No, Yes)
├─ n28 (GGraph, 940x200) [无绑定]
├─ name_text (GTextField, 450x72) → m_name_text "夏日沙滩碎片"
├─ desc_text (GRichTextField, 446x141) → m_desc_text "消耗该随便可合…"
├─ good_item (UICommonGoodItem·GButton, 160x160) [pkg=UICommon] → m_good_item
│  Controllers: button(up, down)
│  ├─ bg (GLoader, 160x160)
│  ├─ gift (GLoader, 123x99)
│  └─ number (GTextField, 120x42) "100"
├─ use_button (UICommonYellowNormalButton·GButton, 242x100) [pkg=UICommon] → m_use_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ n0 (GImage, 120x76) [pkg=UICommon] ← 图层1_698
│  ├─ title (GTextField, 120x76)
│  └─ icon (GLoader, 120x76)
└─ get_source_button (UICommonBlueNormalButton·GButton, 242x100) [pkg=UICommon] → m_get_source_button
   Controllers: button(up, down, over, selectedOver)
   ├─ n0 (GImage, 132x72) [pkg=UICommon] ← 图层1_697
   ├─ title (GTextField, 120x76)
   └─ icon (GLoader, 120x76)
```

## UIBagTypeItem (GButton, 200x124)

```text
Controllers: button(up, down)
├─ normal (GImage, 200x124) [pkg=UICommon] → m_normal ← 图层1_54
├─ select (GImage, 203x124) [pkg=UICommon] → m_select ← 图层1_6
├─ title (GTextField, 200x124) [无绑定]
└─ icon (GLoader, 200x124) [无绑定]
```

## UIBag (UIBag·GComponent, 1080x2160) [exported]

```text
├─ bg (GGraph, 1080x2160) → m_bg
├─ n39 (GTextField, 612x97) [无绑定] "点击空白处关闭"
└─ content (GComponent, 1000x1024) → m_content
   Controllers: IsSelectedItem(No, Yes)
   ├─ n22 (GImage, 1000x1024) [pkg=UICommon] ← 图层1_636
   ├─ n26 (GGraph, 1000x985)
   ├─ list (GList, 988x628)
   │     └─ defaultItem: UIBagItem (UIBagItem·GComponent)
   ├─ info (GComponent, 940x200)
   │  Controllers: IsCanUse(No, Yes)
   │  ├─ n28 (GGraph, 940x200)
   │  ├─ name_text (GTextField, 450x72) "夏日沙滩碎片"
   │  ├─ desc_text (GRichTextField, 446x141) "消耗该随便可合…"
   │  ├─ good_item (UICommonGoodItem·GButton, 160x160) [pkg=UICommon]
   │  │  Controllers: button(up, down)
   │  │  ├─ bg (GLoader, 160x160)
   │  │  ├─ gift (GLoader, 123x99)
   │  │  └─ number (GTextField, 120x42) "100"
   │  ├─ use_button (UICommonYellowNormalButton·GButton, 242x100) [pkg=UICommon]
   │  │  Controllers: button(up, down, over, selectedOver)
   │  │  ├─ n0 (GImage, 120x76) [pkg=UICommon] ← 图层1_698
   │  │  ├─ title (GTextField, 120x76)
   │  │  └─ icon (GLoader, 120x76)
   │  └─ get_source_button (UICommonBlueNormalButton·GButton, 242x100) [pkg=UICommon]
   │     Controllers: button(up, down, over, selectedOver)
   │     ├─ n0 (GImage, 132x72) [pkg=UICommon] ← 图层1_697
   │     ├─ title (GTextField, 120x76)
   │     └─ icon (GLoader, 120x76)
   └─ type_list (GList, 960x124)
         └─ defaultItem: UIBagTypeItem (GComponent)
```

## UIBagItem (UIBagItem·GButton, 136x136) [exported]

```text
Controllers: button(up, down)
├─ n37 (GImage, 134x136) [pkg=UICommon] [无绑定] ← 图层1_2
└─ good_item (UICommonGoodItem·GButton, 122x122) [pkg=UICommon] → m_good_item
   Controllers: button(up, down)
   ├─ bg (GLoader, 160x160)
   ├─ gift (GLoader, 123x99)
   └─ number (GTextField, 120x42) "100"
```
