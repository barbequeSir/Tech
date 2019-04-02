using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Build :Editor {    
    [MenuItem("Tools/AssetBundle/Win64")]
    public static void Bundle_Win64()
    {
        BundleMaker.BuildAssets(BundleConfig.assetsPathRoot, false, BundleConfig.developOutputPath, BuildTarget.StandaloneWindows64);
    }
    [MenuItem("Tools/AssetBundle/Android")]
    public static void Bundle_Android()
    {
        BundleMaker.BuildAssets(BundleConfig.assetsPathRoot, false, BundleConfig.developOutputPath, BuildTarget.Android);
    }
}
