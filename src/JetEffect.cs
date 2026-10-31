using UnityEngine;
using System.Collections.Generic;

namespace NitrousAccelerate
{
    public class JetEffect : UpdatableAndDeletable, IDrawable
    {
        // 内部类：记录单个粒子的状态
        private class Particle
        {
            public Vector2 pos;
            public Vector2 vel;
            public float life;
            public float maxLife;
            public float rotation;
            public float baseScale;
            public bool bounced = false;
        }

        //Debug
        private const int parnum = 30;
        private const float angle = 15f;
        private const float minspeed = 10f;
        private const float maxspeed = 20f;
        private const float difference = 5f;
        private const float mintime = 10f;
        private const float maxtime = 20f;
        private const float scalemutiple = 0.5f;


        private Vector2 origin;
        private Vector2 baseDir;
        private List<Particle> particles = new List<Particle>();
        private const int ParticleCount = parnum; // 粒子数量
        private Room nowRoom;

        public JetEffect(Vector2 pos, Vector2 dir, Room nowRoom)
        {
            this.nowRoom = nowRoom;
            this.origin = pos;
            this.baseDir = dir;

            // 初始化所有粒子
            for (int i = 0; i < ParticleCount; i++)
            {
                // 基础方向加上随机偏转，形成扇形扩散（最大偏转约45度）
                float randomAngle = Random.Range(-angle, angle);
                Vector2 pVel = RotateVector(baseDir, randomAngle);

                // 赋予不同的速度 (基础速度 + 随机扰动)
                float speed = Random.Range(minspeed, maxspeed);
                pVel *= speed;

                // 粒子初始位置稍微在屁股后面一点
                Vector2 pPos = pos + baseDir * Random.Range(0f, difference);

                // 随机生命周期（10帧到20帧）
                float pLife = Random.Range(mintime, maxtime);

                particles.Add(new Particle
                {
                    pos = pPos,
                    vel = pVel,
                    life = pLife,
                    maxLife = pLife,
                    rotation = Random.Range(0f, 360f),
                    baseScale = Random.Range(0.5f, 1.5f)
                });
            }

            
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            bool anyAlive = false;
            foreach (var p in particles)
            {
                if (p.life <= 0) continue;
                anyAlive = true;

                // 1. 先移动
                p.pos += p.vel;

                // 2. 碰撞检测
                if (!p.bounced && room != null)
                {
                    // 获取粒子当前所在的地形块
                    Room.Tile tile = room.GetTile(p.pos);

                    // 检查是否是实心墙壁
                    if (tile != null && tile.Solid)
                    {
                        p.bounced = true;

                        // 简易碰撞响应：反向弹开，并加入随机散射
                        p.vel = -p.vel * 0.3f; // 反向，并大幅减速（能量损失）

                        // 添加随机散射，让粒子看起来是“撞散”的，而不是完美镜面反射
                        p.vel += new Vector2(
                            UnityEngine.Random.Range(-1.5f, 1.5f),
                            UnityEngine.Random.Range(-1.5f, 1.5f)
                        );

                        // 可选：碰撞后稍微缩短粒子寿命，让它更快消失
                        p.life *= 0.5f;
                    }
                }

                // 3. 生命周期与物理衰减
                p.life -= 1f;
                p.vel *= 0.9f;
                p.rotation += UnityEngine.Random.Range(-5f, 5f);
            }

            if (!anyAlive) this.Destroy();
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            // 分配精灵数组
            sLeaser.sprites = new FSprite[ParticleCount];

            for (int i = 0; i < ParticleCount; i++)
            {
                // 使用 Futile_White 或内置的圆形纹理
                var sprite = new FSprite("Futile_White");
                sprite.anchorX = 0.5f;
                sprite.anchorY = 0.5f;
                sprite.shader = rCam.game.rainWorld.Shaders["Basic"];
                sLeaser.sprites[i] = sprite;
            }

            this.AddToContainer(sLeaser, rCam, null);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            // 关键调整 1：将特效放回 Midground（中景层）
            // 这个层包含了墙壁、地形和玩家，所以它会被墙壁正确遮挡，且不会被背景挡住
            FContainer container = newContatiner ?? rCam.ReturnFContainer("Midground");

            for (int i = 0; i < ParticleCount; i++)
            {
                var sprite = sLeaser.sprites[i];
                container.AddChild(sprite);

                // 关键调整 2：设置一个较小的 sortZ 值
                // 这样粒子会绘制在玩家的后面。如果发现还是穿墙，可以把这个值改得更小（比如 -10f）
                sprite.sortZ = -2f;
            }

            // 强制容器根据 sortZ 重新排序，确保遮挡关系生效
            container.SortByZ();
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < ParticleCount; i++)
            {
                var sprite = sLeaser.sprites[i];
                var p = particles[i];

                if (p.life <= 0)
                {
                    sprite.isVisible = false;
                    continue;
                }

                sprite.isVisible = true;
                sprite.SetPosition(p.pos - camPos);
                sprite.rotation = p.rotation;

                // 颜色统一使用配置中的颜色
                sprite.color = NitrousAccelerateOptions.Instence.accelerateColor.Value;

                // 计算透明度进度 (1.0 -> 0.0)
                float progress = p.life / p.maxLife;
                sprite.alpha = progress;

                // 粒子随着生命流逝而变大（模拟气体膨胀），同时变淡
                float scale = p.baseScale * (2f - progress) * scalemutiple;
                sprite.scaleX = scale;
                sprite.scaleY = scale;
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

        // 辅助方法：旋转向量
        private Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }
    }
}