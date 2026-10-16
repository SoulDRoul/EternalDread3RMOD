using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 技能点刷新数据的持久化。
    /// 存储：BepInEx/plugins/EternalDread3RMOD/SkillPointData/skillpoint_slot{N}.xml
    /// </summary>
    public static class SkillPointPersistence
    {
        private static string _dataDir = null;

        public static string GetDataDir()
        {
            if (_dataDir != null) return _dataDir;
            try
            {
                string dllPath = Assembly.GetExecutingAssembly().Location;
                string dllDir = Path.GetDirectoryName(dllPath);
                _dataDir = Path.Combine(Path.Combine(dllDir, "EternalDread3RMOD"), "SkillPointData");
                if (!Directory.Exists(_dataDir))
                    Directory.CreateDirectory(_dataDir);
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] SkillPoint GetDataDir failed: " + e.Message);
                _dataDir = Path.GetTempPath();
            }
            return _dataDir;
        }

        public static string GetFilePath()
        {
            int slot = 0;
            try { slot = GlobalVar.eventsId[95]; } catch { }
            return Path.Combine(GetDataDir(), "skillpoint_slot" + slot + ".xml");
        }

        public static void Save(SkillPointSaveData data)
        {
            if (data == null) return;
            try
            {
                string path = GetFilePath();
                var serializer = new XmlSerializer(typeof(SkillPointSaveData));
                using (var writer = new StreamWriter(path))
                    serializer.Serialize(writer, data);
                Plugin.DebugLog("[ED3R] SkillPoint saved: " + path
                    + " (records=" + (data.Records != null ? data.Records.Count : 0) + ")");
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] SkillPoint save failed: " + e.Message);
            }
        }

        public static SkillPointSaveData Load()
        {
            try
            {
                string path = GetFilePath();
                if (!File.Exists(path))
                    return new SkillPointSaveData();

                var serializer = new XmlSerializer(typeof(SkillPointSaveData));
                using (var reader = new StreamReader(path))
                {
                    var data = (SkillPointSaveData)serializer.Deserialize(reader);
                    if (data == null) data = new SkillPointSaveData();
                    if (data.Records == null) data.Records = new System.Collections.Generic.List<SkillPointRecord>();
                    return data;
                }
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] SkillPoint load failed: " + e.Message);
                return new SkillPointSaveData();
            }
        }
    }
}