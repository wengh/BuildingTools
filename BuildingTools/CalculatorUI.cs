using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Texts;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Tips;
using System.Linq;
using UnityEngine;
using BrilliantSkies.Ui.Layouts.DropDowns;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Modding.Types;
using System;
using System.Text;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Unity;
using static BuildingTools.BuildingToolsPlugin;

namespace BuildingTools
{
    public class CalculatorUI : ConsoleUi<Calculator>
    {
        public CalculatorUI(Calculator focus) : base(focus) { }
        public string expression = "";

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            var window = NewWindow("Calculator", WindowSizing.GetSizeCentral(0.5f, 0.8f));
            window.DisplayTextPrompt = false;

            var seg1 = window.Screen.CreateStandardSegment();
            seg1.AddInterpretter(new CodeDisplayArea<Calculator>(_focus, M.m<Calculator>(x => x.Log), M.m<Calculator>(x => x.LineNumber),
                M.m<Calculator>("Log"), M.m<Calculator>(new ToolTip(""))));

            var seg2 = window.Screen.CreateStandardSegment();
            seg2.AddInterpretter(TextInputWithKeyListener<Calculator>.Quick(_focus, M.m<Calculator>(x => expression), "Expression",
                new ToolTip(
                    "Math expression\n" +
                    "Type \"help()\" to view a list of functions and variables\n" +
                    "Press <Enter> to evaluate\n" +
                    "Press <Up>/<Down> to use previous inputs\n" +
                    "Use \"let <name> = <value>\" to define custom variable\n" +
                    "The variable \"_\" is the last output"),
                (x, exp) => expression = exp,
                CreateKeyPressEvent(() => Event.current.type == EventType.KeyDown &&
                    (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter), () =>
                    {
                        _focus.Evaluate(expression);
                        expression = "";
                    }),
                CreateKeyPressEvent(KeyCode.UpArrow, true, () => expression = _focus.GetPreviousInput()),
                CreateKeyPressEvent(KeyCode.DownArrow, true, () => expression = _focus.GetNextInput()),
                CreateKeyPressEvent(KeyCode.Insert, true, () => DeactivateGui()),
                CreateKeyPressEvent(KeyCode.Escape, true, () => DeactivateGui())));

            return window;
        }
    }
}
