using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 轻量本地化模块。
    /// - 默认语言 zh-CN，直接使用代码中的中文 fallback。
    /// - 其他语言通过 BepInEx/config/ED3R_Lang/&lt;lang&gt;.txt 覆盖。
    /// - 语言选择文件：BepInEx/config/ED3R_Language.txt，内容：Language = en-US
    /// - 文件格式：key=value，忽略空行和 # 注释。
    /// - 缺失键自动回退中文 fallback，不崩溃。
    /// </summary>
    public static class Loc
    {
        public static string CurrentLanguage { get; private set; } = "zh-CN";
        private static readonly Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static bool _initialized;

        // 缓存"当前是否 zh-CN"。避免 T() / CfgDesc() 在热路径上重复做字符串比较。
        private static bool _isZhCN = true;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                string configDir = Paths.ConfigPath;
                string langSettingFile = Path.Combine(configDir, "ED3R_Language.txt");
                string langDir = Path.Combine(configDir, "ED3R_Lang");

                // 读取语言设置
                if (File.Exists(langSettingFile))
                {
                    foreach (string line in File.ReadAllLines(langSettingFile))
                    {
                        string trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                        string[] parts = trimmed.Split(new[] { '=' }, 2);
                        if (parts.Length == 2 && parts[0].Trim().Equals("Language", StringComparison.OrdinalIgnoreCase))
                        {
                            string lang = parts[1].Trim();
                            if (!string.IsNullOrEmpty(lang))
                                CurrentLanguage = lang;
                            break;
                        }
                    }
                }

                _isZhCN = CurrentLanguage.Equals("zh-CN", StringComparison.OrdinalIgnoreCase);

                // 中文直接使用 fallback
                if (_isZhCN)
                    return;

                // 加载对应语言文件
                string file = Path.Combine(langDir, CurrentLanguage + ".txt");
                if (!File.Exists(file))
                {
                    Plugin.Log?.LogWarning($"[Loc] Language file not found: {file}, falling back to zh-CN");
                    CurrentLanguage = "zh-CN";
                    _isZhCN = true;
                    return;
                }

                foreach (string line in File.ReadAllLines(file))
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                    string[] parts = trimmed.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        if (!string.IsNullOrEmpty(key))
                            _translations[key] = value;
                    }
                }

                Plugin.Log?.LogInfo($"[Loc] Loaded language: {CurrentLanguage}, {_translations.Count} entries");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[Loc] Init failed: {ex.Message}");
                CurrentLanguage = "zh-CN";
                _isZhCN = true;
            }
        }

        /// <summary>获取本地化文本，找不到时返回 fallback。</summary>
        public static string T(string key, string fallback)
        {
            if (_isZhCN)
                return fallback;

            if (_translations.TryGetValue(key, out string value))
                return value;

            return fallback;
        }

        /// <summary>配置项描述本地化。键格式：cfg.&lt;section&gt;.&lt;key&gt;.desc</summary>
        public static string CfgDesc(string section, string key, string zhDesc)
        {
            if (_isZhCN)
                return zhDesc;

            string locKey = $"cfg.{section}.{key}.desc";
            return T(locKey, zhDesc);
        }
    }
}