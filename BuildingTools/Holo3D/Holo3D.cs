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
using Object = UnityEngine.Object;

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

        public bool StillSavingStringsLikeThis => true;

        public bool IsValid(string path)
        {
            path = path.Trim('"', ' ');
            return extensions.Contains(System.IO.Path.GetExtension(path).ToLower()) && File.Exists(path);
        }

        public void Reload()
        {
            if (!IsValid(_path)) return;
            hologram.Destroy();
            try
            {
                hologram = CarryThisWithUs(OBJLoader.LoadOBJFile(_path, shader), LevelOfDetail.High) as CarriedObjectReference;
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
            hologram?.SetScale(scale * baseScale);
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
            new Holo3DUI(this).ActivateGui(GuiActivateType.Stack);

        public override InteractionReturn Secondary()
        {
            InteractionReturn interactionReturn = new InteractionReturn
            {
                SpecialNameField = "3D Hologram Projector",
                SpecialBasicDescriptionField = "Can display a 3D model from a local .obj file"
            };

            interactionReturn.AddExtraLine("Press <<Q>> to modify the hologram");

            return interactionReturn;
        }

        public override void BlockStart()
        {
            try
            {
                base.BlockStart();
                hologram = CarryEmptyWithUs(LevelOfDetail.Standard) as CarriedObjectReference;
                if (shaders == null || !shaders.Any())
                {
                    if (BuildingToolsPlugin.bundle != null)
                    {
                        shaders = BuildingToolsPlugin.bundle.LoadAllAssets<Shader>().ToList();
                        shaders.RemoveAll(x => x.name.Contains("AddShader"));
                        shaders.Add(R_VehicleShaders.Blocks.Get());
                        shaders.Add(Shader.Find("Standard"));
                    }
                    else
                    {
                        shaders = new List<Shader> {
                            Shader.Find("Standard"),
                            R_VehicleShaders.Blocks.Get(),
                        };
                    }
                }
                shader = shaders[0];
            }
            catch (Exception ex)
            {
                BuildingToolsPlugin.ShowError(ex);
            }
            if (UniqueId <= 0)
                UniqueId = MainConstruct.UniqueIdsRestricted.CheckOutANewUniqueId();
        }
        public override void PlacedAsPrefab() => UniqueId = MainConstruct.UniqueIdsRestricted.CheckOutANewUniqueId();

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

        public string SetText(string str)
        {
            try
            {
                JsonConvert.PopulateObject(str, this);
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
