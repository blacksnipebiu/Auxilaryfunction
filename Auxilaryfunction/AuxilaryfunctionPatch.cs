using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Auxilaryfunction.Auxilaryfunction;

namespace Auxilaryfunction
{
    public class AuxilaryfunctionPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameLoader), "CreateLoader")]
        public static void GameLoaderCreateLoaderPrefix()
        {
            GameDataImported = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameLoader), "SelfDestroy")]
        public static void GameLoaderCreateLoaderPostfix()
        {
            GameDataImported = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactorySystem), "NewMinerComponent")]
        public static void NewMinerComponentPatch(ref int __result, FactorySystem __instance)
        {
            if (auto_supply_station.Value && __instance.factory.entityPool[__instance.minerPool[__result].entityId].protoId == 2316)
            {
                __instance.minerPool[__result].speed = veincollectorspeed.Value * 1000;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(PowerSystem), "NewGeneratorComponent")]
        public static void NewGeneratorComponentPatch(ref int __result, PowerSystem __instance)
        {
            if (__instance.genPool[__result].fuelMask == 4)
            {
                if (autosetSomevalue_bool.Value)
                {
                    int fuelId = auto_supply_starfuelID.Value;
                    if (fuelId < 1803 || fuelId > 1804)
                    {
                        return;
                    }
                    short fuelcount = (short)player.package.TakeItem(fuelId, auto_supply_starfuel.Value, out int inc);
                    if (fuelcount != 0)
                    {
                        __instance.genPool[__result].SetNewFuel(fuelId, fuelcount, (short)inc);
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactorySystem), "NewEjectorComponent")]
        public static void NewEjectorComponentPatch(ref int __result, FactorySystem __instance)
        {
            if (EjectorDictionary.ContainsKey(__instance.planet.id))
                EjectorDictionary[__instance.planet.id].Add(__result);
            else
            {
                EjectorDictionary[__instance.planet.id] = new List<int>
                {
                    __result
                };
            }

            __instance.ejectorPool[__result].SetOrbit(1);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetTransport), "NewStationComponent")]
        public static void NewStationComponentPatch(ref StationComponent __result, PlanetTransport __instance)
        {
            if (auto_supply_station.Value && !__result.isCollector && !__result.isVeinCollector)
            {
                __result.idleDroneCount = player.package.TakeItem(5001, __result.isStellar ? auto_supply_drone.Value : (auto_supply_drone.Value > 50 ? 50 : auto_supply_drone.Value), out _);
                __result.tripRangeDrones = Math.Cos(stationdronedist.Value * Math.PI / 180);
                __instance.planet.factory.powerSystem.consumerPool[__result.pcId].workEnergyPerTick = (long)stationmaxpowerpertick.Value * 16667;
                if (stationmaxpowerpertick.Value > 60 && !__result.isStellar)
                {
                    __instance.planet.factory.powerSystem.consumerPool[__result.pcId].workEnergyPerTick = (long)60 * 16667;
                }

                __result.deliveryDrones = DroneStartCarry.Value == 0 ? 1 : (int)(DroneStartCarry.Value * 10) * 10;
                if (__result.isStellar)
                {
                    __result.warperCount = player.package.TakeItem(1210, auto_supply_warp.Value, out _);
                    __result.warpEnableDist = stationwarpdist.Value * GalaxyData.AU;
                    __result.deliveryShips = ShipStartCarry.Value == 0 ? 1 : (int)(ShipStartCarry.Value * 10) * 10;
                    __result.idleShipCount = player.package.TakeItem(5002, auto_supply_ship.Value, out _);
                    __result.tripRangeShips = stationshipdist.Value > 60 ? 24000000000 : stationshipdist.Value * 2400000;
                    if (GameMain.data.history.TechUnlocked(3404)) __result.warperCount = player.package.TakeItem(1210, auto_supply_warp.Value, out _);
                }
            }
            if (auto_supply_station.Value && __result.isVeinCollector)
            {
                __instance.factory.factorySystem.minerPool[__result.minerId].speed = veincollectorspeed.Value * 1000;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactorySystem), "RemoveEjectorComponent")]
        public static void RemoveEjectorComponentPatch(int id, FactorySystem __instance)
        {
            if (EjectorDictionary[__instance.planet.id].Contains(id))
                EjectorDictionary[__instance.planet.id].Remove(id);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetTransport), "NewDispenserComponent")]
        public static void NewDispenserComponentPatch(int __result, PlanetTransport __instance)
        {
            if (autosetCourier_bool.Value)
            {
                __instance.dispenserPool[__result].idleCourierCount = player.package.TakeItem(5003, auto_supply_Courier.Value, out _);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBlueprintBrowser), "SetCurrentDirectory")]
        public static void UIBlueprintBrowserSetCurrentDirectory(string fullpath)
        {
            if (SaveLastOpenBluePrintBrowserPathConfig.Value)
            {
                LastOpenBluePrintBrowserPathConfig.Value = fullpath;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIBlueprintBrowser), "_OnOpen")]
        public static void UIBlueprintBrowser_OnOpen(ref UIBlueprintBrowser __instance)
        {
            if (SaveLastOpenBluePrintBrowserPathConfig.Value && string.IsNullOrEmpty(__instance.openPath) && !string.IsNullOrEmpty(LastOpenBluePrintBrowserPathConfig.Value))
            {
                __instance.openPath = LastOpenBluePrintBrowserPathConfig.Value;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactoryModel), "Update")]
        public static bool FactoryModelUpdatePatch()
        {
            return !norender_entity_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactoryModel), "LateUpdate")]
        public static bool FactoryModelLateUpdatePatch()
        {
            return !norender_entity_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LogisticDroneRenderer), "Draw")]
        public static bool DroneDrawPatch()
        {
            return !norender_shipdrone_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LogisticShipUIRenderer), "Draw")]
        public static bool LogisticShipUIRendererDrawPatch()
        {
            return !norender_shipdrone_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LogisticCourierRenderer), "Draw")]
        public static bool CourierDrawPatch()
        {
            return !norender_shipdrone_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LogisticShipRenderer), "Draw")]
        public static bool ShipDrawPatch()
        {
            return !norender_shipdrone_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LabRenderer), "Render")]
        public static bool LabRendererPatch(LabRenderer __instance)
        {
            if (__instance.modelId == 70 || __instance.modelId == 455)
                return !norender_lab_bool.Value && !simulatorrender;
            return true;
        }

        /// <summary>
        /// 黑雾基地
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SectorModel), "LateUpdate")]
        public static bool EnemyDFGroundRendererRenderPatch()
        {
            return !norender_DarkFog.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DysonSphere), "DrawModel")]
        public static bool DysonDrawModelPatch()
        {
            return !norender_dysonshell_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DysonSphere), "DrawPost")]
        public static bool DysonDrawPostPatch()
        {
            return !norender_dysonswarm_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawArea")]
        public static bool UIPowerGizmoDrawAreaPatch()
        {
            return !norender_powerdisk_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIDarkFogMonitor), "Determine")]
        public static bool UIDarkFogMonitorDetermine()
        {
            return !HideDarkFogMonitor && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIDarkFogAssaultTip), "Determine")]
        public static bool UIDarkFogAssaultTipDetermine()
        {
            return !HideDarkFogAssaultTip && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawCover")]
        public static bool UIPowerGizmoDrawCoverPatch()
        {
            return !norender_powerdisk_bool.Value && !simulatorrender;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoContainer), "Draw")]
        public static bool PathRenderingBatchDrawPatch()
        {
            return !norender_beltitem.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildingParameters), "CopyFromFactoryObject")]
        public static void CopyFromFactoryObjectPatch(int objectId, PlanetFactory factory)
        {
            if (stationcopyItem_bool.Value)
            {
                if (player != null && player.controller != null && player.controller.actionBuild != null)
                {
                    PlayerAction_Build build = player.controller.actionBuild;
                    if (build.blueprintCopyTool != null || build.blueprintPasteTool != null)
                    {
                        if (build.blueprintPasteTool.active || build.blueprintCopyTool.active)
                            return;
                    }
                }
                EntityData[] entitypool = factory.entityPool;
                if (objectId > entitypool.Length || objectId <= 0) return;
                int stationId = entitypool[objectId].stationId;
                if (stationId <= 0)
                    return;

                StationComponent sc = factory.transport.stationPool[stationId];
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        stationcopyItem[i, j] = 0;
                    }
                }
                for (int i = 0; i < sc.storage.Length && i < 5; i++)
                {
                    if (sc.storage[i].itemId <= 0) continue;
                    stationcopyItem[i, 0] = sc.storage[i].itemId;
                    stationcopyItem[i, 1] = sc.storage[i].max;
                    stationcopyItem[i, 2] = (int)sc.storage[i].localLogic;
                    stationcopyItem[i, 3] = (int)sc.storage[i].remoteLogic;
                    stationcopyItem[i, 4] = sc.storage[i].localOrder;
                    stationcopyItem[i, 5] = sc.storage[i].remoteOrder;
                }
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildTool_Path), "DeterminePreviews")]
        public static void BuildTool_PathPatch(BuildTool_Path __instance)
        {
            if (!KeepBeltHeight.Value) return;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                PlanetAuxData planetAux = player.controller.actionBuild.planetAux;
                if (planetAux == null) return;
                if (__instance.altitude == 0)
                {
                    if (ObjectIsBeltOrSplitter(__instance, __instance.castObjectId))
                    {
                        __instance.altitude = Altitude(__instance.castObjectPos, planetAux);
                    }
                    else if (__instance.startObjectId != 0)
                    {
                        __instance.altitude = Altitude(__instance.pathPoints[0], planetAux);
                    }
                }
                //else if (Input.GetKey(KeyCode.LeftControl) && ObjectIsBeltOrSplitter(__instance, __instance.castObjectId))
                //{
                //    __instance.altitude = Altitude(__instance.castObjectPos, planetAux);
                //    if (__instance.altitude == 0)
                //    {
                //        __instance.altitude = 1;
                //    }
                //}
            }
        }

        private static int Altitude(Vector3 pos, PlanetAuxData aux)
        {
            Vector3 b = aux.Snap(pos, true);
            return (int)Math.Round((double)(Vector3.Distance(pos, b) / 1.3333333f));
        }

        private static bool ObjectIsBeltOrSplitter(BuildTool_Path tool, int objId)
        {
            if (objId == 0) return false;
            ItemProto itemProto = LDB.items.Select(tool.factory.entityPool[Math.Abs(objId)].protoId);
            return itemProto != null && itemProto.prefabDesc != null && (itemProto.prefabDesc.isBelt || itemProto.prefabDesc.isSplitter);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildingParameters), "PasteToFactoryObject")]
        public static void PasteToFactoryObjectPatch(int objectId, PlanetFactory factory)
        {
            if (stationcopyItem_bool.Value)
            {
                EntityData[] entitypool = factory.entityPool;
                if (objectId > entitypool.Length || objectId <= 0) return;
                int stationId = entitypool[objectId].stationId;
                if (stationId <= 0)
                    return;

                StationComponent sc = factory.transport.stationPool[stationId];
                if (sc.isVeinCollector || sc.isCollector) return;
                for (int i = 0; i < sc.storage.Length && i < 5; i++)
                {
                    if (stationcopyItem[i, 0] > 0)
                    {
                        if (sc.storage[i].count > 0 && sc.storage[i].itemId != stationcopyItem[i, 0])
                            player.TryAddItemToPackage(sc.storage[i].itemId, sc.storage[i].count, 0, false);
                        factory.transport.SetStationStorage(stationId, i, stationcopyItem[i, 0], stationcopyItem[i, 1], (ELogisticStorage)stationcopyItem[i, 2]
                            , (ELogisticStorage)stationcopyItem[i, 3], player);
                    }
                    else
                        factory.transport.SetStationStorage(stationId, i, 0, 0, ELogisticStorage.None, ELogisticStorage.None, player);

                }
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIVariousPopupCard), "Refresh")]
        public static bool UIVariousPopupCardRefresh(UIVariousPopupCard __instance)
        {
            if (CloseMilestone.Value)
            {
                MilestoneProto milestoneProto = __instance.data as MilestoneProto;
                if (milestoneProto != null)
                {
                    __instance._Close();
                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIAdvisorTip), "RequestAdvisorTip")]
        public static bool UIAdvisorTipRequestAdvisorTip()
        {
            return !CloseUIAdvisor.Value;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRandomTip), "_OnOpen")]
        public static void UIBuildMenu_OnOpenPatch(ref UIRandomTip __instance)
        {
            if (CloseUIRandomTip.Value)
            {
                __instance._Close();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UITutorialTip), "PopupTutorialTip")]
        public static bool UITutorialTipPopupTutorialTip()
        {
            if (CloseUITutorialTip.Value)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UITechTree), "UpdateScale")]
        public static bool UITechTreeUpdateScalePatch(UITechTree __instance)
        {
            if (noscaleuitech_bool.Value && (__instance.selected != null || __instance.centerViewNode != null)) return false;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UniverseSimulator), "GameTick")]
        public static void GameDataOnDrawPatch(UniverseSimulator __instance)
        {
            if (simulatorchanging)
            {
                int num = 0;
                __instance.backgroundStars.gameObject.SetActive(!simulatorrender);
                while (__instance.planetSimulators != null && num < __instance.planetSimulators.Length)
                {
                    if (__instance.planetSimulators[num] != null)
                    {
                        __instance.planetSimulators[num].gameObject.SetActive(!simulatorrender);
                    }
                    num++;
                }
                num = 0;
                while (__instance.starSimulators != null && num < __instance.starSimulators.Length)
                {
                    if (__instance.starSimulators[num] != null)
                    {
                        if (__instance.starSimulators[num].starData.type == EStarType.NeutronStar && Configs.builtin.neutronStarPrefab.streamRenderer != null)
                        {
                            num++;
                            continue;
                        }
                        __instance.starSimulators[num].gameObject.SetActive(!simulatorrender);
                    }
                    num++;
                }
                simulatorchanging = false;
            }
        }

    }

    public class PlayerOperation
    {
        private static Harmony _patch;
        private static bool enable;
        public static bool Enable
        {
            get => enable;
            set
            {
                if (enable == value) return;
                enable = value;
                if (enable)
                {
                    _patch = Harmony.CreateAndPatchAll(typeof(PlayerOperation));
                }
                else
                {
                    _patch.UnpatchSelf();
                    FlyStatus = false;
                    ClearFollow();
                }
            }
        }
        public static float t = 20;
        public static int AuDis = (int)GalaxyData.AU;
        public static bool FlyStatus;

        private static Mecha mecha => player.mecha;
        public static bool NeedNavigation => GameMain.mainPlayer.navigation.indicatorAstroId > 0 || GameMain.mainPlayer.navigation._indicatorMsgId > 0 || GameMain.mainPlayer.navigation._indicatorEnemyId > 0;

        private static PlanetData focusPlanet;
        private static StarData focusStar;
        private static double max_acc => player.controller.actionSail.max_acc;
        private static float maxSailSpeed => player.controller.actionSail.maxSailSpeed;
        private static float maxWarpSpeed => player.controller.actionSail.maxWarpSpeed;
        private static int indicatorAstroId => player.navigation.indicatorAstroId;
        private static int indicatorMsgId => player.navigation.indicatorMsgId;
        private static int indicatorEnemyId => player.navigation.indicatorEnemyId;
        private static bool CanWarp => LocalPlanet == null && autowarpcommand.Value && !player.warping && mecha.coreEnergy > mecha.warpStartPowerPerSpeed * maxWarpSpeed;
        private static ESpaceGuideType guidetype;
        public static void ClearFollow()
        {
            if (player?.navigation != null)
            {
                player.navigation.indicatorAstroId = 0;
                player.navigation.indicatorMsgId = 0;
                player.navigation.indicatorEnemyId = 0;
            }
            t = 20;
            focusPlanet = null;
            focusStar = null;
            Debug.Log("Auxilaryfunction.ClearFollow");
        }

        //操纵人物
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerController), "GetInput")]
        public static void Postfix(PlayerController __instance)
        {
            #region 自动导航
            if (autonavigation_bool.Value)
            {
                if (indicatorAstroId <= 1000000 && indicatorAstroId > 0)
                {
                    if (indicatorAstroId % 100 != 0)
                    {
                        if (focusPlanet == null)
                        {
                            FlyStatus = true;
                            focusPlanet = GameMain.galaxy.PlanetById(indicatorAstroId);
                            guidetype = ESpaceGuideType.Planet;
                        }
                        else if (focusPlanet.id != indicatorAstroId)
                        {
                            FlyStatus = true;
                            focusPlanet = GameMain.galaxy.PlanetById(indicatorAstroId);
                            guidetype = ESpaceGuideType.Planet;
                        }
                    }
                    else
                    {
                        if (focusStar == null)
                        {
                            focusPlanet = null;
                            FlyStatus = true;
                            focusStar = GameMain.galaxy.StarById(indicatorAstroId / 100);
                            guidetype = ESpaceGuideType.Star;
                        }
                        else if (focusStar.id != indicatorAstroId / 100)
                        {
                            FlyStatus = true;
                            focusStar = GameMain.galaxy.StarById(indicatorAstroId / 100);
                            guidetype = ESpaceGuideType.Star;
                        }
                    }
                }
                else if (indicatorAstroId > 1000000)
                {
                    if (focusPlanet != null || focusStar != null)
                    {
                        focusStar = null;
                        focusPlanet = null;
                    }
                }
                else if (indicatorAstroId == 0)
                {
                    if (focusPlanet != null || focusStar != null)
                        ClearFollow();
                }
            }
            if (autonavigation_bool.Value && FlyStatus && NeedNavigation)
            {
                if (indicatorAstroId <= 1000000 && indicatorAstroId == LocalPlanet?.id)
                {
                    FlyStatus = false;
                    return;
                }
                else if (indicatorAstroId > 1000000 && !AutoNavigateToDarkFogHive.Value)
                {
                    FlyStatus = false;
                    return;
                }
                if (!player.sailing)
                {
                    FlyAwayPlanet();
                    return;
                }

                VectorLF3 uPosition;
                if (focusPlanet != null)
                {
                    uPosition = focusPlanet.uPosition;
                }
                else if (focusStar != null)
                {
                    uPosition = focusStar.uPosition;
                }
                else if (indicatorAstroId > 1000000)
                {
                    uPosition = GameMain.data.spaceSector.astros[player.navigation.indicatorAstroId - 1000000].uPos;
                    guidetype = ESpaceGuideType.DFHive;
                }
                else if (indicatorEnemyId != 0)
                {
                    ref EnemyData ptr = ref GameMain.data.spaceSector.enemyPool[indicatorEnemyId];
                    GameMain.data.spaceSector.TransformFromAstro_ref(ptr.astroId, out uPosition, ref ptr.pos);
                    guidetype = ESpaceGuideType.DFCarrier;
                }
                else if (indicatorMsgId != 0)
                {
                    if (indicatorMsgId > CosmicMessageProto.maxProtoId)
                    {
                        guidetype = ESpaceGuideType.DFCommunicator;
                    }
                    else
                    {
                        guidetype = ESpaceGuideType.CosmicMessage;
                    }
                    uPosition = GameMain.gameScenario.cosmicMessageManager.messages[player.navigation.indicatorMsgId].uPosition;
                }
                else
                {
                    return;
                }

                double distance = (player.uPosition - uPosition).magnitude;
                bool canWarp = distance > autowarpdistance.Value * 2_400_000 && CanWarp && (mecha.coreEnergy * 100 / mecha.coreEnergyCap) >= autowarpdistanceEnergyPercent.Value;
                bool needstop = false;
                if (distance < 8000)
                {
                    needstop = true;
                }
                else if (distance < 100_000 && guidetype == ESpaceGuideType.Star)
                {
                    needstop = true;
                }
                else if (distance < AutoNavigateToDarkFogHiveKeepDistance.Value * 100 + 1000 && guidetype == ESpaceGuideType.DFHive)
                {
                    needstop = true;
                }

                if (player.warping && __instance.actionSail.currentWarpSpeed > 15000 && distance < AuDis * 70)
                {
                    if (distance > AuDis * 26)
                    {
                        if (__instance.actionSail.currentWarpSpeed > AuDis * 120)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.008;
                        }
                    }
                    else if (distance > AuDis * 20)
                    {
                        if (__instance.actionSail.currentWarpSpeed > AuDis * 80)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.008;
                        }
                    }
                    else if (distance > AuDis * 15)
                    {
                        if (__instance.actionSail.currentWarpSpeed > AuDis * 40)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.008;
                        }
                        if (__instance.actionSail.currentWarpSpeed > AuDis * 80)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.016;
                        }
                    }
                    else if (distance > AuDis * 6)
                    {
                        if (__instance.actionSail.currentWarpSpeed > AuDis * 20)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.008;
                        }
                        if (__instance.actionSail.currentWarpSpeed > AuDis * 40)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.016;
                        }
                    }
                    else if (distance > AuDis)
                    {
                        if (__instance.actionSail.currentWarpSpeed > AuDis)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.008;
                        }
                        if (__instance.actionSail.currentWarpSpeed > AuDis * 20)
                        {
                            __instance.actionSail.warpSpeedControl -= 0.016;
                        }
                    }
                }

                if (guidetype == ESpaceGuideType.DFHive)
                {
                    var vector = default(Vector3);
                    GameMain.spaceSector.astros[indicatorAstroId - 1000000].VelocityU(ref vector, out Vector3 followObjectVel);
                    FollowTaget(uPosition, followObjectVel, false, true);
                }
                else if (distance > 100_000)
                {
                    FlyTo(uPosition);
                }
                else if (distance > 5000)
                {
                    if (guidetype == ESpaceGuideType.Star)
                    {
                        FlyStatus = false;
                    }
                    FlyTo(uPosition);
                }
                else
                {
                    if (indicatorEnemyId != 0)
                    {
                        FollowTaget(uPosition, GameMain.data.spaceSector.enemyPool[indicatorEnemyId].vel, false);
                    }
                    else if (focusPlanet != null)
                    {
                        var radius = focusPlanet.radius;
                        var factoryLoaded = focusPlanet.factoryLoaded;
                        if (distance < radius + 500 && factoryLoaded)
                        {
                            FlyStatus = false;
                        }
                        else if (distance < radius + 1000 && !factoryLoaded)
                        {
                            var directVector = (uPosition - player.uPosition).normalized;
                            if (Vector3.Angle(player.uVelocity.normalized, directVector) < 10)
                            {
                                player.uVelocity = directVector * (distance - radius);
                            }
                            else if (player.uVelocity.magnitude < 1000)
                            {
                                player.uVelocity += directVector * (max_acc);
                                __instance.actionSail.UseSailEnergy(max_acc);
                            }
                        }
                        else
                        {
                            FlyTo(uPosition);
                        }
                    }
                    else if (guidetype == ESpaceGuideType.DFCommunicator)
                    {
                        FollowTaget(uPosition, new VectorLF3(), true);
                    }
                }
                if (player.warpCommand && needstop)
                {
                    player.warpCommand = false;
                }
                if (canWarp && !needstop && mecha.UseWarper() && distance > 10000)
                {
                    player.warpCommand = true;
                }

            }
            #endregion
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControlGizmo), "AddOrderGizmo")]
        public static bool DisableAddOrderGizmo()
        {
            if (StartAutoMovetounbuilt)
            {
                return false;
            }
            return true;
        }

        private static void FollowTaget(VectorLF3 target, VectorLF3 targetVel, bool targetIsStatic, bool isOrbit = false)
        {
            double distance = (player.uPosition - target).magnitude;
            if (targetIsStatic)
            {
                var playertotagetnormalized = (target - player.uPosition).normalized;
                var playerVelMagnitude = player.uVelocity.magnitude;
                var targetVelnormalized = player.uVelocity.normalized;
                float dot = Vector3.Dot(player.uVelocity, playertotagetnormalized);
                var directVector = distance < 300 || dot < 0 ? targetVelnormalized : playertotagetnormalized;
                int NumberSign = dot >= 0 ? 1 : -1;
                if (distance > 6000)
                {
                    FlyTo(target);
                }
                else if (distance > 3000)
                {
                    var speeddis = 1000 * NumberSign - playerVelMagnitude;
                    var acc = Math.Min(Math.Abs(speeddis), 10);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else if (distance > 1500)
                {
                    var speeddis = 300 * NumberSign - playerVelMagnitude;
                    var acc = Math.Min(Math.Abs(speeddis), 10);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else if (distance > 300)
                {
                    var speeddis = 200 * NumberSign - playerVelMagnitude;
                    var acc = Math.Min(Math.Abs(speeddis), 5);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else if (distance > 5)
                {
                    var speeddis = distance * NumberSign - playerVelMagnitude;
                    var acc = Math.Min(Math.Abs(speeddis), 5);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                player.uVelocity = directVector * player.uVelocity.magnitude;
            }
            else if (!isOrbit)
            {
                var targetVelnormalized = targetVel.normalized;
                var playertotagetnormalized = (target - player.uPosition).normalized;
                float dot = Vector3.Dot(targetVelnormalized, playertotagetnormalized);
                var directVector = distance < 300 || dot < 0 ? targetVelnormalized : playertotagetnormalized;
                int NumberSign = dot >= 0 ? 1 : -1;

                var enemyVelMagnitude = targetVel.magnitude;
                var playerVelMagnitude = player.uVelocity.magnitude;
                if (distance > 5000)
                {
                    FlyTo(target);
                }
                else if (distance > 2000)
                {
                    var speeddis = enemyVelMagnitude - playerVelMagnitude + 800 * NumberSign;
                    var acc = Math.Min(Math.Abs(speeddis), 5);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else if (distance > 800)
                {
                    var speeddis = enemyVelMagnitude - playerVelMagnitude + 500 * NumberSign;
                    var acc = Math.Min(Math.Abs(speeddis), 5);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else if (distance > 400)
                {
                    var speeddis = enemyVelMagnitude - playerVelMagnitude + 300 * NumberSign;
                    var acc = Math.Min(Math.Abs(speeddis), 5);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else if (distance > 200)
                {
                    var speeddis = enemyVelMagnitude - playerVelMagnitude + 50 * NumberSign;
                    var acc = Math.Min(Math.Abs(speeddis), 5);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else
                {
                    var speeddis = enemyVelMagnitude - playerVelMagnitude;
                    var acc = Math.Min(Math.Abs(speeddis), 1);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += directVector * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                player.uVelocity = directVector * player.uVelocity.magnitude;
            }
            else
            {
                var targetVelnormalized = targetVel.normalized;
                var playertotagetnormalized = (target - player.uPosition).normalized;
                VectorLF3 directVector = Vector3.Lerp(GameMain.mainPlayer.uVelocity.normalized, targetVelnormalized, 0.1f);

                var enemyVelMagnitude = targetVel.magnitude;
                var playerVelMagnitude = player.uVelocity.magnitude;
                var keepdistance = AutoNavigateToDarkFogHiveKeepDistance.Value * 100;
                if (distance > keepdistance + 1000)
                {
                    FlyTo(target);
                    return;
                }
                else if (distance >= keepdistance + 200)
                {
                    directVector = Vector3.Lerp(GameMain.mainPlayer.uVelocity.normalized, playertotagetnormalized, 0.1F);
                    if (enemyVelMagnitude >= playerVelMagnitude)
                    {
                        player.uVelocity += directVector * 3;
                        player.controller.actionSail.UseSailEnergy(3);
                    }
                    else if (playerVelMagnitude - 5 > enemyVelMagnitude)
                    {
                        player.uVelocity -= directVector * 3;
                        player.controller.actionSail.UseSailEnergy(3);
                    }
                }
                else if (distance > keepdistance - 50 && distance < keepdistance + 200)
                {
                    var speeddis = enemyVelMagnitude - playerVelMagnitude;
                    var acc = Math.Min(Math.Abs(speeddis), 3);
                    acc = speeddis > 0 ? acc : -acc;
                    player.uVelocity += player.uVelocity.normalized * acc;
                    player.controller.actionSail.UseSailEnergy(acc);
                }
                else if (distance < keepdistance - 50)
                {
                    bool needAddVel = autoAddPlayerVel.Value;
                    directVector = Vector3.Lerp(GameMain.mainPlayer.uVelocity.normalized, -playertotagetnormalized, 0.1F);
                    if (needAddVel && playerVelMagnitude < enemyVelMagnitude + 150)
                    {
                        player.uVelocity += directVector * 3;
                        player.controller.actionSail.UseSailEnergy(3);
                    }
                }
                else
                {
                    return;
                }
                player.uVelocity = directVector * player.uVelocity.magnitude;
            }
        }

        private static void FlyTo(VectorLF3 uPosition)
        {
            VectorLF3 direction = (uPosition - player.uPosition).normalized;

            bool needAddVel = autoAddPlayerVel.Value;

            if (GameMain.localStar != null)
            {
                PlanetData nearestPlanet = LocalPlanet;
                double num = double.MaxValue;
                if (nearestPlanet == null)
                {
                    foreach (var planet in GameMain.localStar.planets)
                    {
                        if (planet == null) continue;
                        double magnitude = (player.uPosition - planet.uPosition).magnitude;
                        if (magnitude < num)
                        {
                            nearestPlanet = planet;
                            num = magnitude;
                        }
                    }
                }
                if (nearestPlanet != null && nearestPlanet != focusPlanet)
                {
                    if (num < 2000)
                    {
                        float dot = Vector3.Dot((player.uPosition - nearestPlanet.uPosition).normalized, direction);
                        if (dot >= 0)
                        {
                            direction = (player.uPosition - nearestPlanet.uPosition).normalized;
                        }
                        else
                        {
                            var NormalVector = Vector3.Cross(direction, (player.uPosition - nearestPlanet.uPosition).normalized);
                            direction = -Vector3.Cross(NormalVector, (player.uPosition - nearestPlanet.uPosition).normalized).normalized;
                        }
                    }
                }
            }
            direction = Vector3.Lerp(player.uVelocity.normalized, direction, 1);

            if (player.uVelocity.magnitude + 5 >= maxSailSpeed)
            {
                player.uVelocity = direction * maxSailSpeed;
            }
            else
            {
                if (needAddVel)
                {
                    player.uVelocity += direction * 5;
                    player.controller.actionSail.UseSailEnergy(5);
                }
                player.uVelocity = direction * player.uVelocity.magnitude;
            }
        }

        private static void FlyAwayPlanet()
        {
            PlayerController controller = player.controller;
            controller.input0.z = 1;
            controller.input1.y += 1;
            if (controller.actionFly.currentAltitude > 49 && controller.horzSpeed < 12.5)
            {
                controller.velocity = (player.uPosition - LocalPlanet.uPosition).normalized * t++;
            }
        }


    }

    public static class SunLightPatch
    {
        private static Harmony _patch;
        private static bool enable;
        public static bool Enable
        {
            get => enable;
            set
            {
                if (enable == value) return;
                enable = value;
                if (enable)
                {
                    _patch = Harmony.CreateAndPatchAll(typeof(SunLightPatch));
                }
                else
                {
                    _patch.UnpatchSelf();
                }
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(PostEffectController), "Update")]
        public static void PostEffectControllerUpdatePatch(PostEffectController __instance)
        {
            if (!SunLightOpen.Value || __instance.sunShaft == null || GameMain.localStar == null || GameMain.localPlanet == null || SunLight == null || FactoryModel.whiteMode0)
            {
                return;
            }
            float magnitude = GameCamera.main.transform.localPosition.magnitude;
            if (GameMain.universeSimulator != null && GameMain.localStar.type != EStarType.BlackHole)
            {
                StarSimulator starSimulator = GameMain.universeSimulator.LocalStarSimulator();
                if (starSimulator != null)
                {
                    __instance.sunShaft.sunTransform = SunLight.transform;
                    __instance.sunShaft.sunColor = GameMain.universeSimulator.sunshaftColor.Evaluate(starSimulator.sunColorParam);
                    SunLight.shadowBias = Mathf.Clamp01((magnitude - 300f) / 700f) * 0.5f + 0.045f;
                }
            }
            __instance.sunShaft.enabled = (__instance.sunShaft.sunTransform != null);
            __instance.sunShaft.sunShaftIntensity = 0.25f + Mathf.Clamp01((300f - GameMain.mainPlayer.position.magnitude + GameMain.localPlanet.realRadius) / 200f) * 1.25f;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StarSimulator), "LateUpdate")]
        public static bool StarSimulatorLateUpdate(ref StarSimulator __instance, Material ___bodyMaterial, Material ___haloMaterial)
        {
            if (!SunLightOpen.Value || FactoryModel.whiteMode0 || GameMain.localStar != __instance.starData)
            {
                return true;
            }
            __instance.sunLight.enabled = false;
            Shader.SetGlobalVector("_Global_SunDir", GameMain.mainPlayer.transform.up);
            Shader.SetGlobalColor("_Global_SunsetColor0", Color.Lerp(Color.white, __instance.sunsetColor0, __instance.useSunsetColor));
            Shader.SetGlobalColor("_Global_SunsetColor1", Color.Lerp(Color.white, __instance.sunsetColor1, __instance.useSunsetColor));
            Shader.SetGlobalColor("_Global_SunsetColor2", Color.Lerp(Color.white, __instance.sunsetColor2, __instance.useSunsetColor));
            ___bodyMaterial.renderQueue = 2981;
            ___haloMaterial.renderQueue = 2981;
            __instance.blackRenderer.enabled = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetSimulator), "LateUpdate")]
        public static bool PlanetSimulatorLateUpdatePatch(PlanetSimulator __instance)
        {
            PlanetData localPlanet = GameMain.localPlanet;
            if (!GameDataImported || !SunLightOpen.Value || FactoryModel.whiteMode0 || localPlanet == null || localPlanet != __instance.planetData || SunLight == null)
            {
                return true;
            }
            Vector3 vector3 = GameMain.mainPlayer.transform.up;
            if (__instance.surfaceRenderer?.Length != 0)
                __instance.surfaceRenderer[0]?.sharedMaterial?.SetVector("_SunDir", vector3);
            __instance.reformMat0?.SetVector("_SunDir", vector3);
            __instance.reformMat1?.SetVector("_SunDir", vector3);
            __instance.atmoMat?.SetVector("_SunDir", vector3);
            __instance.atmoMatLate?.SetVector("_SunDir", vector3);
            __instance.cloudSimulator?.OnLateUpdate();
            if (__instance.oceanMat != null)
            {
                if (__instance.oceanRenderQueue > 2987)
                {
                    __instance.oceanMat.renderQueue = ((localPlanet == __instance.planetData) ? __instance.oceanRenderQueue : 2988);
                }
                __instance.oceanMat.SetColor("_Planet_WaterAmbientColor0", __instance.planetData.ambientDesc.waterAmbientColor0);
                __instance.oceanMat.SetColor("_Planet_WaterAmbientColor1", __instance.planetData.ambientDesc.waterAmbientColor1);
                __instance.oceanMat.SetColor("_Planet_WaterAmbientColor2", __instance.planetData.ambientDesc.waterAmbientColor2);
            }
            if (__instance.atmoTrans0 != null && __instance.planetData != null && !__instance.planetData.loading && !__instance.planetData.factoryLoading)
            {
                StarSimulator star = GameMain.universeSimulator.FindStarSimulator(__instance.planetData.star); ;
                __instance.atmoTrans0.rotation = Camera.main.transform.localRotation;
                Vector4 value = (GameCamera.generalTarget == null) ? Vector3.zero : GameCamera.generalTarget.position;
                Vector3 position = GameCamera.main.transform.position;
                if (value.sqrMagnitude == 0f)
                {
                    if (GameCamera.instance.isPlanetMode)
                    {
                        value = (position + GameCamera.main.transform.forward * 30f).normalized * __instance.planetData.realRadius;
                    }
                    else
                    {
                        value = GameMain.mainPlayer.position;
                    }
                }
                Vector3 lhs = Camera.main.transform.localPosition - __instance.transform.localPosition;
                float magnitude = lhs.magnitude;
                VectorLF3 vectorLF = __instance.planetData.uPosition;
                if (localPlanet != null)
                {
                    vectorLF = VectorLF3.zero;
                }
                else if (GameMain.mainPlayer != null)
                {
                    vectorLF -= GameMain.mainPlayer.uPosition;
                }
                float d = 1f;
                Vector3 vector;
                UniverseSimulator.VirtualMapping(vectorLF.x, vectorLF.y, vectorLF.z, position, out vector, out d);
                float num = Vector3.Dot(lhs, Camera.main.transform.forward);
                __instance.atmoTrans1.localPosition = new Vector3(0f, 0f, Mathf.Clamp(num + 10f, 0f, 320f));
                float value2 = Mathf.Clamp01(8000f / magnitude);
                float num2 = Mathf.Clamp01(4000f / magnitude);
                float value3 = Mathf.Max(0f, magnitude / 6000f - 1f);
                Vector4 vector2 = __instance.atmoMatRadiusParam;
                vector2.z = vector2.x + (__instance.atmoMatRadiusParam.z - __instance.atmoMatRadiusParam.x) * (2.7f - num2 * 1.7f);
                vector2 *= d;
                __instance.atmoMat.SetVector("_PlanetPos", __instance.transform.localPosition);
                __instance.atmoMat.SetVector("_SunDir", vector3);
                __instance.atmoMat.SetVector("_PlanetRadius", vector2);
                __instance.atmoMat.SetColor("_Color4", star.sunAtmosColor);
                __instance.atmoMat.SetColor("_Sky4", star.sunriseAtmosColor);
                __instance.atmoMat.SetVector("_LocalPos", value);
                __instance.atmoMat.SetFloat("_SunRiseScatterPower", Mathf.Max(60f, (magnitude - __instance.planetData.realRadius * 2f) * 0.18f));
                __instance.atmoMat.SetFloat("_IntensityControl", value2);
                __instance.atmoMat.SetFloat("_DistanceControl", value3);
                __instance.atmoMat.renderQueue = ((__instance.planetData == localPlanet) ? 2991 : 2989);
                __instance.atmoMatLate.SetVector("_PlanetPos", __instance.transform.localPosition);
                __instance.atmoMatLate.SetVector("_SunDir", vector3);
                __instance.atmoMatLate.SetVector("_PlanetRadius", vector2);
                __instance.atmoMatLate.SetColor("_Color4", star.sunAtmosColor);
                __instance.atmoMatLate.SetColor("_Sky4", star.sunriseAtmosColor);
                __instance.atmoMatLate.SetVector("_LocalPos", value);
                __instance.atmoMatLate.SetFloat("_SunRiseScatterPower", Mathf.Max(60f, (magnitude - __instance.planetData.realRadius * 2f) * 0.18f));
                __instance.atmoMatLate.SetFloat("_IntensityControl", value2);
                __instance.atmoMatLate.SetFloat("_DistanceControl", value3);
                if (__instance.planetData == localPlanet)
                {
                    __instance.atmoMatLate.renderQueue = 3200;
                    __instance.atmoMatLate.SetInt("_StencilRef", 2);
                    __instance.atmoMatLate.SetInt("_StencilComp", 3);
                }
                else
                {
                    __instance.atmoMatLate.renderQueue = 2989;
                    __instance.atmoMatLate.SetInt("_StencilRef", 0);
                    __instance.atmoMatLate.SetInt("_StencilComp", 1);
                }
            }
            if (PerformanceMonitor.GpuProfilerOn)
            {
                if (__instance.surfaceRenderer != null)
                {
                    foreach (Renderer renderer in __instance.surfaceRenderer)
                    {
                        if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                        {
                            PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.Universe, 1, renderer.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                        }
                    }
                }
                if (__instance.reformRenderer != null && __instance.reformRenderer.enabled && __instance.reformRenderer.gameObject.activeInHierarchy)
                {
                    PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.Universe, 1, __instance.reformRenderer.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                }
                if (__instance.oceanCollider != null && __instance.oceanCollider.gameObject.activeInHierarchy)
                {
                    MeshFilter component = __instance.oceanCollider.GetComponent<MeshFilter>();
                    if (component != null)
                    {
                        PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.Universe, 1, component.sharedMesh.vertexCount);
                    }
                }
            }
            return false;
        }

    }

}
