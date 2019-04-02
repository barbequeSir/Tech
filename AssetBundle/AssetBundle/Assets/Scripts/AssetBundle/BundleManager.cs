using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.Profiling;

//public delegate void LoadCallBackHandler(HandlerParam p_handleParam);
//public delegate void ResourceCallBackHander(object blso, string bundleResName, UnityEngine.Object obj, LoadCallBackHandler loadCallBack, HandlerParam HP, bool loadAssets);

//public class HandlerParam
//{
//    public bool isSucess;                       //是否加载成功
//    public string assetName;                    // 资源名
//    public System.Object paramObj;              // 回调参数组    
//    public UnityEngine.Object assetObj;        // 加载资源
//    public int count;
//    public string fullAssetName;                //


//    public HandlerParam()
//    {
//        assetName = string.Empty;
//        paramObj = null;        
//        assetObj = null;
//        count = 0;
//        fullAssetName = string.Empty;
//    }
//}

public class BundleManager: MonoBehaviour
{
    private LinkedList<ObjectPair> mReleaseLinkedList = new LinkedList<ObjectPair>();
    private Dictionary<string, HashSet<UnityEngine.Object>> nBundleObjectAssetList = new Dictionary<string, HashSet<Object>>();

    private static Dictionary<string, List<string>> mUITextureDicList = new Dictionary<string, List<string>>();

    private HashSet<string> m_DependenciesSet;
    private BundleLoader mAssetLoader = new BundleLoader();

	public static BundleManager Instance = null;

