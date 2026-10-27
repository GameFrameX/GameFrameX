# UIProduction 包结构

> 由 gencode 插件自动生成（发布时更新），请勿手改。
> 标注说明：`Wrapper·FGUI类型·mode`；`→ varName` 为生成的 C# 属性名（m_ 前缀）；`[无绑定]` 表示该节点未生成 C# 属性（无名称或 n+数字 自动名）；`[pkg=X]` 跨包引用；`← 资源名` 图片资源；`"文本"` 文案；`(↺)` 循环引用已省略。

## UIProduction (UIProduction·GComponent, 1080x2160) [exported]

```text
Controllers: tab(生产, 产线), window(隐藏, 显示)
├─ screen_base_image (GGraph, 1080x2160) → m_screen_base_image
├─ screen_chrome (UIProductionScreenChrome·GComponent, 1080x2160) → m_screen_chrome
│  ├─ screen_background_part0 (GImage, 1080x2048) ← screen_background_p0
│  ├─ screen_background_part1 (GImage, 1080x112) ← screen_background_p1
│  ├─ back_button (UIProductionBackButton·GButton, 100x100)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ up (GImage, 99x99) ← back_button
│  ├─ production_navigation_button (UIProductionProductionNavigationButton·GButton, 270x104)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 270x103) ← production_background
│  │  ├─ production_title_shadow (GTextField, 99x47) "生产"
│  │  └─ title (GTextField, 99x47) "生产"
│  ├─ adventure_navigation_button (UIProductionAdventureNavigationButton·GButton, 270x104)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 270x103) ← adventure_background
│  │  ├─ adventure_title_shadow (GTextField, 101x48) "探险"
│  │  └─ title (GTextField, 102x48) "探险"
│  ├─ training_navigation_button (UIProductionTrainingNavigationButton·GButton, 1080x2160)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 270x103) ← training_background
│  │  ├─ training_title_shadow (GTextField, 99x49) "培养"
│  │  └─ title (GTextField, 100x48) "培养"
│  ├─ production_tab_button (UIProductionProductionTabButton·GButton·Check, 210x62)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 209x62) ← production_tab_background
│  │  └─ title (GTextField, 73x31) "生 产"
│  ├─ production_lines_tab_button (UIProductionProductionLinesTabButton·GButton·Check, 210x62)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 209x62) ← production_lines_tab_background
│  │  └─ title (GTextField, 75x31) "产 线"
│  ├─ screen_title_shadow (GTextField, 185x89) "生产"
│  └─ title (GTextField, 185x89) "生产"
├─ buildings_panel (UIProductionBuildingsPanel·GComponent, 1036x1544) → m_buildings_panel
│  ├─ description_outer_panel (GImage, 1033x175) ← deco_descriptionouterpanel
│  ├─ description_header_background (GImage, 1003x49) ← deco_descriptionheaderbackground
│  ├─ description_body_background (GImage, 1011x153) ← deco_descriptionbodybackground
│  ├─ description_right_corner_decoration (GImage, 25x25) ← deco_descriptionrightcornerdecoration
│  ├─ description_left_corner_decoration (GImage, 25x25) ← deco_descriptionleftcornerdecoration
│  ├─ description_heading (GTextField, 80x32) "描 述"
│  ├─ description_body (GTextField, 957x65) "配置生产线最终…"
│  ├─ available_workers_background (GImage, 547x57) ← available_workers_background
│  ├─ available_workers (GTextField, 284x27) "可分配工人：100/20…"
│  ├─ all_category_button (UIProductionAllCategoryButton·GButton·Check, 170x54)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ all_category_border (GImage, 167x50) ← all_category_border
│  │  ├─ up (GImage, 167x50) ← all_category_background
│  │  ├─ all_category_decoration (GImage, 15x15) ← all_category_decoration
│  │  └─ title (GTextField, 59x25) "全 部"
│  ├─ left_category_button (UIProductionProductionLeftCategoryButton·GButton·Check, 170x52)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ production_left_category_border (GImage, 168x50) ← production_left_category_border
│  │  ├─ up (GImage, 168x50) ← production_left_category_background
│  │  ├─ production_left_category_decoration (GImage, 15x15) ← production_left_category_decoration
│  │  └─ title (GTextField, 60x25) "生 产"
│  ├─ center_left_category_button (UIProductionProductionCenterLeftCategoryButton·GButton·Check, 170x52)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ production_center_left_category_border (GImage, 168x50) ← production_center_left_category_border
│  │  ├─ up (GImage, 168x50) ← production_center_left_category_background
│  │  ├─ production_center_left_category_decoration (GImage, 15x15) ← production_center_left_category_decoration
│  │  └─ title (GTextField, 60x25) "生 产"
│  ├─ center_right_category_button (UIProductionProductionCenterRightCategoryButton·GButton·Check, 170x52)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ production_center_right_category_border (GImage, 168x50) ← production_center_left_category_border
│  │  ├─ up (GImage, 168x50) ← production_center_left_category_background
│  │  ├─ production_center_right_category_decoration (GImage, 15x15) ← production_center_left_category_decoration
│  │  └─ title (GTextField, 60x25) "生 产"
│  ├─ right_category_button (UIProductionProductionRightCategoryButton·GButton·Check, 172x54)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ production_right_category_border (GImage, 167x50) ← production_right_category_border
│  │  ├─ up (GImage, 167x50) ← production_right_category_background
│  │  ├─ production_right_category_decoration (GImage, 15x15) ← production_right_category_decoration
│  │  └─ title (GTextField, 60x25) "生 产"
│  ├─ production_lines_category_button (UIProductionProductionLinesCategoryButton·GButton·Check, 170x52)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ production_lines_category_border (GImage, 167x50) ← production_lines_category_border
│  │  ├─ up (GImage, 167x50) ← production_lines_category_background
│  │  ├─ production_lines_category_decoration (GImage, 15x15) ← production_lines_category_decoration
│  │  └─ title (GTextField, 61x25) "产 线"
│  └─ building_list (GList, 1010x868)
│        └─ defaultItem: UIProductionBuildingCard (UIProductionBuildingCard·GComponent)
├─ production_lines_panel (UIProductionProductionLinesPanel·GComponent, 1038x1334) → m_production_lines_panel
│  ├─ deco_descriptionouterpanel (GImage, 1033x175) ← deco_descriptionouterpanel
│  ├─ deco_descriptionheaderbackground (GImage, 1003x49) ← deco_descriptionheaderbackground
│  ├─ deco_descriptionbodybackground (GImage, 1011x153) ← deco_descriptionbodybackground
│  ├─ deco_descriptionrightcornerdecoration (GImage, 25x25) ← deco_descriptionrightcornerdecoration
│  ├─ deco_descriptionleftcornerdecoration (GImage, 25x25) ← deco_descriptionleftcornerdecoration
│  ├─ description_heading (GTextField, 80x32) "描 述"
│  ├─ description_body (GTextField, 957x65) "配置生产线最终…"
│  ├─ deco_availableworkersbackground (GImage, 547x57) ← deco_availableworkersbackground
│  ├─ available_workers (GTextField, 289x28) "可分配工人：100/20…"
│  ├─ line_list (GList, 1034x1054)
│  │     └─ defaultItem: UIProductionProductionLineCard (UIProductionProductionLineCard·GComponent)
│  └─ empty_line_card (UIProductionEmptyProductionLineCard·GComponent, 1036x336)
│     ├─ deco_cardpanel2 (GImage, 1033x287) ← deco_cardpanel2
│     ├─ deco_cardcornerdecoration2 (GImage, 13x13) ← deco_cardcornerdecoration2
│     ├─ production_line_name (GTextField, 122x33) "生产线3"
│     ├─ deco_editlinebutton3 (GImage, 40x40) ← deco_editlinebutton3
│     ├─ deco_deletelinebutton3 (GImage, 37x41) ← deco_deletelinebutton3
│     ├─ create_line_hint (GTextField, 258x36) "点击新建生产线"
│     ├─ create_line_button (UIProductionCreateLineButton·GButton, 84x86)
│     │  Controllers: button(up, down, over, selectedOver)
│     │  └─ up (GImage, 84x86) ← deco_createlinebutton
│     └─ create_line_hint (GTextField, 258x36) "点击新建生产线"
├─ modal_dim_overlay (GGraph, 1080x2160) → m_modal_dim_overlay
├─ create_line_window_frame (UIProductionCreateLineWindowFrame·GComponent, 910x1028) → m_create_line_window_frame
│  ├─ window_center_decoration (GImage, 31x30) ← window_center_decoration
│  ├─ window_outer_panel (GImage, 899x997) ← window_outer_panel
│  ├─ window_inner_panel (GImage, 867x966) ← window_inner_panel
│  ├─ window_top_highlight (GImage, 31x7) ← window_top_highlight
│  ├─ window_top_highlight_dot (GImage, 7x7) ← window_top_highlight_dot
│  ├─ window_bottom_highlight (GImage, 31x7) ← window_bottom_highlight
│  ├─ window_bottom_highlight_dot (GImage, 7x7) ← window_bottom_highlight_dot
│  ├─ title_small_star (GImage, 17x18) ← title_small_star
│  ├─ title_left_star (GImage, 32x32) ← title_left_star
│  ├─ title_right_star (GImage, 32x32) ← title_right_star
│  ├─ title_top_star (GImage, 16x15) ← title_top_star
│  ├─ title_bottom_star (GImage, 16x16) ← title_bottom_star
│  ├─ close_button (UIProductionCloseLineWindowButton·GButton, 58x60)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ up (GImage, 57x59) ← closebutton
│  ├─ title (UIProductionCreateLineWindowTitle·GComponent, 228x80)
│  │  ├─ window_title_background (GImage, 228x79) ← window_title_background
│  │  └─ title (GTextField, 125x30) "建立产线"
│  └─ window_content_divider (GImage, 315x12) ← window_content_divider
└─ create_line_content (UIProductionCreateLineContent·GComponent, 770x868) → m_create_line_content
   ├─ product_catalog (UIProductionProductCatalog·GComponent, 770x750)
   │  ├─ catalog_outer_panel (GImage, 766x747) ← catalog_outer_panel
   │  ├─ catalog_header_background (GImage, 743x66) ← catalog_header_background
   │  ├─ catalog_body_background (GImage, 750x730) ← catalog_body_background
   │  ├─ catalog_right_corner_decoration (GImage, 19x19) ← catalog_right_corner_decoration
   │  ├─ catalog_left_corner_decoration (GImage, 19x19) ← catalog_left_corner_decoration
   │  ├─ era_selected_background (GImage, 121x70) ← era_selected_background
   │  ├─ era_button_0 (UIProductionEraButton·GButton·Check, 100x70)
   │  │  Controllers: button(up, down, over, selectedOver)
   │  │  └─ title (GTextField, 56x28) "全部"
   │  ├─ era_button_1 (UIProductionEraButton·GButton·Check, 100x70)
   │  │  Controllers: button(up, down, over, selectedOver)
   │  │  └─ title (GTextField, 56x28) "全部"
   │  ├─ era_button_2 (UIProductionEraButton·GButton·Check, 100x70)
   │  │  Controllers: button(up, down, over, selectedOver)
   │  │  └─ title (GTextField, 56x28) "全部"
   │  ├─ era_button_3 (UIProductionEraButton·GButton·Check, 100x70)
   │  │  Controllers: button(up, down, over, selectedOver)
   │  │  └─ title (GTextField, 56x28) "全部"
   │  ├─ era_button_4 (UIProductionEraButton·GButton·Check, 100x70)
   │  │  Controllers: button(up, down, over, selectedOver)
   │  │  └─ title (GTextField, 56x28) "全部"
   │  ├─ era_button_5 (UIProductionEraButton·GButton·Check, 100x70)
   │  │  Controllers: button(up, down, over, selectedOver)
   │  │  └─ title (GTextField, 56x28) "全部"
   │  └─ product_list (GList, 719x622)
   │        └─ defaultItem: UIProductionProductCard (UIProductionProductCard·GComponent)
   ├─ production_line_instructions (GTextField, 691x29) "选择2级以上或产…"
   └─ production_recipe_instructions (GTextField, 526x28) "可以手动修改产…"
```

