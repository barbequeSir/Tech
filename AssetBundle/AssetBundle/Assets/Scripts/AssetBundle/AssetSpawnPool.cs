using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void LoadCallBackHandler(HandlerParam p_handleParam);
public delegate void ResourceCallBackHander(object blso, string bundleResName, UnityEngine.Object obj, LoadCallBackHandler loadCallBack, HandlerParam HP, bool loadAssets);
public delegate void LoadObjectHander(string _assetPath, LoadCallBackHandler loadCallBack, HandlerParam HP,string spawnName = "", bool loadAssets = true);
public delegate UnityEngine.Object LoadBundleObjectHander(string _assetPath);

public class HandlerParam
{
    public bool isSucess;                       //是否加载成功
    public string assetName;                    // 资源名
    public System.Object paramObj;              // 回调参数组
    //public AssetBundleData m_assetBundleData;   // 加载数据
    public UnityEngine.Object assetObj;        // 加载资源
    public int count;
    public string fullAssetName;                //


    public HandlerParam()
    {
        assetName = string.Empty;
        paramObj = null;
      //  m_assetBundleData = null;
        assetObj = null;
        count = 0;
        fullAssetName = string.Empty;
    }
}

/// <summary>
/// 资源生成池
/// </summary>
public class AssetSpawnPool
{
    private string spawnName;
    private string assetPath;
    private string assetExtension;
    private bool isUsePool;
    private bool isAsynchLoad;

    /// <summary>
    /// 调试 AB 使用：true 调试 AB
    /// </summary>
    public static bool isDebug = false;

    public AssetSpawnPool(string spawnName = "")
    {
        this.spawnName = spawnName;
    }

    /// <summary>
    /// 资源路径
    /// </summary>
    public string AssetPath
    {
        get { return assetPath; }
        set { assetPath = value; }
    }

    /// <summary>
    /// 资源扩展名
    /// </summary>
    public string AssetExtension
    {
        get { return assetExtension; }
        set { assetExtension = value; }
    }

    /// <summary>
    /// 是否使用内存池
    /// </summary>
    public bool IsUsePool
    {
        get { return isUsePool; }
        set { isUsePool = value; }
    }

    /// <summary>
    /// 是否异步加载
    /// </summary>
    public bool IsAsynchLoad
    {
        get { return isAsynchLoad; }
        set { isAsynchLoad = value; }
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T">加载资源类型</typeparam>
    /// <param name="assetName">资源名</param>
    /// <param name="CallBack">加载回调</param>
    /// <param name="paramsArr">加载传递参数</param>
    /// <param name="useType">加载解包时，是否使用类型 T</param>
    public void LoadAsset<T>(string assetName, LoadCallBackHandler CallBack, System.Object paramsArr, bool useType = false, bool isAsync = true, bool loadAssets = true) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return;
        }
        string _assetPath = string.Format("{0}{1}{2}", AssetPath, assetName, assetExtension);

        HandlerParam HP = new HandlerParam();
        HP.assetName = assetName;
        HP.paramObj = paramsArr;

