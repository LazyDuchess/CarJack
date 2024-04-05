using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BundleBuilder : MonoBehaviour
{
    [MenuItem("CarJack/Build Asset Bundles")]
    private static void BuildAssetBundle()
    {
        Directory.CreateDirectory("Build");
        BuildPipeline.BuildAssetBundles("Build", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
