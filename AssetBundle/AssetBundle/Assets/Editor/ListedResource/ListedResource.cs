using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;


//Prefab为最小单位，不将不同的perfab打包到一个bundle中
public class ListedResource
{
    private BundleList m_BundleList;

    public ListedResource()
    {
        m_BundleList = new BundleList();
    }
    private void _UnnamedAllResource(string rootpath)
    {
        List<string> reslist = BundleUtility.GetAllFilePath(rootpath, BundleConfig.metaExtension);
        foreach (string path in reslist)
        {
            BundleUtility.NamedMetaFile(path, "");
        }
    }

    private void _ListAllAssets(List<string> allAssets, Dictionary<string, List<string>> cList)
    {
        string[] strArray = new string[1];
        foreach (string path in allAssets)
        {
            strArray[0] = path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
            string[] deparry = AssetDatabase.GetDependencies(strArray);
            List<string> deps = BundleUtility.RemoveTargetAndScript(deparry, strArray[0]);
            if (strArray[0].IndexOf(BundleConfig.prefabBundlePath) >= 0)
            {
                m_BundleList.AddResourceAndDependencies(strArray[0], deps,false);
            }
            else
            {
                m_BundleList.AddResourceAndDependencies(strArray[0], deps, true);
            }
        }
    }

    private void _ListAllBundle(string rootPath, string fileEnd, Dictionary<string, List<string>> cList)
    {
        if (string.IsNullOrEmpty(fileEnd) || string.IsNullOrEmpty(rootPath))
        {
            return;
        }
       
        List<string> allPerfab = BundleUtility.GetAllFilePath(rootPath, fileEnd);
        _ListAllAssets(allPerfab,cList);
    }

    private void _ListAllSceneBundle()
    {
        //Dictionary<int, List<Tab_SceneClass>> tabSceneClass = TableManager.GetSceneClass();
        //List<string> allScene = new List<string>();

        //Dictionary<int, List<Tab_SceneClass>>.Enumerator sceneClassTableEtor = tabSceneClass.GetEnumerator();
        //while (sceneClassTableEtor.MoveNext())
        //{
        //    string sceneName = sceneClassTableEtor.Current.Value[0].ResName;
        //    string scenePath = string.Format("Assets/Scenes/{0}.unity", sceneName);
        //    if (!allScene.Contains(scenePath) && !sceneName.Contains("Login") && !sceneName.Contains("Loading"))
        //    {
        //        allScene.Add(scenePath);
        //    }
        //}
       // _ListAllAssets(allScene);
    }

    public AssetBundleBuild[] ListAllAssets(string rootPath,bool buildScene)
    {
        Dictionary<string, List<string>> containterList = new Dictionary<string, List<string>>();
        _ListAllBundle(rootPath, "All", containterList);

        List<AssetBundleBuild> result = new List<AssetBundleBuild>();

        DirectoryInfo DirInfo = new DirectoryInfo(rootPath);
        foreach(DirectoryInfo di in DirInfo.GetDirectories())
        {
            if(di.FullName.EndsWith(BundleConfig.allinone_suffix))
            {
                AssetBundleBuild abb = new AssetBundleBuild();
                string fileName = di.FullName.Replace("\\","/").Replace(Application.dataPath, "Assets");                
                m_BundleList.RemoveRepeatBundleAndDep(fileName);
                List<string> allPerfab = BundleUtility.GetAllFilePath(di.FullName, "All");

                for (int i = 0; i < allPerfab.Count; i++)
                {
                    allPerfab[i] = allPerfab[i].Replace("\\", "/").Replace(Application.dataPath, "Assets");
                }
                abb.assetNames = allPerfab.ToArray();
                abb.assetBundleName = ProcessBundleName(fileName);
                result.Add(abb);                
            }
        }

        foreach (string asset in m_BundleList)
        {
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetNames = new string[] { asset };
            abb.assetBundleName = ProcessBundleName(asset);
            result.Add(abb);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        return result.ToArray();
    }

    //命名全部的perfab和perfab依赖的公共的资源，非公共资源不单独打包，最大限度降低碎片度
    public AssetBundleBuild[] ListedAllAndPublicAsset(string rootPath, bool buildScene)
    {
        _UnnamedAllResource(rootPath);
        
        Dictionary<string, List<string>> containterList = new Dictionary<string, List<string>>();
        _ListAllBundle(rootPath, "All", containterList);

        foreach (string fileEnd in BundleConfig.assetPrefab)
        {
           
            m_BundleList.RemoveRepeatBundleAndDep("Assets" + fileEnd);
            _ListAllBundle(Application.dataPath + fileEnd, ".prefab", new Dictionary<string, List<string>>());
        }

        AssetBundleBuild[] ConfigBundleArray = new AssetBundleBuild[BundleConfig.tableAssetsPathRoot.Length];

        int j = 0;
        foreach (BundlePathConfigPair pair in BundleConfig.tableAssetsPathRoot)
        {
            m_BundleList.RemoveRepeatBundleAndDep("Assets" + pair.mAssetPath);
            List<string> allPerfab = BundleUtility.GetAllFilePath(Application.dataPath + pair.mAssetPath, "All");

            for (int i = 0; i < allPerfab.Count; i++)
            {
                allPerfab[i] = allPerfab[i].Replace("\\", "/").Replace(Application.dataPath, "Assets");
            }
            ConfigBundleArray[j].assetNames = allPerfab.ToArray();
            ConfigBundleArray[j].assetBundleName = pair.mBundleName + ".data";
            ++j;
        }

        j = 0;
        AssetBundleBuild[] abbArray = new AssetBundleBuild[m_BundleList.Count];
    
        foreach (string asset in m_BundleList)
        {
            abbArray[j].assetNames = new string[] { asset };
            abbArray[j].assetBundleName = ProcessBundleName(asset);
           
            //abbArray[i].assetBundleVariant = ProcessBundleName(asset);
            ++j;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        List<AssetBundleBuild> resultList = new List<AssetBundleBuild>();
        resultList.AddRange(abbArray);
        resultList.AddRange(ConfigBundleArray);
       // resultList.AddRange(resourceBundleArray);

        //List<AssetBundleBuild> LastResultList = new List<AssetBundleBuild>(resultList);
        //List<string> bundleList = new List<string>();//<AssetBundleBuild>();

        //for(int i = LastResultList.Count -1; i >=0; i--)
        //{
        //    if (bundleList.Contains(LastResultList[i].assetBundleName))
        //    {
        //        resultList.RemoveAt(i);
        //    }
        //    else
        //    {
        //        bundleList.Add(LastResultList[i].assetBundleName);
        //    }
        //}
        return resultList.ToArray();
    }

    private string tableBundleName(string asset)
    {
        string retAsset = Path.GetDirectoryName(asset);
        retAsset = retAsset.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        retAsset = retAsset.Replace("Assets/", "") + "/table.bytes.data";
        return retAsset.ToLower();
    }

    private string ProcessBundleName(string asset)
    {
        string retAsset = "";
        if(asset.EndsWith(BundleConfig.allinone_suffix))
        {
            asset = asset.Substring(0, asset.Length - 1 - BundleConfig.allinone_suffix.Length);
        }
        else if(asset.EndsWith(BundleConfig.onebyone_suffix))
        {
            asset = asset.Substring(0, asset.Length - 1 - BundleConfig.onebyone_suffix.Length);
        }
        
        retAsset = asset.Replace(".", "_");
        retAsset = asset.Replace("Assets/", "") + ".data";
        return retAsset;
    }
}
