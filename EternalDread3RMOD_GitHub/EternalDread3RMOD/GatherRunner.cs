using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    public class GatherRunner : MonoBehaviour
    {
        public HarvestSource src;
        public GameObject player;
        public bool active;
        public Transform tool;
        public float nextEndCheckTime;

        // 待提取队列：元素是"应该 Extract 的时间"
        public Queue<float> pendingExtracts = new Queue<float>();

        private void Update()
        {
            if (!active) return;
            if (src == null || player == null) { EndGather(); return; }

            // ===== 处理待提取 =====
            // ★ 改动 2：只有 mainNode 还在时才 Extract + 播声音。
            // 树被采完后 mainNode 会销毁，队列里可能还有多余 pending，
            // 这些不应该再播声音。
            while (pendingExtracts.Count > 0 && Time.time >= pendingExtracts.Peek())
            {
                pendingExtracts.Dequeue();
                if (src.mainNode != null)
                {
                    src.mainNode.Extract(999);
                    Patch_HarvestSource_QuickGather.PlaySoundAndEffect(src);
                }
            }

            // ===== 打断检测 =====
            bool interrupted = Patch_HarvestSource_QuickGather.ShouldInterrupt();

            if (interrupted)
            {
                // 打断时清空待提取队列，全部立刻出（保护物品）
                while (pendingExtracts.Count > 0)
                {
                    pendingExtracts.Dequeue();
                    if (src.mainNode != null)
                        src.mainNode.Extract(999);
                    Patch_HarvestSource_QuickGather.PlaySoundAndEffect(src);
                }
                EndGather();
                return;
            }

            // ===== 无 pending 且动画播完 → 恢复 =====
            if (pendingExtracts.Count == 0 && Time.time >= nextEndCheckTime)
                EndGather();
        }

        public void EndGather()
        {
            if (!active) return;
            active = false;
            pendingExtracts.Clear();

            if (player != null)
            {
                var status = player.GetComponent<Status>();
                var motor = player.GetComponent<CharacterMotorC>();
                var atk = player.GetComponent<AttackTrigger>();

                if (motor != null) motor.stability = false;
                if (status != null)
                {
                    status.freeze = false;
                    if (Plugin.QuickGatherInvincibleDuringHarvest.Value)
                        status.evImmortal = false;

                    if (status.mainModel != null && src != null && src.harvestAnim != null)
                    {
                        var legacy = status.mainModel.GetComponent<Animation>();
                        if (legacy != null) legacy.Stop(src.harvestAnim.name);
                    }
                }
                if (atk != null && atk.weaponPosition != null)
                    atk.weaponPosition.gameObject.SetActive(true);
            }

            if (tool != null)
            {
                Object.Destroy(tool.gameObject);
                tool = null;
            }

            if (src != null && (src.mainNode == null
                || Patch_HarvestSource_QuickGather.IsDepleted(src.mainNode)))
            {
                Patch_HarvestSource_QuickGather.CleanupHarvestSource(src);
            }
        }
    }
}