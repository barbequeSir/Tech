using System;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _3rd
{
	public class EDebug
	{
		public static Action<LogType,string> OnLogT2File;
		public static bool Enable { get; set; }
		private static string _logCacheFilePath = "EDebug.log";

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Tools/EDebug/OpenLogFile")]
		public static void OpenLogFile()
		{
			if (_writer != null)
			{
				_writer.Close();
				_writer.Dispose();
				_writer = null;
			}
			EProcessCmd.ProcessCmd("notepad",LogFile);
		}
		
		[UnityEditor.MenuItem("Tools/EDebug/Clear")]
		public static void Clear()
		{
			if (_writer != null)
			{
				_writer.Close();
				_writer.Dispose();
				_writer = null;
			}
			File.Delete(LogFile);
		}
#endif

#region MyRegion
		public static void Log(object message)
		{
			if (Enable == false) return;
			SaveLog2File(message.ToString(),LogType.Log);
			Debug.Log(message);
		}
		
		public static void Log(object message, Object context)
		{
			if (Enable == false) return;
			string content = string.Format("{0} \r\n {1}", message, context);
			SaveLog2File(content,LogType.Log);
			Debug.Log(message,context);
		}
		
		public static void LogFormat(string format, params object[] args)
		{
			if (Enable == false) return;
			string content = string.Format(format,args);
			SaveLog2File(content, LogType.Log);
			Debug.LogFormat(format, args);
		}
		
		
		public static void LogError(object message)
		{
			if (Enable == false) return;
			SaveLog2File(message.ToString(), LogType.Error);
			Debug.LogError(message);
		}
		
		public static void LogError(object message, Object context)
		{
			if (Enable == false) return;
			string content = string.Format("{0} \r\n {1}", message, context);
			SaveLog2File(content, LogType.Error);
			Debug.Log(message, context);
		}

		public static void LogErrorFormat(string format, params object[] args)
		{
			if (Enable == false) return;
			string content = string.Format(format, args);
			SaveLog2File(content, LogType.Error);
			Debug.LogErrorFormat(format, args);
		}

		public static void LogErrorFormat(Object context, string format, params object[] args)
		{
			if (Enable == false) return;
			string content = string.Format(format,args);
			SaveLog2File(content, LogType.Error);			
			Debug.LogErrorFormat( context, format, args);
		}
		
		public static void LogException(Exception exception)
		{
			SaveLog2File(exception.ToString(), LogType.Exception);
			Debug.LogException(exception,null);
		}

		public static void LogException(Exception exception, Object context)
		{
			string content = string.Format("{0} \r\n{1}", exception,context);
			SaveLog2File(content, LogType.Exception);
			Debug.LogException(exception, context);
		}
		
		
		public static void LogWarning(object message)
		{
			if (Enable) return;
			SaveLog2File(message.ToString(), LogType.Warning);
			Debug.unityLogger.Log(LogType.Warning, message);
		}

		public static void LogWarning(object message, Object context)
		{
			if (Enable) return;
			string content = string.Format("{0} \r\n {1}", message, context);
			SaveLog2File(content, LogType.Warning);
			Debug.LogWarning(message, context);
		}

		public static void LogWarningFormat(string format, params object[] args)
		{
			if (Enable) return;
			string content = string.Format(format,args);
			SaveLog2File(content, LogType.Warning);
			Debug.LogWarningFormat(format, args);
		}

		public static void LogWarningFormat(Object context, string format, params object[] args)
		{
			if (Enable) return;
			string message = string.Format(format,args);
			string content = string.Format("{0}\r\n{1}", message, context);
			SaveLog2File(content, LogType.Warning);
			Debug.LogWarningFormat(context, format, args);
		}
#endregion
		
		protected static void SaveLog2File(string log,LogType ltype)
		{
			#if UNITY_EDITOR
			if (OnLogT2File != null)
			{
				OnLogT2File.Invoke(ltype,log);
			}
			string content = string.Format("{0}:{1}", ltype, log);
			Writer.WriteLine(string.Empty);
			Writer.WriteLine(content);
			#endif
		}

		internal static string LogFile
		{
			get
			{
				return Path.Combine(Application.dataPath, _logCacheFilePath);
			}
		}

		private static StreamWriter _writer;
		internal static StreamWriter Writer
		{
			get
			{
				if (_writer == null)
				{
					if (File.Exists(LogFile) == false)
					{
						_writer = File.CreateText(LogFile);
						string[] infos = new string[2];
						infos[0] = string.Format("Create: {0}", DateTime.Now.ToLocalTime());
						infos[1] = string.Format("--------------------------------{0}--------------------------------",
							"Gorgeous separation lines.");
						foreach (var info in infos)
						{
							Writer.WriteLine(info);
						}
					}
					else
					{
						_writer = new StreamWriter(LogFile,true,Encoding.UTF8);
					}
					
					_writer.AutoFlush = true;
				}
				return _writer;
			}
		}
		
	}
#if UNITY_EDITOR
	public static class EProcessCmd
	{
		public static void ProcessCmd(string command, string argument){
			System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo(command)
			{
				Arguments = argument,
				CreateNoWindow = false,
				ErrorDialog = true,
				UseShellExecute = true
			};

			if(start.UseShellExecute){
				start.RedirectStandardOutput = false;
				start.RedirectStandardError = false;
				start.RedirectStandardInput = false;
			} else{
				start.RedirectStandardOutput = true;
				start.RedirectStandardError = true;
				start.RedirectStandardInput = true;
				start.StandardOutputEncoding = Encoding.UTF8;
				start.StandardErrorEncoding = Encoding.UTF8;
			}
 
			System.Diagnostics.Process process = System.Diagnostics.Process.Start(start);

			if (!start.UseShellExecute)
			{
				var message = string.Format("{0} /r/n {1}", process.StandardOutput, process.StandardError);
				UnityEditor.EditorUtility.DisplayDialog("EPrcessCmd", message, "OK");
			}

			process.WaitForExit();
			process.Close();
		}
	}
#endif
}
