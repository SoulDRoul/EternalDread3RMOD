using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 生态数据的持久化。
    /// 
    /// 存储位置：BepInEx/plugins/EternalDread3RMOD/EcologyData/
    /// 文件名：
    ///   独立模式 → ecology_slot<N>.xml（N = 存档槽位号）
    ///   共通模式 → ecology_shared.xml
    /// </summary>
    public static class EcologyPersistence
    {
        private static string _dataDir = null;

        public static string GetDataDir()
        {
            if (_dataDir != null) return _dataDir;
            try
            {
                string dllPath = Assembly.GetExecutingAssembly().Location;
                string dllDir = Path.GetDirectoryName(dllPath);
                _dataDir = Path.Combine(Path.Combine(dllDir, "EternalDread3RMOD"), "EcologyData");
                if (!Directory.Exists(_dataDir))
                    Directory.CreateDirectory(_dataDir);
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] Ecology GetDataDir failed: " + e.Message);
                _dataDir = Path.GetTempPath();
            }
            return _dataDir;
        }

        public static string GetFilePath()
        {
            return GetFilePath(-1);
        }

        public static string GetFilePath(int slotOverride)
        {
            string dir = GetDataDir();
            if (Plugin.CrossSaveShare != null && Plugin.CrossSaveShare.Value)
                return Path.Combine(dir, "ecology_shared.xml");

            int slot = (slotOverride >= 0) ? slotOverride : GetCurrentSlot();
            return Path.Combine(dir, "ecology_slot" + slot + ".xml");
        }

        private static int GetCurrentSlot()
        {
            try { return GlobalVar.eventsId[95]; }
            catch { return 0; }
        }

        public static void Save(EcologySaveData data)
        {
            if (data == null) return;
            try
            {
                string path = GetFilePath();
                var serializer = new XmlSerializer(typeof(EcologySaveData));
                using (var writer = new StreamWriter(path))
                {
                    serializer.Serialize(writer, data);
                }
                if (Plugin.Log != null)
                    Plugin.DebugLog("[ED3R] Ecology saved: " + path
                        + " (regions=" + (data.Regions != null ? data.Regions.Count : 0)
                        + ", totalHours=" + data.TotalGameHours.ToString("F2") + ")");
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] Ecology save failed: " + e.Message);
            }
        }

        public static EcologySaveData Load()
        {
            return Load(-1);
        }

        public static EcologySaveData Load(int slotOverride)
        {
            try
            {
                string path = GetFilePath(slotOverride);
                if (!File.Exists(path))
                {
                    var fresh = new EcologySaveData();
                    fresh.TotalGameHours = GlobalVar.clock.x + GlobalVar.clock.y / 60f;
                    if (Plugin.Log != null)
                        Plugin.DebugLog("[ED3R] Ecology file not found, init from clock: "
                            + path + " -> " + fresh.TotalGameHours.ToString("F2"));
                    return fresh;
                }

                var serializer = new XmlSerializer(typeof(EcologySaveData));
                using (var reader = new StreamReader(path))
                {
                    var data = (EcologySaveData)serializer.Deserialize(reader);
                    if (data == null) data = new EcologySaveData();
                    if (data.Regions == null) data.Regions = new List<RegionState>();
                    if (Plugin.Log != null)
                        Plugin.DebugLog("[ED3R] Ecology loaded: " + path
                            + " (regions=" + data.Regions.Count
                            + ", totalHours=" + data.TotalGameHours.ToString("F2") + ")");
                    return data;
                }
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] Ecology load failed: " + e.Message);
                return new EcologySaveData();
            }
        }
    }
}