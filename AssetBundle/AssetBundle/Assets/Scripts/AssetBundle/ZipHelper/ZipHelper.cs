using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Core;
using UnityEngine;
using System.Collections;

public delegate void unzipProgressDelegate(float progress);
public delegate void ZipFinishUnZipDelegate(bool isSucess,string errorMsg);
public class ZipHelper
{
    public static ZipFinishUnZipDelegate m_finishUnzip;
    private static string errorMsg = "";
    public static void Compress(string sourceFile, string targetPath)
    {
        string tmpPath = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(tmpPath))
        {
            Directory.CreateDirectory(tmpPath);
        }
        FileStream fs = File.Create(targetPath);
        ZipOutputStream s = new ZipOutputStream(fs);

        s.SetLevel(6);
        Compress(sourceFile, ref s, sourceFile);
        s.Finish();
        s.Close();
    }

    public static void CompressFile(string sourceFile, string targetPath)
    {
        string tmpPath = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(tmpPath))
        {
            Directory.CreateDirectory(tmpPath);
        }
        ZipOutputStream s = new ZipOutputStream(File.Create(targetPath));

        s.SetLevel(6);
        CompressFile(sourceFile, ref s, sourceFile);
        s.Finish();
        s.Close();
    }

    /// <summary>
    /// 压缩
    /// </summary>
    /// <param name="source">源目录</param>
    /// <param name="s">ZipOutputStream对象</param>
    public static void Compress(string source, ref ZipOutputStream s,string rootPath)
    {
        string[] filenames = Directory.GetFileSystemEntries(source);
        foreach (string file in filenames)
        {
            if (Directory.Exists(file))
            {
                Compress(file, ref s,rootPath);  //递归压缩子文件夹
            }
            else
            {
                UnityEngine.Debug.Log(file);
               if (file.EndsWith("zip")) continue;
               
                using (FileStream fs = File.OpenRead(file))
                {
                    byte[] buffer = new byte[fs.Length];
                    ZipEntry entry = new ZipEntry(file.Replace(rootPath, ""));     //此处去掉盘符，如D:\123\1.txt 去掉D:
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    s.PutNextEntry(entry);

                    int sourceBytes;
                    do
                    {
                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                        s.Write(buffer, 0, sourceBytes);
                    } while (sourceBytes > 0);
                    
                }
            }
        }
    }

    public static void CompressFile(string sourceFile, ref ZipOutputStream s, string rootPath)
    {
        string file = Path.GetFileName(sourceFile);
        using (FileStream fs = File.OpenRead(sourceFile))
        {
            byte[] buffer = new byte[fs.Length];
            ZipEntry entry = new ZipEntry(file.Replace(rootPath, ""));     //此处去掉盘符，如D:\123\1.txt 去掉D:
            entry.DateTime = DateTime.Now;
            entry.Size = fs.Length;
            s.PutNextEntry(entry);

            int sourceBytes;
            do
            {
                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                s.Write(buffer, 0, sourceBytes);
            } while (sourceBytes > 0);
        }
    }

    /// <summary>
    /// 解压缩
    /// </summary>
    /// <param name="sourceFile">源文件</param>
    /// <param name="targetPath">目标路经</param>
    public static bool Decompress(string sourceFile, string targetPath, unzipProgressDelegate deleg)
    {
        errorMsg = "";
        long zipFileCount = 0;
        using (ZipFile zip = new ZipFile(sourceFile))
        {
            zipFileCount = zip.Count;
            zip.Close();
        }
        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException(string.Format("未能找到文件 '{0}' ", sourceFile));
        }
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        using (ZipInputStream s = new ZipInputStream(File.OpenRead(sourceFile)))
        {
            int unZipCount = 0;
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                try
                {
                    string directorName = Path.Combine(targetPath, Path.GetDirectoryName(theEntry.Name));
                    string fileName = directorName + "/" + Path.GetFileName(theEntry.Name);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    // 创建目录
                    if (Directory.Exists(directorName) == false && directorName.Length > 0)
                    {
                        Directory.CreateDirectory(directorName);
                    }
                    if (fileName != string.Empty)
                    {
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            int size = 4096;
                            byte[] data = new byte[4 * 1024];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else break;
                                //deleg((float)((double)s.Position / s.Length));
                            }
                            streamWriter.Close();
                            unZipCount++;
                        }
                    }
                    if (deleg != null)
                    {
                        deleg((float)((float)unZipCount / (float)zipFileCount));
                    }
                }
                catch(Exception ex)
                {
                    errorMsg = ex.Message;
                    return false;
                }
            }
        }
        return true;
    }

    private delegate bool unzipDecompressionDelegate(string inFile, string outFile, unzipProgressDelegate progress);
    public static void DecompressionAsyn(string inFile, string outFile, unzipProgressDelegate progress)
    {
        unzipDecompressionDelegate mydelegate = new unzipDecompressionDelegate(Decompress);
        IAsyncResult result = mydelegate.BeginInvoke(inFile, outFile, progress, AsyncCallbackMethod, mydelegate);
    }

    static void AsyncCallbackMethod(IAsyncResult ar)
    {
        if (ar == null) return;
        unzipDecompressionDelegate parma = (unzipDecompressionDelegate)ar.AsyncState;
        bool Result = parma.EndInvoke(ar);
        m_finishUnzip(Result,errorMsg);
    }
}
