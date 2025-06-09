using System.Collections.Generic;
using System.Linq;

namespace Auxilaryfunction.Models
{
    internal class BluePrintBatchModifyBuild
    {
        public static List<int> objIds = new List<int>();
        public static Dictionary<int, int> buildIconsDictionary = new Dictionary<int, int>();

        public static bool NeedRefresh;
        public static ERemoteRoutePriority eRemoteRoutePriority;

        //传送带图标
        public static int BeltSignalIconId = 0;
        //传送带信号下标
        public static int BeltSignalNumber = 0;

        public static BuildTool_BlueprintCopy blue_copy => GameMain.mainPlayer?.controller?.actionBuild?.blueprintCopyTool;
        public static string ModifyStationName = "";
        public static bool[] StationGroup = new bool[30];
        public static bool[] TurretConfigIsSelected = new bool[4];
        public static int[] TurretConfigs = new int[4];
        public static int SelectedTurretGroup = 0;
        public static BuildType CurrentSelectedBuildType;
        public static List<int> CurrentSelectedBuilds = new List<int>();
        public static StationStoreConfig[] stationStoreConfigs = new StationStoreConfig[5];
        private static string currentSelectedBuildName;
        public static List<int> CanModifyBuildList = new List<int>()
        {
            2103,//物流运输站
            2104,//星际物流运输站
            2105,//轨道采集器
            2208,//射线接收站
            2209,//能量枢纽
            baseSmelter,//熔炉 2315 v2 2319 v3
            baseAssemblerMachine,//组装机 2304 v2,2305 v3,2318 v4
            baseRefine,//原油精炼站
            baseChemical,//化工厂 2317 v2
            baseParticle,//微型粒子对撞机
            2311,//电磁轨道弹射器
            baseLab,//矩阵研究站 2902 v2
            Gaussturret,//高斯机枪塔
            Laserturret,//导弹防御塔
            Cannonturret,//聚爆加农炮
            MagnetizedPlasmaturret,//高频激光塔
            Missileturret,//磁化电浆炮
            GroundPlasmaturret,//近程激光炮
        };

        //熔炉ID
        public static List<int> SmelterIDs = new List<int>() { 2302, 2315, 2319 };
        //制造台ID
        public static List<int> AssemblerMachineIDs = new List<int>() { 2303, 2304, 2305, 2318 };
        //研究站ID
        public static List<int> LabIDs = new List<int>() { 2901, 2902 };
        //化工厂ID
        public static List<int> ChemicalIDs = new List<int>() { 2309, 2317 };

        //炮塔ID
        public static List<int> Turrets = new List<int>() { 3001, 3002, 3003, 3004, 3005, 3010 };
        public static Dictionary<int, BuildType> itemBuildTypeDic = new Dictionary<int, BuildType>();
        public const int baseSmelter = 2302;
        public const int baseAssemblerMachine = 2303;
        public const int baseLab = 2901;
        public const int baseRefine = 2308;
        public const int baseChemical = 2309;
        public const int baseParticle = 2310;

        public const int Gaussturret = 3001;
        public const int Laserturret = 3002;
        public const int Cannonturret = 3003;
        public const int MagnetizedPlasmaturret = 3004;
        public const int Missileturret = 3005;
        public const int GroundPlasmaturret = 3010;

        public static List<int> turretpool = new List<int>();
        public static List<int> beltpools = new List<int>();
        public static List<int> powerexchangerpools = new List<int>();
        public static List<int> labpools = new List<int>();
        public static List<int> ejectorpools = new List<int>();
        public static List<int> powergenGammapools = new List<int>();
        public static List<int> stellarstations = new List<int>();
        public static List<int> normalstations = new List<int>();
        public static List<int> AssemblePools = new List<int>();
        public static List<int> ParticlePools = new List<int>();
        public static List<int> RefinePools = new List<int>();
        public static List<int> ChemicalPools = new List<int>();
        public static List<int> SmeltPools = new List<int>();

        public static List<int> FilterAssemblePools = new List<int>();
        public static List<int> Filterlabpools = new List<int>();

        public static string CurrentSelectedBuildName
        {
            get
            {
                return currentSelectedBuildName;
            }

            set
            {
                currentSelectedBuildName = value;
            }
        }

