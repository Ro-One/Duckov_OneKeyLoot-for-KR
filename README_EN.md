# Escape from Duckov Mod —— One-Key Loot

[中文](README.md) | English | [한국어](README_KR.md)

## Steam WorkShop

<https://steamcommunity.com/sharedfiles/filedetails/?id=3589344509>

## [Features]

- Show the **[Collect All]** button
- Add a **[One-key Loot (by Quality)]** button group
- Add a **[One-key Loot (by Value)]** button group
- Add a **[One-key Loot (by Value per unit Weight)]** button group
- Support **hot switching between Chinese/English localization**
- Support **hot switching of custom dynamic configuration**:
  - Show/Hide the [Collect All / One-key Loot] button or button groups
  - Customize the number of One-key Loot buttons
  - Customize the color of One-key Loot buttons
  - Customize the One-key Loot range
- Support **ModConfig（Optional）**  
  *<https://steamcommunity.com/sharedfiles/filedetails/?id=3590674339>*  
  *Currently, ModConfig’s configuration text does not support hot switching between Chinese and English; after modifying it, you must restart the game for changes to take effect.*

> The UI portion of this Mod was assisted by ChatGPT-5.

---

## [Button Logic]

### [Collect All]

- Pick up all items in the container with one click, until inventory slots are full.

### [One-key Loot (by Quality)]

- Conditional pickup, until inventory slots are full:
  - Item is marked with a **Heart (Favorite)**
  - Item Quality is **≥ the minimum Quality threshold shown on the button**

### [One-key Loot (by Value)]

- Conditional pickup, until inventory slots are full:
  - Item is marked with a **Heart (Favorite)**
  - Item’s store selling price is **≥ the minimum price threshold shown on the button**

### [One-key Loot (by Value per unit Weight)]

- Conditional pickup, until inventory slots are full:
  - Item is marked with a **Heart (Favorite)**
  - Item's total store selling price / Item's total weight is **≥ the minimum $/kg threshold shown on the button**

---

## [How to Use]

1. Subscribe to this Mod on the Steam Workshop page.
2. Enter the game → open the **Mod** menu → check **One-key Loot**.
3. If you have **ModConfig** installed: also check **ModConfig** and place it above **One-key Loot** in the load order.
4. **Restart the game once** to ensure the configuration loads correctly.
5. Enter the game → load a save → enter the map.
6. Open any loot container; you will see the default 3 button groups. Click as needed.

---

---

## [Custom Configuration]

*After updating, on first use, please follow the steps in [How to Use] end-to-end to correctly generate/load the configuration.*

### Using [ModConfig] (Recommended, supports hot switching)

- After entering the game and loading your save, open **ESC → Settings → Mod Settings**.

### Without [ModConfig] (hot switching not supported)

- Close the game, then open and edit the following file in Windows, and save it:

> Escape from Duckov\Duckov_Data\StreamingAssets\OneKeyLootConfig.txt

### [Quality Range]

- **Default:** 2,3,4,5
- Represents 4 buttons: **Quality ≥ 2**, **Quality ≥ 3**, **Quality ≥ 4**, **Quality ≥ 5**
- Each value is a minimum Quality; the in-game Quality range is **1 ~ 9**.

### [Value Range]

- **Default:** 100,500,1000
- Represents 3 buttons: **Value ≥ 100**, **Value ≥ 500**, **Value ≥ 1000**
- Each value is a store selling price threshold (usually half of an item’s normal value).

### [Value/Weight Range]

- **Default:** 500,2500,5000
- Represents 3 buttons: **$/kg ≥ 500**, **$/kg ≥ 2500**, **$/kg ≥ 5000**
- Each value is a Value per unit Weight threshold (the Weight includes all sub-items in the item slots).

### [Button Group Color]

- **Default:** #4CAF4F,#42A5F5,#BA68C6,#BF7F33
- Represents 4 buttons: the colors are **#4CAF4F**, **#42A5F5**, **#BA68C6**, **#BF7F33**
- Each color string must be in hexadecimal and begin with #. Opacity is supported (#4CAF4F is equivalent to #4CAF4FFF).

### Configuration Format Rules

- [Quality Range], [Value Range], [Value/Weight Range] and [Button Group Color] each support **1~4** custom values
- Separate values with a half-width comma **,**
- If the custom format is invalid, defaults will be used instead

---

## [Compatibility]

- This Mod’s Quality filtering is based on the **vanilla game’s officially defined hidden Quality** and does not conflict with “custom loot color” type Mods.
- Limited by the Compiler, currently compiled only as a **.NET Framework 4.8** DLL. If .NET Framework 4.8 is not installed on your system, the Mod may fail to run (uncertain).
- This Mod’s UI is anchored to the game’s originally hidden “one-click pickup” button; if another Mod modifies that logic, conflicts may occur.

---

## [TODO]

- Dynamically display corresponding Quality-level buttons based on the container’s loot
- Further UI optimization
- More customizable options
