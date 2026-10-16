using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 过场动画期间隐藏小地图。
    /// 信号：UiTab.healthBar.activeInHierarchy == false（原版过场会隐藏血条）。
    /// 进入过场：快照 minimapCam / fullMapCam / questPointer 的 active 状态，
    ///          全部强制 false。
    /// 过场中：每帧强制 false（MinimapCamera.Update 会主动复活 questPointer）。
    /// 退出过场：按快照恢复。
    /// </summary>
    public class CutsceneMinimapHider : MonoBehaviour
    {
        private UiTab _cachedUiTab = null;
        private float _lastUiTabCheck = -999f;

        private bool _hiding = false;
        private MinimapOnOff _cutsceneMinimap = null;
        private bool _snapMinimap = false;
        private bool _snapFullMap = false;
        private bool _snapQuest = false;

        private const float UiTabCheckIntervalSec = 1.0f;

        private void Update()
        {
            // 配置关闭时，若正在隐藏则立刻恢复
            if (!Plugin.HideMinimapOnCutscene.Value)
            {
                if (_hiding) EndHide();
                return;
            }

            if (GlobalVar.mainPlayer == null) return;

            bool cutscene = IsPlayerHudHidden();

            if (cutscene && !_hiding)
            {
                StartHide();
            }
            else if (!cutscene && _hiding)
            {
                EndHide();
            }

            if (_hiding && _cutsceneMinimap != null)
            {
                ForceHide();
            }
        }

        /// <summary>
        /// 玩家 HUD 是否被隐藏（过场动画 / 死亡界面期间原版会隐藏血条）。
        /// </summary>
        private bool IsPlayerHudHidden()
        {
            if (Time.unscaledTime - _lastUiTabCheck > UiTabCheckIntervalSec)
            {
                _lastUiTabCheck = Time.unscaledTime;
                _cachedUiTab = Object.FindObjectOfType<UiTab>();
            }
            if (_cachedUiTab == null) return false;
            if (_cachedUiTab.healthBar == null) return false;
            return !_cachedUiTab.healthBar.activeInHierarchy;
        }

        private void StartHide()
        {
            _cutsceneMinimap = Object.FindObjectOfType<MinimapOnOff>();
            if (_cutsceneMinimap == null) return;

            _snapMinimap = _cutsceneMinimap.minimapCam != null
                && _cutsceneMinimap.minimapCam.activeSelf;
            _snapFullMap = _cutsceneMinimap.fullMapCam != null
                && _cutsceneMinimap.fullMapCam.activeSelf;
            _snapQuest = _cutsceneMinimap.questPointer != null
                && _cutsceneMinimap.questPointer.activeSelf;

            _hiding = true;
            ForceHide();
        }

        private void EndHide()
        {
            if (_cutsceneMinimap != null)
            {
                if (_cutsceneMinimap.minimapCam != null)
                    _cutsceneMinimap.minimapCam.SetActive(_snapMinimap);
                if (_cutsceneMinimap.fullMapCam != null)
                    _cutsceneMinimap.fullMapCam.SetActive(_snapFullMap);
                if (_cutsceneMinimap.questPointer != null)
                    _cutsceneMinimap.questPointer.SetActive(_snapQuest);
            }
            _hiding = false;
            _cutsceneMinimap = null;
        }

        private void ForceHide()
        {
            if (_cutsceneMinimap == null) return;

            if (_cutsceneMinimap.minimapCam != null && _cutsceneMinimap.minimapCam.activeSelf)
                _cutsceneMinimap.minimapCam.SetActive(false);
            if (_cutsceneMinimap.fullMapCam != null && _cutsceneMinimap.fullMapCam.activeSelf)
                _cutsceneMinimap.fullMapCam.SetActive(false);
            if (_cutsceneMinimap.questPointer != null && _cutsceneMinimap.questPointer.activeSelf)
                _cutsceneMinimap.questPointer.SetActive(false);
        }
    }
}