using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Auxilaryfunction.Auxilaryfunction;

namespace Auxilaryfunction.Patch
{
    public class PlayerOperationPatch
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
                    _patch = Harmony.CreateAndPatchAll(typeof(PlayerOperationPatch));
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
}
