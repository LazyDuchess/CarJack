using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CarJack.Common.WhipRemix;

[CustomEditor(typeof(RecolorableCar))]
public class RecolorableCarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var recolorableMaterialsProp = serializedObject.FindProperty("RecolorableMaterials");
        var recolorableCar = target as RecolorableCar;
        if (GUILayout.Button("Clear"))
        {
            recolorableMaterialsProp.ClearArray();
        }
        if (GUILayout.Button("Make all Materials recolorable"))
        {
            recolorableMaterialsProp.ClearArray();

            var materialSet = new HashSet<Material>();
            var renderers = recolorableCar.GetComponentsInChildren<Renderer>();

            foreach(var renderer in renderers)
            {
                var sharedMats = renderer.sharedMaterials;
                foreach(var material in sharedMats)
                {
                    var mainTex = material.GetTexture("_MainTex");
                    var emission = material.GetTexture("_Emission");

                    if (mainTex == null && emission == null) continue;

                    materialSet.Add(material);
                }
            }

            foreach (var material in materialSet)
            {
                var mainTex = material.GetTexture("_MainTex");
                var emission = material.GetTexture("_Emission");

                if (mainTex != null)
                {
                    var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mainTex)) as TextureImporter;
                    if (!importer.isReadable)
                    {
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                    }
                }

                if (emission != null)
                {
                    var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(emission)) as TextureImporter;
                    if (!importer.isReadable)
                    {
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                    }
                }

                recolorableMaterialsProp.InsertArrayElementAtIndex(0);
                recolorableMaterialsProp.GetArrayElementAtIndex(0).objectReferenceValue = material;
            }

            AssetDatabase.Refresh();
        }
        serializedObject.ApplyModifiedProperties();
        DrawDefaultInspector();
    }
}
