using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using static Auxilaryfunction.Auxilaryfunction;
using static Auxilaryfunction.Constant;
using static Auxilaryfunction.AuxConfig;

namespace Auxilaryfunction.Services
{
    /// <summary>
    /// 物流相关功能服务。
    /// 提供配送机/物流站飞机与飞船/翘曲器填充、
    /// 人造恒星燃料补充、物流站参数批量设置、
    /// 大型采矿机采集速率设置、气态星球轨道采集器批量铺设等操作。
    /// 依赖当前星球 <c>LocalPlanet</c> 与玩家背包 <c>player.package</c>，
    /// 读取插件配置项以执行对应逻辑。
    /// </summary>
    public static class LogisticsService
    {
        /// <summary>
        /// 为当前星球的配送机（Dispenser）填充目标数量的配送运输机。
        /// 会从玩家背包中取出物品 ID 5003 并补充到空闲配送机数量。
        /// </summary>
        public static void AddCourierToAllStar()
        {
            if (LocalPlanet == null) return;
            int autoCourierValue = auto_supply_Courier.Value;
            foreach (var dc in LocalPlanet.factory.transport.dispenserPool)
            {
                if (dc == null) continue;
                int needCourierNum = autoCourierValue - dc.idleCourierCount - dc.workCourierCount;
                if (needCourierNum > 0)
                    dc.idleCourierCount += player.package.TakeItem(5003, needCourierNum, out _);
            }
        }

