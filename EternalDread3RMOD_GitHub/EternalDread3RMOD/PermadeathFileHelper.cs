using System.IO;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 永久死亡状态存为 sidecar 文件，与游戏原存档分离。
    /// 路径：Application.dataPath + "/UqkasKjbHas{slot}.permadeath"
    /// 只在 permadeath=true 时创建文件；普通存档不创建。
    /// </summary>
    public static class PermadeathFileHelper
    {
        private const string FILE_PREFIX = "UqkasKjbHas";
        private const string FILE_SUFFIX = ".permadeath";

        private static string GetPath(int slot)
        {
            return Application.dataPath + "/" + FILE_PREFIX + slot + FILE_SUFFIX;
        }

        public static bool Exists(int slot)
        {
            try { return File.Exists(GetPath(slot)); }
            catch { return false; }
        }

        /// <summary>写入 sidecar。permadeath=false 时删除文件。</summary>
        public static void Save(int slot, bool permadeath, bool dead)
        {
            try
            {
                string path = GetPath(slot);
                if (!permadeath)
                {
                    if (File.Exists(path)) File.Delete(path);
                    return;
                }
                using (var sw = new StreamWriter(path, false))
                {
                    sw.WriteLine("permadeath=1");
                    sw.WriteLine("dead=" + (dead ? "1" : "0"));
                }
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] Permadeath Save failed: " + e.Message);
            }
        }

        public static bool ReadPermadeath(int slot)
        {
            return ReadFlag(slot, "permadeath");
        }

        public static bool ReadDead(int slot)
        {
            return ReadFlag(slot, "dead");
        }

        /// <summary>标记已死亡。仅当文件已存在时更新。</summary>
        public static void MarkDead(int slot)
        {
            try
            {
                string path = GetPath(slot);
                if (!File.Exists(path)) return;
                using (var sw = new StreamWriter(path, false))
                {
                    sw.WriteLine("permadeath=1");
                    sw.WriteLine("dead=1");
                }
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] Permadeath MarkDead failed: " + e.Message);
            }
        }

        public static void Delete(int slot)
        {
            try
            {
                string path = GetPath(slot);
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }

        private static bool ReadFlag(int slot, string key)
        {
            try
            {
                string path = GetPath(slot);
                if (!File.Exists(path)) return false;
                foreach (var line in File.ReadAllLines(path))
                {
                    var t = line.Trim();
                    if (t.StartsWith(key + "="))
                    {
                        string v = t.Substring(key.Length + 1).Trim().ToLower();
                        return v == "1" || v == "true";
                    }
                }
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] Permadeath Read failed: " + e.Message);
            }
            return false;
        }
    }
}