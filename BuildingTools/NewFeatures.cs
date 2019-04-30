using BrilliantSkies.Core.ChangeControl;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Special.PopUps;
using BrilliantSkies.Ui.Special.PopUps.Internal;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingTools
{
    public class ReceivedFeatures : ProfileModule<ReceivedFeatures.InternalData>
    {
        public override ModuleType ModuleType => ModuleType.PlayerProgress;

        protected override string FilenameAndExtension => "profile.receivedfeaturesBT";

        public List<Change> Changes
        {
            get {
                if (_changes == null)
                {
                    ConfigurationManagement.Instance.SetDefaults();
                    _changes = ConfigurationManagement.Instance.GetSelected();
                }
                return _changes;
            }
        }
        private List<Change> _changes = null;

        public V LatestVersion
        {
            get
            {
                if (_latestVersion == default)
                {
                    ConfigurationManagement.Instance.SetDefaults();
                    _latestVersion = Changes.Max().Version;
                }
                return _latestVersion;
            }
        }
        private V _latestVersion = default;

        protected override void Presave()
        {
            Internal.Version = LatestVersion;
            Internal.Time = DateTime.Now;
            Internal.ReceivedFeatures = (
                from change in Changes
                where change.Version == LatestVersion
                select change.Description).ToArray();
        }

        public IEnumerable<Change> GetNewFeatures()
        {
            Debug.Log("GetNewFeatures()");
            Debug.Log(string.Join("\n", Changes.Select(x => x.ToString())));
            Debug.Log(Internal.Version);
            Debug.Log(Internal.Time);
            return
                from change in Changes
                where change.Version.CompareTo(Internal.Version) >= 0 // change.Version >= Internal.version
                where !Internal.ReceivedFeatures.Contains(change.Description)
                select change;
        }

        public string FormatChange(Change c)
        {
            return
                $"<color=lime>{c.DateOfRelease.ToString("yyyy-MM-dd")}</color>    " +
                $"<color=yellow>{c.Version}</color>\t" +
                $"<color=cyan>{c.Type}</color>\t" +
                $"<color=orange>{c.Tag}</color>  :  " +
                $"{c.Description}";
        }

        public string GetNewFeaturesString()
        {
            return string.Join("\n", GetNewFeatures().Select(x => FormatChange(x)));
        }

        public void ShowPopup()
        {
            var newFeatures = GetNewFeatures();
            if (!newFeatures.Any()) return;
            GuiPopUp.Instance.Add(new NewFeaturePopup(
                $"Changes since {Internal.Time.ToString("yyyy-MM-dd HH:mm")} (FtD v{Internal.Version})",
                newFeatures));
        }

        public override void PostLoad(LoadStatus loadStatus)
        {
            Debug.Log(loadStatus);
        }

        public class InternalData
        {
            public DateTime Time { get; set; } = new DateTime(2014, 8, 7);
            public V Version { get; set; } = new V(0, 0, 0);
            public string[] ReceivedFeatures { get; set; } = new string[0];
        }

        public class NewFeaturePopup : AbstractPopup<PopSimple>
        {
            protected override int Width => 1100;
            protected override int Height => 700;

            protected Change[] changes;

            public NewFeaturePopup(string title, IEnumerable<Change> changes) : base(title, new PopSimple())
            {
                this.changes = changes.OrderByDescending(x => x.DateOfRelease).ToArray();
            }

            protected override void AddContentToWindow(ConsoleWindow window)
            {
                var seg0 = window.Screen.CreateTableSegment(5, changes.Length);

                seg0.SqueezeTable = true;
                seg0.SetColumnHeadings(new string[]
                {
                    "Version",
                    "Date",
                    "Type",
                    "Component",
                    "Description"
                });
                seg0.BackgroundStyleWhereApplicable = _s.Segments.OptionalSegmentDarkBackground.Style;
                seg0.eTableOrder = ScreenSegmentTable.TableOrder.Rows;

                foreach (var c in changes)
                {
                    var label = seg0.AddInterpretter(StringDisplay.Quick(
                        $"<color=lime>{c.Version}</color>",
                        "The version this change was released in"));
                    label.WrapText = false;
                    label.PrescribedWidth = new FractionalSizing(0.03f, Dimension.Width);

                    label = seg0.AddInterpretter(StringDisplay.Quick(
                        $"<color=yellow>{c.DateOfRelease.ToString("yyyy-MM-dd")}</color>",
                        "The date of release of this version"));
                    label.WrapText = false;
                    label.PrescribedWidth = new FractionalSizing(0.04f, Dimension.Width);

                    label = seg0.AddInterpretter(StringDisplay.Quick(
                        $"<color=cyan>{c.Type}</color>",
                        "The type of change"));
                    label.WrapText = false;
                    label.PrescribedWidth = new FractionalSizing(0.04f, Dimension.Width);

                    label = seg0.AddInterpretter(StringDisplay.Quick(
                        $"<color=orange>{c.Tag}</color>",
                        "The component which has been changed"));
                    label.WrapText = false;
                    label.PrescribedWidth = new FractionalSizing(0.1f, Dimension.Width);

                    label = seg0.AddInterpretter(StringDisplay.Quick(
                        c.Description.ToString(),
                        "The description of change"));
                    label.WrapText = true;
                    label.Justify = TextAnchor.MiddleLeft;
                    //.PrescribedWidth = new FractionalSizing(0.4f, Dimension.Width);
                }

                window.Screen.CreateSpace(0);

                var seg1 = window.Screen.CreateStandardHorizontalSegment();
                //seg1.SqueezeSides = true;
                seg1.AddInterpretter(Button.Quick("Continue", new ToolTip("Close this popup"), () => _focus.Do()));
            }
        }

        public class PopSimple : Pop
        {
            public void Do() => Done = true;
        }
    }
}
