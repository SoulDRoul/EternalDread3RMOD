# ED3R Mod — 永恒恐惧3 功能扩展

**一个为《永恒恐惧3》制作的大型功能扩展 Mod。**
默认配置偏“增强爽游”，但每一条都可以调，也可以还原原版。

> ⚠️ **维护状态说明**：本 Mod 发布后基本不再维护，如遇问题请自行研究源码或查阅 Issues。

---

## ✨ 主要功能

### 操作与便利
* 冲刺体力消耗默认 25%，任意方向可冲刺。
* 采集和恢复类物品使用可被打断，效果立即生效。
* 拾取范围扩大，物品栏和装备栏自动扩展一页。
* 打开物品栏自动排序，排序规则可切换。

### 战斗与技能
* 背刺、绕后、眩晕、冰冻、倒地、格挡反击都会影响战局。
* 中毒和燃烧可致死，燃烧会蔓延，翻滚可一次灭火。
* 多重暴击、等级差补正、冰冻穿透等机制可选。
* 技能组前置放宽，技能点可刷新，被动槽位可扩展。

### 敌人与 AI
* 锥形视野、墙体遮挡、噪音警觉、发现提示。
* 敌人等级跟随玩家的上限按难度设置，不再完全跟随。
* 夜晚生成的敌人按难度概率获得随机词条和等级加成。

### 连杀与生态
* 连续击杀按 D / C / B / A / S / SS / SSS 评级。
* 区域怪物密度会因杀戮下降，随时间恢复。
* 任务区域豁免生态过滤，保证剧情怪正常刷新。

### 界面与小地图
* 威胁环指示锁定和攻击中的敌人。
* 小地图显示敌人、友军、资源、宝箱、技能点。
* 按住 Ctrl 快速跳过对话，弹窗快捷确认。

### 永久死亡（可选）
* 创角界面可勾选永久死亡模式，死亡即删档。
* 给熟悉系统之后想要真正硬核体验的玩家准备。

---

## 🔧 前置要求 / Prerequisites / 前提条件

* **中文**：必须安装 **BepInEx x64**（推荐版本：`BepInEx_win_x64_5.4.23.5`）。<br>
  **English**: **BepInEx x64** must be installed (recommended version: `BepInEx_win_x64_5.4.23.5`).<br>
  **日本語**: **BepInEx x64** が必要です（推奨バージョン：`BepInEx_win_x64_5.4.23.5`）。
* **中文**：针对《永恒恐惧3》`v1.0` 版本开发。<br>
  **English**: Developed for *Eternal Dread 3* `v1.0`.<br>
  **日本語**: 『Eternal Dread 3』`v1.0` 向けに開発されています。
