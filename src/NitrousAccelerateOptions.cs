using BepInEx;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//配置菜单


namespace NitrousAccelerate
{
    enum AccelerateSound { Sound1, Sound2 , Sound3, None}
    enum Effect { 方形粒子, 烟雾}
    internal class NitrousAccelerateOptions : OptionInterface
    {
        public static NitrousAccelerateOptions Instence;

        public readonly Configurable<int> accelerateForce;
        public readonly Configurable<Color> accelerateColor;
        public readonly Configurable<bool> isConsumeFood;
        public readonly Configurable<float> rockChance;
        public readonly Configurable<Surprise> surpriseClass;
        public readonly Configurable<AccelerateSound> accelerateSound;
        public readonly Configurable<float> volume;
        
        public readonly Configurable<Effect> effect;
        public readonly Configurable<bool> isRainbow;

        public NitrousAccelerateOptions(BaseUnityPlugin plugin)
        {
            Instence = this;

            accelerateForce = config.Bind("AccelerateForce", defaultValue: 15,new ConfigAcceptableRange<int>(0,30));
            accelerateColor = config.Bind("AccelerateColor", defaultValue: Color.green);
            isConsumeFood = config.Bind("IsConsumeFood", defaultValue: false);
            rockChance = config.Bind("RockChance", defaultValue: 0.5f, new ConfigAcceptableRange<float>(0, 1f));
            surpriseClass = config.Bind("SurpriseClass", defaultValue: Surprise.石头);
            accelerateSound = config.Bind("AccelerateSound",defaultValue: AccelerateSound.Sound1);
            volume = config.Bind("AccelerateVolume", defaultValue: 1.0f, new ConfigAcceptableRange<float>(0f, 1f));

            effect = config.Bind("effect", defaultValue: Effect.方形粒子);
            isRainbow = config.Bind("isRainbow", defaultValue:false);
        }

        public override void Initialize()
        {
            base.Initialize();

            OpTab mainTab = new OpTab(this, "基本");
            OpTab effectTap = new OpTab(this,"特效");
            Tabs = new OpTab[] { mainTab ,effectTap };

            // 雨世界的 UI 坐标系：左下角是 (0,0)，右上角大概是 (600, 600)
            UIelement[] uiElements = new UIelement[]
            {
                // 标题
                new OpLabel(new Vector2(10f, 570f), new Vector2(400f, 30f), "氮气加速设置", bigText: true),

                // 1. 喷气力度
                new OpSlider(accelerateForce, new Vector2(10f, 510f), 220),
                new OpLabel(240f, 511f, "喷气力度"),

                // 2. 拉石头概率
                new OpFloatSlider(rockChance, new Vector2(10f, 470f), 220, 2),
                new OpLabel(240f, 471f, "\"惊喜\"概率"),

                new OpResourceSelector(surpriseClass,new Vector2(340f,465f),180),
                new OpLabel(340f, 490f, "\"惊喜\"种类"),

                // 3. 喷气颜色 
                new OpLabel(10f, 440f, "喷气颜色"),
                new OpColorPicker(accelerateColor, new Vector2(10f, 280f)),

                // 5. 是否消耗饱食度 
                new OpCheckBox(isConsumeFood, 10f, 230f),
                new OpLabel(45f, 231f, "喷气消耗饱食度"),

                // 4. 音量调节 
                new OpFloatSlider(volume, new Vector2(10f, 180f), 220, 2),
                new OpLabel(240f, 181f, "音效音量"),

                // 6. 喷气音效
                new OpResourceSelector(accelerateSound, new Vector2(340f, 175f), 180),
                new OpLabel(340f, 200f, "喷气音效"),


            };
            UIelement[] effElements = new UIelement[]
            {
                new OpLabel(new Vector2(10f, 570f), new Vector2(400f, 30f), "喷气效果自定义(Beta)", bigText: true),

                new OpResourceSelector(effect, new Vector2(10f, 510f), 220),
                new OpLabel(240f, 511f, "特效种类"),

                new OpCheckBox(isRainbow, 10f, 440f),
                new OpLabel(45f, 441f, "彩虹屁"),
            };
            mainTab.AddItems(uiElements);
            effectTap.AddItems(effElements);
        }
    }
}
