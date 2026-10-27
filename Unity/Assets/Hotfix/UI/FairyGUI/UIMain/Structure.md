# UIMain 包结构

> 由 gencode 插件自动生成（发布时更新），请勿手改。
> 标注说明：`Wrapper·FGUI类型·mode`；`→ varName` 为生成的 C# 属性名（m_ 前缀）；`[无绑定]` 表示该节点未生成 C# 属性（无名称或 n+数字 自动名）；`[pkg=X]` 跨包引用；`← 资源名` 图片资源；`"文本"` 文案；`(↺)` 循环引用已省略。

## BagButton (GButton, 326x100)

```text
├─ n0 (GGraph, 325x100) [无绑定]
└─ title (GTextField, 184x42) [无绑定] "获取背包信息"
```

## UIMain (UIMain·GComponent, 1080x2160) [exported]

```text
├─ bg (GLoader, 1080x2160) → m_bg
├─ production_button (GButton, 119x100) → m_production_button
│  ├─ n0 (GGraph, 325x100)
│  └─ title (GTextField, 184x42) "获取背包信息"
├─ bag_button (GButton, 119x100) → m_bag_button
│  ├─ n0 (GGraph, 325x100)
│  └─ title (GTextField, 184x42) "获取背包信息"
├─ room_button (GButton, 119x100) → m_room_button
│  ├─ n0 (GGraph, 325x100)
│  └─ title (GTextField, 184x42) "获取背包信息"
├─ player_icon (GLoader, 164x164) → m_player_icon
├─ player_name (GTextField, 376x61) → m_player_name
└─ player_level (GTextField, 376x61) → m_player_level
```
