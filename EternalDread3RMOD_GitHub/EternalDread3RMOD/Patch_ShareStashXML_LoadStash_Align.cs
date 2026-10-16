using System;
using System.IO;
using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 读共享箱子 XML 前，把 stash 的 Inventory 数组扩到 XML 长度。
    ///
    /// 【时序陷阱】
    /// ShareStashXML.Start 里 myData = new StashData()（空对象），
    /// LoadXML 只把文件内容读进 _data 字段，不反序列化。
    /// myData 只在 LoadStash 方法体内部才被反序列化。
    /// 所以 Prefix 时刻反射读 myData.itemStash.itid 会得到 null / 0。
    ///
    /// 【解决方案】
    /// Prefix 里主动读 XML 文件 + 反射调用 DeserializeObject，拿到真实长度。
    /// </summary>
    [HarmonyPatch(typeof(ShareStashXML), "LoadStash")]
    public static class Patch_ShareStashXML_LoadStash_Align
    {
        static void Prefix(ShareStashXML __instance, Inventory stash)
        {
            if (__instance == null || stash == null) return;
            if (Plugin.InventoryExpandEnabled == null
                || !Plugin.InventoryExpandEnabled.Value) return;
            if (stash.gameObject == null) return;
            if (stash.gameObject.name != "ShareStash") return;

            try
            {
                int xmlItemLen = ReadXmlArrayLength(__instance, "itid");
                int xmlEqLen = ReadXmlArrayLength(__instance, "eqid");

                if (xmlItemLen > 0 && stash.items != null
                    && stash.items.Length < xmlItemLen)
                    InventoryExpandHelper.EnsureItems(stash, xmlItemLen);

                if (xmlEqLen > 0 && stash.equipments != null
                    && stash.equipments.Length < xmlEqLen)
                    InventoryExpandHelper.EnsureEquipments(stash, xmlEqLen);

                Plugin.DebugLog("[ED3R] ShareStash LoadStash align:"
                    + " xmlItems=" + xmlItemLen
                    + " xmlEquips=" + xmlEqLen
                    + " -> stashItems=" + stash.items.Length
                    + " stashEquips=" + stash.equipments.Length);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] ShareStash LoadStash align error: " + e);
            }
        }

        /// <summary>
        /// 主动读 XML 文件，反射反序列化，取指定 int[] 字段的长度。
        /// 任何一步失败返回 0（不改 stash，走原版行为）。
        /// </summary>
        private static int ReadXmlArrayLength(ShareStashXML instance, string arrayFieldName)
        {
            try
            {
                // 1. 反射读文件路径字段
                var locField = AccessTools.Field(typeof(ShareStashXML), "_FileLocation");
                var nameField = AccessTools.Field(typeof(ShareStashXML), "_FileName");
                if (locField == null || nameField == null) return 0;

                string loc = locField.GetValue(instance) as string;
                string name = nameField.GetValue(instance) as string;
                if (string.IsNullOrEmpty(loc) || string.IsNullOrEmpty(name)) return 0;

                string path = loc + "\\" + name;
                if (!File.Exists(path)) return 0;

                // 2. 读文件内容
                string text = File.ReadAllText(path);
                if (string.IsNullOrEmpty(text)) return 0;

                // 3. 反射调 DeserializeObject 得到临时 StashData
                var deserMethod = AccessTools.Method(typeof(ShareStashXML), "DeserializeObject");
                if (deserMethod == null) return 0;

                var myData = deserMethod.Invoke(instance, new object[] { text });
                if (myData == null) return 0;

                // 4. 反射链 myData → itemStash → 目标数组字段
                var itemStashField = AccessTools.Field(myData.GetType(), "itemStash");
                if (itemStashField == null) return 0;

                var itemStash = itemStashField.GetValue(myData);
                if (itemStash == null) return 0;

                var arrField = AccessTools.Field(itemStash.GetType(), arrayFieldName);
                if (arrField == null) return 0;

                var arr = arrField.GetValue(itemStash) as int[];
                return arr != null ? arr.Length : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}