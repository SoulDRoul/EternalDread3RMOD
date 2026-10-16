using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(HarvestSource), "Harvesting")]
    public static class Patch_HarvestSource_QuickGather
    {
        // ============ 硬编码常量：Gather.MaxDurationSec ============
        // 原 Gather.MaxDurationSec —— 采集动画的最长等待时间（秒），防止动画 clip 异常导致的无限等待
        private const float MAX_DURATION_SEC = 3f;

        private static float _lastStartTime = -999f;

        static bool Prefix(HarvestSource __instance)
        {
            if (!Plugin.QuickGatherEnabled.Value) return true;
            if (__instance.player == null || __instance.harvestAnim == null)
                return true;

            // ★ 改动 1：mainNode 没了，但 GatherRunner 还在跑 → 采集正在收尾，
            // 忽略这次按键，等动画自然播完。
            if (__instance.mainNode == null)
            {
                var r = __instance.GetComponent<GatherRunner>();
                if (r != null && r.active) return false;
                CleanupHarvestSource(__instance);
                return false;
            }

            var node = __instance.mainNode;
            if (node.type != ResourceTypes.Log && node.type != ResourceTypes.Ore)
                return true;

            // ★ 改动 2：IsDepleted 同理。采集正在收尾时忽略按键。
            if (IsDepleted(node))
            {
                var r = __instance.GetComponent<GatherRunner>();
                if (r != null && r.active) return false;
                CleanupHarvestSource(__instance);
                return false;
            }

            var status = __instance.player.GetComponent<Status>();
            if (status == null || status.freezeStack > 0) return true;

            var atk = __instance.player.GetComponent<AttackTrigger>();
            if (atk == null || atk.onAttacking) return true;

            if (Time.time - _lastStartTime < Plugin.QuickGatherMinIntervalSec.Value)
                return false;
            _lastStartTime = Time.time;

            var runner = __instance.GetComponent<GatherRunner>();
            if (runner == null)
                runner = __instance.gameObject.AddComponent<GatherRunner>();
            runner.src = __instance;
            runner.player = __instance.player;
            runner.active = true;

            int current = (node.type == ResourceTypes.Log)
                ? GlobalVar.sapling[node.nodeId]
                : GlobalVar.ore[node.nodeId];
            int remaining = node.durability - current;

            // 队列没满才入队。满了也不中断采集动作。
            if (runner.pendingExtracts.Count < remaining)
            {
                float delay = Plugin.QuickGatherExtractDelaySec.Value;
                runner.pendingExtracts.Enqueue(Time.time + delay);
            }

            // ===== 状态设置 =====
            status.freeze = true;
            if (Plugin.QuickGatherInvincibleDuringHarvest.Value)
                status.evImmortal = true;

            var motor = __instance.player.GetComponent<CharacterMotorC>();
            if (motor != null) motor.stability = true;

            Vector3 lookPos = node.transform.position;
            lookPos.y = __instance.player.transform.position.y;
            __instance.player.transform.LookAt(lookPos);

            if (__instance.hideWeapon && atk.weaponPosition != null)
                atk.weaponPosition.gameObject.SetActive(false);

            // 每次按 E 都重建工具
            if (runner.tool != null)
            {
                Object.DestroyImmediate(runner.tool.gameObject);
                runner.tool = null;
            }
            if (__instance.harvestToolPrefab != null && atk.combatWeaponPosition != null)
            {
                runner.tool = Object.Instantiate(__instance.harvestToolPrefab,
                    atk.combatWeaponPosition.position,
                    atk.combatWeaponPosition.rotation);
                runner.tool.parent = atk.combatWeaponPosition;
            }

            // 重播动画
            float clipLen = PlayHarvestAnim(status, __instance.harvestAnim);

            // 上限（保险丝）
            float maxDur = MAX_DURATION_SEC;
            if (maxDur > 0f && clipLen > maxDur) clipLen = maxDur;

            // 下限 = ExtractDelaySec
            float extractDelay = Plugin.QuickGatherExtractDelaySec.Value;
            if (clipLen < extractDelay) clipLen = extractDelay;

            runner.nextEndCheckTime = Time.time + clipLen;

            return false;
        }

        static float PlayHarvestAnim(Status status, AnimationClip clip)
        {
            float clipLen = 0.5f;
            var legacy = status.mainModel.GetComponent<Animation>();
            var anim = status.mainModel.GetComponent<Animator>();

            if (legacy != null && clip != null)
            {
                legacy.Stop(clip.name);
                var state = legacy[clip.name];
                if (state != null)
                {
                    state.layer = 26;
                    legacy.Play(clip.name);
                    clipLen = state.length;
                }
            }
            else if (anim != null && clip != null)
            {
                anim.Play(clip.name, -1, 0f);
                var ac = anim.runtimeAnimatorController;
                if (ac != null)
                {
                    var clips = ac.animationClips;
                    for (int i = 0; i < clips.Length; i++)
                    {
                        if (clips[i].name == clip.name)
                        {
                            clipLen = clips[i].length;
                            break;
                        }
                    }
                }
            }
            return clipLen;
        }

        public static void CleanupHarvestSource(HarvestSource src)
        {
            if (src == null) return;

            var runner = src.GetComponent<GatherRunner>();
            if (runner != null)
            {
                while (runner.pendingExtracts.Count > 0)
                {
                    runner.pendingExtracts.Dequeue();
                    if (src.mainNode != null)
                        src.mainNode.Extract(999);
                    PlaySoundAndEffect(src);
                }

                runner.active = false;
                if (runner.tool != null)
                {
                    Object.Destroy(runner.tool.gameObject);
                    runner.tool = null;
                }
            }

            var player = src.player;
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

                    if (status.mainModel != null && src.harvestAnim != null)
                    {
                        var legacy = status.mainModel.GetComponent<Animation>();
                        if (legacy != null) legacy.Stop(src.harvestAnim.name);
                    }
                }
                if (atk != null && atk.weaponPosition != null)
                    atk.weaponPosition.gameObject.SetActive(true);

                var pic = player.GetComponent<PlayerInputController>();
                if (pic != null) pic.RemoveActivator(src.gameObject);
                var tdc = player.GetComponent<TopDownController>();
                if (tdc != null) tdc.RemoveActivator(src.gameObject);
            }

            Object.Destroy(src.gameObject);
        }

        public static bool IsDepleted(ResourceNode node)
        {
            if (node == null) return true;
            switch (node.type)
            {
                case ResourceTypes.Log:
                    return GlobalVar.sapling[node.nodeId] >= node.durability;
                case ResourceTypes.Ore:
                    return GlobalVar.ore[node.nodeId] >= node.durability;
            }
            return false;
        }

        public static void PlaySoundAndEffect(HarvestSource src)
        {
            if (src == null) return;
            if (src.playSound)
            {
                var audio = src.GetComponent<AudioSource>();
                if (audio != null)
                {
                    audio.volume = OptionData.effectVolume;
                    audio.Play();
                }
            }
            if (src.effect != null)
            {
                var eff = Object.Instantiate(src.effect,
                    src.transform.position, src.transform.rotation);
                var sources = eff.GetComponentsInChildren<AudioSource>();
                for (int i = 0; i < sources.Length; i++)
                    sources[i].volume = OptionData.effectVolume;
            }
        }

        public static bool ShouldInterrupt()
        {
            if (Plugin.QuickGatherInterruptAnyKey.Value && Input.anyKeyDown)
                return true;

            if (Mathf.Abs(ButtonInputOption.Horizontal) > 0.1f) return true;
            if (Mathf.Abs(ButtonInputOption.Vertical) > 0.1f) return true;

            if (Input.GetKeyDown(ButtonInputOption.jump)) return true;
            if (Input.GetKeyDown(ButtonInputOption.joyJump)) return true;
            if (Input.GetKeyDown(ButtonInputOption.attack)) return true;
            if (Input.GetKeyDown(ButtonInputOption.guard)) return true;
            if (Input.GetKeyDown(ButtonInputOption.joyGuard)) return true;
            if (Input.GetKeyDown(ButtonInputOption.joySwitchWeapon)) return true;

            if (Input.GetAxis("Item Set 1") != 0f) return true;
            if (Input.GetAxis("Item Set 2") != 0f) return true;
            if (Input.GetButton("Skill Set 1")) return true;
            if (Input.GetButton("Skill Set 2")) return true;

            return false;
        }
    }
}