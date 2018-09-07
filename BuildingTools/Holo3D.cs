using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrilliantSkies.Common.CarriedObjects;
using BrilliantSkies.Core;
using BrilliantSkies.Core.ResourceAccess.Async.Materials;
using BrilliantSkies.FromTheDepths.Game.UserInterfaces;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Layouts;
using BrilliantSkies.Ui.Special.PopUps;
using Newtonsoft.Json;
using UnityEngine;

namespace BuildingTools
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Holo3D : Block, IBlockWithText
    {
        public static List<string> extensions = new List<string> { ".obj" };
        public static List<Shader> shaders;
        public Shader shader;

        public CarriedObjectReference hologram;
        private bool hasHologram = false;
        private bool reloading = false;

        public Vector3 size;
        public float baseScale = 1.5f;
        private string _path = "";
        private bool _enabled = true;

        [JsonProperty]
        public Vector3 scale = Vector3.one;
        [JsonProperty]
        public Vector3 pos = Vector3.zero;
        [JsonProperty]
        public Vector3 rot = Vector3.zero;
        [JsonProperty]
        public bool displayOnStart = false;
        [JsonProperty]
        public bool threaded = false;
        [JsonProperty]
        public string Path
        {
            get => _path;
            set => _path = value.Trim('"', ' ');
        }
        [JsonProperty]
        public string ShaderName
        {
            get => shader.name;
            set => shader = shaders.Find(x => x.name == value);
        }
        [JsonProperty]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                hologram.SetActive(value);
            }
        }

        public int UniqueId { get; set; }

        public bool IsValid(string path)
        {
            path = path.Trim('"', ' ');
            return extensions.Contains(System.IO.Path.GetExtension(path).ToLower()) && File.Exists(path);
        }

        public void ReloadAdv()
        {
            if (reloading)
            {
                GuiPopUp.Instance.Add(new PopupInfo("Alert", "A 3D Hologram is already loading in background."));
                return;
            }
            if (threaded == false)
                Reload();
            else
            {
                reloading = true;
                var reloadTask = Task.Run(() => Reload());
                reloadTask.ContinueWith((x) => reloading = false);
            }
        }

        public void Reload()
        {
            if (!IsValid(_path)) return;
            hologram?.Destroy();
            try
            {
                hologram = CarryThisWithUs(OBJLoader.LoadOBJFile(_path, shader));
                hologram.Ruleset = CarriedObjectReferenceRules.DestroyWhenBlockRemovedDeactivateWhenBlockDead;
                hologram.SetActive(Enabled);
                hasHologram = true;
                size = GetBounds(hologram.ObjectItself).size;
                SetLocalTransform();
            }
            catch (Exception ex)
            {
                BuildingToolsPlugin.ShowError(ex);
                hasHologram = false;
            }
        }

        public void SetLocalTransform()
        {
            if (!hasHologram)
            {
                return;
            }
            hologram?.SetLocalPosition(pos);
            hologram?.SetLocalRotation(Quaternion.Euler(rot));
            hologram?.SetScale(new Vector3(scale.z, scale.z, scale.z) * baseScale);
        }

        public static Bounds GetBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            Bounds bounds = new Bounds();
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        public override void Secondary(Transform T) =>
            //new GenericBlockGUI().ActivateGui(this, GuiActivateType.Stack);
            new Holo3DUI(this).ActivateGui(GuiActivateType.Stack);

        public override InteractionReturn Secondary()
        {
            InteractionReturn interactionReturn = new InteractionReturn
            {
                SpecialNameField = "3D Hologram Projector",
                SpecialBasicDescriptionField = "Can display a 3D model from a local .obj file"
            };

            interactionReturn.AddExtraLine("<Press <<Q>> to modify the hologram>");

            return interactionReturn;
        }

        public override void BlockStart()
        {
            try
            {
                base.BlockStart();
                hologram = CarryEmptyWithUs();
                if (shaders == null || !shaders.Any())
                {
                    var standard = R_VehicleShaders.Blocks.Get();
                    if (BuildingToolsPlugin.bundle != null)
                    {
                        shaders = BuildingToolsPlugin.bundle.LoadAllAssets<Shader>().ToList();
                        shaders.Add(standard);
                    }
                    else
                    {
                        shaders = new List<Shader> { standard };
                    }
                }
                shader = shaders[0];
            }
            catch (Exception ex)
            {
                BuildingToolsPlugin.ShowError(ex);
            }
            if (UniqueId <= 0)
                UniqueId = MainConstruct.iUniqueIdentifierCreator.CheckOutANewUniqueId();
        }
        public override void PlacedAsPrefab() => UniqueId = MainConstruct.iUniqueIdentifierCreator.CheckOutANewUniqueId();

        public override Vector4 GetParameters1()
        {
            if (UniqueId <= 0)
                UniqueId = MainConstruct.iUniqueIdentifierCreator.CheckOutANewUniqueId();
            return new Vector4(0f, 0f, 0f, UniqueId);
        }

        public override void SetParameters1(Vector4 v) => UniqueId = (int)v.w;

        public override void StateChanged(IBlockStateChange change)
        {
            base.StateChanged(change);
            bool initiatedOrInitiatedInUnrepairedState_OnlyCalledOnce = change.InitiatedOrInitiatedInUnrepairedState_OnlyCalledOnce;
            if (initiatedOrInitiatedInUnrepairedState_OnlyCalledOnce)
            {
                GetConstructableOrSubConstructable().iBlocksWithText.BlocksWithText.Add(this);
            }
            else
            {
                bool isPerminentlyRemovedOrConstructDestroyed = change.IsPerminentlyRemovedOrConstructDestroyed;
                if (isPerminentlyRemovedOrConstructDestroyed)
                    GetConstructableOrSubConstructable().iBlocksWithText.BlocksWithText.Remove(this);
            }
        }

        public string SetText(string str, bool sync = true)
        {
            try
            {
                JsonConvert.PopulateObject(str, this);
                if (sync)
                    GetConstructableOrSubConstructable().iMultiplayerSyncroniser.RPCRequest_SyncroniseBlock(this, GetText());
                if (displayOnStart) Reload();
                return Path;
            }
            catch (Exception ex)
            {
                BuildingToolsPlugin.ShowError(ex);
                return "";
            }
        }

        public string GetText() => JsonConvert.SerializeObject(this, new JsonSerializerSettings { ContractResolver = new VectorContractResolver() });
    }
}
