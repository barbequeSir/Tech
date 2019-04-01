using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleUtility
{
    public static List<string> GetAllAssetPath(string root, string ext)
    {
        List<string> res = new List<string>();
        string[] patharray = AssetDatabase.GetAllAssetPaths();
        foreach (string path in patharray)
        {
            if (isTargetExtension(path, ext))
            {
                res.Add(path);
            }
        }
        return res;
    }

    public static bool isTargetExtension(string path, string ext)
    {
        bool res = false;
        int index = path.IndexOf('.');
        if (-1 != index)
        {
            string target = path.Substring(path.IndexOf('.'));
            res = ext == target.ToLower();
        }
        return res;
    }
    private static List<string> _GetAllFilePath(DirectoryInfo dir, string target)//搜索文件夹中的文件
    {
        List<string> FileList = new List<string>();

        FileInfo[] allFile = dir.GetFiles();
        foreach (FileInfo fi in allFile)
        {
            if ((fi.Extension.ToLower() != BundleConfig.metaExtension && !fi.Extension.Equals(".DS_Store") && fi.Extension.ToLower() != ".cs" && target == "All")
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

    public static List<string> GetAllFilePath(string path, string target)//搜索文件夹中的文件
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        return _GetAllFilePath(dir, target);
    }

    public static void NamedMetaFile(string path, string name)
    {
        StreamReader fs = new StreamReader(path);
        List<string> ret = new List<string>();
        bool isReplace = false;
        string line;
        while ((line = fs.ReadLine()) != null)
        {
            line = line.Replace("\n", "");
            if (line.IndexOf(BundleConfig.assetNameIndex) != -1)
            {
                if (!string.IsNullOrEmpty(line.Substring(BundleConfig.assetNameIndex.Length).Trim()))
                {
                    line = BundleConfig.assetNameIndex + name.ToLower();
                    isReplace = true;
                }
            }
            ret.Add(line);
        }
        fs.Close();
        fs.Dispose();
        fs = null;
        if (isReplace)
        {
            StreamWriter writer = new StreamWriter(path + ".tmp");
            foreach (var each in ret)
            {
                writer.WriteLine(each);
            }
            writer.Close();
            writer.Dispose();
            writer = null;

            File.Delete(path);
            File.Copy(path + ".tmp", path, true);
            File.Delete(path + ".tmp");
        }
    }

    public static List<string> RemoveTargetAndScript(string[] strs, string target)
    {
        List<string> res = new List<string>();
        foreach (string str in strs)
        {
            if (str != target && !str.EndsWith(".cs") && !str.EndsWith(".js"))
            {
                res.Add(str.Replace("\\", "/"));
            }
        }

        return res;
    }

}
