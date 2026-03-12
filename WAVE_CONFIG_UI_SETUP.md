# Wave Configuration UI Setup Guide

This guide explains how to set up the UI prefabs and scene configuration for the Custom Wave Configuration system in the Map Editor.

## Overview

The Wave Configuration UI system consists of two main prefabs that need to be created in Unity:

1. **Wave Card Prefab** - Displays a summary of each wave in the list
2. **Enemy Group Prefab** - Shows configuration options for each enemy group within a wave

These prefabs are referenced by `WaveConfigurationUI.cs` and instantiated dynamically when editing waves.

---

## Prerequisites

- Unity 2022.3 LTS or later
- TextMeshPro package installed
- Map Editor scene set up with MapEditorUI

---

## Wave Card Prefab Setup

**File Location:** `Assets/Prefabs/UI/WaveCard.prefab`

### Hierarchy Structure

```
WaveCard (GameObject)
├── Background (Image)
├── WaveNumberText (TextMeshProUGUI)
├── EnemyCountText (TextMeshProUGUI)
├── RewardText (TextMeshProUGUI)
├── EditButton (Button)
│   └── Text (TextMeshProUGUI)
└── DeleteButton (Button)
    └── Text (TextMeshProUGUI)
```

### Component Details

#### Root GameObject: WaveCard
- **RectTransform**:
  - Anchors: Stretch horizontal (0,0,1,1)
  - Height: 80-100 pixels
  - Pivot: (0.5, 0.5)
- **Layout Element** (optional):
  - Min Height: 80
  - Preferred Height: 80
  - Flexible Width: 1

