using BrilliantSkies.Ftd.Avatar.Items;
using BrilliantSkies.PlayerProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuildingTools
{
    public class InstantBinoculars : Binoculars
    {
        public void Zoom(float rate)
        {

            zoom += rate * stepPerSecond * Time.fixedUnscaledDeltaTime;
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            var module = ProfileManager.Instance.GetModule<MViewAndControl>();
            module.SetFovFactor(1f / zoom);
            module.SetMouseSpeedFactor(1f / zoom);
        }

        public new void Update()
        {
            if (GameSpeedManager.Instance.GamePaused) // Paused by F11
            {
                if (Input.GetMouseButton(0))
                    Zoom(1);
                if (Input.GetMouseButton(1))
                    Zoom(-1);
            }
            base.Update();
        }
    }
}
