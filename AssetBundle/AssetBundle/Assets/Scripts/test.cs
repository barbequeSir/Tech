using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class test : MonoBehaviour {    
    AssetBundleManifest abm;
    // Use this for initialization

    void Start() {
        StartCoroutine(GetText());
    }

    IEnumerator GetText() {
        UnityWebRequest www = UnityWebRequest.Get("file:///D:/SVN/Tech/AssetBundle/Http/WWWRoot/Window/BundleAssets/versionfile.txt");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            byte[] results = www.downloadHandler.data;

            ///AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
        }
    }   
	
	// Update is called once per frame
	void Update () {
		
	}
}
