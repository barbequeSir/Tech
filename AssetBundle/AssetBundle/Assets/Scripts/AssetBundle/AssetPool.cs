using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetPool
{
    public static AssetPool Instance = new AssetPool();

    /// <summary>
    /// 怪物预设
    /// </summary>
    private AssetSpawnPool _Test;

    public AssetPool()
    {
        _Test = new AssetSpawnPool("Npc");
        _Test.AssetPath = "prefabs/";
        _Test.IsUsePool = true;
        _Test.IsAsynchLoad = true;
        _Test.AssetExtension = ".prefab";
    }

    /// <summary>
    /// 怪物预设
    /// </summary>
    public AssetSpawnPool Test { get { return _Test; } }
   
}
