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
    public class AuxilaryfunctionPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRoot), "OpenMainMenuUI")]
        public static void UIMainMenu_Open()
        {
            Debug.Log(1);
        }

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIDETopFunction), "SetDysonComboBox")]
        public static void UIDETopFunctionSetDysonComboBoxPatch()
        {
            if (autoClearEmptyDyson.Value && GameMain.data?.dysonSpheres != null)
            {
                for (int i = 0; i < GameMain.data.dysonSpheres.Length; i++)
                {
                    var dysonsphere = GameMain.data.dysonSpheres[i];
                    if (dysonsphere != null && dysonsphere.totalNodeCount == 0)
                    {
                        if (dysonsphere.starData.index == (GameMain.localStar?.index ?? 0)) continue;
                        GameMain.data.dysonSpheres[i] = null;
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
                List<int> temp = new List<int>();
                temp.Add(__result);
                EjectorDictionary[__instance.planet.id] = temp;
            }

            __instance.ejectorPool[__result].SetOrbit(1);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactorySystem), "RemoveEjectorComponent")]
        public static void RemoveEjectorComponentPatch(int id, FactorySystem __instance)
        {
            if (EjectorDictionary[__instance.planet.id].Contains(id))
                EjectorDictionary[__instance.planet.id].Remove(id);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), "GameTick")]
        public static bool GameTick1Patch(ref long time, GameData __instance)
        {
            if (stopfactory)
            {
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

                if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
                {
                    PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalAudio);
                    __instance.localPlanet.audio.GameTick();
                    PerformanceMonitor.EndSample(ECpuWorkEntry.LocalAudio);
                }
                __instance.preferences.Collect();
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetTransport), "NewDispenserComponent")]
        public static void NewDispenserComponentPatch(int __result, PlanetTransport __instance)
        {
            if (auto_supply_station.Value)
            {
                __instance.dispenserPool[__result].idleCourierCount = player.package.TakeItem(5003, auto_supply_Courier.Value, out _);
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
                __result.deliveryDrones = (int)(DroneStartCarry.Value * 10) * 10;
                if (__result.isStellar)
                {
                    __result.warperCount = player.package.TakeItem(1210, auto_supply_warp.Value, out _);
                    __result.warpEnableDist = stationwarpdist.Value * GalaxyData.AU;
                    __result.deliveryShips = (int)(ShipStartCarry.Value * 10) * 10;
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
            if (__instance.modelId == 70)
                return !norender_lab_bool.Value && !simulatorrender;
            return true;
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
                        __instance.altitude = Altitude(__instance.castObjectPos, planetAux, __instance);
                    }
                    else if (__instance.startObjectId != 0)
                    {
                        __instance.altitude = Altitude(__instance.pathPoints[0], planetAux, __instance);
                    }
                }
                else if (Input.GetKey(KeyCode.LeftControl) && ObjectIsBeltOrSplitter(__instance, __instance.castObjectId))
                {
                    __instance.altitude = Altitude(__instance.castObjectPos, planetAux, __instance);
                    if (__instance.altitude == 0)
                    {
                        __instance.altitude = 1;
                    }
                }
            }
        }

        private static int Altitude(Vector3 pos, PlanetAuxData aux, BuildTool_Path buildtoolpath)
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

        //星图方向指引启动自动导航
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStarmap), "OnCursorFunction3Click")]
        public static void AutonavigationPatch(UIStarmap __instance)
        {
            if (autonavigation_bool.Value)
            {
                if (__instance.focusPlanet != null && __instance.focusPlanet.planet.id != GameMain.localPlanet?.id)
                    PlayerOperation.fly = true;
                else if (__instance.focusStar != null && __instance.focusStar.star.id != GameMain.localStar?.id)
                    PlayerOperation.fly = true;
                if (PlayerOperation.fly)
                {
                    PlayerOperation.flyfocusPlanet = null;
                    PlayerOperation.flyfocusStar = null;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIGeneralTips), "OnTechUnlocked")]
        public static bool OnTechUnlockedPatch()
        {
            return !close_alltip_bool.Value;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRandomTip), "_OnOpen")]
        public static void UIBuildMenu_OnOpenPatch(ref UIRandomTip __instance)
        {
            if (close_alltip_bool.Value)
            {
                __instance._Close();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITutorialTip), "Determine")]
        public static void UITutorialWindow_OnOpenPatch(UITutorialTip __instance)
        {
            if (close_alltip_bool.Value)
            {
                __instance._Close();
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
                    short fuelcount = (short)player.package.TakeItem(1803, auto_supply_starfuel.Value, out int inc);
                    if (fuelcount > 0)
                    {
                        __instance.genPool[__result].SetNewFuel(1803, fuelcount, (short)inc);
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MilestoneSystem), "NotifyUnlockMilestone")]
        public static bool UIMilestoneTipNotifyUnlockMilestonePatch()
        {
            return !close_alltip_bool.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControlGizmo), "AddOrderGizmo")]
        public static bool PlayerControlGizmoPatch()
        {
            return !closecollider;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UITechTree), "UpdateScale")]
        public static bool UITechTreeUpdateScalePatch(UITechTree __instance)
        {
            if (noscaleuitech_bool.Value && (__instance.selected != null || __instance.centerViewNode != null)) return false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSController), "Update")]
        public static void FPSControllerUpdatePatch()
        {
            if (!changeups)
                return;
            Time.fixedDeltaTime = 1 / (upsfix * 60);
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAudio), "Update")]
        public static bool PlayerAudioUpdatePatch()
        {
            return !closeplayerflyaudio.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerFootsteps), "PlayFootstepSound")]
        public static bool PlayerFootstepsPatch()
        {
            return !closeplayerflyaudio.Value;
        }
    }

    //操纵人物
    [HarmonyPatch(typeof(PlayerController), "GetInput")]
    public class PlayerOperation
    {
        public static float t = 20;
        public static bool fly;
        public static PlanetData flyfocusPlanet = null;
        public static StarData flyfocusStar = null;
        private static StarData LocalStar => GameMain.localStar;
        private static Mecha mecha => player.mecha;

        private static double max_acc => player.controller.actionSail.max_acc;
        private static float maxSailSpeed => player.controller.actionSail.maxSailSpeed;
        private static float maxWarpSpeed => player.controller.actionSail.maxWarpSpeed;
        private static int indicatorAstroId => player.navigation.indicatorAstroId;
        private static bool CanWarp => LocalPlanet == null && autowarpcommand.Value && !player.warping && mecha.coreEnergy > mecha.warpStartPowerPerSpeed * maxWarpSpeed;

        public static void Postfix(PlayerController __instance)
        {
            #region 寻找建筑
            if (automovetounbuilt.Value && LocalPlanet != null && closecollider)
            {
                if (autobuildThread == null)
                {
                    autobuildThread = new Thread(delegate ()
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
                                    if (lasthasitempd == -1 || mindistance > (pd.pos - player.position).magnitude)
                                    {
                                        lasthasitempd = pd.id;
                                        mindistance = (pd.pos - player.position).magnitude;
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
                                        int packageGridLen = player.package.grids.Length;
                                        for (int j = packageGridLen - 1; j >= 0 && player.package.grids[j].count == 0; j--, stackSize++) { }
                                        for (int i = 1; i < GameMain.data.warningSystem.warningCursor && stackSize > 0; i++)
                                        {
                                            if (getItem.Contains(warningpools[i].detailId)) continue;
                                            if (player.package.GetItemCount(warningpools[i].detailId) > 0) break;
                                            getItem.Add(warningpools[i].detailId);
                                            FindItemAndMove(warningpools[i].detailId, warningCounts[warningpools[i].signalId]);
                                        }
                                    }
                                }
                                else if (GameMain.localPlanet.factory.prebuildPool[lasthasitempd].id != 0)
                                {
                                    __instance.player.Order(new OrderNode() { target = GameMain.localPlanet.factory.prebuildPool[lasthasitempd].pos, type = EOrderType.Move }, false);
                                    if ((GameMain.localPlanet.factory.prebuildPool[lasthasitempd].pos - __instance.player.position).magnitude > 30)
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

            #region 自动导航
            if (fly && autonavigation_bool.Value && indicatorAstroId != 0)
            {
                if (indicatorAstroId == LocalStar?.id / 100 || indicatorAstroId == LocalPlanet?.id)
                    fly = false;
                if (!player.sailing)
                    FlyAwayPlanet();
                else
                {
                    //如果是100的整数倍，根据id生成规则，一定是星系
                    if (indicatorAstroId % 100 == 0)
                    {
                        flyfocusStar = flyfocusStar ?? GameMain.galaxy.StarById(indicatorAstroId / 100);
                        var uPosition = flyfocusStar.uPosition;
                        if ((player.uPosition - uPosition).magnitude < 100_000)
                        {
                            fly = false;
                            if (player.warping)
                            {
                                player.warpCommand = false;
                            }
                        }
                        else
                        {
                            FlyTo(uPosition);
                            if (CanWarp && mecha.UseWarper())
                            {
                                player.warpCommand = true;
                            }
                        }
                    }
                    else
                    {
                        flyfocusPlanet = flyfocusPlanet ?? GameMain.galaxy.PlanetById(indicatorAstroId);
                        var uPosition = flyfocusPlanet.uPosition;
                        var radius = flyfocusPlanet.radius;
                        var factoryLoaded = flyfocusPlanet.factoryLoaded;
                        double distance = (player.uPosition - uPosition).magnitude;
                        if (distance < radius + 500 && factoryLoaded)
                        {
                            fly = false;
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
                            if (player.warping && distance < 10_000)
                            {
                                player.warpCommand = false;
                            }
                            if (CanWarp && distance > autowarpdistance.Value * 2_400_000 && distance > 10_000 && mecha.UseWarper())
                            {
                                player.warpCommand = true;
                            }
                        }
                    }
                }
            }
            if (!fly && (flyfocusPlanet != null || flyfocusStar != null))
            {
                flyfocusPlanet = null;
                flyfocusStar = null;
                t = 20;
            }
            #endregion
        }

        private static void FlyTo(VectorLF3 uPosition)
        {
            VectorLF3 direction = (uPosition - player.uPosition).normalized;

            if (LocalPlanet != null)
            {
                VectorLF3 diff = player.uPosition - LocalPlanet.uPosition;
                double altitude = diff.magnitude - LocalPlanet.radius;
                float upFactor = Mathf.Clamp((float)((1000.0 - altitude) / 1000.0), 0.0f, 1.0f);
                upFactor *= upFactor * upFactor;
                direction = ((direction * (1.0f - upFactor)) + diff.normalized * upFactor).normalized;
            }

            if (player.uVelocity.magnitude + max_acc >= maxSailSpeed)
            {
                player.uVelocity = direction * maxSailSpeed;
            }
            else
            {
                player.uVelocity += direction * max_acc;
                player.controller.actionSail.UseSailEnergy(max_acc);
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


        private static void FindItemAndMove(int itemId, int itemCount)
        {
            if (LocalPlanet == null || LocalPlanet.factory == null || LocalPlanet.gasItems != null) return;
            if (LDB.items == null || LDB.items.Select(itemId) == null) return;
            int packageGridLen = player.package.grids.Length;
            int stackMax;
            int stackSize = 0;
            if (player.package.grids[packageGridLen - 1].count != 0)
            {
                player.package.Sort();
            }
            for (int i = packageGridLen - 1; i >= 0; i--, stackSize++)
            {
                if (player.package.grids[i].count != 0) break;
            }
            stackMax = LDB.items.Select(itemId).StackSize * stackSize;
            itemCount = stackMax > itemCount ? itemCount : stackMax;
            PlanetFactory pf = LocalPlanet.factory;

            if (pf.transport != null && pf.transport.stationPool != null)
            {
                int result = 0;
                int resultinc = 0;
                foreach (StationComponent sc in pf.transport.stationPool)
                {
                    if (itemCount == 0) break;
                    if (sc == null || sc.storage == null) continue;
                    int tempItemCount = itemCount;
                    int tempItemId = itemId;
                    sc.TakeItem(ref tempItemId, ref tempItemCount, out int inc);
                    result += tempItemCount;
                    itemCount -= tempItemCount;
                    resultinc += inc;
                }
                player.TryAddItemToPackage(itemId, result, resultinc, false);
            }

            if (pf.factoryStorage != null && pf.factoryStorage.storagePool != null)
            {
                if (pf.factoryStorage.storagePool != null)
                {
                    foreach (StorageComponent sc in pf.factoryStorage.storagePool)
                    {
                        if (itemCount == 0) break;
                        if (sc != null)
                        {
                            int temp = sc.TakeItem(itemId, itemCount, out int inc);
                            itemCount -= temp;
                            if (temp > 0)
                            {
                                player.TryAddItemToPackage(itemId, temp, inc, false);
                            }
                        }
                    }
                }
                if (pf.factoryStorage.tankPool != null)
                {
                    foreach (TankComponent tc in pf.factoryStorage.tankPool)
                    {
                        if (itemCount == 0) break;
                        if (tc.id > 0 && tc.fluidId > 0 && tc.fluidCount > 0 && itemId == tc.fluidId)
                        {
                            int tempItemCount = tc.fluidCount > itemCount ? itemCount : tc.fluidCount;

                            int inc = (int)(tc.fluidInc / tc.fluidCount + 0.5) * tempItemCount;
                            pf.factoryStorage.tankPool[tc.id].fluidCount -= tempItemCount;
                            pf.factoryStorage.tankPool[tc.id].fluidInc -= inc;

                            itemCount -= tempItemCount;
                            if (tempItemCount > 0)
                            {
                                player.TryAddItemToPackage(itemId, tempItemCount, inc, false);
                            }
                        }
                    }
                }
            }
        }
    }

}