        /// <summary>
        /// 为当前星球的物流站补充飞机、飞船与翘曲器。
        /// 本地站飞机上限为 50，星际站为 100；飞船仅星际站补充。
        /// 分别消耗物品 ID：飞机 5001、飞船 5002、翘曲器 1210。
        /// </summary>
        public static void AddDroneShipToStation()
        {
            if (LocalPlanet == null || LocalPlanet.type == EPlanetType.Gas) return;
            int autoShipValue = auto_supply_ship.Value;
            int autoDroneValue = auto_supply_drone.Value;
            int autoWrapValue = auto_supply_warp.Value;
            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null || sc.isVeinCollector) continue;
                int dronelimit = sc.isStellar ? 100 : 50;
                int tempdrone = autoDroneValue > dronelimit ? dronelimit : autoDroneValue;
                int needDroneNum = tempdrone - sc.workDroneCount - sc.idleDroneCount;
                if (needDroneNum > 0)
                    sc.idleDroneCount += player.package.TakeItem(5001, needDroneNum, out _);
                int needShipNum = autoShipValue - sc.idleShipCount - sc.workShipCount;
                if (sc.isStellar && needShipNum > 0)
                    sc.idleShipCount += player.package.TakeItem(5002, needShipNum, out _);
                int needWrapNum = autoWrapValue - sc.warperCount;
                if (needWrapNum > 0)
                    sc.warperCount += player.package.TakeItem(1210, needWrapNum, out _);
            }
        }

        /// <summary>
        /// 为当前星球的人造恒星补充燃料（仅支持 1803/1804）。
        /// 会从玩家背包取出指定燃料，按配置补齐至目标数量。
        /// </summary>
        public static void AddFuelToAllStar()
        {
            if (LocalPlanet == null) return;
            PlanetFactory fs = LocalPlanet.factory;
            int fuelId = auto_supply_starfuelID.Value;
            if (fuelId < 1803 || fuelId > 1804)
            {
                return;
            }
            foreach (PowerGeneratorComponent pgc in fs.powerSystem.genPool)
            {
                if (pgc.fuelMask == 4 && pgc.fuelCount < auto_supply_starfuel.Value)
                {
                    int needNum = auto_supply_starfuel.Value - pgc.fuelCount;
                    short fuelcount = ((short)player.package.TakeItem(fuelId, needNum, out int inc));
                    if (fuelcount == 0)
                    {
                        return;
                    }
                    if (pgc.fuelCount == 0)
                    {
                        fs.powerSystem.genPool[pgc.id].SetNewFuel(fuelId, fuelcount, (short)inc);
                    }
                    else if (pgc.fuelCount < auto_supply_starfuel.Value && fuelId == pgc.fuelId)
                    {
                        fs.powerSystem.genPool[pgc.id].fuelCount += fuelcount;
                        fs.powerSystem.genPool[pgc.id].fuelInc += (short)inc;
                    }
                }
            }
        }

        /// <summary>
        /// 批量设置当前星球所有物流站参数：运输机/运输船最远距离、
        /// 最大充电功率、起送量、翘曲启用距离等。
        /// </summary>
        public static void ChangeAllStationConfig()
        {
            if (LocalPlanet == null || LocalPlanet.factory == null || LocalPlanet.factory.transport == null || LocalPlanet.factory.transport.stationPool == null) return;
            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null || sc.isVeinCollector) continue;
                sc.tripRangeDrones = Math.Cos(stationdronedist.Value * Math.PI / 180);
                LocalPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)stationmaxpowerpertick.Value * 16667;
                if (stationmaxpowerpertick.Value > 60 && !sc.isStellar)
                {
                    LocalPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)60 * 16667;
                }
                sc.deliveryDrones = DroneStartCarry.Value == 0 ? 1 : (int)(DroneStartCarry.Value * 10) * 10;
                if (sc.isStellar)
                {
                    sc.warpEnableDist = stationwarpdist.Value * GalaxyData.AU;
                    sc.deliveryShips = ShipStartCarry.Value == 0 ? 1 : (int)(ShipStartCarry.Value * 10) * 10;
                    sc.tripRangeShips = stationshipdist.Value > 60 ? 24000000000 : stationshipdist.Value * 2400000;
                }
            }
        }

        /// <summary>
        /// 批量设置当前星球所有大型采矿机的采集速率。
        /// </summary>
        public static void ChangeAllVeinCollectorSpeedConfig()
        {
            if (LocalPlanet == null || LocalPlanet.type == EPlanetType.Gas) return;
            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null || !sc.isVeinCollector) continue;
                LocalPlanet.factory.factorySystem.minerPool[sc.minerId].speed = veincollectorspeed.Value * 1000;
            }
        }

        /// <summary>
        /// 在气态星球一键批量铺设 40 个轨道采集器。
        /// 要求该星球无物流站/预建；不足 40 个采集器则提示失败。
        /// </summary>
        public static void SetGasStation()
        {
            if (LocalPlanet == null || LocalPlanet.type != EPlanetType.Gas)
                return;
            PlanetFactory pf = LocalPlanet.factory;
            if (pf != null)
            {
                foreach (StationComponent sc in pf.transport.stationPool)
                {
                    if (sc != null && sc.isCollector)
                    {
                        UIMessageBox.Show("铺满轨道采集器失败", "当前星球存在采集器，请先清空所有建筑", "确定", 3);
                        return;
                    }
                }
                if (pf.prebuildCount > 0)
                {
                    UIMessageBox.Show("铺满轨道采集器失败", "当前星球存在采集器，请先清空所有建筑", "确定", 3);
                    return;
                }
            }
            StorageComponent package = player.package;
            int sum = 0;
            for (int index = 0; index < package.size; ++index)
            {
                if (package.grids[index].itemId == 2105)
                {
                    sum += package.grids[index].count;
                    if (sum >= 40)
                        break;
                }
            }
            if (sum >= 40) package.TakeItem(2105, 40, out _);
            else
            {
                UIMessageBox.Show("铺满轨道采集器失败", "背包里没有40个采集器，请重新准备", "确定", 3);
                return;
            }
            for (int i = 0; i < 40; i++)
            {
                Vector3 pos;
                double posx = 0;
                double posz = 0;
                if (i % 10 == 0)
                {
                    if (i < 20)
                    {
                        posx = i == 0 ? posxz[0, 0] : -posxz[0, 0];
                        posz = i == 0 ? posxz[0, 1] : -posxz[0, 1];
                    }
                    else
                    {
                        posx = i == 20 ? posxz[0, 1] : -posxz[0, 1];
                        posz = i == 20 ? posxz[0, 0] : -posxz[0, 0];
                    }
                }
                else if (0 < i && i < 10)
                {
                    posx = posxz[i, 0];
                    posz = posxz[i, 1];
                }
                else if (i > 10 && i < 20)
                {
                    posx = -posxz[i - 10, 0];
                    posz = posxz[i - 10, 1];
                }
                else if (i > 20 && i < 30)
                {
                    posx = posxz[i - 20, 0];
                    posz = -posxz[i - 20, 1];
                }
                else if (i > 30)
                {
                    posx = -posxz[i - 30, 0];
                    posz = -posxz[i - 30, 1];
                }
                pos = new Vector3((float)(posx * player.planetData.realRadius), 0, (float)(posz * player.planetData.realRadius));
                Vector3 vector3_3 = 0.025f * pos.normalized * player.planetData.realRadius;
                Quaternion quaternion3 = Maths.SphericalRotation(pos, player.controller.actionBuild.clickTool.yaw);
                pos += vector3_3;
                PrebuildData prebuild = new()
                {
                    protoId = 2105,
                    modelIndex = 117,
                    pos = pos + quaternion3 * Vector3.zero,
                    pos2 = pos + quaternion3 * Vector3.zero,
                    rot = quaternion3 * Quaternion.identity,
                    rot2 = quaternion3 * Quaternion.identity,
                    pickOffset = 0,
                    insertOffset = 0,
                    recipeId = 0,
                    filterId = 0,
                    paramCount = 0
                };
                prebuild.InitParametersArray(0);
                player.controller.actionBuild.clickTool.factory.AddPrebuildDataWithComponents(prebuild);
            }
        }

        /// <summary>
        /// 获取逻辑的中文
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="isStellar"></param>
        /// <param name="careStellar"></param>
        /// <returns></returns>
        public static string GetLogisticText(ELogisticStorage logic, bool isStellar, bool careStellar)
        {
            string prefix = careStellar ? (isStellar ? "星际" : "本地") : "";
            string core = logic == ELogisticStorage.Supply ? "供应" : logic == ELogisticStorage.Demand ? "需求" : "仓储";
            return (prefix + core).Translate();
        }
    }
}
