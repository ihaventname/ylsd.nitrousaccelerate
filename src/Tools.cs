using ImprovedInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


//小工具
//重复使用静态的函数可以放到这里

namespace NitrousAccelerate
{
    public static class Tools
    {
        public static float[] RGBToHSV(Color rgb)
        {
            float h, s, b;
            Color.RGBToHSV(rgb, out h, out s, out b);
            return new float[]{h, s, b};
        }
    }
}
