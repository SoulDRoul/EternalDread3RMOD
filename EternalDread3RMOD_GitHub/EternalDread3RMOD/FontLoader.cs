using System;
using System.Collections.Generic;
using Microsoft.Win32;
using UnityEngine;

namespace EternalDread3RMOD
{
    public static class FontLoader
    {
        private static Dictionary<string, Font> _fontCache
            = new Dictionary<string, Font>();

        // 缓存：文件名(小写) -> 内部名
        private static Dictionary<string, string> _fileToInternalName = null;

        public static Font GetFont(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            string key = input.Trim().ToLower();
            if (_fontCache.TryGetValue(key, out Font cached)) return cached;

            string realName = ResolveFontName(input.Trim());

            // ===== 新增：检查 Unity 能不能看到这个字体 =====
            bool visibleToUnity = false;
            try
            {
                string[] osFonts = Font.GetOSInstalledFontNames();
                if (osFonts != null)
                {
                    foreach (string n in osFonts)
                    {
                        if (string.Equals(n, realName, StringComparison.OrdinalIgnoreCase))
                        {
                            visibleToUnity = true;
                            break;
                        }
                    }
                }
            }
            catch { }

            if (!visibleToUnity)
            {
                Plugin.Log.LogWarning("[ED3R] 字体 '" + realName + "' 不在 Unity 的字体列表中。\n"
                    + "  可能原因：字体是用【普通安装】装的，只对当前用户生效。\n"
                    + "  或者是字体不支持（例如大部分中文字体）。\n"
                    + "  解决方法：右键字体文件 → 选择【为所有用户安装】，或者换个字体。");
                _fontCache[key] = null;
                return null;
            }

            Plugin.Log.LogInfo("[ED3R] 字体可见于 Unity: " + realName);
            // ===== 新增结束 =====

            try
            {
                Font f = Font.CreateDynamicFontFromOSFont(realName, 32);
                if (f != null)
                {
                    _fontCache[key] = f;
                    return f;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("[ED3R] 创建字体失败 '" + realName + "': " + e.Message);
            }

            _fontCache[key] = null;
            return null;
        }

        // 把玩家输入解析成 Unity 认识的内部名
        private static string ResolveFontName(string input)
        {
            // 1. 没有 .ttf / .ttc / .otf 后缀，直接当内部名
            string lower = input.ToLower();
            bool looksLikeFile = lower.EndsWith(".ttf")
                              || lower.EndsWith(".ttc")
                              || lower.EndsWith(".otf")
                              || lower.EndsWith(".fon");

            if (!looksLikeFile)
                return input;

            // 2. 是文件名，查注册表映射
            if (_fileToInternalName == null)
                BuildFileToInternalMap();

            string fileName = System.IO.Path.GetFileName(lower);
            if (_fileToInternalName.TryGetValue(fileName, out string internalName))
            {
                Plugin.DebugLog("[ED3R] 从文件名识别: " + input + " -> " + internalName);
                return internalName;
            }

            // 3. 注册表里找不到，退回原名
            Plugin.Log.LogWarning("[ED3R] 注册表里找不到字体文件: " + input);
            return input;
        }

        // 读取注册表，建立"文件名 -> 内部名"的映射
        private static void BuildFileToInternalMap()
        {
            _fileToInternalName = new Dictionary<string, string>();

            TryReadFontKey(Registry.LocalMachine);
            TryReadFontKey(Registry.CurrentUser);

            Plugin.DebugLog("[ED3R] 已从注册表读取 "
                + _fileToInternalName.Count + " 个字体映射");
        }

        private static void TryReadFontKey(RegistryKey root)
        {
            try
            {
                using (RegistryKey key = root.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts"))
                {
                    if (key == null)
                    {
                        Plugin.DebugLog("[ED3R] 注册表位置无权限或不存在: "
                            + root.Name);
                        return;
                    }

                    foreach (string valueName in key.GetValueNames())
                    {
                        try
                        {
                            string fileName = key.GetValue(valueName) as string;
                            if (string.IsNullOrEmpty(fileName)) continue;

                            // 注册表值可能是纯文件名，也可能是完整路径
                            string fileKey = System.IO.Path
                                .GetFileName(fileName.ToLower());

                            // 去掉 "(TrueType)" "(OpenType)" 之类的后缀
                            string displayName = valueName;
                            int paren = displayName.LastIndexOf(" (");
                            if (paren > 0) displayName = displayName.Substring(0, paren);

                            if (!_fileToInternalName.ContainsKey(fileKey))
                            {
                                _fileToInternalName[fileKey] = displayName;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("[ED3R] 读注册表失败: " + e.Message);
            }
        }
    }
}