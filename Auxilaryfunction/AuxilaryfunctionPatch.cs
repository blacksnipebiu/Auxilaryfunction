using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Events;
using static Auxilaryfunction.Auxilaryfunction;
using System.Threading;

namespace Auxilaryfunction
{
    public static class AuxilaryfunctionPatch
    {
        [HarmonyPatch(typeof(FactorySystem), "NewMinerComponent")]
        class NewMinerComponentPatch
        {
            public static void Postfix(ref int __result, FactorySystem __instance)
            {
                if (auto_supply_station.Value && __instance.factory.entityPool[__instance.minerPool[__result].entityId].protoId == 2316)
                {
                    __instance.minerPool[__result].speed = veincollectorspeed.Value * 1000;
                }
            }
        }
        [HarmonyPatch(typeof(FactorySystem), "NewEjectorComponent")]
        class NewEjectorComponentPatch
        {
            public static void Postfix(ref int __result, FactorySystem __instance)
            {
                if (EjectorDictionary.ContainsKey(__instance.planet.id))
                    EjectorDictionary[__instance.planet.id].Add(__result);
                else
                {
                    List<int> temp = new List<int>();
                    temp.Add(__result);
                    EjectorDictionary[__instance.planet.id] = temp;
                }

                __instance.ejectorPool[__result].SetOrbit(1);
            }
        }
        [HarmonyPatch(typeof(FactorySystem), "RemoveEjectorComponent")]
        class RemoveEjectorComponentPatch
        {
            public static void Prefix(int id, FactorySystem __instance)
            {
                if (EjectorDictionary[__instance.planet.id].Contains(id))
                    EjectorDictionary[__instance.planet.id].Remove(id);
            }
        }
        [HarmonyPatch(typeof(GameData), "GameTick")]
        class GameTick1Patch
        {
            public static bool Prefix(ref long time, GameData __instance)
            {
                if (!stopDysonSphere && !stopfactory)
                    return true;
                PerformanceMonitor.BeginSample(ECpuWorkEntry.Statistics);
                if (!DSPGame.IsMenuDemo)
                {
                    __instance.statistics.PrepareTick();
                    __instance.history.PrepareTick();
                }
                __instance.mainPlayer.packageUtility.Count();
                PerformanceMonitor.EndSample(ECpuWorkEntry.Statistics);
                if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
                {
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalPhysics);
                    __instance.localPlanet.factory.cargoTraffic.ClearStates();
                    __instance.localPlanet.physics.GameTick();
                    PerformanceMonitor.EndSample(ECpuWorkEntry.LocalPhysics);
                }
                PerformanceMonitor.BeginSample(ECpuWorkEntry.Scenario);
                if (__instance.guideMission != null)
                {
                    __instance.guideMission.GameTick();
                }
                PerformanceMonitor.EndSample(ECpuWorkEntry.Scenario);
                PerformanceMonitor.BeginSample(ECpuWorkEntry.Player);
                if (__instance.mainPlayer != null)
                    __instance.mainPlayer.GameTick(time);
                __instance.DetermineRelative();
                PerformanceMonitor.EndSample(ECpuWorkEntry.Player);
                PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonSphere);
                if (!stopDysonSphere)
                {
                    for (int i = 0; i < __instance.dysonSpheres.Length; i++)
                    {
                        if (__instance.dysonSpheres[i] != null)
                        {
                            __instance.dysonSpheres[i].BeforeGameTick(time);
                        }
                    }
                }
                PerformanceMonitor.EndSample(ECpuWorkEntry.DysonSphere);

