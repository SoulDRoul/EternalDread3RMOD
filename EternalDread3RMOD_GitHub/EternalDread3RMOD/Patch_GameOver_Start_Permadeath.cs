using HarmonyLib;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 永久死亡模式：任何 GameOver 都视为死亡，显示死亡界面。
    /// 教程场景（IntroVillage）例外：走原版，不触发 permadeath。
    /// 删档不在这里执行，而在 DeathUI 倒计时结束时执行（允许玩家强退保留）。
    /// </summary>
    [HarmonyPatch(typeof(GameOver), "Start")]
    public static class Patch_GameOver_Start_Permadeath
    {
        static bool Prefix(GameOver __instance)
        {
            if (!PermadeathState.CurrentSavePermadeath)
                return true;

            // ★ 教程场景保护
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "IntroVillage")
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogInfo("[ED3R] Permadeath skipped in tutorial scene");
                return true;
            }

            if (__instance.canvasUi != null)
                __instance.canvasUi.SetActive(false);

            PermadeathDeathUI.Show();

            if (Plugin.Log != null)
                Plugin.Log.LogInfo("[ED3R] Permadeath death: slot=" + GlobalVar.eventsId[95]);

            return false;
        }
    }
}