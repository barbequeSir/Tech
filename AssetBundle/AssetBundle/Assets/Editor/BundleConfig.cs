using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BundlePathPair
{
    public string mAssetPath { get; private set; }
    public string mBundlePath { get; private set; }

    public BundlePathPair(string assetPath, string bundlePath)
    {
        mAssetPath = assetPath;
        mBundlePath = bundlePath;
    }
}

public class BundlePathConfigPair
{
    public string mAssetPath { get; private set; }
    public string mBundleName { get; private set; }
    public int mBundlePart { get; private set; }

    public BundlePathConfigPair(string assetPath, string bundleName,int BundlePart)
    {
        mAssetPath = assetPath;
        mBundleName = bundleName;
        mBundlePart = BundlePart;
    }
}

public class BundleConfig
{
    public static string allinone_suffix = "allinone";
    public static string onebyone_suffix = "onebyone";
    public static string MainFolderName = "Arts";
    public static string[] assetPrefab = new string[] 
    {
        //"/DefaultResources/NewAtlas"
    };
    public const string metaExtension = ".meta";
    public const string assetNameIndex = "  assetBundleName: ";

    public const string unkownUIName = "_load_texture_unkonw_uiname";
    public const string dynamicAtlasUIName = "_load_texture_by_dynamic_atlas";

    public const int stringBuildCapacity = 256;
    public const float bundleObjectCacheTime = 60.0f;
    public const float limitedTimeEachFrame = 0.015f;

    public const string tableBundleName = "Table/bytes/table.bytes";
    public const string flagBundleName = "Image/Icon_Flag/icon_flag.png";


    public static string assetsPathRoot = Application.dataPath + "/"+ BundleConfig.MainFolderName+"/";
    public static string sceneAssetsPathRoot = Application.dataPath + "/Scenes/";
    public static BundlePathConfigPair[] tableAssetsPathRoot = new BundlePathConfigPair[] 
    {
        //new BundlePathConfigPair("/Table/bytes/", tableBundleName, 0),
        //new BundlePathConfigPair("/Main/Part0/Image/Icon_Flag/",flagBundleName,0)
    };

    public static string prefabBundlePath = "/Prefabs/";
    public static string localPathRoot = Application.persistentDataPath + "/BundleAssets/";
    public static string localDownloadPathRoot = Application.persistentDataPath + "/temp/";
    public static string localDownloadObbPath;

#if UNITY_ANDROID
    public static string developOutputPath = Application.dataPath + "/../_Developer/Android/BundleAssets/";
#elif UNITY_IOS
    public static string developOutputPath = Application.dataPath + "/../_Developer/IOS/BundleAssets/";
#else
    public static string developOutputPath = Application.dataPath + "/../_Developer/Windows/BundleAssets/";
#endif
    public static string appOutputPath = Application.streamingAssetsPath + "/BundleAssets/";
    public static string updateOutputPath = Application.dataPath + "/../Release/BundleAssets/";

    public static string apkPath
    {
        //  /data/app/com.zw.zombieworld.gp-1.apk
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.Tole.lmchat.MainActivity"))
        {
            string apkassetsPath = jc.CallStatic<string>("getApkAssetsPath");
            return "jar:file://" + apkassetsPath + "!/assets/BundleAssets/";
        }
#else 
            return Application.streamingAssetsPath + "/BundleAssets/";
#endif
        }
   }
    
    public static string mainManifestFile = "BundleAssets";

    public static string developOutputPath_Android = Application.dataPath + "/../_Developer/Android/BundleAssets/";
    public static string developOutputPath_Ios = Application.dataPath + "/../_Developer/IOS/BundleAssets/";
    public static string developOutputPath_Windows = Application.dataPath + "/../_Developer/Windows/BundleAssets/";

    public static string resourceVersionFileName = "versionfile.txt";
    public static string allResourceZipFileName = "BundleAssets";
}
