using System;
using System.Collections.Generic;
using BrilliantSkies.Core;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.ScriptableObjects;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Tips;
using UnityEngine;

namespace BuildingTools
{
    public class CodeDisplayArea<T> : SubjectiveAbstract<T>
    {
        private IVS<string, T> _fnGetStringCurrently = null;
        private IVS<string, T> _fnGetLineNumberCurrently = null;
        private Vector2 scrollPos = Vector2.zero;
        private static SO_LuaEditor _s = LazyLoader.LuaEditor.Get();
        private int lastHash;

        public CodeDisplayArea(T subject, IVS<string, T> fnGetStringCurrently, IVS<string, T> fnGetLineNumberCurrently, IVS<string, T> displayString, IVS<IToolTip, T> toolTip,
            params string[] keys) : base(subject, displayString, toolTip, keys)
        {
            _fnGetStringCurrently = fnGetStringCurrently;
            _fnGetLineNumberCurrently = fnGetLineNumberCurrently;
        }

        public override void Draw(SO_BuiltUi styles)
        {
            string text = _fnGetStringCurrently.GetFromSubject(Subject);
            var hash = text.GetHashCode();
            if (hash != lastHash)
            {
                lastHash = hash;
                scrollPos.y = Mathf.Infinity;
            }

            GUIContent content = new GUIContent(_fnGetStringCurrently.GetFromSubject(Subject));

            var monoFont = _s.STYLE_EditArea.font;
            var codeStyle = styles.TextEdit.TextEnter.Style;
            codeStyle.font = monoFont;

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Join("\n", _fnGetLineNumberCurrently.GetFromSubject(Subject)), codeStyle, GUILayout.ExpandWidth(false));

            GUI.SetNextControlName("CalculatorLog");
            GUILayout.TextArea(text, codeStyle, GUILayout.ExpandWidth(true));
            
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        public override string GetKeyString(string key)
        {
            return string.Format("{0}<size=10>[your text here]</size>", key);
        }

        protected override bool ProcessInputOnceKeyFound(List<string> wordsInInputExcludingKey, bool apply, ref string outcome)
        {
            return false;
        }
    }
}
