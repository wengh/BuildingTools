using System;
using System.Collections.Generic;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Unity;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Texts;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Tips;

namespace BuildingTools
{
    public class TextInputWithEvent<T> : TextInput<T>
    {
        private IEnumerable<KeyPressEvent> events;

        public TextInputWithEvent(T subject, IVS<string, T> fnGetStringCurrently, IVS<string, T> displayString,
            IVS<IToolTip, T> toolTip, Action<T, string> actionToDo, IEnumerable<KeyPressEvent> events, Func<T, string, string> effectOfAction,
            Func<string, string> stringCleaner, Func<T, string, string> stringChecker, params string[] keys) :
            base(subject, fnGetStringCurrently, displayString, toolTip, actionToDo, effectOfAction, stringCleaner, stringChecker, keys)
            => this.events = events;

        public static TextInputWithEvent<T> Quick(T subject, IVS<string, T> getString, string label, ToolTip tip,
            Action<T, string> changeAction, params KeyPressEvent[] events) => new TextInputWithEvent<T>(subject, getString, M.m<T>(label), M.m<T>(tip), changeAction, events, null, s => s, (x, s) => null);

        public override void Draw(SO_BuiltUi styles)
        {
            foreach (var ev in events)
            {
                ev.CheckAndCallEvents();
            }

            base.Draw(styles);
        }
    }
}