        public static void Init()
        {
            Reset();
            ResetStationConfig();
            itemBuildTypeDic.Clear();
            foreach (var item in LDB.items.dataArray)
            {
                var prefab = item.prefabDesc;
                var buildType = BuildType.None;
                if (prefab.isLab)
                {
                    buildType = BuildType.Lab;
                }
                else if (prefab.isEjector)
                {
                    buildType = BuildType.Ejector;
                }
                else if (prefab.isPowerExchanger)
                {
                    buildType = BuildType.PowerExchanger;
                }
                else if (prefab.gammaRayReceiver)
                {
                    buildType = BuildType.PowergenGamma;
                }
                else if (prefab.isStation && !prefab.isStellarStation && !prefab.isCollectStation && !prefab.isVeinCollector)
                {
                    buildType = BuildType.NormalStation;
                }
                else if (prefab.isStellarStation)
                {
                    buildType = BuildType.StellarStation;
                }
                else if (prefab.isTurret)
                {
                    buildType = BuildType.Turret;
                }
                else if (prefab.isBelt)
                {
                    buildType = BuildType.Belt;
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Assemble)
                {
                    buildType = BuildType.Assemble;
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Smelt)
                {
                    buildType = BuildType.Smelt;
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Chemical)
                {
                    buildType = BuildType.Chemical;
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Refine)
                {
                    buildType = BuildType.Refine;
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Particle)
                {
                    buildType = BuildType.Particle;
                }
                if (buildType == BuildType.None)
                {
                    continue;
                }
                itemBuildTypeDic.Add(item.ID, buildType);
            }
        }
        private static void AddBuildNumber(int itemId)
        {
            if (buildIconsDictionary.ContainsKey(itemId))
            {
                buildIconsDictionary[itemId]++;
            }
            else
            {
                buildIconsDictionary.Add(itemId, 1);
            }
        }
        public static void CheckBluePrint()
        {
            if (!NeedRefresh) return;
            NeedRefresh = false;
            turretpool.Clear();
            beltpools.Clear();
            powerexchangerpools.Clear();
            labpools.Clear();
            ejectorpools.Clear();
            powergenGammapools.Clear();
            stellarstations.Clear();
            normalstations.Clear();
            AssemblePools.Clear();
            ParticlePools.Clear();
            RefinePools.Clear();
            ChemicalPools.Clear();
            SmeltPools.Clear();
            objIds.Clear();
            buildIconsDictionary.Clear();
            var LocalPlanet = GameMain.localPlanet;
            foreach (BuildPreview bp in blue_copy.bpPool)
            {
                if (bp == null || bp.item == null || bp.objId <= 0 || objIds.Contains(bp.objId))
                {
                    continue;
                }
                objIds.Add(bp.objId);
                var prefab = bp.item.prefabDesc;
                var buildType = BuildType.None;
                if (prefab.isLab)
                {
                    labpools.Add(LocalPlanet.factory.entityPool[bp.objId].labId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.isEjector)
                {
                    ejectorpools.Add(LocalPlanet.factory.entityPool[bp.objId].ejectorId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.isPowerExchanger)
                {
                    powerexchangerpools.Add(LocalPlanet.factory.entityPool[bp.objId].powerExcId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.gammaRayReceiver)
                {
                    powergenGammapools.Add(LocalPlanet.factory.entityPool[bp.objId].powerGenId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.isStation && !prefab.isStellarStation && !prefab.isCollectStation && !prefab.isVeinCollector)
                {
                    normalstations.Add(LocalPlanet.factory.entityPool[bp.objId].stationId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.isStellarStation)
                {
                    stellarstations.Add(LocalPlanet.factory.entityPool[bp.objId].stationId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.isTurret)
                {
                    turretpool.Add(LocalPlanet.factory.entityPool[bp.objId].turretId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.isBelt)//LocalPlanet.factory.entitySignPool[bp.objId].iconId0 > 0
                {
                    beltpools.Add(LocalPlanet.factory.entityPool[bp.objId].id);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Assemble)
                {
                    AssemblePools.Add(LocalPlanet.factory.entityPool[bp.objId].assemblerId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Smelt)
                {
                    SmeltPools.Add(LocalPlanet.factory.entityPool[bp.objId].assemblerId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Chemical)
                {
                    ChemicalPools.Add(LocalPlanet.factory.entityPool[bp.objId].assemblerId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Refine)
                {
                    RefinePools.Add(LocalPlanet.factory.entityPool[bp.objId].assemblerId);
                    AddBuildNumber(bp.item.ID);
                }
                else if (prefab.assemblerRecipeType == ERecipeType.Particle)
                {
                    ParticlePools.Add(LocalPlanet.factory.entityPool[bp.objId].assemblerId);
                    AddBuildNumber(bp.item.ID);
                }
                if (buildType == BuildType.None)
                {
                    continue;
                }
            }
            if (CurrentSelectedBuilds.Count == 0 && buildIconsDictionary.Count > 0)
            {
                SelectItemId(buildIconsDictionary.First().Key);
            }
            OnFilterAssemblePoolsConditionChanged();
        }

        public static void Reset()
        {
            CurrentSelectedBuilds.Clear();
            CurrentSelectedBuildName = "暂无选中设备".getTranslate();
            for (int i = 0; i < 4; i++)
            {
                TurretConfigs[i] = 0;
            }
        }

        public static void ResetStationConfig()
        {
            ModifyStationName = "";
            eRemoteRoutePriority = ERemoteRoutePriority.Ignore;
            stationStoreConfigs = new StationStoreConfig[5];
            for (int i = 0; i < 5; i++)
            {
                stationStoreConfigs[i] = new StationStoreConfig();
            }
        }

        public static void SelectItemId(int itemId)
        {
            if (CurrentSelectedBuilds.Contains(itemId))
            {
                UnSelectItemId(itemId);
            }
            else
            {
                CurrentSelectedBuilds.Add(itemId);
                CurrentSelectedBuildType = itemBuildTypeDic[itemId];
            }

            for (int i = CurrentSelectedBuilds.Count - 1; i >= 0; i--)
            {
                var tempid = CurrentSelectedBuilds[i];
                if (itemBuildTypeDic[tempid] != CurrentSelectedBuildType)
                {
                    CurrentSelectedBuilds.RemoveAt(i);
                }
            }
            OnFilterAssemblePoolsConditionChanged();
        }

        public static void UnSelectItemId(int itemId)
        {
            if (CurrentSelectedBuilds.Contains(itemId))
            {
                CurrentSelectedBuilds.Remove(itemId);
            }
            if (CurrentSelectedBuilds.Count == 0)
            {
                CurrentSelectedBuildType = BuildType.None;
            }
            OnFilterAssemblePoolsConditionChanged();
        }

        private static void OnFilterAssemblePoolsConditionChanged()
        {
            var LocalPlanet = GameMain.localPlanet;
            List<int> tempassemblerpools;
            FilterAssemblePools.Clear();
            Filterlabpools.Clear();
            switch (CurrentSelectedBuildType)
            {
                case BuildType.Assemble:
                    tempassemblerpools = AssemblePools;
                    break;
                case BuildType.Smelt:
                    tempassemblerpools = SmeltPools;
                    break;
                case BuildType.Chemical:
                    tempassemblerpools = ChemicalPools;
                    break;
                case BuildType.Particle:
                    tempassemblerpools = ParticlePools;
                    break;
                case BuildType.Refine:
                    tempassemblerpools = RefinePools;
                    break;
                default:
                    tempassemblerpools = new List<int>();
                    break;
            }
            for (int j = 0; j < tempassemblerpools.Count; j++)
            {
                var assembler = LocalPlanet.factory.factorySystem.assemblerPool[tempassemblerpools[j]];
                var entityId = assembler.entityId;
                var itemId = LocalPlanet.factory.entityPool[entityId].protoId;
                if (CurrentSelectedBuilds.Contains(itemId))
                {
                    FilterAssemblePools.Add(tempassemblerpools[j]);
                }
            }
            for (int i = 0; i < labpools.Count; i++)
            {
                var lab = LocalPlanet.factory.factorySystem.labPool[labpools[i]];
                var entityId = lab.entityId;
                var itemId = LocalPlanet.factory.entityPool[entityId].protoId;
                if (CurrentSelectedBuilds.Contains(itemId))
                {
                    Filterlabpools.Add(labpools[i]);
                }
            }
        }
    }
}