* **中文**：BepInEx 官方下载地址：[https://github.com/BepInEx/BepInEx/releases](https://github.com/BepInEx/BepInEx/releases)<br>
  **English**: Official BepInEx download: [https://github.com/BepInEx/BepInEx/releases](https://github.com/BepInEx/BepInEx/releases)<br>
  **日本語**: BepInEx 公式ダウンロード先：[https://github.com/BepInEx/BepInEx/releases](https://github.com/BepInEx/BepInEx/releases)

---

## 📥 安装方法 / Installation / インストール方法

1. **中文**：安装 BepInEx x64 版本，解压到游戏根目录，先运行一次游戏。<br>
   **English**: Install BepInEx x64, extract it to the game root directory, and run the game once first.<br>
   **日本語**: BepInEx x64 をインストールし、ゲームのルートディレクトリに解凍して、まず一度ゲームを起動します。
2. **中文**：点击本仓库右侧的 Releases 标签，下载最新的安装包（.zip），解压后将 BepInEx 文件夹覆盖到游戏根目录即可。<br>
   **English**: Click the Releases tab on the right side of this repository, download the latest package (.zip), extract it, and overwrite the BepInEx folder into the game root directory.<br>
   **日本語**: 本リポジトリ右側の Releases タブをクリックし、最新のインストールパッケージ（.zip）をダウンロードして解凍し、BepInEx フォルダをゲームのルートディレクトリに上書きしてください。
3. **中文**：重启游戏，出现连杀 UI 等界面元素即安装成功。<br>
   **English**: Restart the game. If UI elements such as the killstreak UI appear, the installation is successful.<br>
   **日本語**: ゲームを再起動し、キルストリークUIなどのUI要素が表示されればインストール成功です。

*提示：游戏路径建议纯英文，避免中文或特殊符号。*<br>
**English**: *Tip: It is recommended to use only English characters in the game path, and avoid Chinese characters or special symbols.*<br>
**日本語**: *ヒント：ゲームパスは英字のみを推奨します。中国語や特殊記号は避けてください。*

---

## ⚙️ 配置与本地化 / Configuration & Localization / 設定とローカライズ

**配置文件位置 / Config file location / 設定ファイルの場所：**<br>
`BepInEx\config\com.SoulDRoul.EternalDread3Rmod.cfg`

**中文**：默认配置就是推荐配置，直接玩即可。想还原原版、潜行、连杀爽游或硬核挑战，完整手册第十一章有对应调整方向。改完配置需要重启游戏才生效。<br>
**English**: The default configuration is the recommended configuration; just play. If you want to restore vanilla, play stealth, killstreak power fantasy, or hardcore challenge, Chapter 11 of the full manual has the corresponding adjustment directions. Configuration changes take effect after restarting the game.<br>
**日本語**: デフォルト設定が推奨設定です。そのまま遊べます。バニラ（原作）に戻す、ステルス、キルストリーク爽快プレイ、ハードコアチャレンジにしたい場合、完全マニュアル第11章に対応する調整方針があります。設定変更はゲーム再起動後に有効になります。

**本地化支持 / Localization support / ローカライズ対応：**<br>
**中文**：内置中文、英文、日文，默认中文。通过 `BepInEx\config\ED3R_Language.txt` 切换，改一行、重启游戏即可。翻译文件缺失时自动回退中文，不会报错。<br>
**English**: Built-in Chinese, English, and Japanese; Chinese is the default. Switch via `BepInEx\config\ED3R_Language.txt`; change one line and restart the game. If translation files are missing, it automatically falls back to Chinese without errors.<br>
**日本語**: 中国語、英語、日本語を内蔵。デフォルトは中国語。`BepInEx\config\ED3R_Language.txt` で切り替え、1行変更してゲームを再起動するだけです。翻訳ファイルがない場合は自動的に中国語にフォールバックし、エラーは出ません。

**中文**：非常欢迎手册的英文和日文翻译。如果愿意贡献，欢迎提交 Pull Request。<br>
**English**: English and Japanese translations of the manual are highly welcome. If you'd like to contribute, feel free to open a Pull Request.<br>
**日本語**: マニュアルの英語・日本語翻訳を歓迎します。貢献いただける場合は、お気軽に Pull Request を作成してください。

---

## 🧩 兼容性 / Compatibility / 互換性

* **中文**：基于 `BepInEx_win_x64_5.4.23.5` 开发。<br>
  **English**: Developed based on `BepInEx_win_x64_5.4.23.5`.<br>
  **日本語**: `BepInEx_win_x64_5.4.23.5` を基に開発されています。
* **中文**：修改了移动、战斗、AI、UI、小地图、生态等多个系统。<br>
  **English**: Modifies multiple systems such as movement, combat, AI, UI, minimap, ecology, etc.<br>
  **日本語**: 移動、戦闘、AI、UI、ミニマップ、生態系など、複数のシステムを変更します。
* **中文**：如果其他 Mod 也修改了同一套系统，可能会冲突。<br>
  **English**: If other mods also modify the same systems, conflicts may occur.<br>
  **日本語**: 他のModも同じシステムを変更している場合、競合する可能性があります。
* **中文**：建议先只装本 Mod，确认正常后再逐个添加其他 Mod。<br>
  **English**: It is recommended to install only this mod first, confirm it works properly, and then add other mods one by one.<br>
  **日本語**: まず本Modのみを導入し、正常動作を確認してから他のModを一つずつ追加することを推奨します。

---

## 📜 授权与源码 / License & Source Code / ライセンスとソースコード

**中文**：本项目采用 [CC BY-NC 4.0](LICENSE) 协议。<br>
**English**: This project is licensed under [CC BY-NC 4.0](LICENSE).<br>
**日本語**: 本プロジェクトは [CC BY-NC 4.0](LICENSE) ライセンスを採用しています。

* ✅ **中文**：允许任何人查看、学习、修改本 Mod 的源代码。<br>
  **English**: Anyone is allowed to view, study, and modify the source code of this mod.<br>
  **日本語**: 誰でも本Modのソースコードを閲覧、学習、改変することができます。
* ✅ **中文**：修改后的版本无需通知原作者，但必须在显著位置保留原作者姓名。<br>
  **English**: Modified versions do not require notifying the original author, but the original author's name must be retained in a prominent place.<br>
  **日本語**: 改変版は原作者への通知は不要ですが、原作者名を目立つ場所に必ず残す必要があります。
* ❌ **中文**：严禁将本代码（包括修改后的版本）用于任何商业盈利行为或欺诈行为。<br>
  **English**: It is strictly prohibited to use this code (including modified versions) for any commercial profit or fraudulent activities.<br>
  **日本語**: 本コード（改変版を含む）を商業的利益行為または詐欺行為に使用することを固く禁じます。
* **中文**：完整法律文本请查看根目录下的 [LICENSE](LICENSE) 文件。<br>
  **English**: For the full legal text, see the [LICENSE](LICENSE) file in the root directory.<br>
  **日本語**: 完全な法的文書は、ルートディレクトリの [LICENSE](LICENSE) ファイルをご確認ください。

作者：**SoulDRoul**<br>
Author: **SoulDRoul**<br>
作者：**SoulDRoul**
---

## 💬 反馈与致谢

**反馈渠道：**
* GitHub：Issues
* B站：发布视频评论区或私信
* 贴吧：永恒恐惧吧 Mod 发布帖
* Nexus：Posts 区留言

反馈时请附上改过的配置项、问题出现前的操作、BepInEx 日志（`BepInEx\LogOutput.log`），定位会快很多。
【很可能没有看到】

**致谢：**
感谢《永恒恐惧3》的开发者，做出了这样一部气质独特的作品。
感谢 BepInEx 框架的开发者，让 Mod 开发成为可能。
感谢所有测试和反馈的玩家。
也感谢你愿意看到这里，祝游戏愉快。

——SoulDRoul