        LoadAsset<T>(_assetPath, CallBack, HP, loadAssets);
    }

    public void LoadAssetAsync<T>(string assetName, LoadCallBackHandler CallBack, System.Object paramsArr, bool useType = false, bool loadAssets = true) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return;
        }
        string _assetPath = string.Format("{0}{1}{2}", AssetPath, assetName, assetExtension);

        HandlerParam HP = new HandlerParam();
        HP.assetName = assetName;
        HP.paramObj = paramsArr;

        LoadAsyncAsset<T>(_assetPath, CallBack, HP, loadAssets);
    }

    /// <summary>
    /// 加载场景资源
    /// </summary>
    /// <typeparam name="T">加载资源类型</typeparam>
    /// <param name="assetName">资源名</param>
    /// <param name="CallBack">加载回调</param>
    /// <param name="paramsArr">加载传递参数</param>
    /// <param name="useType">加载解包时，是否使用类型 T</param>
    public void LoadScene<T>(string assetName, LoadCallBackHandler CallBack, System.Object paramsArr, bool useType = false, bool isAsync = true, bool loadAssets = true) where T : UnityEngine.Object
    {
        string _assetPath = string.Format("{0}{1}{2}", AssetPath, assetName, assetExtension);

        HandlerParam HP = new HandlerParam();
        HP.assetName = assetName;
        HP.paramObj = paramsArr;

        HP.fullAssetName = _assetPath;

        if (string.IsNullOrEmpty(assetName))
        {
            if(CallBack != null)
            {
                CallBack(HP);
            }
            return;
        }

        
        BundleManager.Instance.LoadObjectAsync(_assetPath, CallBack, HP, loadAssets,true);
    }

    // 从Resource文件夹下异步加载资源
    private void LoadAsyncAsset<T>(string _assetPath, LoadCallBackHandler loadCallBack, HandlerParam HP, bool loadAssets) where T : UnityEngine.Object
    {
        //HP.fullAssetName = _assetPath;
        //BundleManager.Instance.LoadObjectAsync(_assetPath, loadCallBack, HP, loadAssets);
        writeDownloadedFileList(_assetPath);

        HP.fullAssetName = _assetPath;
        UnityEngine.Object obj = BundleManager.Instance.LoadObject(_assetPath, loadAssets);
        if (obj == null)
        {
            string fileName = Path.GetFileNameWithoutExtension(_assetPath);
            _assetPath = _assetPath.Replace(fileName, spawnName + "_Default");
            obj = BundleManager.Instance.LoadObject(_assetPath, true);
        }
        HP.assetObj = obj;
        loadCallBack(HP);
    }


    // 从 Resrouce 文件夹下加载资源
    private void LoadAsset<T>(string _assetPath, LoadCallBackHandler loadCallBack, HandlerParam HP, bool loadAssets) where T : UnityEngine.Object
    {
        writeDownloadedFileList(_assetPath);
        HP.fullAssetName = _assetPath;
        UnityEngine.Object obj = BundleManager.Instance.LoadObject(_assetPath, loadAssets);
        if (obj == null)
        {
            string fileName = Path.GetFileNameWithoutExtension(_assetPath);
            _assetPath = _assetPath.Replace(fileName, spawnName + "_Default");
            obj = BundleManager.Instance.LoadObject(_assetPath, true);
        }
        HP.assetObj = obj;
        loadCallBack(HP);
        //AssetBundleManager.Instance.SyncLoadBundleCall(_assetPath, loadCallBack, HP, spawnName, loadAssets);
    }

    public T LoadObject<T>(string _AssetPathAndName ) where T : UnityEngine.Object
    {
        writeDownloadedFileList(_AssetPathAndName);
        return BundleManager.Instance.LoadObject(_AssetPathAndName) as T;
        //AssetBundleManager.Instance.LoadBundleObjectCall(_AssetPathAndName) as T;
    }

    public void UnLoadAssetBundle(string assetName)
    {
        //string _assetPath = string.Format("{0}{1}{2}", AssetPath, assetName, assetExtension);
        //if (BundleManager.Instance == null)
        //{
        //    return;
        //}
        //BundleManager.Instance.UnloadObject(_assetPath.ToLower());
    }

    void writeDownloadedFileList(string fileName)
    {
		#if UNITY_EDITOR
		string path = string.Format("{0}LoadResources.txt", Application.dataPath + "/../");
		File.AppendAllText(path, "\r\n" + fileName);
		#endif
    }

    public T[] LoadBundleAllAssets<T>(string bundleName) where T : UnityEngine.Object
    {
        return BundleManager.Instance.LoadBundleAllAssets<T>(bundleName);
    }

    public T LoadBundleAssets<T>(string bundleName,string assetName) where T : UnityEngine.Object
    {
        return BundleManager.Instance.LoadBundleAssets<T>(bundleName,assetName);
    }

    public void AddBundleObject(string assetName,UnityEngine.Object obj)
    {
        BundleManager.Instance.AddBundleObject(assetName, obj);
    }
   
}