## UIProductionProductionRightCategoryButton (UIProductionProductionRightCategoryButton·GButton, 172x54) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ production_right_category_border (GImage, 167x50) → m_production_right_category_border ← production_right_category_border
├─ up (GImage, 167x50) → m_up ← production_right_category_background
├─ production_right_category_decoration (GImage, 15x15) → m_production_right_category_decoration ← production_right_category_decoration
└─ title (GTextField, 60x25) [无绑定] "生 产"
```

## UIProductionScreenChrome (UIProductionScreenChrome·GComponent, 1080x2160) [exported]

```text
├─ screen_background_part0 (GImage, 1080x2048) → m_screen_background_part0 ← screen_background_p0
├─ screen_background_part1 (GImage, 1080x112) → m_screen_background_part1 ← screen_background_p1
├─ back_button (UIProductionBackButton·GButton, 100x100) → m_back_button
│  Controllers: button(up, down, over, selectedOver)
│  └─ up (GImage, 99x99) ← back_button
├─ production_navigation_button (UIProductionProductionNavigationButton·GButton, 270x104) → m_production_navigation_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 270x103) ← production_background
│  ├─ production_title_shadow (GTextField, 99x47) "生产"
│  └─ title (GTextField, 99x47) "生产"
├─ adventure_navigation_button (UIProductionAdventureNavigationButton·GButton, 270x104) → m_adventure_navigation_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 270x103) ← adventure_background
│  ├─ adventure_title_shadow (GTextField, 101x48) "探险"
│  └─ title (GTextField, 102x48) "探险"
├─ training_navigation_button (UIProductionTrainingNavigationButton·GButton, 1080x2160) → m_training_navigation_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 270x103) ← training_background
│  ├─ training_title_shadow (GTextField, 99x49) "培养"
│  └─ title (GTextField, 100x48) "培养"
├─ production_tab_button (UIProductionProductionTabButton·GButton·Check, 210x62) → m_production_tab_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 209x62) ← production_tab_background
│  └─ title (GTextField, 73x31) "生 产"
├─ production_lines_tab_button (UIProductionProductionLinesTabButton·GButton·Check, 210x62) → m_production_lines_tab_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 209x62) ← production_lines_tab_background
│  └─ title (GTextField, 75x31) "产 线"
├─ screen_title_shadow (GTextField, 185x89) → m_screen_title_shadow "生产"
└─ title (GTextField, 185x89) → m_title "生产"
```

## UIProductionProductionTabButton (UIProductionProductionTabButton·GButton, 210x62) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 209x62) → m_up ← production_tab_background
└─ title (GTextField, 73x31) [无绑定] "生 产"
```

