# UIRoom 包结构

> 由 gencode 插件自动生成（发布时更新），请勿手改。
> 标注说明：`Wrapper·FGUI类型·mode`；`→ varName` 为生成的 C# 属性名（m_ 前缀）；`[无绑定]` 表示该节点未生成 C# 属性（无名称或 n+数字 自动名）；`[pkg=X]` 跨包引用；`← 资源名` 图片资源；`"文本"` 文案；`(↺)` 循环引用已省略。

## UIRoomButton (GButton, 160x54)

```text
├─ bg (GGraph, 160x54) → m_bg
└─ title (GTextField, 160x54) [无绑定] "按钮"
```

## UIRoomBattlePanel (UIRoomBattlePanel·GComponent, 1080x2160) [exported]

```text
├─ mask (GGraph, 1080x2160) → m_mask
├─ window_bg (GGraph, 960x1580) → m_window_bg
├─ top_bg (GGraph, 880x110) → m_top_bg
├─ room_title_text (GTextField, 560x42) → m_room_title_text "房间 #0"
├─ room_state_text (GTextField, 816x30) → m_room_state_text "等待中 0/2  局数:1 …"
├─ invite_text (GTextField, 816x38) → m_invite_text
├─ invite_button (GButton, 96x52) → m_invite_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ leave_button (GButton, 96x52) → m_leave_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ close_button (GButton, 96x52) → m_close_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ left_player_bg (GGraph, 400x480) → m_left_player_bg
├─ right_player_bg (GGraph, 400x480) → m_right_player_bg
├─ self_avatar (GLoader, 142x142) → m_self_avatar
├─ opponent_avatar (GLoader, 142x142) → m_opponent_avatar
├─ self_name_text (GTextField, 310x36) → m_self_name_text "等待玩家"
├─ opponent_name_text (GTextField, 310x36) → m_opponent_name_text "等待玩家"
├─ self_state_text (GTextField, 310x34) → m_self_state_text "等待中"
├─ opponent_state_text (GTextField, 310x34) → m_opponent_state_text "等待中"
├─ self_gesture_text (GTextField, 310x50) → m_self_gesture_text "-"
├─ opponent_gesture_text (GTextField, 310x50) → m_opponent_gesture_text "-"
├─ arena_bg (GGraph, 880x700) → m_arena_bg
├─ result_text (GTextField, 804x64) → m_result_text "等待开始"
├─ arena_tips_text (GTextField, 764x42) → m_arena_tips_text "房主开始后，双…"
├─ rock_button (UIRoomGestureButton·GButton, 132x112) → m_rock_button
│  Controllers: Gesture(Rock, Scissors, Paper)
│  ├─ bg (GGraph, 84x72)
│  ├─ rock_icon (GGraph, 24x24)
│  ├─ scissors_left (GGraph, 6x32)
│  ├─ scissors_right (GGraph, 6x32)
│  ├─ paper_back (GGraph, 24x30)
│  ├─ paper_front (GGraph, 24x30)
│  └─ title (GTextField, 84x22) "石头"
├─ scissors_button (UIRoomGestureButton·GButton, 132x112) → m_scissors_button
│  Controllers: Gesture(Rock, Scissors, Paper)
│  ├─ bg (GGraph, 84x72)
│  ├─ rock_icon (GGraph, 24x24)
│  ├─ scissors_left (GGraph, 6x32)
│  ├─ scissors_right (GGraph, 6x32)
│  ├─ paper_back (GGraph, 24x30)
│  ├─ paper_front (GGraph, 24x30)
│  └─ title (GTextField, 84x22) "石头"
├─ paper_button (UIRoomGestureButton·GButton, 132x112) → m_paper_button
│  Controllers: Gesture(Rock, Scissors, Paper)
│  ├─ bg (GGraph, 84x72)
│  ├─ rock_icon (GGraph, 24x24)
│  ├─ scissors_left (GGraph, 6x32)
│  ├─ scissors_right (GGraph, 6x32)
│  ├─ paper_back (GGraph, 24x30)
│  ├─ paper_front (GGraph, 24x30)
│  └─ title (GTextField, 84x22) "石头"
├─ start_button (GButton, 160x56) → m_start_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ sync_button (GButton, 160x56) → m_sync_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
└─ tips_text (GTextField, 880x38) → m_tips_text
```

