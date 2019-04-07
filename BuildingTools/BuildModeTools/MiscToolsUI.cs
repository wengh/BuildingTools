using BrilliantSkies.Ui.Consoles;

namespace BuildingTools
{
    public class MiscToolsUI : ConsoleUi<object>
    {
        private readonly BlockSearch blockSearch;

        public MiscToolsUI() : base()
        {
            blockSearch = new BlockSearch();
        }

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            var window = NewWindow(string.Format("Build Mode Tools"), WindowSizing.GetSizeCentral(0.3f, 0.9f));

            window.DisplayTextPrompt = false;
            window.AllScreens.Clear();

            window.AllScreens.Add(new BlockSearchTab(() => DeactivateGui(), window, blockSearch));
            window.AllScreens.Add(new BlockCounterTab(window));

            window.Screen = window.AllScreens[0];
            return window;
        }
    }
}