## UIProductionProductionLinesTabButton (UIProductionProductionLinesTabButton·GButton, 210x62) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 209x62) → m_up ← production_lines_tab_background
└─ title (GTextField, 75x31) [无绑定] "产 线"
```

## UIProductionProductionNavigationButton (UIProductionProductionNavigationButton·GButton, 270x104) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 270x103) → m_up ← production_background
├─ production_title_shadow (GTextField, 99x47) → m_production_title_shadow "生产"
└─ title (GTextField, 99x47) [无绑定] "生产"
```

## UIProductionAdventureNavigationButton (UIProductionAdventureNavigationButton·GButton, 270x104) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 270x103) → m_up ← adventure_background
├─ adventure_title_shadow (GTextField, 101x48) → m_adventure_title_shadow "探险"
└─ title (GTextField, 102x48) [无绑定] "探险"
```

## UIProductionTrainingNavigationButton (UIProductionTrainingNavigationButton·GButton, 1080x2160) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 270x103) → m_up ← training_background
├─ training_title_shadow (GTextField, 99x49) → m_training_title_shadow "培养"
└─ title (GTextField, 100x48) [无绑定] "培养"
```

## UIProductionBackButton (UIProductionBackButton·GButton, 100x100) [exported]

```text
Controllers: button(up, down, over, selectedOver)
└─ up (GImage, 99x99) → m_up ← back_button
```