## UIRoomGestureButton (UIRoomGestureButton·GButton, 84x72) [exported]

```text
Controllers: Gesture(Rock, Scissors, Paper)
├─ bg (GGraph, 84x72) → m_bg
├─ rock_icon (GGraph, 24x24) → m_rock_icon
├─ scissors_left (GGraph, 6x32) → m_scissors_left
├─ scissors_right (GGraph, 6x32) → m_scissors_right
├─ paper_back (GGraph, 24x30) → m_paper_back
├─ paper_front (GGraph, 24x30) → m_paper_front
└─ title (GTextField, 84x22) [无绑定] "石头"
```

## UIRoomListPanel (UIRoomListPanel·GComponent, 1080x2160) [exported]

```text
├─ mask (GGraph, 1080x2160) → m_mask
├─ window_bg (GGraph, 960x1360) → m_window_bg
├─ title_text (GTextField, 420x48) → m_title_text "石头剪刀布大厅"
├─ summary_text (GTextField, 520x32) → m_summary_text "当前玩家:0  房间…"
├─ refresh_button (GButton, 112x52) → m_refresh_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ create_button (GButton, 112x52) → m_create_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ close_button (GButton, 86x52) → m_close_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ list_bg (GGraph, 880x1100) → m_list_bg
├─ list_title (GTextField, 240x34) → m_list_title "可加入房间"
├─ room_list (GList, 816x990) → m_room_list
│     └─ defaultItem: UIRoomListItem (UIRoomListItem·GComponent)
└─ tips_text (GTextField, 816x30) → m_tips_text "加入或创建房间…"
```

## UIRoomPanel (UIRoomPanel·GComponent, 1080x2160) [exported]

```text
├─ mask (GGraph, 1080x2160) → m_mask
├─ window_bg (GGraph, 960x1620) → m_window_bg
├─ title_text (GTextField, 600x54) → m_title_text "石头剪刀布房间"
├─ summary_text (GTextField, 820x34) → m_summary_text "当前玩家:0  房间…"
├─ refresh_button (GButton, 112x54) → m_refresh_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ create_button (GButton, 112x54) → m_create_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ close_button (GButton, 92x54) → m_close_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ room_list_bg (GGraph, 460x880) → m_room_list_bg
├─ room_list_title (GTextField, 220x34) → m_room_list_title "房间列表"
├─ room_list (GList, 416x780) → m_room_list
│     └─ defaultItem: UIRoomListItem (UIRoomListItem·GComponent)
├─ room_detail_bg (GGraph, 410x880) → m_room_detail_bg
├─ room_detail_title (GTextField, 220x34) → m_room_detail_title "当前房间"
├─ room_detail_text (GTextField, 352x300) → m_room_detail_text "尚未进入房间"
├─ start_button (GButton, 112x54) → m_start_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ leave_button (GButton, 112x54) → m_leave_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ invite_button (GButton, 104x54) → m_invite_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ sync_button (GButton, 156x54) → m_sync_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ restart_button (GButton, 156x54) → m_restart_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
├─ invite_text (GTextField, 352x72) → m_invite_text
├─ players_bg (GGraph, 880x180) → m_players_bg
├─ players_title (GTextField, 220x34) → m_players_title "玩家状态"
├─ player_list (GList, 832x100) → m_player_list
│     └─ defaultItem: UIRoomPlayerItem (UIRoomPlayerItem·GComponent)
├─ game_bg (GGraph, 880x280) → m_game_bg
├─ game_title (GTextField, 220x34) → m_game_title "出拳与结算"
├─ result_text (GTextField, 832x40) → m_result_text "未进入房间"
├─ self_gesture_text (GTextField, 400x32) → m_self_gesture_text "你: 未出拳"
├─ opponent_gesture_text (GTextField, 400x32) → m_opponent_gesture_text "对手: 未出拳"
├─ rock_button (UIRoomGestureButton·GButton, 84x72) → m_rock_button
│  Controllers: Gesture(Rock, Scissors, Paper)
│  ├─ bg (GGraph, 84x72)
│  ├─ rock_icon (GGraph, 24x24)
│  ├─ scissors_left (GGraph, 6x32)
│  ├─ scissors_right (GGraph, 6x32)
│  ├─ paper_back (GGraph, 24x30)
│  ├─ paper_front (GGraph, 24x30)
│  └─ title (GTextField, 84x22) "石头"
├─ scissors_button (UIRoomGestureButton·GButton, 84x72) → m_scissors_button
│  Controllers: Gesture(Rock, Scissors, Paper)
│  ├─ bg (GGraph, 84x72)
│  ├─ rock_icon (GGraph, 24x24)
│  ├─ scissors_left (GGraph, 6x32)
│  ├─ scissors_right (GGraph, 6x32)
│  ├─ paper_back (GGraph, 24x30)
│  ├─ paper_front (GGraph, 24x30)
│  └─ title (GTextField, 84x22) "石头"
├─ paper_button (UIRoomGestureButton·GButton, 84x72) → m_paper_button
│  Controllers: Gesture(Rock, Scissors, Paper)
│  ├─ bg (GGraph, 84x72)
│  ├─ rock_icon (GGraph, 24x24)
│  ├─ scissors_left (GGraph, 6x32)
│  ├─ scissors_right (GGraph, 6x32)
│  ├─ paper_back (GGraph, 24x30)
│  ├─ paper_front (GGraph, 24x30)
│  └─ title (GTextField, 84x22) "石头"
└─ tips_text (GTextField, 880x34) → m_tips_text "房间流程: 创建或…"
```

