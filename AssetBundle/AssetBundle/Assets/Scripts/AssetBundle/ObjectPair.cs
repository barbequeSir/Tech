using UnityEngine;
using System.Collections;

public class ObjectPair
{
    public float mTime { get; private set; }
    public string mResName { get; private set; }
    public bool mDelRes { get; private set; }
    public UnityEngine.Object mObject { get; private set; }
    

    public ObjectPair(string bundleName, UnityEngine.Object go, bool bDelRes)
    {
        mTime = 0;
        mResName = bundleName;
        mObject = go;
        mDelRes = bDelRes;
    }

    public bool isDead(float deltaTime)
    {
        mTime += deltaTime;
        return BundleConfig.bundleObjectCacheTime < mTime;
    }

    public void Kill()
    {
        mTime += BundleConfig.bundleObjectCacheTime;
    }
}
