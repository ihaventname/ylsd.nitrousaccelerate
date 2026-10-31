using Smoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NitrousAccelerate
{
    public class SmokeEffect : UpdatableAndDeletable
    {
        // ================= Debug / 参数调整区 =================
        private const int EMIT_COUNT = 16;        // 一次性喷发几团烟雾 (越多颜色越丰富)
        private const float SPEED_MIN = 10f;      // 最小初速度
        private const float SPEED_MAX = 20f;      // 最大初速度
        private const float SPREAD = 1f;          // 喷口随机扩散范围
        private const float POWER = 0.5f;         // 烟雾扩散膨胀力度
        // =======================================================

        public SmokeEffect(Vector2 pos, Vector2 dir, Room nowRoom)
        {
            var smoke = new CustomVultureSmoke(nowRoom, pos);
            nowRoom.AddObject(smoke);

            for (int i = 0; i < EMIT_COUNT; i++)
            {
                float randomSpeed = UnityEngine.Random.Range(SPEED_MIN, SPEED_MAX);
                Vector2 randomVel = dir * randomSpeed + UnityEngine.Random.insideUnitCircle * SPREAD;
                smoke.EmitSmoke(pos, randomVel, POWER);
            }
        }
    }

    public class CustomVultureSmoke : PositionedSmokeEmitter
    {
        public override float ParticleLifeTime => 40f;

        public CustomVultureSmoke(Room room, Vector2 pos)
            : base(SmokeSystem.SmokeType.NewVultureSmoke, room, pos, 1, 0f, false, 200f, 10)
        {
        }

        public override SmokeSystemParticle CreateParticle()
        {
            return new CustomVultureSmokeSegment();
        }

        public void EmitSmoke(Vector2 pos, Vector2 vel, float power)
        {
            // 这里转换成我们自定义的类，以便赋予 hueOffset
            var particle = AddParticle(pos, vel, ParticleLifeTime) as CustomVultureSmokeSegment;

            if (particle != null)
            {
                particle.power = power;
                // 🌟 关键修复：给每个粒子分配独立的随机色相偏移，实现五颜六色
                particle.hueOffset = UnityEngine.Random.value;
            }
        }
    }

    public class CustomVultureSmokeSegment : NewVultureSmoke.NewVultureSmokeSegment
    {
        public float hueOffset = 0f; // 新增：记录该粒子的独立色相

        public override Color MyColor(float timeStacker)
        {
            // 🌟 关键修复：判断 owner 是不是我们创建的烟雾系统
            // 如果是原版秃鹫的烟雾（owner 不是 CustomVultureSmoke），走原版逻辑，不污染
            if (owner is CustomVultureSmoke)
            {
                if (NitrousAccelerateOptions.Instence.isRainbow.Value)
                {
                    // 利用粒子的独立 hueOffset 加上生命周期流逝，产生彩虹流动
                    float hue = (hueOffset + owner.counter * 0.01f + life * 0.05f) % 1f;
                    return Color.HSVToRGB(hue, 1f, 1f);
                }

                // 如果不是彩虹屁，用玩家配置的固定颜色
                return NitrousAccelerateOptions.Instence.accelerateColor.Value;
            }

            // 原版秃鹫的烟雾颜色
            return base.MyColor(timeStacker);
        }
    }
}