	public static bool LoadBundle = true;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }       
        mAssetLoader.LoadManifest(BundleConfig.mainManifestFile);
        string platform = string.Empty;        
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void LoadAsyncAsset(string _assetPath, LoadCallBackHandler loadCallBack, HandlerParam HP,string pp,bool loadAssets)
    {
        LoadObjectAsync(_assetPath, loadCallBack, HP, loadAssets);
    }


    // 从 Resrouce 文件夹下加载资源
    private void LoadAsset(string _assetPath, LoadCallBackHandler loadCallBack, HandlerParam HP,string spawnName, bool loadAssets)
    {
        UnityEngine.Object obj = LoadObject(_assetPath, loadAssets);
        if (obj == null)
        {
            string fileName = Path.GetFileNameWithoutExtension(_assetPath);
            _assetPath = _assetPath.Replace(fileName, spawnName + "_Default");
            obj = BundleManager.Instance.LoadObject(_assetPath, true);
        }
        HP.assetObj = obj;
        loadCallBack(HP);
    }

    public void Update()
    {
        _UnloadUpdate();
    }

    public string wwwPath
    {
        get;
        set;
    }

    public UnityEngine.Object LoadObject(string bundleName)
    {
        return LoadObject(bundleName, true);
    }
    public UnityEngine.Object LoadObject(string bundleName, bool loadAssets = true)
    {
#if UNITY_EDITOR
        if (LoadBundle)
        {
            return mAssetLoader.LoadAssetBundle(bundleName, loadAssets);
        }
        UnityEngine.Object obj = null;

        obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(string.Format("Assets/Arts/{1}", bundleName));
        if (obj != null)
        {
            return obj;
        }
        
        return obj;
#else
        return mAssetLoader.LoadAssetBundle(bundleName, loadAssets);
#endif
    }

    public T[] LoadBundleAllAssets<T>(string bundleName, bool loadAssets = true) where T : Object
    {
        return mAssetLoader.LoadBundleAllAssets<T>(bundleName);
    }

    public T LoadBundleAssets<T>(string bundleName, string assetName) where T : Object
    {
#if UNITY_EDITOR
        if (LoadBundle)
        {
            return mAssetLoader.LoadBundleAssets<T>(bundleName, assetName);
        }
        Object obj = null;
       
        obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(string.Format("Assets/Arts/{1}/{2}", Path.GetDirectoryName(bundleName) , assetName));
        if (obj != null)
        {
            return obj as T ;
        }
        
        return null;
#else
        return mAssetLoader.LoadBundleAssets<T>(bundleName, assetName);
#endif
    }

    public T[] LoadAllBundleAssets<T>(string bundleName) where T : Object
    {
        string path = string.Format("Assets/{0}/", Path.GetDirectoryName(bundleName));
#if UNITY_EDITOR
        if (LoadBundle)
        {
            mAssetLoader.LoadAssetBundle(bundleName, false);
            return mAssetLoader.LoadBundleAllAssets<T>(bundleName);
        }
        List<string> allAssets = GetAllFilePath(path, Path.GetExtension(bundleName));
        string assetsName;
        T[] obj = new T[allAssets.Count];
        for (int i = 0; i < allAssets.Count; i++)
        {
            assetsName = allAssets[i].Replace("\\", "/").Replace(Application.dataPath, "Assets");
            obj[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetsName);
        }
        return obj;
#else
        mAssetLoader.LoadAssetBundle(bundleName, false);
        return mAssetLoader.LoadBundleAllAssets<T>(bundleName);
#endif
    }

    public void LoadObjectAsync(string bundleName, LoadCallBackHandler loadCallBack, HandlerParam HP,bool loadAssets,bool isScene = false)
    {
#if UNITY_EDITOR
        if (LoadBundle)
        {
            mAssetLoader.LoadAssetAsyncBundle(bundleName, loadCallBack, HP, loadAssets, isScene);
            return;
        }
        
        UnityEngine.Object obj = null;
       
        obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(string.Format("Assets/Arts/{1}",  bundleName));
        if (obj != null)
        {
            HP.assetObj = obj;
            loadCallBack(HP);
            return;
        }
        
        loadCallBack(HP);
#else
        mAssetLoader.LoadAssetAsyncBundle(bundleName, loadCallBack, HP, loadAssets, isScene);
#endif
    }

    public void UnloadObject(string bundleName, UnityEngine.Object obj, bool delRes = true)
    {
        mReleaseLinkedList.AddLast(new ObjectPair(bundleName, obj, delRes));
    }

    public void UnloadObject(string bundleName, bool delRes = true)
    {
        mReleaseLinkedList.AddLast(new ObjectPair(bundleName, null, delRes));
    }

    public void UnloadAllBundle()
    {
        mAssetLoader.UnloadAllAssetBundle();
    }

    public void AddBundleObject(string assetName,UnityEngine.Object obj)
    {
        if(nBundleObjectAssetList.ContainsKey(assetName))
        {
            if (nBundleObjectAssetList[assetName].Contains(obj)) return;
            nBundleObjectAssetList[assetName].Add(obj);
        }
        else
        {
            nBundleObjectAssetList.Add(assetName, new HashSet<Object> { obj });
        }
    }

    private void _UnloadUpdate()
    {
        List<string> nBundleObjectKeys = new List<string>(nBundleObjectAssetList.Keys);
        int nCount = nBundleObjectKeys.Count;
        for (int i = 0; i < nCount; i++)
        {
            nBundleObjectAssetList[nBundleObjectKeys[i]].RemoveWhere(item =>
            {
                if (item == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });

            if (nBundleObjectAssetList[nBundleObjectKeys[i]].Count == 0)
            {
                UnloadObject(nBundleObjectKeys[i]);
                nBundleObjectAssetList.Remove(nBundleObjectKeys[i]);
            }
        }
        LinkedListNode<ObjectPair> node = mReleaseLinkedList.First;
        while (node != null)
        {
            LinkedListNode<ObjectPair> nextNode = node.Next;

            float timeLeft = BundleConfig.limitedTimeEachFrame;
            float begin = Time.realtimeSinceStartup;
            {//具体逻辑
                ObjectPair op = node.Value;
                mReleaseLinkedList.Remove(node);
                if (node.Value != null)
                {
                    if (node.Value.mDelRes && node.Value.mObject != null)
                    {
                        GameObject.Destroy(node.Value.mObject);
                    }
                    mAssetLoader.UnloadAssetBundle(node.Value.mResName, node.Value.mDelRes);
                }
                
            }
            timeLeft -= (Time.realtimeSinceStartup - begin);
            if (timeLeft < 0.0f)
            {
                break;
            }
            
            node = nextNode;
        }
    }

    private List<string> GetAllFilePath(string path, string target)//搜索文件夹中的文件
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        return _GetAllFilePath(dir, target);
    }

    private List<string> _GetAllFilePath(DirectoryInfo dir, string target)//搜索文件夹中的文件
    {
        List<string> FileList = new List<string>();

        FileInfo[] allFile = dir.GetFiles();
        foreach (FileInfo fi in allFile)
        {
            if ((fi.Extension.ToLower() != BundleConfig.metaExtension && fi.Extension.ToLower() != ".cs" && target == "All")
                || fi.Extension.ToLower() == target)
            {
                FileList.Add(fi.FullName);
            }
        }

        DirectoryInfo[] allDir = dir.GetDirectories();
        foreach (DirectoryInfo d in allDir)
        {
            FileList.AddRange(_GetAllFilePath(d, target));
        }
        return FileList;
    }

    public static GameObject LoadUI(string szBundleName)
    {
        UnityEngine.Object obj = Instance.LoadObject(szBundleName + ".data");
        if (obj == null)
        {
            obj = Resources.Load("UI/" + szBundleName);
        }
        GameObject retObj = null;
        if (obj != null)
        {
            retObj = obj as GameObject;
        }

        return retObj;
    }

    // 当UI销毁时调用，如果未调用可能引起资源泄露
    public static void OnUIDestroy(string szBundleName, UnityEngine.Object obj)
    {
        Instance.UnloadObject(szBundleName, obj);
        ReleaseUITexture(szBundleName);
    }

    public static void ReleaseUITexture(string szBundleName)
    {
        if (string.IsNullOrEmpty(szBundleName))
        {
            szBundleName = BundleConfig.unkownUIName;
        }
        List<string> uiTextureList = null;
        if (mUITextureDicList.TryGetValue(szBundleName, out uiTextureList))
        {
            if (uiTextureList != null)
            {
                List<string>.Enumerator etor = uiTextureList.GetEnumerator();
                while (etor.MoveNext())
                {
                    Instance.UnloadObject(etor.Current, null);
                }
                uiTextureList.Clear();
            }
            mUITextureDicList.Remove(szBundleName);
        }
    }

    public static void ReleaseUITextureByTextureName(string uiName, string texName)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            uiName = BundleConfig.unkownUIName;
        }
        List<string> uiTextureList = null;
        if (mUITextureDicList.TryGetValue(uiName, out uiTextureList))
        {
            if (uiTextureList != null)
            {
                int listCont = uiTextureList.Count;
                for (int i = 0; i < listCont; ++i)
                {
                    if (uiTextureList[i] == texName)
                    {
                        uiTextureList.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    public static void ReleaseAllUITexture()
    {
        Dictionary<string, List<string>>.Enumerator etorDic = mUITextureDicList.GetEnumerator();
        while (etorDic.MoveNext())
        {
            List<string>.Enumerator etorList = etorDic.Current.Value.GetEnumerator();
            while (etorList.MoveNext())
            {
                Instance.UnloadObject(etorList.Current, null);
            }
            etorDic.Current.Value.Clear();
        }
        mUITextureDicList.Clear();
    }
}
