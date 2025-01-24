using Auxilaryfunction.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using static Auxilaryfunction.Auxilaryfunction;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Auxilaryfunction
{
    public class AuxilaryfunctionPatch
    {
        public static float originUnlockVolumn;
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
        [HarmonyPatch(typeof(UIBlueprintInspector), "Refresh")]
        public static void UIBlueprintInspectorRefresh(bool refreshComponent)
        {
            BluePrintBatchModifyBuild.NeedRefresh = refreshComponent;
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
        [HarmonyPatch(typeof(UIGeneralTips), "OnTechUnlocked")]
        public static bool UIGeneralTipsOnTechUnlocked()
        {
            if (CloseUIGeneralTip.Value)
            {
                return false;
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



}
