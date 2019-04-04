using _3rd;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VersionCheck : MonoBehaviour {
    string version = "";
    List<string> ResList = new List<string>();
	// Use this for initialization
	void Start () {
        EDebug.Enable = true;
        string versionFile = BundleConfig.PersistentDataPath + BundleConfig.resourceOldVersionFileName;
        if(File.Exists(versionFile))
        {
            File.Delete(versionFile);
        }
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
        EDebug.Log("unzip");
       // yield return StartCoroutine(CopyUnZipBundle());
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
        StreamReader sr = new StreamReader(destUrl);
        version = sr.ReadLine();
        while (!sr.EndOfStream)
        {
            string str = sr.ReadLine();            
            string[] strList = str.Split('\t');
            string fileName = strList[0].ToLower();
                        
            if (fileName.EndsWith(".zip"))
            {
                continue;
            }
            string downloadUrl = BundleConfig.localDownloadPathRoot + fileName;
            EDebug.Log(downloadUrl);
            WWW wwwRes = new WWW(downloadUrl);
            yield return wwwRes;
            if (string.IsNullOrEmpty(wwwRes.error) == false)
            {
                EDebug.Log("download error:" + www.error);
                continue;
            }

            string dir = BundleConfig.PersistentDataPath + fileName.Substring(0, fileName.LastIndexOf('/')+1);
            EDebug.Log("download:"+downloadUrl);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(BundleConfig.PersistentDataPath + fileName,wwwRes.bytes);
        }
        EDebug.Log(version);
        sr.Close();
        sr.Dispose();
        
    }

    IEnumerator CopyUnZipBundle()
    {
        string filename = "bundleassets_" + version + ".zip";

        string url = BundleConfig.StreamingAssetPath + filename;
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
        {
            EDebug.Log("copy versionfile from streamingasset to persistentdatapath error:" + www.error);
            yield break;
        }
        string destUrl = BundleConfig.PersistentDataPath + filename;
        File.WriteAllBytes(destUrl, www.bytes);
        ZipHelper.Decompress(destUrl, BundleConfig.PersistentDataPath, null);                   
        yield return null;
    }

    void CheckVersion()
    {
        EDebug.Log("check version");
        GameStart();
    }
}