## UIProductionBuildingsPanel (UIProductionBuildingsPanel·GComponent, 1036x1544) [exported]

```text
├─ description_outer_panel (GImage, 1033x175) → m_description_outer_panel ← deco_descriptionouterpanel
├─ description_header_background (GImage, 1003x49) → m_description_header_background ← deco_descriptionheaderbackground
├─ description_body_background (GImage, 1011x153) → m_description_body_background ← deco_descriptionbodybackground
├─ description_right_corner_decoration (GImage, 25x25) → m_description_right_corner_decoration ← deco_descriptionrightcornerdecoration
├─ description_left_corner_decoration (GImage, 25x25) → m_description_left_corner_decoration ← deco_descriptionleftcornerdecoration
├─ description_heading (GTextField, 80x32) → m_description_heading "描 述"
├─ description_body (GTextField, 957x65) → m_description_body "配置生产线最终…"
├─ available_workers_background (GImage, 547x57) → m_available_workers_background ← available_workers_background
├─ available_workers (GTextField, 284x27) → m_available_workers "可分配工人：100/20…"
├─ all_category_button (UIProductionAllCategoryButton·GButton·Check, 170x54) → m_all_category_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ all_category_border (GImage, 167x50) ← all_category_border
│  ├─ up (GImage, 167x50) ← all_category_background
│  ├─ all_category_decoration (GImage, 15x15) ← all_category_decoration
│  └─ title (GTextField, 59x25) "全 部"
├─ left_category_button (UIProductionProductionLeftCategoryButton·GButton·Check, 170x52) → m_left_category_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ production_left_category_border (GImage, 168x50) ← production_left_category_border
│  ├─ up (GImage, 168x50) ← production_left_category_background
│  ├─ production_left_category_decoration (GImage, 15x15) ← production_left_category_decoration
│  └─ title (GTextField, 60x25) "生 产"
├─ center_left_category_button (UIProductionProductionCenterLeftCategoryButton·GButton·Check, 170x52) → m_center_left_category_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ production_center_left_category_border (GImage, 168x50) ← production_center_left_category_border
│  ├─ up (GImage, 168x50) ← production_center_left_category_background
│  ├─ production_center_left_category_decoration (GImage, 15x15) ← production_center_left_category_decoration
│  └─ title (GTextField, 60x25) "生 产"
├─ center_right_category_button (UIProductionProductionCenterRightCategoryButton·GButton·Check, 170x52) → m_center_right_category_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ production_center_right_category_border (GImage, 168x50) ← production_center_left_category_border
│  ├─ up (GImage, 168x50) ← production_center_left_category_background
│  ├─ production_center_right_category_decoration (GImage, 15x15) ← production_center_left_category_decoration
│  └─ title (GTextField, 60x25) "生 产"
├─ right_category_button (UIProductionProductionRightCategoryButton·GButton·Check, 172x54) → m_right_category_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ production_right_category_border (GImage, 167x50) ← production_right_category_border
│  ├─ up (GImage, 167x50) ← production_right_category_background
│  ├─ production_right_category_decoration (GImage, 15x15) ← production_right_category_decoration
│  └─ title (GTextField, 60x25) "生 产"
├─ production_lines_category_button (UIProductionProductionLinesCategoryButton·GButton·Check, 170x52) → m_production_lines_category_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ production_lines_category_border (GImage, 167x50) ← production_lines_category_border
│  ├─ up (GImage, 167x50) ← production_lines_category_background
│  ├─ production_lines_category_decoration (GImage, 15x15) ← production_lines_category_decoration
│  └─ title (GTextField, 61x25) "产 线"
└─ building_list (GList, 1010x868) → m_building_list
      └─ defaultItem: UIProductionBuildingCard (UIProductionBuildingCard·GComponent)
```

## UIProductionBuildingCard (UIProductionBuildingCard·GComponent, 326x272) [exported]

```text
├─ card_panel (GImage, 324x269) → m_card_panel ← card_panel
├─ card_top_left_decoration (GImage, 8x9) → m_card_top_left_decoration ← card_top_left_decoration
├─ card_top_divider (GImage, 175x7) → m_card_top_divider ← card_top_divider
├─ card_bottom_divider (GImage, 175x7) → m_card_bottom_divider ← card_bottom_divider
├─ title (GTextField, 153x22) → m_title "伐木场 LV.256"
├─ quantity (UIProductionWorkerQuantityControl·GComponent, 252x42) → m_quantity
│  ├─ decrease_button (UIProductionDecreaseButton·GButton, 40x40)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 38x38) ← decrease_background
│  │  └─ icon (GImage, 28x9) ← decrease_symbol
│  ├─ increase_button (UIProductionIncreaseButton·GButton, 40x40)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 38x38) ← increase_background
│  │  └─ icon (GImage, 28x27) ← increase_symbol
│  ├─ quantity_background (GImage, 163x37) ← quantity_background
│  ├─ value (GTextField, 26x18) "60"
│  ├─ maximum_button (UIProductionMaximumWorkersButton·GButton, 40x40)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  ├─ up (GImage, 38x38) ← maximum_background
│  │  ├─ maximum_arrow_primary (GTextField, 18x20) ">"
│  │  └─ maximum_arrow_secondary (GTextField, 18x20) ">"
│  └─ minimum_button (UIProductionMinimumWorkersButton·GButton, 40x40)
│     Controllers: button(up, down, over, selectedOver)
│     ├─ up (GImage, 38x38) ← minimum_background
│     ├─ minimum_arrow_primary (GTextField, 18x20) ">"
│     └─ minimum_arrow_secondary (GTextField, 18x20) ">"
└─ building_image (GLoader, 122x146) → m_building_image
```

