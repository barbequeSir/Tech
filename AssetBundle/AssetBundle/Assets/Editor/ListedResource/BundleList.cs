using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BundleList
{
    private HashSet<string> m_BundleList;//需要打包的资源
    private HashSet<string> m_DependenciesSet;//依赖资源表

    public int Count
    {
        get { return m_BundleList.Count; }
    }

    public BundleList()
    {
        m_BundleList = new HashSet<string>();
        m_DependenciesSet = new HashSet<string>();
    }
    private bool _AddDependec(string dep)
    {
        if (!m_DependenciesSet.Contains(dep))
        {
            m_DependenciesSet.Add(dep);
            return true;
        }
        return false;
    }

    public void RemoveRepeatBundleAndDep(string res)
    {
        m_BundleList.RemoveWhere(item =>
        {
            if (item.IndexOf(res) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        });
        m_DependenciesSet.RemoveWhere(item => 
        {
            if (item.IndexOf(res) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        });
    }
    
    public void AddResourceAndDependencies(string res, List<string> deps,bool addDep)
    {
        if (!m_BundleList.Contains(res))
        {
            m_BundleList.Add(res);
            //if (addDep == false)
            //    return;
            foreach (string dep in deps)
            {
                if (_AddDependec(dep))
                {
                    m_BundleList.Add(dep);
                }
            }
        }
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string item in m_BundleList)
        {
            yield return item;
        }
    }
}