#### Background (Image)
- **Component:** Image
- **Color:** Light gray (#E0E0E0) or theme color
- **Raycast Target:** true (for selection)
- **Material:** Default UI Material
- **RectTransform:** Anchored fill (0,0,1,1)

#### WaveNumberText (TextMeshProUGUI)
- **Object Name:** Must be exactly `WaveNumberText` (code searches by this name)
- **RectTransform:**
  - Anchors: Left stretch (0, 0.5, 0, 0.5)
  - Width: 100-120 pixels
  - Offset X: 10-15 (left margin)
- **TextMeshProUGUI Settings:**
  - Font Size: 18-20
  - Font Style: Bold
  - Color: Dark gray or black (#333333)
  - Alignment: Middle Left
  - Text: "Wave 1" (placeholder)
  - Auto Size: false
  - Overflow: Ellipsis

#### EnemyCountText (TextMeshProUGUI)
- **Object Name:** Must be exactly `EnemyCountText`
- **RectTransform:**
  - Anchors: Center middle (0.5, 0.5, 0.5, 0.5)
  - Width: 100-120 pixels
  - Position: Center or slightly left
- **TextMeshProUGUI Settings:**
  - Font Size: 16-18
  - Font Style: Regular
  - Color: Medium gray (#666666)
  - Alignment: Middle Center
  - Text: "15 enemies" (placeholder)
  - Overflow: Ellipsis

#### RewardText (TextMeshProUGUI)
- **Object Name:** Must be exactly `RewardText`
- **RectTransform:**
  - Anchors: Right stretch (1, 0.5, 1, 0.5)
  - Width: 80-100 pixels
  - Offset X: -180 to -200 (space for buttons)
- **TextMeshProUGUI Settings:**
  - Font Size: 16-18
  - Font Style: Bold
  - Color: Gold or green (#FFD700 or #4CAF50)
  - Alignment: Middle Right
  - Text: "$200" (placeholder)
  - Overflow: Ellipsis

#### EditButton (Button)
- **Object Name:** Must be exactly `EditButton`
- **RectTransform:**
  - Anchors: Right middle (1, 0.5, 1, 0.5)
  - Width: 70-80 pixels
  - Height: 40-50 pixels
  - Offset X: -90 to -100
- **Button Component:**
  - Interactable: true
  - Navigation: None (or Automatic)
  - Transition: Color Tint
  - Normal Color: Light blue (#2196F3)
  - Highlighted Color: Bright blue (#42A5F5)
  - Pressed Color: Dark blue (#1976D2)
  - Disabled Color: Gray (#BDBDBD)
- **Child Text:**
  - Text: "Edit"
  - Font Size: 14-16
  - Color: White
  - Alignment: Middle Center

#### DeleteButton (Button)
- **Object Name:** Must be exactly `DeleteButton`
- **RectTransform:**
  - Anchors: Right middle (1, 0.5, 1, 0.5)
  - Width: 70-80 pixels
  - Height: 40-50 pixels
  - Offset X: -10 to -15 (right margin)
- **Button Component:**
  - Interactable: true
  - Navigation: None
  - Transition: Color Tint
  - Normal Color: Light red (#F44336)
  - Highlighted Color: Bright red (#EF5350)
  - Pressed Color: Dark red (#C62828)
  - Disabled Color: Gray (#BDBDBD)
- **Child Text:**
  - Text: "Delete" or "✕"
  - Font Size: 14-16
  - Color: White
  - Alignment: Middle Center

### Visual Example

```
┌───────────────────────────────────────────────────────────────┐
│ Wave 1           15 enemies           $200    [Edit] [Delete] │
└───────────────────────────────────────────────────────────────┘
```

---

## Enemy Group Prefab Setup

**File Location:** `Assets/Prefabs/UI/EnemyGroup.prefab`

### Hierarchy Structure

```
EnemyGroup (GameObject)
├── Background (Image)
├── EnemyTypeDropdown (TMP_Dropdown)
│   ├── Label (TextMeshProUGUI)
│   ├── Arrow (Image)
│   └── Template (ScrollRect)
├── CountInput (TMP_InputField)
│   ├── Text Area (RectTransform)
│   │   ├── Placeholder (TextMeshProUGUI)
│   │   └── Text (TextMeshProUGUI)
├── SpawnIntervalInput (TMP_InputField)
│   ├── Text Area (RectTransform)
│   │   ├── Placeholder (TextMeshProUGUI)
│   │   └── Text (TextMeshProUGUI)
└── DeleteButton (Button)
    └── Text (TextMeshProUGUI)
```

### Component Details

#### Root GameObject: EnemyGroup
- **RectTransform**:
  - Anchors: Stretch horizontal (0,0,1,1)
  - Height: 60-80 pixels
  - Pivot: (0.5, 0.5)
- **Layout Element** (optional):
  - Min Height: 60
  - Preferred Height: 70

#### Background (Image)
- **Component:** Image
- **Color:** Very light gray (#F5F5F5) or transparent
- **Raycast Target:** true
- **RectTransform:** Anchored fill

#### EnemyTypeDropdown (TMP_Dropdown)
- **Object Name:** Must be exactly `EnemyTypeDropdown`
- **RectTransform:**
  - Anchors: Left stretch (0, 0.5, 0, 0.5)
  - Width: 150-180 pixels
  - Height: 40-50 pixels
  - Offset X: 10 (left margin)
- **TMP_Dropdown Component:**
  - Interactable: true
  - Template: Dropdown template (default TextMeshPro setup)
  - Caption Text: Label child object
  - Item Text: Template item text
  - Options: Empty (populated by code)
  - Font Size: 14-16
- **Label (TextMeshProUGUI):**
  - Text: "Soldier" (placeholder)
  - Font Size: 14-16
  - Color: Black
  - Alignment: Middle Left
  - Overflow: Ellipsis

#### CountInput (TMP_InputField)
- **Object Name:** Must be exactly `CountInput`
- **RectTransform:**
  - Anchors: Left stretch (0, 0.5, 0, 0.5)
  - Width: 80-100 pixels
  - Height: 40-50 pixels
  - Offset X: 200 (after dropdown)
- **TMP_InputField Component:**
  - Interactable: true
  - Content Type: Integer Number
  - Line Type: Single Line
  - Character Limit: 3
  - Text Component: Text child
  - Placeholder: Placeholder child
- **Text (TextMeshProUGUI):**
  - Font Size: 14-16
  - Color: Black
  - Alignment: Middle Center
- **Placeholder (TextMeshProUGUI):**
  - Text: "Count"
  - Font Size: 14-16
  - Color: Gray (#888888)
  - Alignment: Middle Center

#### SpawnIntervalInput (TMP_InputField)
- **Object Name:** Must be exactly `SpawnIntervalInput`
- **RectTransform:**
  - Anchors: Left stretch (0, 0.5, 0, 0.5)
  - Width: 80-100 pixels
  - Height: 40-50 pixels
  - Offset X: 310 (after count input)
- **TMP_InputField Component:**
  - Interactable: true
  - Content Type: Decimal Number
  - Line Type: Single Line
  - Character Limit: 5
  - Text Component: Text child
  - Placeholder: Placeholder child
- **Text (TextMeshProUGUI):**
  - Font Size: 14-16
  - Color: Black
  - Alignment: Middle Center
- **Placeholder (TextMeshProUGUI):**
  - Text: "Interval"
  - Font Size: 14-16
  - Color: Gray
  - Alignment: Middle Center

#### DeleteButton (Button)
- **Object Name:** Must be exactly `DeleteButton`
- **RectTransform:**
  - Anchors: Right middle (1, 0.5, 1, 0.5)
  - Width: 60-70 pixels
  - Height: 40-50 pixels
  - Offset X: -10 (right margin)
- **Button Component:**
  - Interactable: true
  - Transition: Color Tint
  - Normal Color: Light red (#F44336)
  - Highlighted Color: Bright red (#EF5350)
  - Pressed Color: Dark red (#C62828)
- **Child Text:**
  - Text: "✕" or "Delete"
  - Font Size: 16-18
  - Color: White
  - Alignment: Middle Center

### Visual Example

```
┌──────────────────────────────────────────────────────────┐
│ [Scout ▼]    [10]    [0.5]                         [✕]  │
└──────────────────────────────────────────────────────────┘
   Enemy Type  Count   Interval                     Delete
```

---

## Scene Setup: Map Editor

### WaveConfigurationUI GameObject Hierarchy

In your **MapEditor** scene, create or modify the UI structure:

```
Canvas
└── MapEditorUI
    ├── EditorPanel
    │   ├── ... (existing editor UI)
    │   └── ConfigureWavesButton (Button)
    └── WaveConfigurationUI (GameObject + WaveConfigurationUI script)
        ├── WaveConfigPanel (GameObject - main panel)
        │   ├── Background (Image)
        │   ├── Header (GameObject)
        │   │   ├── TitleText (TextMeshProUGUI) - "Wave Configuration"
        │   │   └── CloseButton (Button)
        │   ├── WaveListSection (GameObject)
        │   │   ├── HeaderRow (GameObject)
        │   │   │   ├── WaveCountText (TextMeshProUGUI)
        │   │   │   └── AddWaveButton (Button)
        │   │   └── WaveListContainer (GameObject + Vertical Layout Group)
        │   │       └── (Wave cards instantiated here)
        │   ├── WaveEditorPanel (GameObject - editing panel)
        │   │   ├── WaveNumberText (TextMeshProUGUI)
        │   │   ├── PropertiesSection (GameObject)
        │   │   │   ├── CreditsRewardInput (TMP_InputField)
        │   │   │   ├── TimeBetweenGroupsInput (TMP_InputField)
        │   │   │   ├── BossWaveToggle (Toggle)
        │   │   │   └── BossTypeDropdown (TMP_Dropdown)
        │   │   ├── EnemyGroupsSection (GameObject)
        │   │   │   ├── SectionLabel (TextMeshProUGUI)
        │   │   │   ├── AddEnemyGroupButton (Button)
        │   │   │   └── EnemyGroupContainer (GameObject + Vertical Layout Group)
        │   │   │       └── (Enemy groups instantiated here)
        │   │   └── SaveButton (Button)
        │   ├── QuickSetupSection (GameObject)
        │   │   ├── SectionLabel (TextMeshProUGUI)
        │   │   ├── WaveCountInput (TMP_InputField)
        │   │   ├── DifficultyDropdown (TMP_Dropdown)
        │   │   └── GenerateDefaultWavesButton (Button)
        │   └── ValidationPanel (GameObject)
        │       ├── Background (Image)
        │       ├── ValidationText (TextMeshProUGUI)
        │       └── CloseButton (Button)
        ├── waveCardPrefab (Prefab reference)
        └── enemyGroupPrefab (Prefab reference)
```

### WaveConfigurationUI Component Setup

1. **Add WaveConfigurationUI script** to the WaveConfigurationUI GameObject

2. **Assign Serialized Fields** in Inspector:

   **Wave Configuration Panel:**
   - Wave Config Panel: WaveConfigPanel GameObject
   - Open Wave Config Button: ConfigureWavesButton (in MapEditorUI)
   - Close Wave Config Button: CloseButton in header

   **Wave List:**
   - Wave List Container: WaveListContainer GameObject
   - Wave Card Prefab: WaveCard.prefab asset
   - Add Wave Button: AddWaveButton
   - Wave Count Text: WaveCountText

   **Wave Editor:**
   - Wave Editor Panel: WaveEditorPanel GameObject
   - Wave Number Text: WaveNumberText
   - Credits Reward Input: CreditsRewardInput
   - Time Between Groups Input: TimeBetweenGroupsInput
   - Boss Wave Toggle: BossWaveToggle
   - Boss Type Dropdown: BossTypeDropdown

   **Enemy Groups:**
   - Enemy Group Container: EnemyGroupContainer GameObject
   - Enemy Group Prefab: EnemyGroup.prefab asset
   - Add Enemy Group Button: AddEnemyGroupButton

   **Quick Setup:**
   - Generate Default Waves Button: GenerateDefaultWavesButton
   - Wave Count Input: WaveCountInput
   - Difficulty Dropdown: DifficultyDropdown

   **Validation:**
   - Validation Text: ValidationText
   - Validation Panel: ValidationPanel GameObject

3. **Configure Layout Groups:**

   **WaveListContainer:**
   - Add **Vertical Layout Group**:
     - Padding: 10px all sides
     - Spacing: 5-10px
     - Child Force Expand: Width = true, Height = false
     - Child Control Size: Width = true, Height = true
   - Add **Content Size Fitter**:
     - Vertical Fit: Preferred Size

   **EnemyGroupContainer:**
   - Add **Vertical Layout Group**:
     - Padding: 5px all sides
     - Spacing: 5px
     - Child Force Expand: Width = true, Height = false
     - Child Control Size: Width = true, Height = true
   - Add **Content Size Fitter**:
     - Vertical Fit: Preferred Size

4. **Configure Scroll Views** (optional but recommended):

   Add ScrollRect components to both containers for better UX when there are many waves/groups.

### MapEditorUI Integration

1. **Add ConfigureWavesButton** to MapEditorUI panel:
   - Position: Next to Save and Test Play buttons
   - Text: "Configure Waves"
   - Color: Yellow or orange theme (#FFA726)

2. **Reference WaveConfigurationUI in MapEditorUI script:**
   - Open MapEditorUI in Inspector
   - Assign Wave Config UI field to WaveConfigurationUI GameObject

---

## Testing Checklist

### Wave Card Prefab Tests
- [ ] Wave card appears in list when adding waves
- [ ] Wave number displays correctly
- [ ] Enemy count calculates total from all groups
- [ ] Credits reward shows with $ prefix
- [ ] Edit button opens wave editor panel
- [ ] Delete button removes wave (with confirmation)
- [ ] Card layout adjusts to container width

### Enemy Group Prefab Tests
- [ ] Enemy group appears when adding to wave
- [ ] Dropdown shows all 12 enemy types
- [ ] Selected enemy type displays correctly
- [ ] Count input accepts numbers 1-100
- [ ] Spawn interval input accepts decimals 0.1-10.0
- [ ] Delete button removes enemy group
- [ ] Layout adapts to container dimensions

### Full System Tests
- [ ] "Configure Waves" button opens wave configuration panel
- [ ] Can add, edit, and delete waves
- [ ] Can add, edit, and delete enemy groups within waves
- [ ] Quick setup generates 5-30 waves with difficulty scaling
- [ ] Validation shows errors/warnings/suggestions
- [ ] Wave configuration saves with map
- [ ] Test play loads custom waves correctly
- [ ] Return to editor preserves wave configuration

---

## Styling Guidelines

### Color Scheme
- **Primary:** #2196F3 (Blue)
- **Secondary:** #FFA726 (Orange)
- **Success:** #4CAF50 (Green)
- **Warning:** #FFC107 (Yellow)
- **Error:** #F44336 (Red)
- **Background:** #F5F5F5 (Light Gray)
- **Text:** #212121 (Dark Gray)
- **Text Secondary:** #757575 (Medium Gray)

### Font Sizes
- **Headers:** 20-24px, Bold
- **Body Text:** 16-18px, Regular
- **Labels:** 14-16px, Regular
- **Buttons:** 14-16px, Bold
- **Input Fields:** 14-16px, Regular

### Spacing
- **Panel Padding:** 20px
- **Section Spacing:** 15-20px
- **Element Spacing:** 10px
- **Tight Spacing:** 5px

---

## Troubleshooting

### "WaveCardPrefab is null" Error
- Ensure WaveCard.prefab is created in Assets/Prefabs/UI/
- Assign prefab to Wave Card Prefab field in WaveConfigurationUI Inspector
- Check that prefab has all required child objects with exact names

### "Cannot find child object" Warnings
- Verify child object names match exactly (case-sensitive):
  - `WaveNumberText`, `EnemyCountText`, `RewardText`
  - `EditButton`, `DeleteButton`
  - `EnemyTypeDropdown`, `CountInput`, `SpawnIntervalInput`
- Use `transform.Find("ObjectName")` in code to test

### Layout Issues
- Check RectTransform anchor settings match documented values
- Ensure Layout Groups have correct settings
- Verify Content Size Fitters are configured
- Test with different container sizes

### Dropdown Not Showing Options
- Ensure TMP_Dropdown has Template setup correctly
- Check that Template has ScrollRect and Viewport
- Verify Item Text is assigned in dropdown

---

## Additional Resources

- **Unity UI Documentation:** [docs.unity3d.com/Manual/UISystem.html](https://docs.unity3d.com/Manual/UISystem.html)
- **TextMeshPro Guide:** [docs.unity3d.com/Manual/com.unity.textmeshpro.html](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)
- **Layout Groups:** [docs.unity3d.com/Manual/UIAutoLayout.html](https://docs.unity3d.com/Manual/UIAutoLayout.html)

---

**Last Updated:** March 12, 2026  
**Version:** 2.1 - Custom Map Editor  
**Author:** Robot Tower Defense Team
