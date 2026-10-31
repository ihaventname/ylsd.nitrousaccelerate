using System;

namespace NitrousAccelerate
{
    enum Surprise
    {
        石头,蓝果,珍珠,黏菌,手雷,随机,随机无手雷
    }
    internal class SpawnSurprise
    {
        private static Surprise[] SurpriseRandomPool =
            { Surprise.石头,Surprise.蓝果,Surprise.珍珠,Surprise.黏菌,Surprise.手雷 };

        public static AbstractPhysicalObject CreateSurpriseObject(Surprise config, World world, WorldCoordinate pos, EntityID id)
        {
            // 处理随机逻辑
            if (config == Surprise.随机)
            {
                config = SurpriseRandomPool[UnityEngine.Random.Range(0, SurpriseRandomPool.Length)];
            }
            else if (config == Surprise.随机无手雷)
            {
                config = SurpriseRandomPool[UnityEngine.Random.Range(0, 4)]; // 0-3 不含手雷
            }

            switch (config)
            {
                case Surprise.石头:
                    return new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, pos, id);

                case Surprise.蓝果:
                    // 参数: world, 实体对象(null), 坐标, ID, originRoom(-1), consumableIndex(-1), rotted(false), consumableData(null)
                    return new DangleFruit.AbstractDangleFruit(world, null, pos, id, -1, -1, false, null);

                case Surprise.珍珠:
                    // 参数: world, 类型, 实体对象(null), 坐标, ID, originRoom(-1), placedObjectIndex(-1), consumableData(null), 珍珠类型(Misc)
                    return new DataPearl.AbstractDataPearl(
                        world,
                        AbstractPhysicalObject.AbstractObjectType.DataPearl,
                        null,
                        pos,
                        id,
                        -1,
                        -1,
                        null,
                        DataPearl.AbstractDataPearl.DataPearlType.Misc
                    );

                case Surprise.黏菌:
                    // 黏菌没有独立的 Abstract 子类，使用通用的 AbstractConsumable
                    return new AbstractConsumable(
                        world,
                        AbstractPhysicalObject.AbstractObjectType.SlimeMold,
                        null,
                        pos,
                        id,
                        -1,
                        -1,
                        null
                    );

                case Surprise.手雷:
                    return new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, pos, id);

                default:
                    return new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, pos, id);
            }
        }
    }
}
