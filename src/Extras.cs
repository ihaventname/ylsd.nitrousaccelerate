using System;
using System.Security.Permissions;
using UnityEngine;

//由于雨世界的一些特性
//游戏在加载模组时会遇到一些问题
//这个文件是用来解决这些问题的
//大多数情况下它能让你的模组正常加载(前提是你的模组不存在逻辑错误)
//如果你看不懂就不要动这个文件


/*
 * This file contains fixes to some common problems when modding Rain World.
 * Unless you know what you're doing, you shouldn't modify anything here.
 */

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618


internal static class Extras
{
    private static bool _initialized;

    // Ensure resources are only loaded once and that failing to load them will not break other mods
    public static On.RainWorld.hook_OnModsInit WrapInit(Action<RainWorld> loadResources)
    {
        return (orig, self) =>
        {
            orig(self);

            try
            {
                if (!_initialized)
                {
                    _initialized = true;
                    loadResources(self);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        };
    }
}