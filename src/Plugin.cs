using BepInEx;
using ImprovedInput;
using SlugBase.Features;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using static SlugBase.Features.FeatureTypes;

//项目主逻辑

namespace NitrousAccelerate
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "nitrousaccelerate";
        private const string MOD_NAME = "Nitrous Accelerate";
        private const string MOD_VERSION = "1.1.0";

        NitrousAccelerateOptions options;
        public static PlayerKeybind actKey;

        // 用于记录每个玩家的冷却时间 (ConditionalWeakTable 不会造成内存泄漏)
        private static readonly ConditionalWeakTable<Player, StrongBox<int>> cooldowns = new ConditionalWeakTable<Player, StrongBox<int>>();
        // 记录视觉喷气特效的剩余时间（帧数）
        //public static readonly ConditionalWeakTable<Player, StrongBox<float>> jetVisuals = new ConditionalWeakTable<Player, StrongBox<float>>();

        public static SoundID sound1;
        public static SoundID sound2;
        public static SoundID sound3;
        public const string soundid1 = "sound1";
        public const string soundid2 = "sound2";
        public const string soundid3 = "sound3";


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.OnModsInit += Options;
            On.Player.Update += Accelerate;
        }

        private void Awake()
        {
            sound1 = new SoundID(soundid1, true);
            sound2 = new SoundID(soundid2, true);
            sound3 = new SoundID(soundid3, true);
        }

        private void OnDestroy()
        {
            On.RainWorld.OnModsInit -= Extras.WrapInit(LoadResources);
            On.RainWorld.OnModsInit -= Options;
            On.Player.Update -= Accelerate;
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            actKey = PlayerKeybind.Register("nitrous_boost", MOD_NAME, "氮气加速", KeyCode.C, KeyCode.JoystickButton3);
        }

        private void Options(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            options = new NitrousAccelerateOptions(this);

            //加载配置界面
            MachineConnector.SetRegisteredOI(MOD_ID, options);
        }

        private void Accelerate(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self,eu);

            if (NitrousAccelerateOptions.Instence == null || actKey == null || self.dead || self.inShortcut) return;

            // 获取或创建该玩家的冷却状态
            var cdBox = cooldowns.GetOrCreateValue(self);

            // 冷却时间递减 (如果大于0)
            if (cdBox.Value > 0) cdBox.Value--;


            if (actKey.CheckRawPressed(self.playerState.playerNumber) && cdBox.Value <= 0)
            {
                float force = NitrousAccelerateOptions.Instence.accelerateForce.Value;
                if (force <= 0) return;

                if(NitrousAccelerateOptions.Instence.isConsumeFood.Value)
                {
                    if (self.FoodInStomach > 0) self.SubtractFood(1);
                    else return;
                }

                // 获取从屁股(bodyChunks[1]) 指向头(bodyChunks[0]) 的方向
                Vector2 dir = RWCustom.Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos);
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    self.bodyChunks[i].vel += dir * force;
                }

                // 设置冷却（例如 15 帧，大约 0.375 秒）
                cdBox.Value = 15;

                Vector2 headPos = self.bodyChunks[0].pos;
                Vector2 hipsPos = self.bodyChunks[1].pos;
                Vector2 dirc = RWCustom.Custom.DirVec(headPos, hipsPos);

                // 把 JetEffect 添加到玩家所在的房间
                switch (NitrousAccelerateOptions.Instence.effect.Value)
                {
                    case Effect.方形粒子:
                        self.room.AddObject(new JetEffect(hipsPos, dirc, self.room));
                        break;
                    case Effect.烟雾:
                        self.room.AddObject(new SmokeEffect(hipsPos, dirc, self.room));
                        break;
                }
                //self.room.AddObject(new JetEffect(hipsPos, dirc,self.room));

                // 3. 播放音效
                PlayAccelerateSound(self);

                // 4. 概率掉落石头
                if (UnityEngine.Random.value <= NitrousAccelerateOptions.Instence.rockChance.Value)
                {
                    SpawnSurprise(self);
                }
            }
        }

        private void PlayAccelerateSound(Player self)
        {
            if (self.room == null) return;
            // 根据配置播放音效（这里先放几个原版音效的占位）
            SoundID sound = SoundID.Slugcat_Belly_Slide_Init; // 默认音效

            switch (NitrousAccelerateOptions.Instence.accelerateSound.Value)
            {
                case AccelerateSound.Sound1: sound = sound1; break;
                case AccelerateSound.Sound2: sound = sound2; break;
                case AccelerateSound.Sound3: sound = sound3; break;
                case AccelerateSound.None: return;
            }
            float volume = NitrousAccelerateOptions.Instence.volume.Value;
            self.room.PlaySound(sound, self.bodyChunks[1],false,volume,1f);
        }

        private void SpawnSurprise(Player self)
        {
            Room room = self.room;
            if (room == null || room.world == null) return;

            EntityID id = room.world.game.GetNewID();
            WorldCoordinate coord = room.GetWorldCoordinate(self.bodyChunks[1].pos);

            // 调用工厂方法直接生成正确的子类对象
            AbstractPhysicalObject surpriseAbstract = NitrousAccelerate.SpawnSurprise.CreateSurpriseObject(
                NitrousAccelerateOptions.Instence.surpriseClass.Value,
                room.world,
                coord,
                id
            );

            // 放入房间并实体化
            room.abstractRoom.AddEntity(surpriseAbstract);
            surpriseAbstract.RealizeInRoom();
        }


