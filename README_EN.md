[**中文**](README.md) | [**English**](README_EN.md) | [**日本語**](README_JP.md)

# ED3R Mod — Eternal Dread 3 Feature Expansion

**A large feature expansion mod for *Eternal Dread 3*.**
The default configuration leans towards an "enhanced power fantasy", but every single setting can be adjusted or reverted to vanilla.

> ⚠️ **Maintenance Status**: This mod is basically no longer maintained after release. If you encounter issues, please study the source code or check Issues (the author may not reply in time).

---

## ✨ Main Features

### Controls & Convenience
* Sprint stamina cost is 25% by default, sprint in any direction.
* Gathering and recovery item usage can be interrupted, effects apply instantly.
* Pickup range expanded, inventory and equipment slots automatically expand by one page.
* Inventory automatically sorts when opened, sorting rules can be switched.

### Combat & Skills
* Backstabs, flanking, stun, freeze, knockdown, and guard counters affect combat.
* Poison and burning can be lethal, burning spreads, rolling extinguishes fire once.
* Multi-crit, level difference correction, freeze penetration mechanics are optional.
* Skill tree prerequisites relaxed, skill points can be refreshed, passive slots expandable.

### Enemies & AI
* Cone vision, wall occlusion, noise alertness, detection prompts.
* Enemy level following cap is set by difficulty, no longer completely following the player.
* Night-spawned enemies gain random modifiers and level bonuses based on difficulty.

### Killstreak & Ecology
* Continuous kills are rated D / C / B / A / S / SS / SSS.
* Regional monster density decreases with kills and recovers over time.
* Quest areas are exempt from ecology filtering, ensuring story monsters spawn normally.

### UI & Minimap
* Threat ring indicates enemies locking on and attacking.
* Minimap shows enemies, allies, resources, chests, and skill points.
* Hold Ctrl to quickly skip dialogue, quick confirmation for popups.

### Permadeath (Optional)
* Can be checked at character creation, death deletes the save.
* For players who want a true hardcore experience after mastering the system.

---

## 🔧 Prerequisites

* **BepInEx x64** must be installed (recommended version: `BepInEx_win_x64_5.4.23.5`).
* Developed for *Eternal Dread 3* `v1.0`.
* Official BepInEx download: [https://github.com/BepInEx/BepInEx/releases](https://github.com/BepInEx/BepInEx/releases)

---

## 📥 Installation

1. Install BepInEx x64, extract it to the game root directory, and run the game once first.
2. Click the **Releases** tab on the right side of this repository, download the latest package (`.zip`), extract it, and overwrite the `BepInEx` folder into the game root directory.
3. Restart the game. If UI elements such as the killstreak UI appear, the installation is successful.

*Tip: It is recommended to use only English characters in the game path, and avoid Chinese characters or special symbols.*

---

## ⚙️ Configuration & Localization

**Config file location:**
`BepInEx\config\com.SoulDRoul.EternalDread3Rmod.cfg`

The default configuration is the recommended configuration; just play. If you want to restore vanilla, play stealth, killstreak power fantasy, or hardcore challenge, Chapter 11 of the full manual has the corresponding adjustment directions. Configuration changes take effect after restarting the game.

**Localization support:**
Built-in Chinese, English, and Japanese; Chinese is the default. Switch via `BepInEx\config\ED3R_Language.txt`; change one line and restart the game. If translation files are missing, it automatically falls back to Chinese without errors.

> 📖 **About the Full Manual**: Currently only available in Chinese. Due to limited energy, full English/Japanese translations are not provided. Community translations via Pull Requests are highly welcome!

---

## 🧩 Compatibility

* Developed based on `BepInEx_win_x64_5.4.23.5`.
* Modifies multiple systems such as movement, combat, AI, UI, minimap, ecology, etc.
* If other mods also modify the same systems, conflicts may occur.
* It is recommended to install only this mod first, confirm it works properly, and then add other mods one by one.

---

## 📜 License & Source Code

This project is licensed under [CC BY-NC 4.0](LICENSE).

* ✅ Anyone is allowed to view, study, and modify the source code of this mod.
* ✅ Modified versions do not require notifying the original author, but the original author's name must be retained in a prominent place.
* ❌ It is strictly prohibited to use this code (including modified versions) for any commercial profit or fraudulent activities.
* For the full legal text, see the [LICENSE](LICENSE) file in the root directory.

Author: **SoulDRoul**

---

## 💬 Feedback & Credits

**Feedback Channels:**
* GitHub: Issues
* Bilibili: Video comments or private messages
* Tieba: Eternal Dread Tieba Mod release post
* Nexus: Posts section

When reporting issues, please include the modified config items, actions before the issue occurred, and the BepInEx log (`BepInEx\LogOutput.log`) to help locate the problem faster.

**Credits:**
Thanks to the developers of *Eternal Dread 3* for creating such a uniquely atmospheric game.
Thanks to the BepInEx framework developers for making mod development possible.
Thanks to all players who tested and provided feedback.
And thank you for reading this far. Enjoy the game!

——SoulDRoul