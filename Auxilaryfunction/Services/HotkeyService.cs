using BepInEx.Configuration;
using UnityEngine;
using static Auxilaryfunction.AuxConfig;
using Auxilaryfunction.Patch;

namespace Auxilaryfunction.Services
{
    /// <summary>
    /// 快捷键服务。
    /// 负责主面板与蓝图面板快捷键的捕获与更新，
    /// 支持在“修改中”状态下互斥捕获，避免两个快捷键同时被设置。
    /// 兼容数字键、字母键、功能键 F1-F10，修饰键支持 LeftShift/LeftControl/LeftAlt。
    /// </summary>
    public static class HotkeyService
    {
        /// <summary>
        /// 处理快捷键修改流程：
        /// - 互斥控制两个快捷键设置流程
        /// - 在启用 <c>upsquickset</c> 时，支持通过小键盘 + / - 快速调整逻辑帧倍数
        /// - 当修改完成时提交到配置项（<c>QuickKey</c> / <c>BluePrintShowWindow</c>）
        /// </summary>
        public static void HandleQuickKeyChange()
        {
            if (upsquickset.Value)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetKey(KeyCode.KeypadPlus)) SpeedUpPatch.SpeedMultiple += 0.01f;
                    if (Input.GetKey(KeyCode.KeypadMinus)) SpeedUpPatch.SpeedMultiple -= 0.01f;
                    if (SpeedUpPatch.SpeedMultiple < 0.01) SpeedUpPatch.SpeedMultiple = 0.01f;
                    else if (SpeedUpPatch.SpeedMultiple > 10) SpeedUpPatch.SpeedMultiple = 10;
                }
            }
            if (ChangingQuickKey)
            {
                ChangeBluePrintQuickKey = false;
                ChangingBluePrintQuickKey = false;
            }
            else if (ChangingBluePrintQuickKey)
            {
                ChangeQuickKey = false;
                ChangingQuickKey = false;
            }

            if (ChangeQuickKey && !ChangingBluePrintQuickKey)
            {
                SetMainQuickKey();
                ChangingQuickKey = true;
            }
            else if (!ChangeQuickKey && ChangingQuickKey)
            {
                QuickKey.Value = tempShowWindow;
                ChangingQuickKey = false;
            }

            if (ChangeBluePrintQuickKey && !ChangingQuickKey)
            {
                SetBluePrintQuickKey();
                ChangingBluePrintQuickKey = true;
            }
            else if (!ChangeBluePrintQuickKey && ChangingBluePrintQuickKey)
            {
                BluePrintShowWindow.Value = tempBluePrintShowWindow;
                ChangingBluePrintQuickKey = false;
            }
        }
        /// <summary>
        /// 捕获当前按键并设置蓝图窗口快捷键的临时值。
        /// </summary>
        public static void SetBluePrintQuickKey()
        {
            if (TryBuildShortcutFromPressedKeys(out var shortcut))
            {
                tempBluePrintShowWindow = shortcut;
            }
        }

        /// <summary>
        /// 捕获当前按键并设置主面板快捷键的临时值。
        /// </summary>
        public static void SetMainQuickKey()
        {
            if (TryBuildShortcutFromPressedKeys(out var shortcut))
            {
                tempShowWindow = shortcut;
            }
        }

        /// <summary>
        /// 读取当前按下的修饰键与主键，构造快捷键。
        /// 优先级：数字 0-9，其次字母 A-Z，最后功能键 F1-F10；修饰键仅支持左侧 Shift/Control/Alt。
        /// 返回值为是否成功检测到有效按键。
        /// </summary>
        public static bool TryBuildShortcutFromPressedKeys(out KeyboardShortcut shortcut)
        {
            int mod = 0;
            if (Input.GetKey(KeyCode.LeftShift)) mod = (int)KeyCode.LeftShift;
            else if (Input.GetKey(KeyCode.LeftControl)) mod = (int)KeyCode.LeftControl;
            else if (Input.GetKey(KeyCode.LeftAlt)) mod = (int)KeyCode.LeftAlt;

            int main = 0;
            for (int i = (int)KeyCode.Alpha0; i <= (int)KeyCode.Alpha9; i++)
            {
                if (Input.GetKey((KeyCode)i)) { main = i; break; }
            }
            if (main == 0)
            {
                for (int i = (int)KeyCode.A; i <= (int)KeyCode.Z; i++)
                {
                    if (Input.GetKey((KeyCode)i)) { main = i; break; }
                }
            }
            if (main == 0)
            {
                for (int i = (int)KeyCode.F1; i <= (int)KeyCode.F10; i++)
                {
                    if (Input.GetKey((KeyCode)i)) { main = i; break; }
                }
            }

            if (mod != 0 && main != 0)
            {
                shortcut = new KeyboardShortcut((KeyCode)mod, (KeyCode)main);
                return true;
            }
            if (mod != 0 || main != 0)
            {
                var single = (KeyCode)(mod != 0 ? mod : main);
                shortcut = new KeyboardShortcut(single);
                return true;
            }
            shortcut = tempBluePrintShowWindow;
            return false;
        }
    }
}