## UIProductionWorkerQuantityControl (UIProductionWorkerQuantityControl·GComponent, 252x42) [exported]

```text
├─ decrease_button (UIProductionDecreaseButton·GButton, 40x40) → m_decrease_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 38x38) ← decrease_background
│  └─ icon (GImage, 28x9) ← decrease_symbol
├─ increase_button (UIProductionIncreaseButton·GButton, 40x40) → m_increase_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 38x38) ← increase_background
│  └─ icon (GImage, 28x27) ← increase_symbol
├─ quantity_background (GImage, 163x37) → m_quantity_background ← quantity_background
├─ value (GTextField, 26x18) → m_value "60"
├─ maximum_button (UIProductionMaximumWorkersButton·GButton, 40x40) → m_maximum_button
│  Controllers: button(up, down, over, selectedOver)
│  ├─ up (GImage, 38x38) ← maximum_background
│  ├─ maximum_arrow_primary (GTextField, 18x20) ">"
│  └─ maximum_arrow_secondary (GTextField, 18x20) ">"
└─ minimum_button (UIProductionMinimumWorkersButton·GButton, 40x40) → m_minimum_button
   Controllers: button(up, down, over, selectedOver)
   ├─ up (GImage, 38x38) ← minimum_background
   ├─ minimum_arrow_primary (GTextField, 18x20) ">"
   └─ minimum_arrow_secondary (GTextField, 18x20) ">"
```

## UIProductionMaximumWorkersButton (UIProductionMaximumWorkersButton·GButton, 40x40) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 38x38) → m_up ← maximum_background
├─ maximum_arrow_primary (GTextField, 18x20) → m_maximum_arrow_primary ">"
└─ maximum_arrow_secondary (GTextField, 18x20) → m_maximum_arrow_secondary ">"
```

## UIProductionMinimumWorkersButton (UIProductionMinimumWorkersButton·GButton, 40x40) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 38x38) → m_up ← minimum_background
├─ minimum_arrow_primary (GTextField, 18x20) → m_minimum_arrow_primary ">"
└─ minimum_arrow_secondary (GTextField, 18x20) → m_minimum_arrow_secondary ">"
```

## UIProductionIncreaseButton (UIProductionIncreaseButton·GButton, 40x40) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 38x38) → m_up ← increase_background
└─ icon (GImage, 28x27) [无绑定] ← increase_symbol
```

## UIProductionDecreaseButton (UIProductionDecreaseButton·GButton, 40x40) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ up (GImage, 38x38) → m_up ← decrease_background
└─ icon (GImage, 28x9) [无绑定] ← decrease_symbol
```

## UIProductionProductionLinesPanel (UIProductionProductionLinesPanel·GComponent, 1038x1334) [exported]

```text
├─ deco_descriptionouterpanel (GImage, 1033x175) → m_deco_descriptionouterpanel ← deco_descriptionouterpanel
├─ deco_descriptionheaderbackground (GImage, 1003x49) → m_deco_descriptionheaderbackground ← deco_descriptionheaderbackground
├─ deco_descriptionbodybackground (GImage, 1011x153) → m_deco_descriptionbodybackground ← deco_descriptionbodybackground
├─ deco_descriptionrightcornerdecoration (GImage, 25x25) → m_deco_descriptionrightcornerdecoration ← deco_descriptionrightcornerdecoration
├─ deco_descriptionleftcornerdecoration (GImage, 25x25) → m_deco_descriptionleftcornerdecoration ← deco_descriptionleftcornerdecoration
├─ description_heading (GTextField, 80x32) → m_description_heading "描 述"
├─ description_body (GTextField, 957x65) → m_description_body "配置生产线最终…"
├─ deco_availableworkersbackground (GImage, 547x57) → m_deco_availableworkersbackground ← deco_availableworkersbackground
├─ available_workers (GTextField, 289x28) → m_available_workers "可分配工人：100/20…"
├─ line_list (GList, 1034x1054) → m_line_list
│     └─ defaultItem: UIProductionProductionLineCard (UIProductionProductionLineCard·GComponent)
└─ empty_line_card (UIProductionEmptyProductionLineCard·GComponent, 1036x336) → m_empty_line_card
   ├─ deco_cardpanel2 (GImage, 1033x287) ← deco_cardpanel2
   ├─ deco_cardcornerdecoration2 (GImage, 13x13) ← deco_cardcornerdecoration2
   ├─ production_line_name (GTextField, 122x33) "生产线3"
   ├─ deco_editlinebutton3 (GImage, 40x40) ← deco_editlinebutton3
   ├─ deco_deletelinebutton3 (GImage, 37x41) ← deco_deletelinebutton3
   ├─ create_line_hint (GTextField, 258x36) "点击新建生产线"
   ├─ create_line_button (UIProductionCreateLineButton·GButton, 84x86)
   │  Controllers: button(up, down, over, selectedOver)
   │  └─ up (GImage, 84x86) ← deco_createlinebutton
   └─ create_line_hint (GTextField, 258x36) "点击新建生产线"
```

