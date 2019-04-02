using _3rd;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class test : MonoBehaviour {    
    AssetBundleManifest AssetBundleManifest;
    // Use this for initialization
    void Awake()
    {
        
    }
    void Start() {
        Load();
    }
	
    void Load()
    {
        GameObject go= AssetPool.Instance.Test.LoadObject<GameObject>("Arts/Prefabs/Sphere.prefab");
        GameObject.Instantiate(go);
    }
	void Update () {
		
	}
}