#if OUT
        [Obsolete()]
        private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

            var jetSprite = new FSprite("Futile_White");
            // 关键修复 1：为精灵指定基础着色器，确保它参与渲染
            jetSprite.shader = rCam.game.rainWorld.Shaders["Basic"];

            jetSprite.anchorX = 0.5f;
            jetSprite.anchorY = 1f;

            sLeaser.sprites[sLeaser.sprites.Length - 1] = jetSprite;
        }
        [Obsolete()]
        private void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            FContainer container = rCam.ReturnFContainer("Foreground");
            var jetSprite = sLeaser.sprites[sLeaser.sprites.Length - 1];

            container.AddChild(jetSprite);
            jetSprite.sortZ = -20f;

            // 强制根据 sortZ 重新排序，确保层级生效
            container.SortByZ();
        }
        [Obsolete()]
        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            var jetSprite = sLeaser.sprites[sLeaser.sprites.Length - 1];
            Player player = self.player;

            if (!jetVisuals.TryGetValue(player, out var vBox) || vBox.Value <= 0)
            {
                jetSprite.isVisible = false;
                return;
            }

            jetSprite.isVisible = true;

            Vector2 headPos = Vector2.Lerp(player.bodyChunks[0].lastPos, player.bodyChunks[0].pos, timeStacker);
            Vector2 hipsPos = Vector2.Lerp(player.bodyChunks[1].lastPos, player.bodyChunks[1].pos, timeStacker);

            Vector2 dir = RWCustom.Custom.DirVec(headPos, hipsPos);

            if (dir == Vector2.zero) dir = new Vector2(0, -1);

            // 因为设置了 anchorY = 1f，直接把位置定在屁股即可，不需要再加偏移了
            jetSprite.SetPosition(hipsPos - camPos);

            // 旋转对准屁股方向
            jetSprite.rotation = RWCustom.Custom.VecToDeg(dir) - 90f;

            // 设置颜色
            jetSprite.color = NitrousAccelerateOptions.Instence.accelerateColor.Value;

            // 计算特效进度
            float progress = vBox.Value / 30f;

            // 拉长精灵，模拟喷射的气流
            float length = 60f * progress; // 长度略微加长
            float width = 15f * progress;  // 宽度略微加宽

            jetSprite.scaleX = width;
            jetSprite.scaleY = length;

            jetSprite.alpha = progress;
        }
#endif
    }
}