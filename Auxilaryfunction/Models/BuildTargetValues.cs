using System.Collections.Generic;
using static Auxilaryfunction.AuxConfig;
namespace Auxilaryfunction.Models
{
    public class BuildTargetValues
    {
        public static Dictionary<string, BuildConfig> buildConfigsDic = new Dictionary<string, BuildConfig>();

        /// <summary>
        /// 物流站物流运输机填充数量
        /// </summary>
        public static int StationDroneNumber
        {
            get
            {
                return auto_supply_drone.Value;
            }
            set
            {
                auto_supply_drone.Value = value;
            }
        }

        /// <summary>
        /// 物流站飞船数量
        /// </summary>
        public static int StationShipNumber
        {
            get
            {
                return auto_supply_ship.Value;
            }
            set
            {
                auto_supply_ship.Value = value;
            }
        }

        /// <summary>
        /// 物流站最大充电功率
        /// </summary>
        public static float StationMaxPowerPertick
        {
            get
            {
                return stationmaxpowerpertick.Value;
            }
            set
            {
                stationmaxpowerpertick.Value = value;
            }
        }

        /// <summary>
        /// 物流站运输机最远路程
        /// </summary>
        public static int StationDroneMaxDistance
        {
            get
            {
                return stationdronedist.Value;
            }
            set
            {
                stationdronedist.Value = value;
            }
        }

        /// <summary>
        /// 物流站运输船最远路程
        /// </summary>
        public static int StationShipMaxDistance
        {
            get
            {
                return stationshipdist.Value;
            }
            set
            {
                stationshipdist.Value = value;
            }
        }

        /// <summary>
        /// 物流站曲速启用路程
        /// </summary>
        public static double StationWarpEnableDistance
        {
            get
            {
                return stationwarpdist.Value;
            }
            set
            {
                stationwarpdist.Value = value;
            }
        }

        /// <summary>
        /// 物流站运输机起送量
        /// </summary>
        public static float StationDroneStartCarry
        {
            get
            {
                return DroneStartCarry.Value;
            }
            set
            {
                DroneStartCarry.Value = value;
            }
        }

        /// <summary>
        /// 物流站运输船起送量
        /// </summary>
        public static float StationShipStartCarry
        {
            get
            {
                return ShipStartCarry.Value;
            }
            set
            {
                ShipStartCarry.Value = value;
            }
        }

        /// <summary>
        /// 物流站补充翘曲数量
        /// </summary>
        public static int StationStartWarpNumber
        {
            get
            {
                return auto_supply_warp.Value;
            }
            set
            {
                auto_supply_warp.Value = value;
            }
        }

        /// <summary>
        /// 大型采矿机采矿速率
        /// </summary>
        public static int VeinCollectorSpeed
        {
            get
            {
                return veincollectorspeed.Value;
            }
            set
            {
                veincollectorspeed.Value = value;
            }
        }

        /// <summary>
        /// 自动填充配送运输机数量
        /// </summary>
        public static int AutoSupplyCourierNumber
        {
            get
            {
                return auto_supply_Courier.Value;
            }
            set
            {
                auto_supply_Courier.Value = value;
            }
        }


        static BuildTargetValues()
        {
            buildConfigsDic = new Dictionary<string, BuildConfig>()
            {
                ["填充飞机数量"] = new BuildConfig() { Name = "填充飞机数量", LowLimit = 0, UpperLimit = 100 },
                ["填充飞船数量"] = new BuildConfig() { Name = "填充飞船数量", LowLimit = 0, UpperLimit = 10 },
                ["最大充电功率"] = new BuildConfig() { Name = "最大充电功率", LowLimit = 30, UpperLimit = 300 },
                ["运输机最远路程"] = new BuildConfig() { Name = "运输机最远路程", LowLimit = 20, UpperLimit = 180 },
                ["运输船最远路程"] = new BuildConfig() { Name = "运输船最远路程", LowLimit = 1, UpperLimit = 61 },
                ["曲速启用路程"] = new BuildConfig() { Name = "曲速启用路程", LowLimit = 0.5f, UpperLimit = 60 },
                ["运输机起送量"] = new BuildConfig() { Name = "运输机起送量", LowLimit = 0.01f, UpperLimit = 1 },
                ["运输船起送量"] = new BuildConfig() { Name = "运输船起送量", LowLimit = 0.01f, UpperLimit = 1 },
                ["翘曲填充数量"] = new BuildConfig() { Name = "翘曲填充数量", LowLimit = 0, UpperLimit = 50 },
                ["大型采矿机采矿速率"] = new BuildConfig() { Name = "大型采矿机采矿速率", LowLimit = 10, UpperLimit = 30 },
                ["填充配送机数量"] = new BuildConfig() { Name = "填充配送机数量", LowLimit = 0, UpperLimit = 10 },
            };
        }
    }

    public struct BuildConfig
    {
        public string Name { get; set; }
        public float LowLimit { get; set; }
        public float UpperLimit { get; set; }
    }
}
