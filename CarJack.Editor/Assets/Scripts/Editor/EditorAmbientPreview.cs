using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EditorAmbientPreview
{
    static EditorAmbientPreview()
    {
        Shader.SetGlobalColor("LightColor", AmbientPreview.GlobalLightColor);
        Shader.SetGlobalColor("ShadowColor", AmbientPreview.GlobalShadowColor);
    }
}