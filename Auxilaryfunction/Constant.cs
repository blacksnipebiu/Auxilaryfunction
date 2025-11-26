using UnityEngine;

namespace Auxilaryfunction
{
    public class Constant
    {
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

        public static GUIStyle[] StationStoreStyles = new GUIStyle[3];

        public static GUIStyle GetStationStorelogicStyle(ELogisticStorage i)
        {
            switch (i)
            {
                case ELogisticStorage.None: return StationStoreStyles[0];
                case ELogisticStorage.Supply: return StationStoreStyles[1];
                case ELogisticStorage.Demand: return StationStoreStyles[2];
                default:
                    break;
            }
            return GUI.skin.button;
        }

    }
}
