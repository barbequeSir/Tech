using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Security.Cryptography;
using System.Text;

public class BundleMaker : Editor
{

    public class BundleFileInfo
    {
        public BundleFileInfo(string _md5, long _size/*, int _level, string _zdata_path, string _zdata_md5, long _zdata_szie*/)
        {
            md5 = _md5;
            size = _size;
            downloadsize = 0;
            //level = _level;
            //zip_path = _zdata_path;
            //zip_md5 = _zdata_md5;
            //zip_size = _zdata_szie;
        }
        public string md5;
        public long size;
        public long downloadsize;
    }

    public static void BuildAssets(string rootPath, bool buildScene, string outPath, BuildTarget buildTarget, bool isMakeBundle = true, bool isIncludeBundle = true)
    {
        if (isMakeBundle)
        {

            //删除就是目录
            if (Directory.Exists(outPath))
            {
                Directory.Delete(outPath, true);
            }
            Directory.CreateDirectory(outPath);

            ListedResource nr = new ListedResource();
            //AssetBundleBuild[] list = nr.ListedAllAndPublicAsset(rootPath, buildScene);
            AssetBundleBuild[] list = nr.ListAllAssets(rootPath, buildScene);
            //创建ab包
            BuildPipeline.BuildAssetBundles(outPath, list, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, buildTarget);
            //deleteAllManifestFile(outPath, outPath);
        }
       GenerateInfoData(isMakeBundle,isIncludeBundle, outPath);
        
    }

    private static void deleteAllManifestFile(string targetPath,string basePath)
    {
        if (!Directory.Exists(targetPath))
        {
            return;
        }
        string[] files = Directory.GetFiles(targetPath);
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i].Replace("\\", "/");
            if (filePath.EndsWith(".manifest") || filePath.EndsWith(".manifest.meta"))
            {
                string newFileFullPath = basePath + "../Manifest/" + filePath.Replace(basePath,"");
                string dirPath = Path.GetDirectoryName(newFileFullPath);
                if(Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.Copy(filePath, newFileFullPath,true);
                File.Delete(filePath);
            }
        }