                if (!stopfactory)
                {
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.Factory);
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.PowerSystem);
                    for (int j = 0; j < __instance.factoryCount; j++)
                    {
                        Assert.NotNull(__instance.factories[j]);
                        if (__instance.factories[j] != null)
                        {
                            __instance.factories[j].BeforeGameTick(time);
                        }
                    }
                    PerformanceMonitor.EndSample(ECpuWorkEntry.PowerSystem);
                    if (time == 1L)
                    {
                        Debug.Log("check point before multithread");
                    }
                    if (GameMain.multithreadSystem.multithreadSystemEnable)
                    {
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.PowerSystem);
                        GameMain.multithreadSystem.PrepareBeforePowerFactoryData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.PowerSystem);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.PowerSystem);
                        GameMain.multithreadSystem.PreparePowerSystemFactoryData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time, GameMain.mainPlayer);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.PowerSystem);
                        for (int k = 0; k < __instance.factoryCount; k++)
                        {
                            if (__instance.factories[k].factorySystem != null)
                            {
                                __instance.factories[k].factorySystem.CheckBeforeGameTick();
                            }
                        }
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Facility);
                        GameMain.multithreadSystem.PrepareAssemblerFactoryData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Lab);
                        for (int l = 0; l < __instance.factoryCount; l++)
                        {
                            bool isActive = GameMain.localPlanet == __instance.factories[l].planet;
                            if (__instance.factories[l].factorySystem != null)
                            {
                                __instance.factories[l].factorySystem.GameTickLabResearchMode(time, isActive);
                            }
                        }
                        GameMain.multithreadSystem.PrepareLabOutput2NextData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Lab);
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Facility);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Transport);
                        GameMain.multithreadSystem.PrepareTransportData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Transport);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Storage);
                        for (int m = 0; m < __instance.factoryCount; m++)
                        {
                            if (__instance.factories[m].transport != null)
                            {
                                __instance.factories[m].transport.GameTick_InputFromBelt(time);
                            }
                        }
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Storage);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Inserter);
                        GameMain.multithreadSystem.PrepareInserterData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Inserter);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Storage);
                        for (int n = 0; n < __instance.factoryCount; n++)
                        {
                            bool isActive2 = GameMain.localPlanet == __instance.factories[n].planet;
                            if (__instance.factories[n].factoryStorage != null)
                            {
                                __instance.factories[n].factoryStorage.GameTick(time, isActive2);
                            }
                        }
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Storage);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Belt);
                        GameMain.multithreadSystem.PrepareCargoPathsData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Belt);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Splitter);
                        for (int num = 0; num < __instance.factoryCount; num++)
                        {
                            if (__instance.factories[num].cargoTraffic != null)
                            {
                                __instance.factories[num].cargoTraffic.SplitterGameTick();
                            }
                        }
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Splitter);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Belt);
                        for (int num2 = 0; num2 < __instance.factoryCount; num2++)
                        {
                            if (__instance.factories[num2].cargoTraffic != null)
                            {
                                __instance.factories[num2].cargoTraffic.MonitorGameTick();
                                __instance.factories[num2].cargoTraffic.SpraycoaterGameTick();
                                __instance.factories[num2].cargoTraffic.PilerGameTick();
                            }
                        }
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Belt);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Storage);
                        int stationPilerLevel = GameMain.history.stationPilerLevel;
                        for (int num3 = 0; num3 < __instance.factoryCount; num3++)
                        {
                            if (__instance.factories[num3].transport != null)
                            {
                                __instance.factories[num3].transport.GameTick_OutputToBelt(stationPilerLevel, time);
                                __instance.factories[num3].transport.GameTick_SandboxMode();
                            }
                        }
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Storage);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalCargo);
                        GameMain.multithreadSystem.PreparePresentCargoPathsData(GameMain.localPlanet, __instance.factories, __instance.factoryCount, time);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.LocalCargo);
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.Digital);
                        for (int num4 = 0; num4 < __instance.factoryCount; num4++)
                        {
                            bool isActive3 = GameMain.localPlanet == __instance.factories[num4].planet;
                            if (__instance.factories[num4].digitalSystem != null)
                            {
                                __instance.factories[num4].digitalSystem.GameTick(isActive3);
                            }
                        }
                        PerformanceMonitor.EndSample(ECpuWorkEntry.Digital);
                    }
                    else
                    {
                        for (int num5 = 0; num5 < __instance.factoryCount; num5++)
                        {
                            __instance.factories[num5].GameTick(time);
                        }
                    }
                    if (time == 1L)
                    {
                        Debug.Log("check point after multithread");
                    }
                    PerformanceMonitor.EndSample(ECpuWorkEntry.Factory);
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.Trash);
                    __instance.trashSystem.GameTick(time);
                    PerformanceMonitor.EndSample(ECpuWorkEntry.Trash);
                }

                if (!stopDysonSphere)
                {
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonSphere);
                    if (GameMain.multithreadSystem.multithreadSystemEnable)
                    {
                        for (int num6 = 0; num6 < __instance.dysonSpheres.Length; num6++)
                        {
                            if (__instance.dysonSpheres[num6] != null)
                            {
                                __instance.dysonSpheres[num6].GameTick(time);
                            }
                        }
                        PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonRocket);
                        GameMain.multithreadSystem.PrepareRocketFactoryData(__instance.dysonSpheres, __instance.dysonSpheres.Length);
                        GameMain.multithreadSystem.Schedule();
                        GameMain.multithreadSystem.Complete();
                        PerformanceMonitor.EndSample(ECpuWorkEntry.DysonRocket);
                    }
                    else
                    {
                        for (int num7 = 0; num7 < __instance.dysonSpheres.Length; num7++)
                        {
                            if (__instance.dysonSpheres[num7] != null)
                            {
                                __instance.dysonSpheres[num7].GameTick(time);
                                PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonRocket);
                                __instance.dysonSpheres[num7].RocketGameTick();
                                PerformanceMonitor.EndSample(ECpuWorkEntry.DysonRocket);
                            }
                        }
                    }
                    PerformanceMonitor.EndSample(ECpuWorkEntry.DysonSphere);
                }

                if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
                {
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalAudio);
                    __instance.localPlanet.audio.GameTick();
                    PerformanceMonitor.EndSample(ECpuWorkEntry.LocalAudio);
                }

                if (!stopfactory)
                {
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.Statistics);
                    if (!DSPGame.IsMenuDemo)
                    {
                        __instance.statistics.GameTick(time);
                    }
                    PerformanceMonitor.EndSample(ECpuWorkEntry.Statistics);
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.Digital);
                    if (!DSPGame.IsMenuDemo)
                    {
                        __instance.warningSystem.GameTick(time);
                    }
                    PerformanceMonitor.EndSample(ECpuWorkEntry.Digital);
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.Scenario);
                    __instance.milestoneSystem.GameTick(time);
                    PerformanceMonitor.EndSample(ECpuWorkEntry.Scenario);
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.Statistics);
                    __instance.history.AfterTick();
                    __instance.statistics.AfterTick();
                    PerformanceMonitor.EndSample(ECpuWorkEntry.Statistics);
                }
                __instance.preferences.Collect();
                return false;
            }
        }

        [HarmonyPatch(typeof(PlanetTransport), "NewStationComponent")]
        class NewStationComponentPatch
        {
            public static void Postfix(ref StationComponent __result, PlanetTransport __instance)
            {
                if (auto_supply_station.Value && !__result.isCollector && !__result.isVeinCollector)
                {
                    int inc;
                    __result.idleDroneCount = GameMain.mainPlayer.package.TakeItem(5001, __result.isStellar ? auto_supply_drone.Value : (auto_supply_drone.Value > 50 ? 50 : auto_supply_drone.Value), out inc);
                    __result.tripRangeDrones = Math.Cos(stationdronedist.Value * Math.PI / 180);
                    __instance.planet.factory.powerSystem.consumerPool[__result.pcId].workEnergyPerTick = (long)stationmaxpowerpertick.Value * 16667;
                    if (stationmaxpowerpertick.Value > 60 && !__result.isStellar)
                    {
                        __instance.planet.factory.powerSystem.consumerPool[__result.pcId].workEnergyPerTick = (long)60 * 16667;
                    }
                    __result.deliveryDrones = (int)(DroneStartCarry.Value * 10) * 10;
                    if (__result.isStellar)
                    {
                        __result.warperCount = GameMain.mainPlayer.package.TakeItem(1210, auto_supply_warp.Value, out inc);
                        __result.warpEnableDist = stationwarpdist.Value * AU;
                        __result.deliveryShips = (int)(ShipStartCarry.Value * 10) * 10;
                        __result.idleShipCount = GameMain.mainPlayer.package.TakeItem(5002, auto_supply_ship.Value, out inc);
                        __result.tripRangeShips = stationshipdist.Value > 60 ? 24000000000 : stationshipdist.Value * 2400000;
                        if (GameMain.data.history.TechUnlocked(3404)) __result.warperCount = GameMain.mainPlayer.package.TakeItem(1210, auto_supply_warp.Value, out inc);
                    }
                }
                if (auto_supply_station.Value && __result.isVeinCollector)
                {
                    __instance.factory.factorySystem.minerPool[__result.minerId].speed = veincollectorspeed.Value * 1000;
                }
            }
        }

        [HarmonyPatch(typeof(FactoryModel), "Update")]
        class FactoryModelUpdatePatch
        {
            public static bool Prefix()
            {
                return !norender_entity_bool.Value && !simulatorrender;
            }
        }
        [HarmonyPatch(typeof(FactoryModel), "LateUpdate")]
        class FactoryModelLateUpdatePatch
        {
            public static bool Prefix()
            {
                return !norender_entity_bool.Value && !simulatorrender;
            }
        }
        //[HarmonyPatch(typeof(FactoryModel), "DrawInstancedBatches")]
        //class DrawInstancedBatchesPatch
        //{
        //    public static bool Prefix()
        //    {
        //        return !norender_entity_bool.Value;
        //    }
        //}
        [HarmonyPatch(typeof(LogisticDroneRenderer), "Draw")]
        class DroneDrawPatch
        {
            public static bool Prefix()
            {
                return !norender_shipdrone_bool.Value && !simulatorrender;
            }
        }
        [HarmonyPatch(typeof(LogisticShipUIRenderer), "Draw")]
        class LogisticShipUIRendererDrawPatch
        {
            public static bool Prefix()
            {
                return !norender_shipdrone_bool.Value && !simulatorrender;
            }
        }
        [HarmonyPatch(typeof(LogisticShipRenderer), "Draw")]
        class ShipDrawPatch
        {
            public static bool Prefix()
            {
                return !norender_shipdrone_bool.Value && !simulatorrender;
            }
        }
        [HarmonyPatch(typeof(LabRenderer), "Render")]
        class LabRendererPatch
        {
            public static bool Prefix(LabRenderer __instance)
            {
                if (__instance.modelId == 70)
                    return !norender_lab_bool.Value && !simulatorrender;
                return true;
            }
        }

        [HarmonyPatch(typeof(DysonSphere), "DrawModel")]
        class DysonDrawModelPatch
        {
            public static bool Prefix()
            {
                return !norender_dysonshell_bool.Value && !simulatorrender;
            }
        }
        [HarmonyPatch(typeof(DysonSphere), "DrawPost")]
        class DysonDrawPostPatch
        {
            public static bool Prefix()
            {
                return !norender_dysonswarm_bool.Value && !simulatorrender;
            }
        }
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawArea")]
        class UIPowerGizmoDrawAreaPatch
        {
            public static bool Prefix()
            {
                return !norender_powerdisk_bool.Value && !simulatorrender;
            }
        }
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawCover")]
        class UIPowerGizmoDrawCoverPatch
        {
            public static bool Prefix()
            {
                return !norender_powerdisk_bool.Value && !simulatorrender;
            }
        }

        [HarmonyPatch(typeof(CargoContainer), "Draw")]
        class PathRenderingBatchDrawPatch
        {
            public static bool Prefix()
            {
                return !norender_beltitem.Value;
            }
        }
        [HarmonyPatch(typeof(BuildingParameters), "CopyFromFactoryObject")]
        class CopyFromFactoryObjectPatch
        {
            public static void Prefix(int objectId, PlanetFactory factory)
            {
                if (stationcopyItem_bool.Value)
                {
                    if (GameMain.mainPlayer != null && GameMain.mainPlayer.controller != null && GameMain.mainPlayer.controller.actionBuild != null)
                    {
                        PlayerAction_Build build = GameMain.mainPlayer.controller.actionBuild;
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
        }

        [HarmonyPatch(typeof(BuildTool_Path), "DeterminePreviews")]
        class BuildTool_PathPatch
        {
            public static void Postfix(BuildTool_Path __instance)
            {
                if (!KeepBeltHeight.Value) return;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    PlanetAuxData planetAux = GameMain.mainPlayer.controller.actionBuild.planetAux;
                    if (planetAux == null) return;
                    if(__instance.altitude == 0)
                    {
                        if (ObjectIsBeltOrSplitter(__instance, __instance.castObjectId))
                        {
                            __instance.altitude = Altitude(__instance.castObjectPos, planetAux, __instance);
                        }
                        else if (__instance.startObjectId != 0)
                        {
                            __instance.altitude = Altitude(__instance.pathPoints[0], planetAux, __instance);
                        }
                    }
                    else if(Input.GetKey(KeyCode.LeftControl) && ObjectIsBeltOrSplitter(__instance, __instance.castObjectId))
                    {
                        __instance.altitude = Altitude(__instance.castObjectPos, planetAux, __instance);
                        if(__instance.altitude == 0)
                        {
                            __instance.altitude =1;
                        }
                    }
                }
            }
            public static int Altitude(Vector3 pos, PlanetAuxData aux, BuildTool_Path buildtoolpath)
            {
                Vector3 b = aux.Snap(pos, true);
                return (int)Math.Round((double)(Vector3.Distance(pos, b) / 1.3333333f));
            }
            public static bool ObjectIsBeltOrSplitter(BuildTool_Path tool, int objId)
            {
                if (objId == 0) return false;
                ItemProto itemProto = LDB.items.Select(tool.factory.entityPool[Math.Abs(objId)].protoId);
                return itemProto != null && itemProto.prefabDesc != null && (itemProto.prefabDesc.isBelt || itemProto.prefabDesc.isSplitter);
            }
        }

        [HarmonyPatch(typeof(BuildingParameters), "PasteToFactoryObject")]
        class PasteToFactoryObjectPatch
        {
            public static void Prefix(int objectId, PlanetFactory factory)
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
                                GameMain.mainPlayer.TryAddItemToPackage(sc.storage[i].itemId, sc.storage[i].count, 0, false);
                            factory.transport.SetStationStorage(stationId, i, stationcopyItem[i, 0], stationcopyItem[i, 1], (ELogisticStorage)stationcopyItem[i, 2]
                                , (ELogisticStorage)stationcopyItem[i, 3], GameMain.mainPlayer);
                        }
                        else
                            factory.transport.SetStationStorage(stationId, i, 0, 0, ELogisticStorage.None, ELogisticStorage.None, GameMain.mainPlayer);

                    }
                }
            }
        }

        //星图方向指引启动自动导航
        [HarmonyPatch(typeof(UIStarmap), "OnCursorFunction3Click")]
        class AutonavigationPatch
        {
            public static void Prefix(UIStarmap __instance)
            {
                if (__instance.focusPlanet != null && autonavigation_bool.Value)
                {
                    flyfocusPlanet = __instance.focusPlanet.planet;
                    fly = true;
                    slowdownsail = false;
                }
            }
        }
        [HarmonyPatch(typeof(UIGeneralTips), "OnTechUnlocked")]
        class OnTechUnlockedPatch
        {
            public static bool Prefix()
            {
                return !close_alltip_bool.Value;
            }
        }

        [HarmonyPatch(typeof(UIRandomTip), "_OnOpen")]
        class UIBuildMenu_OnOpenPatch
        {
            public static void Postfix(ref UIRandomTip __instance)
            {
                if (close_alltip_bool.Value)
                {
                    __instance._Close();
                }
            }
        }
        [HarmonyPatch(typeof(UITutorialTip), "Determine")]
        class UITutorialWindow_OnOpenPatch
        {
            public static void Postfix(UITutorialTip __instance)
            {
                if (close_alltip_bool.Value)
                {
                    __instance._Close();
                }
            }
        }

        [HarmonyPatch(typeof(PowerSystem), "NewGeneratorComponent")]
        class NewGeneratorComponentPatch
        {
            public static void Postfix(ref int __result, PowerSystem __instance)
            {
                if (__instance.genPool[__result].fuelMask == 4)
                {
                    if (autosetSomevalue_bool.Value)
                    {
                        int inc;
                        short fuelcount = (short)GameMain.mainPlayer.package.TakeItem(1803, auto_supply_starfuel.Value, out inc);
                        if (fuelcount > 0)
                        {
                            __instance.genPool[__result].SetNewFuel(1803, fuelcount, (short)inc);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(MilestoneSystem), "NotifyUnlockMilestone")]
        class UIMilestoneTipNotifyUnlockMilestonePatch
        {
            public static bool Prefix()
            {
                return !close_alltip_bool.Value;
            }
        }
        [HarmonyPatch(typeof(PlayerControlGizmo), "AddOrderGizmo")]
        class PlayerControlGizmoPatch
        {
            public static bool Prefix()
            {
                return !closecollider;
            }
        }
        //操纵人物
        [HarmonyPatch(typeof(PlayerController), "GetInput")]
        class PlayerOperation
        {
            public static float t = 20;
            public static void Postfix(PlayerController __instance)
            {
                if (GameMain.localPlanet != null)
                {
                    #region
                    if (automovetounbuilt.Value && closecollider)
                    {
                        if (autobuildThread == null)
                        {
                            autobuildThread = new Thread(delegate()
                            {
                                while (closecollider)
                                {
                                    try
                                    {
                                        float mindistance = 3000;
                                        int lasthasitempd = -1;
                                        foreach (PrebuildData pd in GameMain.localPlanet.factory.prebuildPool)
                                        {
                                            if (pd.id == 0 || pd.itemRequired > 0) continue;
                                            if (lasthasitempd == -1 || mindistance > (pd.pos - GameMain.mainPlayer.position).magnitude)
                                            {
                                                lasthasitempd = pd.id;
                                                mindistance = (pd.pos - GameMain.mainPlayer.position).magnitude;
                                            }
                                        }
                                        if (lasthasitempd == -1)
                                        {
                                            bool getitem = true;
                                            foreach (PrebuildData pd in GameMain.localPlanet.factory.prebuildPool)
                                            {
                                                if (pd.id == 0) continue;
                                                if (__instance.player.package.GetItemCount(pd.protoId) > 0)
                                                {
                                                    __instance.player.Order(new OrderNode() { target = pd.pos, type = EOrderType.Move }, false);
                                                    getitem = false;
                                                    break;
                                                }
                                            }
                                            if (getitem && autobuildgetitem)
                                            {
                                                int[] warningCounts = GameMain.data.warningSystem.warningCounts;
                                                WarningData[] warningpools = GameMain.data.warningSystem.warningPool;
                                                List<int> getItem = new List<int>();
                                                int stackSize = 0;
                                                int packageGridLen = GameMain.mainPlayer.package.grids.Length;
                                                for (int j = packageGridLen - 1; j >= 0 && GameMain.mainPlayer.package.grids[j].count == 0; j--, stackSize++) { }
                                                for (int i = 1; i < GameMain.data.warningSystem.warningCursor && stackSize > 0; i++)
                                                {
                                                    if (getItem.Contains(warningpools[i].detailId)) continue;
                                                    if (GameMain.mainPlayer.package.GetItemCount(warningpools[i].detailId) > 0) break;
                                                    getItem.Add(warningpools[i].detailId);
                                                    FindItemAndMove(warningpools[i].detailId, warningCounts[warningpools[i].signalId]);
                                                }
                                            }
                                        }
                                        else if (GameMain.localPlanet.factory.prebuildPool[lasthasitempd].id != 0)
                                        {
                                            __instance.player.Order(new OrderNode() { target = GameMain.localPlanet.factory.prebuildPool[lasthasitempd].pos, type = EOrderType.Move }, false);
                                            if((GameMain.localPlanet.factory.prebuildPool[lasthasitempd].pos - __instance.player.position).magnitude > 30)
                                            {
                                                __instance.player.currentOrder.targetReached = true;
                                            }
                                        }
                                        Thread.Sleep(2000);
                                    }
                                    catch
                                    {
                                        if (autobuildThread.ThreadState == ThreadState.WaitSleepJoin)
                                            autobuildThread.Interrupt();
                                        else
                                            autobuildThread.Abort();
                                    }
                                }
                            });
                            autobuildThread.Start();
                            autobuildThread.IsBackground = true;
                        }
                    }
                    #endregion
                }
                if (fly && autonavigation_bool.Value && GameMain.mainPlayer != null && GameMain.mainPlayer.navigation != null && GameMain.mainPlayer.navigation._indicatorAstroId != 0)
                {
                    flyfocusPlanet = GameMain.galaxy.PlanetById(GameMain.mainPlayer.navigation._indicatorAstroId);
                    if (flyfocusPlanet == null) return;
                    if (!__instance.player.sailing)
                    {
                        if (flyfocusPlanet != GameMain.mainPlayer.planetData)
                        {
                            __instance.input0.z = 1;
                            __instance.input1.y += 1;
                            if (__instance.actionFly.currentAltitude > 49)
                            {
                                if (__instance.horzSpeed < 12.5)
                                {
                                    __instance.velocity = (__instance.player.uPosition - GameMain.mainPlayer.planetData.uPosition).normalized * t++;
                                }
                            }
                        }
                    }
                    else if (__instance.player.sailing)
                    {
                        double distance = (__instance.player.uPosition - flyfocusPlanet.uPosition).magnitude;
                        if (distance < flyfocusPlanet.radius + 100 && flyfocusPlanet.factoryLoaded)
                        {
                            if (GameMain.mainPlayer.navigation._indicatorAstroId != 0 && GameMain.mainPlayer.navigation._indicatorAstroId == flyfocusPlanet.id)
                                GameMain.mainPlayer.navigation._indicatorAstroId = 0;
                            fly = false;
                            slowdownsail = false;
                            t = 20;
                        }
                        else if (distance < flyfocusPlanet.radius + 1000 && !flyfocusPlanet.factoryLoaded)
                        {
                            if (Vector3.Angle(__instance.player.uVelocity.normalized, (flyfocusPlanet.uPosition - __instance.player.uPosition).normalized) < 10)
                            {
                                __instance.player.uVelocity = (flyfocusPlanet.uPosition - __instance.player.uPosition).normalized * (distance - flyfocusPlanet.radius);
                            }
                            else
                            {
                                if (__instance.player.uVelocity.magnitude < 1000)
                                {
                                    __instance.player.uVelocity += (flyfocusPlanet.uPosition - __instance.player.uPosition).normalized * (__instance.actionSail.max_acc);
                                    __instance.actionSail.UseSailEnergy(__instance.actionSail.max_acc);
                                }
                            }
                        }
                        else
                        {
                            if (__instance.player.uVelocity.magnitude + __instance.actionSail.max_acc >= __instance.actionSail.maxSailSpeed)
                            {
                                __instance.player.uVelocity = (flyfocusPlanet.uPosition - __instance.player.uPosition).normalized * __instance.actionSail.maxSailSpeed;
                            }
                            else
                            {
                                __instance.player.uVelocity += (flyfocusPlanet.uPosition - __instance.player.uPosition).normalized * __instance.actionSail.max_acc;
                                __instance.actionSail.UseSailEnergy(__instance.actionSail.max_acc);
                            }
                        }
                        if (__instance.player.warping)
                        {
                            if (distance < 10000)
                            {
                                __instance.player.warpCommand = false;
                            }
                        }
                        if (__instance.player.planetData == null && autowarpcommand.Value && !__instance.player.warping && __instance.mecha.coreEnergy > __instance.mecha.warpStartPowerPerSpeed * __instance.actionSail.maxWarpSpeed && distance > autowarpdistance.Value * 2400000 && distance > 10000 && __instance.player.mecha.UseWarper())
                        {
                            __instance.player.warpCommand = true;
                        }
                    }
                }

            }
        }
        [HarmonyPatch(typeof(UITechTree), "UpdateScale")]
        class UITechTreeUpdateScalePatch
        {
            public static bool Prefix(UITechTree __instance)
            {
                if (noscaleuitech_bool.Value && (__instance.selected != null || __instance.centerViewNode != null)) return false;
                return true;
            }
        }
        [HarmonyPatch(typeof(FPSController), "Update")]
        class FPSControllerUpdatePatch
        {
            public static void Postfix()
            {
                if (!changeups)
                    return;
                Time.fixedDeltaTime = 1 / (upsfix * 60);
            }
        }

        [HarmonyPatch(typeof(UniverseSimulator), "GameTick")]
        class GameDataOnDrawPatch
        {
            public static void Prefix(UniverseSimulator __instance)
            {
                if (simulatorchanging)
                {
                    int num = 0;
                    __instance.gameObject.SetActive(!simulatorrender);
                    while (__instance.planetSimulators != null && num < __instance.planetSimulators.Length)
                    {
                        if (__instance.planetSimulators[num] != null)
                        {
                            __instance.planetSimulators[num].gameObject.SetActive(!simulatorrender);
                        }
                        num++;
                    }
                    simulatorchanging = false;
                }
            }
        }
        [HarmonyPatch(typeof(PlayerAudio), "Update")]
        class PlayerAudioUpdatePatch
        {
            public static bool Prefix()
            {
                return !closeplayerflyaudio.Value;
            }
        }
        [HarmonyPatch(typeof(PlayerFootsteps), "PlayFootstepSound")]
        class PlayerFootstepsPatch
        {
            public static bool Prefix()
            {
                return !closeplayerflyaudio.Value;
            }
        }
    }
}
