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
    internal class NitrousAccelerateOptions : OptionInterface
    {
        public static NitrousAccelerateOptions Instence;

        public readonly Configurable<int> accelerateForce;
        public readonly Configurable<Color> accelerateColor;
        public readonly Configurable<bool> isConsumeFood;
        public readonly Configurable<float> rockChance;
        public readonly Configurable<AccelerateSound> accelerateSound;
        public readonly Configurable<float> volume;
        public NitrousAccelerateOptions(BaseUnityPlugin plugin)
        {
            Instence = this;

            accelerateForce = config.Bind("AccelerateForce", defaultValue: 15,new ConfigAcceptableRange<int>(0,30));
            accelerateColor = config.Bind("AccelerateColor", defaultValue: Color.green);
            isConsumeFood = config.Bind("IsConsumeFood", defaultValue: false);
            rockChance = config.Bind("RockChance", defaultValue: 0.5f, new ConfigAcceptableRange<float>(0, 1f));
            accelerateSound = config.Bind("AccelerateSound",defaultValue: AccelerateSound.Sound1);
            volume = config.Bind("AccelerateVolume", defaultValue: 1.0f, new ConfigAcceptableRange<float>(0f, 1f));
        }

        public override void Initialize()
        {
            base.Initialize();

            OpTab mainTab = new OpTab(this, "Nitrous Accelerate");

            Tabs = new OpTab[] { mainTab };

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

                // 3. 喷气颜色 
                new OpLabel(10f, 420f, "喷气颜色"),
                new OpColorPicker(accelerateColor, new Vector2(10f, 255f)),

                // 5. 是否消耗饱食度 
                new OpCheckBox(isConsumeFood, 10f, 210f),
                new OpLabel(45f, 211f, "喷气消耗饱食度"),

                // 4. 音量调节 
                new OpFloatSlider(volume, new Vector2(10f, 160f), 220, 2),
                new OpLabel(240f, 161f, "音效音量"),

                // 6. 喷气音效
                new OpResourceSelector(accelerateSound, new Vector2(10f, 90f), 220),
                new OpLabel(240f, 91f, "喷气音效"),


            };

            mainTab.AddItems(uiElements);
        }
    }
}
