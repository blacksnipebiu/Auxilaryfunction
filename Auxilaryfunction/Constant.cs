using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Reflection;

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
        public static string[] StationNames = new string[6] { "星球矿机", "垃圾站", "星球无限供货机", "星球量子传输站", "星系量子传输站", "设置翘曲需求" };

        public static string GetStationlogic(int i)
        {
            switch (i)
            {
                case 0: return "仓储";
                case 1: return "供应";
                case 2: return "需求";
            }
            return "";
        }
    }
}