## UIProductionProductionLineCard (UIProductionProductionLineCard·GComponent, 1034x334) [exported]

```text
├─ deco_cardpanel (GImage, 1033x287) → m_deco_cardpanel ← deco_cardpanel
├─ deco_cardcornerdecoration (GImage, 13x13) → m_deco_cardcornerdecoration ← deco_cardcornerdecoration
├─ deco_carddivider (GImage, 397x15) → m_deco_carddivider ← deco_carddivider
├─ production_line_name (GTextField, 113x33) → m_production_line_name "生产线1"
├─ product_label (UIProductionProductionLineProductLabel·GComponent, 202x48) → m_product_label
│  ├─ deco_productlabelbackground (GImage, 201x47) ← deco_productlabelbackground
│  └─ title (GTextField, 162x27) "现代建筑组件"
├─ edit_line_button (UIProductionEditLineButton·GButton, 40x40) → m_edit_line_button
│  Controllers: button(up, down, over, selectedOver)
│  └─ up (GImage, 40x40) ← deco_editlinebutton
├─ delete_line_button (UIProductionDeleteLineButton·GButton, 38x42) → m_delete_line_button
│  Controllers: button(up, down, over, selectedOver)
│  └─ up (GImage, 37x41) ← deco_deletelinebutton
├─ output_label (GTextField, 80x33) → m_output_label "产出："
├─ output_rate (GTextField, 123x32) → m_output_rate "x0.5/天"
├─ inventory_value (GTextField, 249x35) → m_inventory_value "(库存：1.6/100）"
├─ output_product_label (UIProductionOutputProductLabel·GComponent, 162x38) → m_output_product_label
│  ├─ deco_outputproductbackground (GImage, 161x37) ← deco_outputproductbackground
│  └─ title (GTextField, 130x21) "现代建筑组件"
├─ worker_allocation_label (GTextField, 125x28) → m_worker_allocation_label "工人分配："
└─ quantity (UIProductionWorkerQuantityControl·GComponent, 396x40) → m_quantity
   ├─ decrease_button (UIProductionDecreaseButton·GButton, 40x40)
   │  Controllers: button(up, down, over, selectedOver)
   │  ├─ up (GImage, 38x38) ← decrease_background
   │  └─ icon (GImage, 28x9) ← decrease_symbol
   ├─ increase_button (UIProductionIncreaseButton·GButton, 40x40)
   │  Controllers: button(up, down, over, selectedOver)
   │  ├─ up (GImage, 38x38) ← increase_background
   │  └─ icon (GImage, 28x27) ← increase_symbol
   ├─ quantity_background (GImage, 163x37) ← quantity_background
   ├─ value (GTextField, 26x18) "60"
   ├─ maximum_button (UIProductionMaximumWorkersButton·GButton, 40x40)
   │  Controllers: button(up, down, over, selectedOver)
   │  ├─ up (GImage, 38x38) ← maximum_background
   │  ├─ maximum_arrow_primary (GTextField, 18x20) ">"
   │  └─ maximum_arrow_secondary (GTextField, 18x20) ">"
   └─ minimum_button (UIProductionMinimumWorkersButton·GButton, 40x40)
      Controllers: button(up, down, over, selectedOver)
      ├─ up (GImage, 38x38) ← minimum_background
      ├─ minimum_arrow_primary (GTextField, 18x20) ">"
      └─ minimum_arrow_secondary (GTextField, 18x20) ">"
```

## UIProductionProductionLineProductLabel (UIProductionProductionLineProductLabel·GComponent, 202x48) [exported]

```text
├─ deco_productlabelbackground (GImage, 201x47) → m_deco_productlabelbackground ← deco_productlabelbackground
└─ title (GTextField, 162x27) → m_title "现代建筑组件"
```

## UIProductionOutputProductLabel (UIProductionOutputProductLabel·GComponent, 162x38) [exported]

```text
├─ deco_outputproductbackground (GImage, 161x37) → m_deco_outputproductbackground ← deco_outputproductbackground
└─ title (GTextField, 130x21) → m_title "现代建筑组件"
```

## UIProductionEmptyProductionLineCard (UIProductionEmptyProductionLineCard·GComponent, 1036x336) [exported]

```text
├─ deco_cardpanel2 (GImage, 1033x287) → m_deco_cardpanel2 ← deco_cardpanel2
├─ deco_cardcornerdecoration2 (GImage, 13x13) → m_deco_cardcornerdecoration2 ← deco_cardcornerdecoration2
├─ production_line_name (GTextField, 122x33) → m_production_line_name "生产线3"
├─ deco_editlinebutton3 (GImage, 40x40) → m_deco_editlinebutton3 ← deco_editlinebutton3
├─ deco_deletelinebutton3 (GImage, 37x41) → m_deco_deletelinebutton3 ← deco_deletelinebutton3
├─ create_line_hint (GTextField, 258x36) → m_create_line_hint_2 "点击新建生产线"
├─ create_line_button (UIProductionCreateLineButton·GButton, 84x86) → m_create_line_button
│  Controllers: button(up, down, over, selectedOver)
│  └─ up (GImage, 84x86) ← deco_createlinebutton
└─ create_line_hint (GTextField, 258x36) → m_create_line_hint_2 "点击新建生产线"
```

## UIProductionCreateLineButton (UIProductionCreateLineButton·GButton, 84x86) [exported]

