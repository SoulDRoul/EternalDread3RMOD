using System;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 读档 / 新游戏共用的生态加载流程。
    /// </summary>
    public static class EcologyLoadHelper
    {
        /// <summary>
        /// 是否已经应用过一次生态数据。
        /// 新游戏时，需要等玩家离开 IntroVillage 才应用，避免
        /// clock 被改而场景还是固定天空的 IntroVillage。
        /// </summary>
        public static bool HasApplied = false;

        /// <summary>
        /// 按槽位加载生态数据，应用到运行时，并同步 clock。
        /// slot 用 -1 表示"用当前 GlobalVar.eventsId[95]"。
        /// </summary>
        public static void LoadAndApply(int slot)
        {
            try
            {
                var data = EcologyPersistence.Load(slot);
                EcologyRegistry.ReplaceAll(data.Regions);
                GameTimeTracker.ImportTotalHours(data.TotalGameHours);

                SyncClockToWorldTime(data.TotalGameHours);

                if (Plugin.EcologyEnabled.Value)
                    EcologyRegistry.TickAll(GameTimeTracker.GetTotalGameHours());

                HasApplied = true;

                if (Plugin.Log != null)
                    Plugin.DebugLog("[ED3R] Ecology applied (slot=" + slot
                        + ") regions=" + EcologyRegistry.Count
                        + ", totalHours=" + GameTimeTracker.GetTotalGameHours().ToString("F2")
                        + ", clock=" + GlobalVar.clock.x + ":" + GlobalVar.clock.y);
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] Ecology LoadAndApply failed: " + e.Message);
            }
        }

        /// <summary>把 clock 设为 worldHours 对应的当天时刻。</summary>
        private static void SyncClockToWorldTime(float worldHours)
        {
            if (worldHours < 0f) worldHours = 0f;
            float dayHour = worldHours % 24f;
            int hour = Mathf.FloorToInt(dayHour);
            int minute = Mathf.FloorToInt((dayHour - hour) * 60f);
            if (minute >= 60) minute = 59;

            GlobalVar.clock = new Vector2(hour, minute);
            GameTimeJump.RefreshDayTimePublic();
        }
    }
}