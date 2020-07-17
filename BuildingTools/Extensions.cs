
using System;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Unity;
using BrilliantSkies.PlayerProfiles;
using UnityEngine;

namespace BuildingTools
{
    public static class Extensions
    {
        public static Action<ITimeStep> ToDRegularEvent(this KeyPressEvent self) => (ts) => self.CheckAndCallEvents();
    }
}
