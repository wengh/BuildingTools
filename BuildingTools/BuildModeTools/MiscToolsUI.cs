using BrilliantSkies.Ui.Consoles;

namespace BuildingTools
{
    public class MiscToolsUI : ConsoleUi<BlockSearch>
    {

        public MiscToolsUI() : base(new BlockSearch())
        {
        }

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            var window = NewWindow(135315, "Build Mode Tools", WindowSizing.GetSizeCentral(0.3f, 0.9f));

            window.DisplayTextPrompt = false;
            window.AllScreens.Clear();

            window.AllScreens.Add(new BlockSearchTab(() => DeactivateGui(), window, _focus));
            window.AllScreens.Add(new BlockCounterTab(window));

            window.Screen = window.AllScreens[0];
            return window;
        }
    }
}
