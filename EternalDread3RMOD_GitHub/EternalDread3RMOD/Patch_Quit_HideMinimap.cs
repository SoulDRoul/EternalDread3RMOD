using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch]
    public static class Patch_Quit_HideMinimap
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(PauseMenuCanvas), "QuitDelay");
            yield return AccessTools.Method(typeof(UiTab), "QuitDelay");
        }

        static void Prefix()
        {
            if (!Plugin.HideMinimapOnQuit.Value) return;

            var m = Object.FindObjectOfType<MinimapOnOff>();
            if (m != null)
            {
                if (m.minimapCam) m.minimapCam.SetActive(false);
                if (m.fullMapCam) m.fullMapCam.SetActive(false);
                if (m.questPointer) m.questPointer.SetActive(false);
            }

            // ★ 同步抑制生态 UI
            EcologyUI.SuppressRender = true;
        }
    }
}