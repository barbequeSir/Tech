using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using _3rd;

public class BundleLoaderSharedObject
{
    private string mBundleName = "";
    private string assName = "";
    private int mReferance = 0;    //引用计数
    public AssetBundle mAssetBundle = null;   //assetbundle
    private UnityEngine.Object mMainObject = null; //资源Obj
    private bool mIsLoad = false;
    private Dictionary<string,UnityEngine.Object> mAssetObjectList = new Dictionary<string, Object>(); 

    public BundleLoaderSharedObject()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        mBundleName = "";
        assName = "";
        mReferance = 0;
        mAssetObjectList.Clear();
        mAssetBundle = null;
        mMainObject = null;
        mIsLoad = false;
    }

    public bool Load(string bundleName, bool loadAssets)
    {
        try
        {
            mBundleName = bundleName;
            mAssetBundle = BundleLoader.LoadBundleFromSDCard(mBundleName);
            if (mAssetBundle == null)
            {
                //Logger.ErrorLog("SharedLoaderObject.Load Error!!! LoadFromFile Fild: Path:{0}", bundlePath);
                return false;
            }
            int startIdx = mBundleName.LastIndexOf("/");
            int endIdx = mBundleName.LastIndexOf(".");
            assName = mBundleName.Substring(startIdx + 1, endIdx - startIdx - 1);

            if (loadAssets)
            {
                mMainObject = mAssetBundle.LoadAsset(assName);

            }

            return true;
            
        }
        catch
        {
            return false;
        }
    }

    public IEnumerator LoadAsync(BundleLoader bundleloader, BundleLoaderSharedObject blso, string bundleResName, LoadCallBackHandler loadCallBack, ResourceCallBackHander resCallBack, HandlerParam HP, bool loadAssets)
    {
        mBundleName = bundleResName;
        AssetBundleCreateRequest bundleRequest;
        string path = BundleLoader.LoadBundleAsyncFromSDCard(mBundleName);
        if (File.Exists(path))
        {
            bundleRequest = AssetBundle.LoadFromFileAsync(path);
            yield return bundleRequest;
            mAssetBundle = bundleRequest.assetBundle;
        }

        if (mAssetBundle == null)
        {
			#if UNITY_ANDROID && !_TEST
            if (BundleLoader.mStreamBundleList.Contains(mBundleName))
			#endif
            {
                bundleRequest = BundleLoader.LoadBundleAsyncFromStreamingAssets(mBundleName);
                yield return bundleRequest;
                mAssetBundle = bundleRequest.assetBundle;
            }

#if UNITY_ANDROID
            if (mAssetBundle == null)
            {
                bundleRequest = BundleLoader.LoadBundleAsyncFromApkStreamingAssets(mBundleName);
                yield return bundleRequest;
                mAssetBundle = bundleRequest.assetBundle;
            }
#endif

            if (mAssetBundle == null)
            {
               // DebugConsole.sendmsgServer(mBundleName + "not loaded!");
                bundleloader.RemoveAssetBundle(mBundleName);
                BundleManager.Instance.StartCoroutine(BundleLoader.LoadBundleFromWWW(bundleResName));
            }
        }

        int startIdx = mBundleName.LastIndexOf("/");
        int endIdx = mBundleName.LastIndexOf(".");
        try
        {

            assName = mBundleName.Substring(startIdx + 1, endIdx - startIdx - 1);
        }
        catch(System.Exception e)
        {
        }

        if (loadAssets && loadCallBack != null)
        {
            mMainObject = mAssetBundle.LoadAsset(assName);
        }

        if (resCallBack != null)
        {
            resCallBack(blso, bundleResName, mAssetBundle, loadCallBack, HP, loadAssets);
        }

    }

    public UnityEngine.Object AssetObject(bool loadAssets)
    {
        ++mReferance;
		if (loadAssets && mMainObject == null && mAssetBundle != null) 
		{
			mMainObject = mAssetBundle.LoadAsset(assName);
		}
        return mMainObject;
    }

    public UnityEngine.Object AssetObject(string _assetName)
    {
        if (mAssetBundle != null)
        {
            if(mAssetObjectList.ContainsKey(_assetName))
            {
                return mAssetObjectList[_assetName];
            }
            UnityEngine.Object obj = mAssetBundle.LoadAsset(_assetName);
            mAssetObjectList.Add(_assetName, obj);
            return obj;
        }
        return null;
    }

    public void TryRelease(BundleLoader assetLoader, bool unloadFlag)
    {
        if (0 >= --mReferance)
        {
            assetLoader.RemoveAssetBundle(mBundleName);
            mAssetBundle.Unload(unloadFlag);
            CleanUp();
        }
    }

    //此接口慎用
    public void Release()
    {
        mReferance = 0;
        if (mAssetBundle != null)
        {
            mAssetBundle.Unload(true);
            CleanUp();
        }
    }
}