        string[] dirs = Directory.GetDirectories(targetPath);
        for (int i = 0; i < dirs.Length; i++)
        {
            deleteAllManifestFile(dirs[i].Replace("\\", "/"), basePath);
        }
    }
    
    public static void copyFileToStream(string sourceFile, string destFile)
    {
        string strPath = Path.GetDirectoryName(destFile);
        if (!Directory.Exists(strPath))
        {
            Directory.CreateDirectory(strPath);
        }
        if(File.Exists(destFile))
        {
            File.Copy(sourceFile, destFile, true);
        }
        else
        {
            File.Copy(sourceFile, destFile);
        }
    }

    public static void GenerateInfoData(bool isMakeBundle,bool isIncludeBundle,string outpath)
    {
        if (isMakeBundle)
        {
            if (Directory.Exists(BundleConfig.appOutputPath))
            {
                Directory.Delete(BundleConfig.appOutputPath, true);
            }
            Directory.CreateDirectory(BundleConfig.appOutputPath);

            
            Dictionary<int, Dictionary<string, BundleMaker.BundleFileInfo>> dicGenerateFiles = new Dictionary<int, Dictionary<string, BundleMaker.BundleFileInfo>>();
            List<string> fileList = new List<string>();
            getAllPartFileList(Application.dataPath, BundleConfig.assetsPathRoot + "/", fileList);
            getAllDependenciesPath(fileList);         
            AddFileToInfo(0, fileList, outpath, dicGenerateFiles);

            string resourcesVersion = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            List<int> keylist = new List<int>(dicGenerateFiles.Keys);
            for (int i = 0; i < keylist.Count; i++)
            {
                string buildVersionPath = string.Format("{0}{1}", outpath,BundleConfig.resourceVersionFileName);
                if (File.Exists(buildVersionPath)) System.IO.File.Delete(buildVersionPath);
                string doc = resourcesVersion + "\n";
                
                doc += GetBundMainFestDoc(BundleConfig.mainManifestFile, outpath, false);

                string namePath = string.Format("{0}_{1}.zip", BundleConfig.allResourceZipFileName, System.DateTime.Now.ToString("yyyyMMddHHmmss")).ToLower();
                string zipdir = outpath + "/";
                string zipDoc = "";
                if (Directory.Exists(zipdir))
                {
                    //ZipHelper.Compress(zipdir, outpath + namePath);
                    zipDoc += GetBundMainFestDoc(namePath, outpath, false);
                }
                doc += zipDoc;
                
                doc += getVersionTxt(dicGenerateFiles[i]);

                WriteStringToFile(buildVersionPath, doc,false);
            }
      
            //copyFileToStream(outpath + string.Format(BundleConfig.resourceVersionFileName, (int)BundlePart.BundlePart0), BundleConfig.appOutputPath + string.Format(BundleConfig.resourceOldVersionFileName, (int)BundlePart.BundlePart0));
           // copyFileToStream(outpath + BundleConfig.mainManifestFile, BundleConfig.appOutputPath + BundleConfig.mainManifestFile);
        }        
    }

    static string GetBundMainFestDoc(string fileName,string outputPath,bool isCopy)
    {
        string fullPath = outputPath + fileName;
        if (File.Exists(fullPath) == false)
        {
            return "";
        }

        FileInfo curFileInfo = new FileInfo(fullPath);
        long size = curFileInfo.Length;
        string md5 = GetMD5Hash(fullPath);

        StringBuilder resultDoc = new StringBuilder();

        resultDoc.AppendFormat("{0}\t{1}\t{2}\n", fileName, md5, size);
        return resultDoc.ToString();
    }

    public static void getAllPartFileList(string rootPath, string curPath, List<string> fileList)
    {
        if (!Directory.Exists(curPath))
        {
            return;
        }

        string[] strLocalFile = System.IO.Directory.GetFiles(curPath);
        foreach (string curFile in strLocalFile)
        {
            FileInfo curFileInfo = new FileInfo(curFile);
            string relPath = curFile.Replace('\\', '/');
            relPath = relPath.Replace(Application.dataPath, "Assets");
            if(Path.GetExtension(relPath) == ".meta" || Path.GetExtension(relPath) == ".cs")
            {
                continue;
            }
            if (fileList.Contains(relPath) == false)
            {
                fileList.Add(relPath);
            }
            
        }
        string[] strLocalPath = System.IO.Directory.GetDirectories(curPath);
        foreach (string curSubPath in strLocalPath)
        {
            getAllPartFileList(rootPath, curSubPath, fileList);
        }
    }

    public static void getAllDependenciesPath(List<string> fileList)
    {
        List<string> assetList = new List<string>(fileList);
        for(int i = 0; i < assetList.Count; i++)
        {            
             getDependenciesPath(assetList[i], fileList);
        }
    }

    public static void getDependenciesPath(string fileName, List<string> fileList)
    {
        string[] names = AssetDatabase.GetDependencies(new string[] { fileName });
        for (int i = 0; i < names.Length; i++)
        {
            if (fileList.Contains(names[i]) == false && Path.GetExtension(names[i]) != ".cs")
            {
                fileList.Add(names[i]);
                getDependenciesPath(names[i], fileList);
            }
        }
    }

    public static void AddFileToInfo(int part,List<string> fileList, string curPath, Dictionary<int, Dictionary<string, BundleFileInfo>> curDic)
    {
        if (curDic.ContainsKey(part) == false)
        {
            curDic.Add(part, new Dictionary<string, BundleFileInfo>());
        }

        foreach (string curFile in fileList)
        {
			string subPath = curFile + ".data";
            subPath = subPath.Replace("Assets/", "");
			string fullPath = curPath + subPath.ToLower();

            if (File.Exists(fullPath) == false)
            {
                continue;
            }
            FileInfo curFileInfo = new FileInfo(fullPath);
            long size = curFileInfo.Length;
            string md5 = GetMD5Hash(curPath + subPath.ToLower());

            //copyFileToStream(curPath + subPath.ToLower(), curPath +"Back/"  + subPath.ToLower());
            curDic[part].Add(subPath, new BundleFileInfo(md5, size));
        }
    }

    public static string GetMD5(string str)
    {
        byte[] b = System.Text.Encoding.Default.GetBytes(str);

        b = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(b);
        string ret = "";
        for (int i = 0; i < b.Length; i++)
        {
            ret += b[i].ToString("x").PadLeft(2, '0');
        }
        return ret;
    }

    // 获取MD5
    public static string GetMD5Hash(string pathName)
    {

        string strResult = "";
        string strHashData = "";
#if !UNITY_WP8
        byte[] arrbytHashValue;
#endif
        System.IO.FileStream oFileStream = null;
        MD5CryptoServiceProvider oMD5Hasher = new MD5CryptoServiceProvider();
        try
        {
            oFileStream = new System.IO.FileStream(pathName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
#if UNITY_WP8
                strHashData = oMD5Hasher.ComputeHash(oFileStream);
                oFileStream.Close();
#else
            arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);
            oFileStream.Close();
            strHashData = System.BitConverter.ToString(arrbytHashValue);
            strHashData = strHashData.Replace("-", "");
#endif

            strResult = strHashData;
        }
        catch (System.Exception ex)
        {
            
        }

        return strResult;
    }

    static string getVersionTxt(Dictionary<string, BundleFileInfo> dicFileInfo)
    {
        Dictionary<string, BundleFileInfo>.Enumerator iter = dicFileInfo.GetEnumerator();
        StringBuilder resultDoc = new StringBuilder();
        while (iter.MoveNext())
        {
            resultDoc.AppendFormat("{0}\t{1}\t{2}\n", iter.Current.Key, GetMD5(iter.Current.Value.md5 + iter.Current.Value.size), iter.Current.Value.size);
        }
        return resultDoc.ToString();
    }

    public static void unZipStream(string path)
    {
        FileStream fstream = new FileStream(path, FileMode.Open, FileAccess.Read);
        
        System.IO.Compression.GZipStream gstream = new System.IO.Compression.GZipStream(fstream, System.IO.Compression.CompressionMode.Decompress);
        StreamReader reader = new StreamReader(gstream);
        string cstring = reader.ReadToEnd();
        WriteStringToFile(path + ".bak",cstring,false);
        reader.Close();
        gstream.Close();
        fstream.Close();
    }

    public static void WriteStringToFile(string path, string text,bool isZip = true)
    {
        try
        {
            FileStream fstream = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter swriter;
            System.IO.Compression.GZipStream gstream = null;
            if (isZip)
            {
                gstream = new System.IO.Compression.GZipStream(fstream, System.IO.Compression.CompressionMode.Compress);
                swriter = new StreamWriter(gstream);

                string bakFile = Path.GetDirectoryName(path) + "/../Manifest/" + Path.GetFileName(path);
                FileStream fs = new FileStream(bakFile, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(text);

                sw.Close();
                fs.Close();
            }
            else
            {
                swriter = new StreamWriter(fstream);
            }
            swriter.Write(text.ToString());
            swriter.Close();
            if(gstream != null) gstream.Close();
            fstream.Close();
        }
        catch (Exception ex)
        {
            throw new Exception("Create resources list error + " + ex.ToString());
        }

    }

    static void CopyDirectory(string srcDir, string tgtDir)
    {
        DirectoryInfo source = new DirectoryInfo(srcDir);
        DirectoryInfo target = new DirectoryInfo(tgtDir);

        if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new Exception("父目录不能拷贝到子目录！");
        }

        if (!source.Exists)
        {
            return;
        }

        if (!target.Exists)
        {
            target.Create();
        }

        FileInfo[] files = source.GetFiles();

        for (int i = 0; i < files.Length; i++)
        {
            File.Copy(files[i].FullName, target.FullName + @"\" + files[i].Name, true);
        }

        DirectoryInfo[] dirs = source.GetDirectories();

        for (int j = 0; j < dirs.Length; j++)
        {
            CopyDirectory(dirs[j].FullName, target.FullName + @"\" + dirs[j].Name);
        }
    }
}
