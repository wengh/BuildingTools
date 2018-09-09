using System.Linq;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Texts;
using BrilliantSkies.Ui.Layouts.DropDowns;
using BrilliantSkies.Ui.Tips;
using UnityEngine;

namespace BuildingTools
{
    public class Holo3DUI : ConsoleUi<Holo3D>
    {
        public Holo3DUI(Holo3D focus) : base(focus) { }

        public override void SetGuiSettings()
        {
            base.SetGuiSettings();
            GuiSettings.QGui = true;
            GuiSettings.AllowOrbitCameraRotation = true;
        }

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            var window = NewWindow("3D Hologram Projector (hold E to move the camera)", WindowSizing.GetSizeCentral(0.7f, 0.5f));
            window.DisplayTextPrompt = false;

            window.Screen.CreateHeader("File", new ToolTip("Configure loading options"));

            var seg1 = window.Screen.CreateStandardSegment();
            seg1.AddInterpretter(TextInput<Holo3D>.Quick(_focus, M.m<Holo3D>(x => x.Path), "Path",
                new ToolTip("Type a local path into the box below to load an .obj file for the hologram"), (x, value) => x.Path = value));
            seg1.AddInterpretter(SubjectiveButton<Holo3D>.Quick(_focus, "Reload 3D Model",
                new ToolTip("Load the .obj and all related files and display the model as a hologram"), (x) => x.Reload()));

            var seg2 = window.Screen.CreateStandardHorizontalSegment();
            seg2.AddInterpretter(SubjectiveToggle<Holo3D>.Quick(_focus, "Enabled",
                 new ToolTip("Should the hologram display?"), (x, value) => x.Enabled = value, x => x.Enabled));
            seg2.AddInterpretter(SubjectiveToggle<Holo3D>.Quick(_focus, "Display on start",
                new ToolTip("If turned off, the hologram will only load when you press the \"Reload\" button. " +
                "Otherwise, it will load immediately after the Construct is loaded (it may cause some serious lag)."),
                (x, value) => x.displayOnStart = value, x => x.displayOnStart));

            var seg3 = window.Screen.CreateStandardHorizontalSegment();
            var items = Holo3D.shaders.Select(x => new DropDownMenuAltItem<Shader>
            {
                Name = x.name,
                ObjectForAction = x,
                ToolTip = "Shader to use\nPress \"Reload 3D Model\" button to apply"
            }).ToArray();
            var menu = new DropDownMenuAlt<Shader>();
            menu.SetItems(items);
            seg3.AddInterpretter(new DropDown<Holo3D, Shader>(_focus, menu, (x, shader) => x.shader == shader, (x, shader) => x.shader = shader));

            window.Screen.CreateHeader("Transform", new ToolTip("Configure position, rotation and scale"));

            var seg4 = window.Screen.CreateStandardSegment();
            seg4.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<Holo3D>.Quick(_focus, -100, 100, 0.1f, 0,
                M.m<Holo3D>(x => x.pos.z), "Distance {0}m", (x, value) => { x.pos.z = value; x.SetLocalTransform(); }, new ToolTip("Position.z")));
            seg4.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<Holo3D>.Quick(_focus, -100, 100, 0.1f, 0,
                M.m<Holo3D>(x => x.pos.y), "Up translation {0}m", (x, value) => { x.pos.y = value; x.SetLocalTransform(); }, new ToolTip("Position.y")));
            seg4.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<Holo3D>.Quick(_focus, -100, 100, 0.1f, 0,
                M.m<Holo3D>(x => x.pos.x), "Right translation {0}m", (x, value) => { x.pos.x = value; x.SetLocalTransform(); }, new ToolTip("Position.x")));
            seg4.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<Holo3D>.Quick(_focus, -180, 180, 1, 0,
                M.m<Holo3D>(x => x.rot.x), "Pitch {0}°", (x, value) => { x.rot.x = value; x.SetLocalTransform(); }, new ToolTip("Rotation.x")));
            seg4.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<Holo3D>.Quick(_focus, -180, 180, 1, 0,
                M.m<Holo3D>(x => x.rot.y), "Yaw {0}°", (x, value) => { x.rot.y = value; x.SetLocalTransform(); }, new ToolTip("Rotation.y")));
            seg4.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<Holo3D>.Quick(_focus, -180, 180, 1, 0,
                M.m<Holo3D>(x => x.rot.z), "Roll {0}°", (x, value) => { x.rot.z = value; x.SetLocalTransform(); }, new ToolTip("Rotation.z")));
            seg4.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<Holo3D>.Quick(_focus, 0.01f, 100, 0.01f, 1,
                M.m<Holo3D>(x => x.scale.z), "Scale {0}x", (x, value) => { x.scale.z = value; x.SetLocalTransform(); }, new ToolTip("Scale")));

            return window;
        }
    }
}