## UIRoomPlayerItem (UIRoomPlayerItem·GComponent, 360x76) [exported]

```text
├─ bg (GGraph, 360x76) → m_bg
├─ avatar (GLoader, 52x52) → m_avatar
├─ role_text (GTextField, 150x28) → m_role_text "玩家: 0"
├─ seat_text (GTextField, 150x22) → m_seat_text "座位 1"
└─ state_text (GTextField, 104x28) → m_state_text "已出拳"
```

## UIRoomResultPopup (UIRoomResultPopup·GComponent, 1080x2160) [exported]

```text
├─ mask (GGraph, 1080x2160) → m_mask
├─ window_bg (GGraph, 800x720) → m_window_bg
├─ title_text (GTextField, 700x54) → m_title_text "本局结算"
├─ result_text (GTextField, 700x72) → m_result_text "平局"
├─ round_text (GTextField, 700x34) → m_round_text "第 1 局"
├─ self_bg (GGraph, 300x128) → m_self_bg
├─ opponent_bg (GGraph, 300x128) → m_opponent_bg
├─ self_name_text (GTextField, 206x30) → m_self_name_text "我"
├─ opponent_name_text (GTextField, 206x30) → m_opponent_name_text "对手"
├─ self_gesture_text (GTextField, 206x42) → m_self_gesture_text "石头"
├─ opponent_gesture_text (GTextField, 206x42) → m_opponent_gesture_text "剪刀"
├─ restart_button (GButton, 160x56) → m_restart_button
│  ├─ bg (GGraph, 160x54)
│  └─ title (GTextField, 160x54) "按钮"
└─ close_button (GButton, 160x56) → m_close_button
   ├─ bg (GGraph, 160x54)
   └─ title (GTextField, 160x54) "按钮"
```

## UIRoomListItem (UIRoomListItem·GButton, 520x92) [exported]

```text
├─ bg (GGraph, 520x92) → m_bg
├─ room_name (GTextField, 290x30) → m_room_name "#1 猜拳房间"
├─ room_status (GTextField, 270x24) → m_room_status "等待中 1/2"
└─ join_button (GButton, 108x54) → m_join_button
   ├─ bg (GGraph, 160x54)
   └─ title (GTextField, 160x54) "按钮"
```
