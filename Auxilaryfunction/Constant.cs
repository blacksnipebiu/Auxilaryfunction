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
    }
}
