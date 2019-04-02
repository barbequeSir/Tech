using _3rd;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VersionCheck : MonoBehaviour {

	// Use this for initialization
	void Start () {
        EDebug.Enable = true;
        string versionFile = BundleConfig.PersistentDataPath + BundleConfig.resourceOldVersionFileName;
        if(!File.Exists(versionFile))
        {
            StartCoroutine( FirstLogin());
        }
        else
        {
            CheckVersion();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void GameStart()
    {
        this.gameObject.AddComponent<BundleManager>();
        this.gameObject.AddComponent<test>();
    }

    IEnumerator FirstLogin()
    {
        EDebug.Log("firstLogin");
        yield return StartCoroutine(CopyVersionFileFromStreamingAssetsToPersistentDataPath());
        yield return StartCoroutine(CopyUnZipBundle());
        GameStart();
    }
    IEnumerator CopyVersionFileFromStreamingAssetsToPersistentDataPath()
    {
        string url = BundleConfig.StreamingAssetPath + BundleConfig.resourceVersionFileName;
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
        {
            EDebug.Log("copy versionfile from streamingasset to persistentdatapath error:" + www.error);
            yield break;
        }
        string destUrl = BundleConfig.PersistentDataPath + BundleConfig.resourceOldVersionFileName;
        File.WriteAllBytes(destUrl, www.bytes);
    }
    IEnumerator CopyUnZipBundle()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(BundleConfig.StreamingAssetPath);
        foreach(FileInfo di in dirInfo.GetFiles())
        {
            if(di.Name.EndsWith(".zip"))
            {
                ZipHelper.Decompress(di.FullName, BundleConfig.PersistentDataPath, null);
            }
        }
        yield return null;
    }

    void CheckVersion()
    {
        EDebug.Log("check version");
        GameStart();
    }
}