```text
Controllers: button(up, down, over, selectedOver)
└─ up (GImage, 84x86) → m_up ← deco_createlinebutton
```

## UIProductionEditLineButton (UIProductionEditLineButton·GButton, 40x40) [exported]

```text
Controllers: button(up, down, over, selectedOver)
└─ up (GImage, 40x40) → m_up ← deco_editlinebutton
```

## UIProductionDeleteLineButton (UIProductionDeleteLineButton·GButton, 38x42) [exported]

```text
Controllers: button(up, down, over, selectedOver)
└─ up (GImage, 37x41) → m_up ← deco_deletelinebutton
```

## UIProductionCreateLineWindowFrame (UIProductionCreateLineWindowFrame·GComponent, 910x1028) [exported]

```text
├─ window_center_decoration (GImage, 31x30) → m_window_center_decoration ← window_center_decoration
├─ window_outer_panel (GImage, 899x997) → m_window_outer_panel ← window_outer_panel
├─ window_inner_panel (GImage, 867x966) → m_window_inner_panel ← window_inner_panel
├─ window_top_highlight (GImage, 31x7) → m_window_top_highlight ← window_top_highlight
├─ window_top_highlight_dot (GImage, 7x7) → m_window_top_highlight_dot ← window_top_highlight_dot
├─ window_bottom_highlight (GImage, 31x7) → m_window_bottom_highlight ← window_bottom_highlight
├─ window_bottom_highlight_dot (GImage, 7x7) → m_window_bottom_highlight_dot ← window_bottom_highlight_dot
├─ title_small_star (GImage, 17x18) → m_title_small_star ← title_small_star
├─ title_left_star (GImage, 32x32) → m_title_left_star ← title_left_star
├─ title_right_star (GImage, 32x32) → m_title_right_star ← title_right_star
├─ title_top_star (GImage, 16x15) → m_title_top_star ← title_top_star
├─ title_bottom_star (GImage, 16x16) → m_title_bottom_star ← title_bottom_star
├─ close_button (UIProductionCloseLineWindowButton·GButton, 58x60) → m_close_button
│  Controllers: button(up, down, over, selectedOver)
│  └─ up (GImage, 57x59) ← closebutton
├─ title (UIProductionCreateLineWindowTitle·GComponent, 228x80) → m_title
│  ├─ window_title_background (GImage, 228x79) ← window_title_background
│  └─ title (GTextField, 125x30) "建立产线"
└─ window_content_divider (GImage, 315x12) → m_window_content_divider ← window_content_divider
```

## UIProductionCreateLineWindowTitle (UIProductionCreateLineWindowTitle·GComponent, 228x80) [exported]

```text
├─ window_title_background (GImage, 228x79) → m_window_title_background ← window_title_background
└─ title (GTextField, 125x30) → m_title "建立产线"
```

## UIProductionCreateLineContent (UIProductionCreateLineContent·GComponent, 770x868) [exported]

```text
├─ product_catalog (UIProductionProductCatalog·GComponent, 770x750) → m_product_catalog
│  ├─ catalog_outer_panel (GImage, 766x747) ← catalog_outer_panel
│  ├─ catalog_header_background (GImage, 743x66) ← catalog_header_background
│  ├─ catalog_body_background (GImage, 750x730) ← catalog_body_background
│  ├─ catalog_right_corner_decoration (GImage, 19x19) ← catalog_right_corner_decoration
│  ├─ catalog_left_corner_decoration (GImage, 19x19) ← catalog_left_corner_decoration
│  ├─ era_selected_background (GImage, 121x70) ← era_selected_background
│  ├─ era_button_0 (UIProductionEraButton·GButton·Check, 100x70)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ title (GTextField, 56x28) "全部"
│  ├─ era_button_1 (UIProductionEraButton·GButton·Check, 100x70)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ title (GTextField, 56x28) "全部"
│  ├─ era_button_2 (UIProductionEraButton·GButton·Check, 100x70)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ title (GTextField, 56x28) "全部"
│  ├─ era_button_3 (UIProductionEraButton·GButton·Check, 100x70)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ title (GTextField, 56x28) "全部"
│  ├─ era_button_4 (UIProductionEraButton·GButton·Check, 100x70)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ title (GTextField, 56x28) "全部"
│  ├─ era_button_5 (UIProductionEraButton·GButton·Check, 100x70)
│  │  Controllers: button(up, down, over, selectedOver)
│  │  └─ title (GTextField, 56x28) "全部"
│  └─ product_list (GList, 719x622)
│        └─ defaultItem: UIProductionProductCard (UIProductionProductCard·GComponent)
├─ production_line_instructions (GTextField, 691x29) → m_production_line_instructions "选择2级以上或产…"
└─ production_recipe_instructions (GTextField, 526x28) → m_production_recipe_instructions "可以手动修改产…"
```

## UIProductionProductCatalog (UIProductionProductCatalog·GComponent, 770x750) [exported]

