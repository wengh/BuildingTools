using System;
using System.Linq;
using System.Text;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Unity;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Texts;
using BrilliantSkies.Ui.Layouts.DropDowns;
using BrilliantSkies.Ui.Tips;
using UnityEngine;
using static BuildingTools.BuildingToolsPlugin;

namespace BuildingTools
{
    public class CalculatorUI : ConsoleUi<Calculator>
    {
        public CalculatorUI(Calculator focus) : base(focus) { }
        public string expression = "";

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            var window = NewWindow(135316, "Calculator", WindowSizing.GetSizeCentral(0.5f, 0.8f));
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
                CreateKeyPressEventUniversal(ts =>
                    {
                        _focus.Evaluate(expression);
                        expression = "";
                    }, KeyCode.Return, KeyCode.KeypadEnter),
                CreateKeyPressEventUniversal(ts => expression = _focus.GetPreviousInput(), KeyCode.UpArrow),
                CreateKeyPressEventUniversal(ts => expression = _focus.GetNextInput(), KeyCode.DownArrow),
                CreateKeyPressEventUniversal(ts => DeactivateGui(), BtKeyMap.Instance.GetKeyDef(KeyInputsBt.Calculator).Key),
                CreateKeyPressEventUniversal(ts => DeactivateGui(), KeyCode.Escape)
            ));

            return window;
        }
    }
}
