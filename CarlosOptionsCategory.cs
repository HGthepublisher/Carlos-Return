using MTM101BaldAPI.OptionsAPI;
using UnityEngine;
using UnityEngine.Events;

namespace CarlosReturn
{
    public class CarlosDebugCategory : CustomOptionsCategory
    {
        private MenuToggle debug;
        private MenuToggle debugArrows;
        private MenuToggle debugKeybinds;

        public override void Build()
        {
            debug = CreateToggle("debug", "Debug", CarlosBasePlugin.debug.Value, new Vector3(90, 40, 0), 300);
            debugArrows = CreateToggle("debug", "Debug Arrows", CarlosBasePlugin.debugArrows.Value, new Vector3(105, 0, 0), 300);
            debugKeybinds = CreateToggle("debug", "Debug Keybinds", CarlosBasePlugin.debugKeybinds.Value, new Vector3(105, -40, 0), 300);
            CreateApplyButton(new UnityAction(Apply));
        }

        public void Apply()
        {
            CarlosBasePlugin.debug.Value = debug.Value;
            CarlosBasePlugin.debugArrows.Value = debugArrows.Value;
            CarlosBasePlugin.debugKeybinds.Value = debugKeybinds.Value;
            CarlosBasePlugin.instance.Config.Save();
        }
    }

    public class CarlosModesCategory : CustomOptionsCategory
    {
        private MenuToggle impossible;
        private MenuToggle easy;
        private MenuToggle explorer;

        public override void Build()
        {
            impossible = CreateToggle("impossible", "Impossible Mode", CarlosBasePlugin.impossible.Value, new Vector3(110, 40, 0), 300);
            easy = CreateToggle("easy", "Easy Mode", CarlosBasePlugin.easy.Value, new Vector3(110, 0, 0), 300);
            explorer = CreateToggle("explorer", "Explorer Mode", CarlosBasePlugin.explorer.Value, new Vector3(110, -40, 0), 300);
            CreateApplyButton(new UnityAction(Apply));
        }

        public void Apply()
        {
            CarlosBasePlugin.impossible.Value = impossible.Value;
            CarlosBasePlugin.easy.Value = easy.Value;
            CarlosBasePlugin.explorer.Value = explorer.Value;
            CarlosBasePlugin.instance.Config.Save();
        }
    }
}