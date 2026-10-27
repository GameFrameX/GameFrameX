# UILogin 包结构

> 由 gencode 插件自动生成（发布时更新），请勿手改。
> 标注说明：`Wrapper·FGUI类型·mode`；`→ varName` 为生成的 C# 属性名（m_ 前缀）；`[无绑定]` 表示该节点未生成 C# 属性（无名称或 n+数字 自动名）；`[pkg=X]` 跨包引用；`← 资源名` 图片资源；`"文本"` 文案；`(↺)` 循环引用已省略。

## UILoginAnnouncementContent (GComponent, 972x416)

```text
└─ label_content (GRichTextField, 924x282) → m_label_content "更新文本更新文…"
```

## UILoginPlayerListItemLoginButton (GComponent, 168x168)

```text
├─ n4 (GImage, 168x168) [无绑定] ← Button BG
└─ title (GRichTextField, 82x53) → m_title "登录"
```

## UILoginButton (GButton, 480x128)

```text
├─ n3 (GImage, 480x128) [无绑定] ← Button BG
└─ title (GTextField, 104x66) [无绑定] "登录"
```

## UILoginAnnouncement (UILoginAnnouncement·GComponent, 1080x2160) [exported]

```text
├─ mask_layer (GGraph, 419x6080) → m_mask_layer
├─ text_content (GComponent, 972x600) → m_text_content
│  └─ label_content (GRichTextField, 924x282) "更新文本更新文…"
└─ text_title (GTextField, 104x66) → m_text_title "公告"
```

## UILoginPlayerList (UILoginPlayerList·GComponent, 1080x2160) [exported]

```text
Controllers: IsSelected(No, Yes)
├─ n5 (GImage, 1080x2160) [无绑定] ← Background
├─ player_list (GList, 900x1100) → m_player_list
│     └─ defaultItem: UILoginPlayerListItem (UILoginPlayerListItem·GComponent)
├─ selected_icon (GLoader, 256x256) → m_selected_icon
├─ selected_name (GRichTextField, 704x92) → m_selected_name "喊a韩餐尅就"
├─ selected_level (GRichTextField, 361x81) → m_selected_level "喊a韩餐尅就"
└─ login_button (GButton, 480x128) → m_login_button
   ├─ n3 (GImage, 480x128) ← Button BG
   └─ title (GTextField, 104x66) "登录"
```

## UILoginPlayerListItem (UILoginPlayerListItem·GComponent, 900x178) [exported]

```text
├─ n6 (GGraph, 900x178) [无绑定]
├─ icon (GLoader, 176x172) → m_icon
├─ name_text (GRichTextField, 421x66) → m_name_text "OnGetPlayerList"
├─ level_text (GRichTextField, 421x66) → m_level_text "OnGetPlayerList"
└─ login_button (GComponent, 168x168) → m_login_button
   ├─ n4 (GImage, 168x168) ← Button BG
   └─ title (GRichTextField, 82x53) "登录"
```

## UILoginPlayerCreate (UILoginPlayerCreate·GComponent, 1080x2160) [exported]

```text
├─ n7 (GGraph, 1080x2160) [无绑定]
├─ n2 (GImage, 766x161) [无绑定] ← Button BG
├─ user_name (inputtext, 706x123) → m_user_name
├─ n4 (GTextField, 274x97) [无绑定] "角色名称："
├─ n5 (GTextField, 204x66) [无绑定] "创建角色"
├─ enter (GButton, 480x128) → m_enter
│  ├─ n3 (GImage, 480x128) ← Button BG
│  └─ title (GTextField, 104x66) "登录"
└─ error_text (GTextField, 529x103) → m_error_text
```

## UILogin (UILogin·GComponent, 1080x2160) [exported]

```text
├─ n34 (GGraph, 1080x2160) [无绑定]
├─ n23 (GTextField, 104x66) [无绑定] "登录"
├─ error_text (GTextField, 529x46) → m_error_text
├─ enter (GButton, 480x128) → m_enter
│  ├─ n3 (GImage, 480x128) ← Button BG
│  └─ title (GTextField, 104x66) "登录"
├─ n16 (GImage, 767x97) [无绑定] ← Button BG
├─ user_name (inputtext, 717x59) → m_user_name
├─ n18 (GImage, 767x97) [无绑定] ← Button BG
├─ password (inputtext, 717x59) → m_password
├─ n20 (GTextField, 131x56) [无绑定] "账户名"
├─ n21 (GTextField, 89x56) [无绑定] "密码"
└─ n35 (GGroup, 904x277) [无绑定]
```
