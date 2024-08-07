﻿using UnityEngine;

namespace Auxilaryfunction
{
    public class Constant
    {
        public static string[] PitchLetter = new string[12]
        {
            "C",
            "C#",
            "D",
            "D#",
            "E",
            "F",
            "F#",
            "G",
            "G#",
            "A",
            "A#",
            "B"
        };
        public static double[,] posxz = new double[10, 2]{
            {0,1 },
            { 0.1563, -0.9877},
            { 0.3089,-0.9511 },
            { 0.4539, -0.8911 },
            { 0.5877, -0.8091 },
            { 0.7071, -0.7071 },
            { 0.8091, -0.5877 },
            { 0.8910, -0.4540 },
            { 0.9511, -0.3089 },
            { 0.9877, -0.1564 },
        };
        public static string[] StationNames = new string[8] { "星球矿机", "垃圾站", "星球无限供货机", "喷涂加工厂", "星球熔炉矿机", "星球量子传输站", "星系量子传输站", "设置翘曲需求" };

        public static GUIStyle[] StationStoreStyles = new GUIStyle[3];

        public static GUIStyle GetStationStorelogicStyle(ELogisticStorage i)
        {
            switch (i)
            {
                case ELogisticStorage.None: return StationStoreStyles[0];
                case ELogisticStorage.Supply: return StationStoreStyles[1];
                case ELogisticStorage.Demand: return StationStoreStyles[2];
            }
            return GUI.skin.button;
        }

        public static string GetLogisticText(ELogisticStorage logic, bool isStellar, bool careStellar)
        {
            if (careStellar && !isStellar)
            {
                if (logic == ELogisticStorage.Supply)
                {
                    return "本地供应".Translate();
                }
                if (logic == ELogisticStorage.Demand)
                {
                    return "本地需求".Translate();
                }
                return "本地仓储".Translate();
            }
            else if (careStellar && isStellar)
            {
                if (logic == ELogisticStorage.Supply)
                {
                    return "星际供应".Translate();
                }
                if (logic == ELogisticStorage.Demand)
                {
                    return "星际需求".Translate();
                }
                return "星际仓储".Translate();
            }
            else
            {
                if (logic == ELogisticStorage.Supply)
                {
                    return "供应".Translate();
                }
                if (logic == ELogisticStorage.Demand)
                {
                    return "需求".Translate();
                }
                return "仓储".Translate();
            }
        }
    }
}
