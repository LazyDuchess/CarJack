using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common.WhipRemix
{
    [RequireComponent(typeof(DrivableCar))]
    public class RecolorableCar : MonoBehaviour
    {
        public Material[] RecolorableMaterials;
        [NonSerialized]
        public DrivableCar Car;
        [NonSerialized]
        public Recolor CurrentRecolor = null;
        private List<RecolorableRenderer> _recolorableRenderers;

        private void Awake()
        {
            Car = GetComponent<DrivableCar>();
            _recolorableRenderers = new();
            var renderers = GetComponentsInChildren<Renderer>();
            foreach(var renderer in renderers)
            {
                var sharedMats = renderer.sharedMaterials;
                for(var i = 0; i < sharedMats.Length; i++)
                {
                    var sharedMat = sharedMats[i];
                    for(var n = 0; n < RecolorableMaterials.Length; n++)
                    {
                        var recolorableMat = RecolorableMaterials[n];
                        if (sharedMat == recolorableMat)
                        {
                            var recolorable = new RecolorableRenderer();
                            recolorable.Renderer = renderer;
                            recolorable.MaterialIndex = i;
                            recolorable.OriginalMaterial = recolorableMat;
                            _recolorableRenderers.Add(recolorable);
                        }
                    }
                }
            }
        }

        public void ApplyDefaultColor()
        {
            CurrentRecolor = null;
            foreach(var recolorable in _recolorableRenderers)
            {
                var sharedMats = recolorable.Renderer.sharedMaterials;
                sharedMats[recolorable.MaterialIndex] = recolorable.OriginalMaterial;
                recolorable.Renderer.sharedMaterials = sharedMats;
            }
        }

        public void ApplyRecolor(Recolor recolor)
        {
            CurrentRecolor = recolor;
            foreach (var recolorable in _recolorableRenderers)
            {
                foreach(var recolored in recolor.RecoloredMaterialByName)
                {
                    if (recolored.Key == recolorable.OriginalMaterial.name)
                    {
                        var mat = recolored.Value.Material;
                        if (mat == null)
                        {
                            mat = new Material(recolorable.OriginalMaterial);
                            mat.SetTexture("_MainTex", recolored.Value.MainTexture);
                            mat.SetTexture("_Emission", recolored.Value.EmissionTexture);
                            mat.shader = recolorable.OriginalMaterial.shader;
                            recolored.Value.Material = mat;
                            recolor.AddResourceToCleanUp(mat);
                        }
                        var sharedMats = recolorable.Renderer.sharedMaterials;
                        sharedMats[recolorable.MaterialIndex] = mat;
                        recolorable.Renderer.sharedMaterials = sharedMats;
                    }
                }
            }
        }

        public void ApplySavedRecolor()
        {
#if PLUGIN
            var saveData = RecolorSaveData.Instance;
            if (saveData == null) return;
            var recolorGUID = saveData.GetRecolorGUIDForCar(Car.InternalName);
            if (string.IsNullOrEmpty(recolorGUID))
                ApplyDefaultColor();
            else
            {
                if (RecolorManager.RecolorsByGUID.TryGetValue(recolorGUID, out var result))
                {
                    if (result.Properties.CarInternalName == Car.InternalName)
                        ApplyRecolor(result);
                    else
                        ApplyDefaultColor();
                }
                else
                    ApplyDefaultColor();
            }
#endif
        }

        public class RecolorableRenderer
        {
            public Renderer Renderer;
            public int MaterialIndex = 0;
            public Material OriginalMaterial;
        }
    }
}
