﻿using System;
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
                        recolored.Value.material.shader = recolorable.OriginalMaterial.shader;
                        var sharedMats = recolorable.Renderer.sharedMaterials;
                        sharedMats[recolorable.MaterialIndex] = recolored.Value.material;
                        recolorable.Renderer.sharedMaterials = sharedMats;
                    }
                }
            }
        }

        public class RecolorableRenderer
        {
            public Renderer Renderer;
            public int MaterialIndex = 0;
            public Material OriginalMaterial;
        }
    }
}