```text
├─ catalog_outer_panel (GImage, 766x747) → m_catalog_outer_panel ← catalog_outer_panel
├─ catalog_header_background (GImage, 743x66) → m_catalog_header_background ← catalog_header_background
├─ catalog_body_background (GImage, 750x730) → m_catalog_body_background ← catalog_body_background
├─ catalog_right_corner_decoration (GImage, 19x19) → m_catalog_right_corner_decoration ← catalog_right_corner_decoration
├─ catalog_left_corner_decoration (GImage, 19x19) → m_catalog_left_corner_decoration ← catalog_left_corner_decoration
├─ era_selected_background (GImage, 121x70) → m_era_selected_background ← era_selected_background
├─ era_button_0 (UIProductionEraButton·GButton·Check, 100x70) → m_era_button_0
│  Controllers: button(up, down, over, selectedOver)
│  └─ title (GTextField, 56x28) "全部"
├─ era_button_1 (UIProductionEraButton·GButton·Check, 100x70) → m_era_button_1
│  Controllers: button(up, down, over, selectedOver)
│  └─ title (GTextField, 56x28) "全部"
├─ era_button_2 (UIProductionEraButton·GButton·Check, 100x70) → m_era_button_2
│  Controllers: button(up, down, over, selectedOver)
│  └─ title (GTextField, 56x28) "全部"
├─ era_button_3 (UIProductionEraButton·GButton·Check, 100x70) → m_era_button_3
│  Controllers: button(up, down, over, selectedOver)
│  └─ title (GTextField, 56x28) "全部"
├─ era_button_4 (UIProductionEraButton·GButton·Check, 100x70) → m_era_button_4
│  Controllers: button(up, down, over, selectedOver)
│  └─ title (GTextField, 56x28) "全部"
├─ era_button_5 (UIProductionEraButton·GButton·Check, 100x70) → m_era_button_5
│  Controllers: button(up, down, over, selectedOver)
│  └─ title (GTextField, 56x28) "全部"
└─ product_list (GList, 719x622) → m_product_list
      └─ defaultItem: UIProductionProductCard (UIProductionProductCard·GComponent)
```

## UIProductionProductCard (UIProductionProductCard·GComponent, 176x204) [exported]

```text
Controllers: state(普通, 选中)
├─ product_card_shadow (GImage, 169x197) → m_product_card_shadow ← product_card_shadow
├─ product_card_background (GImage, 170x198) → m_product_card_background ← product_card_background
├─ product_card_highlight (GImage, 572x287) → m_product_card_highlight ← product_card_highlight
├─ product_upper_gradient (GImage, 175x139) → m_product_upper_gradient ← product_upper_gradient
├─ product_lower_gradient (GImage, 175x140) → m_product_lower_gradient ← product_lower_gradient
├─ product_caption_background (GImage, 185x47) → m_product_caption_background ← product_caption_background
├─ title (GTextField, 105x26) → m_title "坦克组件"
└─ product_image (GLoader, 115x137) → m_product_image
```

## UIProductionEraButton (UIProductionEraButton·GButton, 56x28) [exported]

```text
Controllers: button(up, down, over, selectedOver)
└─ title (GTextField, 56x28) [无绑定] "全部"
```

## UIProductionCloseLineWindowButton (UIProductionCloseLineWindowButton·GButton, 58x60) [exported]

```text
Controllers: button(up, down, over, selectedOver)
└─ up (GImage, 57x59) → m_up ← closebutton
```

## UIProductionAllCategoryButton (UIProductionAllCategoryButton·GButton, 170x54) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ all_category_border (GImage, 167x50) → m_all_category_border ← all_category_border
├─ up (GImage, 167x50) → m_up ← all_category_background
├─ all_category_decoration (GImage, 15x15) → m_all_category_decoration ← all_category_decoration
└─ title (GTextField, 59x25) [无绑定] "全 部"
```

## UIProductionProductionLinesCategoryButton (UIProductionProductionLinesCategoryButton·GButton, 170x52) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ production_lines_category_border (GImage, 167x50) → m_production_lines_category_border ← production_lines_category_border
├─ up (GImage, 167x50) → m_up ← production_lines_category_background
├─ production_lines_category_decoration (GImage, 15x15) → m_production_lines_category_decoration ← production_lines_category_decoration
└─ title (GTextField, 61x25) [无绑定] "产 线"
```

## UIProductionProductionLeftCategoryButton (UIProductionProductionLeftCategoryButton·GButton, 170x52) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ production_left_category_border (GImage, 168x50) → m_production_left_category_border ← production_left_category_border
├─ up (GImage, 168x50) → m_up ← production_left_category_background
├─ production_left_category_decoration (GImage, 15x15) → m_production_left_category_decoration ← production_left_category_decoration
└─ title (GTextField, 60x25) [无绑定] "生 产"
```

## UIProductionProductionCenterLeftCategoryButton (UIProductionProductionCenterLeftCategoryButton·GButton, 170x52) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ production_center_left_category_border (GImage, 168x50) → m_production_center_left_category_border ← production_center_left_category_border
├─ up (GImage, 168x50) → m_up ← production_center_left_category_background
├─ production_center_left_category_decoration (GImage, 15x15) → m_production_center_left_category_decoration ← production_center_left_category_decoration
└─ title (GTextField, 60x25) [无绑定] "生 产"
```

## UIProductionProductionCenterRightCategoryButton (UIProductionProductionCenterRightCategoryButton·GButton, 170x52) [exported]

```text
Controllers: button(up, down, over, selectedOver)
├─ production_center_right_category_border (GImage, 168x50) → m_production_center_right_category_border ← production_center_left_category_border
├─ up (GImage, 168x50) → m_up ← production_center_left_category_background
├─ production_center_right_category_decoration (GImage, 15x15) → m_production_center_right_category_decoration ← production_center_left_category_decoration
└─ title (GTextField, 60x25) [无绑定] "生 产"
```
