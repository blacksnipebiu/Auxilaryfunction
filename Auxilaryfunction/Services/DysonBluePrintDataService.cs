using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static Auxilaryfunction.AuxConfig;

namespace Auxilaryfunction.Services
{
    public class DysonBluePrintDataService
    {
        public static TempDysonBlueprintData selectDysonBlueprintData = new();
        public static List<TempDysonBlueprintData> tempDysonBlueprintData = [];

        #region 应用戴森球蓝图
        public static void LoadDysonBluePrintData()
        {
            tempDysonBlueprintData = new List<TempDysonBlueprintData>();
            string path = new StringBuilder(GameConfig.overrideDocumentFolder).Append(GameConfig.gameName).Append("/DysonBluePrint/").ToString();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream("Auxilaryfunction.450节点+框架+32壳面.txt");
                byte[] bs = new byte[sm.Length];
                sm.Read(bs, 0, (int)sm.Length);
                sm.Close();
                UTF8Encoding con = new();
                string strdata = con.GetString(bs);
                try
                {
                    File.WriteAllText(Path.Combine(path, "450节点+框架+32壳面.txt"), strdata);
                }
                catch { }
            }
            var filesPath = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            for (int i = 0; i < filesPath.Length; i++)
            {
                string str64Data = File.ReadAllText(filesPath[i]);
                var data = new TempDysonBlueprintData()
                {
                    name = filesPath[i].Substring(path.Length),
                    path = filesPath[i],
                };
                data.ReadDysonSphereBluePrint(str64Data);
                if (data.type != EDysonBlueprintType.None)
                {
                    tempDysonBlueprintData.Add(data);
                }
            }

        }
        public static string ReaddataFromFile(string path)
        {
            if (selectDysonBlueprintData.path == "")
            {
                UIMessageBox.Show(ErrorTitle.getTranslate(), "路径为空，请重新选择".getTranslate(), "确定".Translate(), 3, null);
                return "";
            }
            if (File.Exists(path))
            {
                try
                {
                    string data = File.ReadAllText(path);
                    return data;
                }
                catch
                {
                    UIMessageBox.Show(ErrorTitle.getTranslate(), "文件读取失败".getTranslate(), "确定".Translate(), 3, null);
                }
            }
            UIMessageBox.Show(ErrorTitle.getTranslate(), "文件不存在".getTranslate(), "确定".Translate(), 3, null);
            return "";

        }
        public static void RemoveLayerById(int Id)
        {
            var dysonSphere = UIRoot.instance.uiGame.dysonEditor.selection.viewDysonSphere;
            if (dysonSphere == null)
                return;
            var layer = dysonSphere.layersIdBased[Id];
            if (layer != null)
            {
                if (layer.nodeCount > 0 || layer.frameCount > 0 || layer.shellCount > 0)
                {
                    UIMessageBox.Show("删除层级非空标题".Translate(), "删除层级非空描述".Translate(), "取消".Translate(), "确定".Translate(), 1, null, new UIMessageBox.Response(() =>
                    {
                        if (dysonSphere.layersSorted.Contains(layer))
                        {
                            layer?.RemoveAllStructure();
                            dysonSphere.RemoveLayer(layer);
                        }
                    }));
                    VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                    return;
                }
                dysonSphere.RemoveLayer(layer);
            }
        }
        private static void ApplySingleLayerBlueprint(string data, DysonSphere dysonSphere, int id)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.SingleLayer, dysonSphere, dysonSphere.layersIdBased[id]);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.StressExceed)
            {
                UIMessageBox.Show("戴森球蓝图纬度过高标题".Translate(), "戴森球蓝图纬度过高描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                switch (dysonBlueprintDataIOError)
                {
                    case DysonBlueprintDataIOError.TypeMismatchSphere:
                        arg = "戴森球".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchSwarm:
                        arg = "戴森云".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchLayers:
                        arg = "戴森壳".Translate();
                        break;
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森层级".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
            VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
        }
        private static void ApplyDysonLayersBlueprint(string data, DysonSphere dysonSphere)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.Layers, dysonSphere, null);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.StressExceed)
            {
                UIMessageBox.Show("戴森球蓝图纬度过高标题".Translate(), "戴森球蓝图纬度过高描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                if (dysonBlueprintDataIOError != DysonBlueprintDataIOError.TypeMismatchSwarm)
                {
                    if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.TypeMismatchSingleLayer)
                    {
                        arg = "戴森层级".Translate();
                    }
                }
                else
                {
                    arg = "戴森云".Translate();
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森壳".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
            VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
        }
        private static void ApplyDysonBlueprint(string data, DysonSphere dysonSphere)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.DysonSphere, dysonSphere, null);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
            }
            else if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.StressExceed)
            {
                UIMessageBox.Show("戴森球蓝图纬度过高标题".Translate(), "戴森球蓝图纬度过高描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
            else if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                switch (dysonBlueprintDataIOError)
                {
                    case DysonBlueprintDataIOError.TypeMismatchSwarm:
                        arg = "戴森云".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchLayers:
                        arg = "戴森壳".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchSingleLayer:
                        arg = "戴森层级".Translate();
                        break;
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森球无空格".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
            else
            {
                UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
        }
        private static void ApplySwarmBlueprint(string data, DysonSphere dysonSphere)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.SwarmOrbits, dysonSphere, null);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
            }
            else if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                if (dysonBlueprintDataIOError != DysonBlueprintDataIOError.TypeMismatchLayers)
                {
                    if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.TypeMismatchSingleLayer)
                    {
                        arg = "戴森层级".Translate();
                    }
                }
                else
                {
                    arg = "戴森壳".Translate();
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森云".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
            else
            {
                UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
        }
        public static void ApplyDysonBlueprintManage(string data, DysonSphere dysonSphere, EDysonBlueprintType type, int id = -1)
        {
            if (data == "" || dysonSphere == null) return;

            switch (type)
            {
                case EDysonBlueprintType.SingleLayer:
                    if (id == -1)
                    {
                        UIMessageBox.Show(ErrorTitle.getTranslate(), "单层壳蓝图请选择层级".Translate(), "确定".Translate(), 3, null);
                        VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                        return;
                    }
                    ApplySingleLayerBlueprint(data, dysonSphere, id);
                    break;
                case EDysonBlueprintType.Layers:
                    UIMessageBox.Show("戴森球多层壳蓝图应用".Translate(), "确定应用多层壳蓝图吗？使用时鼠标最好不要放在戴森球上".Translate(), "取消".Translate(), "确定".Translate(), 1, null, new UIMessageBox.Response(() =>
                    {
                        ApplyDysonLayersBlueprint(data, dysonSphere);
                    }));
                    break;
                case EDysonBlueprintType.SwarmOrbits:
                    ApplySwarmBlueprint(data, dysonSphere);
                    break;
                case EDysonBlueprintType.DysonSphere:
                    ApplyDysonBlueprint(data, dysonSphere);
                    break;
            }
        }
        #endregion
    }
    public class TempDysonBlueprintData
    {
        public string path = "";
        public string name = "";
        public EDysonBlueprintType type;
        public string TypeName = "";

        private bool VerifyDysonBluePrintData(string Content)
        {
            if (Content.Length < 23 || Content.Substring(0, 5) != "DYBP:")
            {
                return false;
            }

            int firstindex = Content.IndexOf('"');
            int lastIndex = Content.LastIndexOf('"');
            if (firstindex == -1 || lastIndex == -1 || lastIndex < firstindex + 32)
            {
                return false;
            }
            string[] array = Content.Substring(5, firstindex - 5).Split(',');
            //Signature
            if (array.Length < 3 || Math.Max(Content.Length - 36, 0) > lastIndex && !MD5F.Compute(Content.Substring(0, lastIndex)).Equals(Content.Substring(lastIndex + 1, 32), StringComparison.Ordinal))
            {
                return false;
            }
            return true;
        }

        public void ReadDysonSphereBluePrint(string Content)
        {
            if (VerifyDysonBluePrintData(Content))
            {
                int firstindex = Content.IndexOf('"');
                string[] array = Content.Substring(5, firstindex - 5).Split(',');
                type = (EDysonBlueprintType)int.Parse(array[3]);
                switch (type)
                {
                    case EDysonBlueprintType.SingleLayer:
                        TypeName = "单层壳";
                        break;
                    case EDysonBlueprintType.Layers:
                        TypeName = "多层壳";
                        break;
                    case EDysonBlueprintType.SwarmOrbits:
                        TypeName = "戴森云";
                        break;
                    case EDysonBlueprintType.DysonSphere:
                        TypeName = "戴森球(包括壳、云)";
                        break;
                }
            }
        }
    }
}
