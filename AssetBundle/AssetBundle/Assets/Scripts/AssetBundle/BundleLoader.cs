using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BundleLoader
{
    AssetBundleManifest mAssetBundleManifest = null;
    StringBuilder mStringBuilder = null;
    Dictionary<string, BundleLoaderSharedObject> mBundleLoaderSharedObjectDict = null;
    public static List<string> mStreamBundleList = new List<string>();

    public BundleLoader()
    {
        mAssetBundleManifest = null;

        mStringBuilder = new StringBuilder(BundleConfig.stringBuildCapacity);

        mBundleLoaderSharedObjectDict = new Dictionary<string, BundleLoaderSharedObject>();
    }

    public void LoadManifest(string manifestName)
    {
        AssetBundle assetBundle = LoadBundleFromSDCard(manifestName);
        if (assetBundle == null)
        {
            return;
        }
        mAssetBundleManifest = assetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        assetBundle.Unload(false);
    }

    public UnityEngine.Object LoadAssetBundle(string bundleName, bool loadAsset = true)
    {
        UnityEngine.Object obj;
        return LoadAssetBundle(bundleName, out obj, loadAsset);
    }

    //异步加载资源，首先加载依赖项
    public void LoadAssetAsyncBundle(string bundleName, LoadCallBackHandler loadCallBack, HandlerParam HP,bool loadAssets = true,bool isScene = false)
    {
        if (mAssetBundleManifest == null)
        {
            HP.isSucess = false;
            if(loadCallBack != null) loadCallBack(HP);
            return;
        }
        bundleName = HP.fullAssetName;
        if (Path.GetExtension(HP.fullAssetName) != ".data")
        {
            bundleName = HP.fullAssetName + ".data";
        }
        bundleName = bundleName.ToLower();

        LoadAssetBundleDependenciesAsync(bundleName);

        BundleLoaderSharedObject blso;
        bool isHave = mBundleLoaderSharedObjectDict.TryGetValue(bundleName, out blso);
        if (!isHave)
        {
            blso = new BundleLoaderSharedObject();
            mBundleLoaderSharedObjectDict.Add(bundleName, blso);
            ResourceCallBackHander resCallBack = ResourceLoadCall;
            if (string.IsNullOrEmpty(bundleName))
            {
                int nn = 0;
            }
            BundleManager.Instance.StartCoroutine(blso.LoadAsync(this, blso, bundleName, loadCallBack, resCallBack, HP, loadAssets));


            return;
        }

        if (blso != null )
        {
            if (blso.mAssetBundle == null) return;
            HP.isSucess = true;
            if(isScene == false)
                HP.assetObj = blso.AssetObject(true);
            if (loadCallBack != null) loadCallBack(HP);
            return;
        }
    }

    //依赖项递归查找,查找所有依赖项
    private void LoadAssetBundleDependenciesAsync(string bundleResName)
    {
        string[] dependBundles = mAssetBundleManifest.GetAllDependencies(bundleResName);
        int dependCount = dependBundles.Length;
        for (int i = 0; i < dependCount; ++i)
        {
            if(string.IsNullOrEmpty(dependBundles[i]))
            {
                continue;
            }
            if (mBundleLoaderSharedObjectDict.ContainsKey(dependBundles[i]) == false)
            {
                BundleLoaderSharedObject blso = new BundleLoaderSharedObject();
                mBundleLoaderSharedObjectDict.Add(dependBundles[i], blso);
                LoadAssetBundleDependenciesAsync(dependBundles[i]);
                BundleManager.Instance.StartCoroutine(blso.LoadAsync(this,blso, dependBundles[i], null, null, null,false));
            }
        }
    }

    //资源异步加载回调
    private void ResourceLoadCall(object blso, string bundleResName, UnityEngine.Object obj, LoadCallBackHandler loadCallBack, HandlerParam HP, bool loadAssets)
    {
        if (obj != null)
        {
            HP.isSucess = true;
            HP.assetObj = ((BundleLoaderSharedObject)blso).AssetObject(loadAssets);
            if (loadCallBack != null) loadCallBack(HP);
        }
        else
        {
            HP.isSucess = false;
            if (loadCallBack != null) loadCallBack(HP);
        }
    }

    //同步加载资源
    public UnityEngine.Object LoadAssetBundle(string bundleName,out UnityEngine.Object obj, bool loadAsset = true)
    {
        if (mAssetBundleManifest == null || string.IsNullOrEmpty(bundleName))
        {
            obj = null;
            return obj;
        }
        
        string bundleResName = bundleName;
        if (Path.GetExtension(bundleName) != ".data")
        {
            bundleResName = bundleName + ".data";
        }
        bundleResName = bundleResName.ToLower();

        //check dependant
        LoadAssetBundleDependencies(bundleResName);
        
        BundleLoaderSharedObject blso;
        if (!mBundleLoaderSharedObjectDict.TryGetValue(bundleResName, out blso))
        {
            blso = new BundleLoaderSharedObject();
            if (blso.Load(bundleResName, loadAsset))
            {
                mBundleLoaderSharedObjectDict.Add(bundleResName, blso);
            }
            else
            {
                obj = null;
                return obj;
            }
        }

        if (blso != null)
        {
			obj = blso.AssetObject(loadAsset);
            return obj;
        }
        obj = null;
        return obj;
    }

    public T[] LoadBundleAllAssets<T>(string bundleName) where T : Object
    {
        string bundleResName = bundleName;
        if (Path.GetExtension(bundleName) != ".data")
        {
            bundleResName = bundleName + ".data";
        }
        bundleResName = bundleResName.ToLower();
        BundleLoaderSharedObject blso;
        if (mBundleLoaderSharedObjectDict.TryGetValue(bundleResName, out blso))
        {
            if(blso.mAssetBundle != null)
            {
                return blso.mAssetBundle.LoadAllAssets<T>();
            }
        }
        return null;
    }

    public T LoadBundleAssets<T>(string bundleName, string assetName) where T : Object
    {
        string bundleResName = bundleName;
        if (Path.GetExtension(bundleName) != ".data")
        {
            bundleResName = bundleName + ".data";
        }
        bundleResName = bundleResName.ToLower();
        BundleLoaderSharedObject blso;
        if (!mBundleLoaderSharedObjectDict.TryGetValue(bundleResName, out blso))
        {
            blso = new BundleLoaderSharedObject();
            if (blso.Load(bundleResName, false))
            {
                mBundleLoaderSharedObjectDict.Add(bundleResName, blso);
            }
            else
            {
                return null;
            }
        }

        if (blso != null)
        {
            return blso.AssetObject(assetName) as T;
        }
        return null;
    }

    private void LoadAssetBundleDependencies(string bundleResName)
    {
        string[] dependBundles = mAssetBundleManifest.GetAllDependencies(bundleResName);
        int dependCount = dependBundles.Length;
        for (int i = 0; i < dependCount; ++i)
        {
            LoadAssetBundle(dependBundles[i], false);
        }
    }

    

    public void UnloadAssetBundle(string bundleName, bool unloadFlag)
    {
        if (mAssetBundleManifest == null)
        {
            return;
        }
        if (Path.GetExtension(bundleName) != ".data")
        {
            bundleName = bundleName + ".data";
        }

        bundleName = bundleName.ToLower();

        string[] dependBundles = mAssetBundleManifest.GetAllDependencies(bundleName);
        int dependCount = dependBundles.Length;
        for (int i = 0; i < dependCount; ++i)
        {
            UnloadAssetBundle(dependBundles[i], unloadFlag);
        }

        BundleLoaderSharedObject blso;
        if (mBundleLoaderSharedObjectDict.TryGetValue(bundleName, out blso))
        {
            blso.TryRelease(this, unloadFlag);
        }
    }

    public void UnloadAllAssetBundle()
    {
        Dictionary<string, BundleLoaderSharedObject>.Enumerator etor = mBundleLoaderSharedObjectDict.GetEnumerator();
        while (etor.MoveNext())
        {
            BundleLoaderSharedObject blso = etor.Current.Value;
            if (blso != null)
            {
                blso.Release();
            }
        }
        mBundleLoaderSharedObjectDict.Clear();
    }

    public void RemoveAssetBundle(string bundleName)
    {
        mBundleLoaderSharedObjectDict.Remove(bundleName);
    }

    //从streaming读取资源
    private static AssetBundle LoadBundleFromStreamingAssets(string bundleName)
    {
        string fullPath = null;
        fullPath = BundleConfig.appOutputPath + bundleName;
        AssetBundle assetBundle = null;
#if UNITY_ANDROID && !_TEST
        if (mStreamBundleList.Contains(bundleName))
#endif
        {
            assetBundle = AssetBundle.LoadFromFile(fullPath);
        }
#if UNITY_ANDROID
        if (assetBundle == null)
        {
            fullPath = BundleConfig.apkPath + bundleName;
#if !_TEST
            if (mStreamBundleList.Contains(bundleName))
#endif
            {
                assetBundle = AssetBundle.LoadFromFile(fullPath);
            }
        }
#endif
            if (assetBundle == null)
        {
            //DebugConsole.sendmsgServer(bundleName + "not loaded!");
            BundleManager.Instance.StartCoroutine(LoadBundleFromWWW(bundleName));
        }
        return assetBundle;
    }

    //从sd卡读取资源
    public static string LoadBundleAsyncFromSDCard(string bundleName)
    {
        string fullPath = null;
#if UNITY_ANDROID || UNITY_IOS
        fullPath = BundleConfig.localPathRoot + bundleName;
#else
    fullPath = BundleConfig.developOutputPath + bundleName;
#endif
        return fullPath;
    }

    public static AssetBundleCreateRequest LoadBundleAsyncFromStreamingAssets(string bundleName)
    {
        string fullPath = null;
        fullPath = BundleConfig.appOutputPath + bundleName;

        return AssetBundle.LoadFromFileAsync(fullPath);
    }

    public static AssetBundleCreateRequest LoadBundleAsyncFromApkStreamingAssets(string bundleName)
    {
        string fullPath = null;
        fullPath = BundleConfig.apkPath + bundleName;

        return AssetBundle.LoadFromFileAsync(fullPath);
    }

    public static AssetBundle LoadBundleFromSDCard(string bundleName)
    {
        try
        {
            string fullPath = null;
//#if UNITY_ANDROID || UNITY_IOS
            fullPath = BundleConfig.localPathRoot + bundleName;
//#else
//            fullPath = BundleConfig.developOutputPath + bundleName;
//#endif
            if (File.Exists(fullPath))
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
                if (assetBundle == null)
                {
                    return LoadBundleFromStreamingAssets(bundleName);
                }
                return assetBundle;
            }
            else
            {
                return LoadBundleFromStreamingAssets(bundleName);
            }
        }
        catch
        {
            return LoadBundleFromStreamingAssets(bundleName);
        }
    }

    public static IEnumerator LoadBundleFromWWW(string bundleName)
    {
        yield break;
        string fullPath = null;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            fullPath = BundleConfig.developOutputPath + bundleName;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            fullPath = BundleConfig.localPathRoot + bundleName;
        }
        else
        {
            fullPath = BundleConfig.localPathRoot + bundleName;
        }

        string wwwPath = BundleManager.Instance.wwwPath + bundleName;
        WWW www = new WWW(wwwPath);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
            yield break;

        File.WriteAllBytes(fullPath, www.bytes);

        //AssetBundle assetBundle = www.assetBundle;
        www.Dispose();  //断开连接
        //return assetBundle;
    }

    //读取配置文件
    public static IEnumerator readVersionText()
    {
        mStreamBundleList.Clear();

        string fullPath = BundleConfig.appOutputPath + BundleConfig.resourceOldVersionFileName;
#if UNITY_EDITOR
        fullPath = "file:///" + fullPath;
#elif UNITY_ANDROID
    fullPath = BundleConfig.apkPath + BundleConfig.resourceOldVersionFileName;
#endif
        WWW www = new WWW(fullPath);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
            yield break;
        string[] lines;
        MemoryStream stream = null;
        System.IO.Compression.GZipStream gstream = null;
        StreamReader reader = null;
        try
        {
            stream = new MemoryStream(www.bytes);

            gstream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            reader = new StreamReader(gstream);
            string cstring = reader.ReadToEnd();
                
            lines = cstring.Split('\n');
        }
        catch
        {
            lines = www.text.Split('\n'); 
        }
        if(reader != null) reader.Close();
        if (stream != null) stream.Close();
        if (gstream != null) gstream.Close();


        for (int i = 1; i < lines.Length; i++)
        {
            string[] infos = lines[i].Split('\t');
            if (infos.Length < 3) continue;
            string fileName = infos[0].ToLower() == BundleConfig.mainManifestFile.ToLower() ? infos[0] : infos[0].ToLower();
            mStreamBundleList.Add(fileName);
        }
    }
    
}
