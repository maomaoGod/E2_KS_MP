using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text;

namespace E2MultiPlayer{
    public static class Log{
        private static string TimeFormat = "HH:mm:ss";
        private static string TimeFormat2 = "yyyyMMddHHmmss";
        private static FileStream s_FileStream;
        private static StreamWriter s_StreamWriter;
        private static StringBuilder s_Sbuilder;

        [RuntimeInitializeOnLoadMethod]
        private static void InstallLogProcessor(){
            Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
            Application.quitting += OnApplicationQuit;
            CreateOutlog();
        }

        private static void OnApplicationQuit()
        {
            Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;

            s_StreamWriter.Flush();
            s_StreamWriter.Dispose();
            s_StreamWriter.Close(); ;
            s_FileStream.Dispose();
            s_FileStream.Close();
        }

        private static string GetDirectory()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            return Application.dataPath + "/../" + "Logs/UniLog";
#else
            return Application.persistentDataPath + "/" + "Logs/UniLog";
#endif
        }

        private static void CreateOutlog()
        {
            string directory = GetDirectory();
            string path = directory + "/" + DateTime.Now.ToString(TimeFormat2)+".log";

            if (!Directory.Exists(directory)) 
                Directory.CreateDirectory(directory);

#if UNITY_STANDALONE || UNITY_EDITOR
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/../" + "Logs/GTrace");
                dirInfo.Attributes |= FileAttributes.Hidden;
            }
            catch (Exception)
            {
            }
#endif

            s_FileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            s_StreamWriter = new StreamWriter(s_FileStream);
            s_Sbuilder = new StringBuilder();
        }


        //[System.Diagnostics.Conditional("LOG_INFO")]
        //[System.Diagnostics.Conditional("LOG_Warning")]
        //[System.Diagnostics.Conditional("LOG_ERROR")]
        //[System.Diagnostics.Conditional("LOG_FATAL")]
        public static void Info(string msg){
            Debug.Log(msg);
        }

        public static void Warning(string msg){
            Debug.LogWarning(msg);
        }

        public static void Error(string msg){
             Debug.LogError(msg);
        }

        private static void LogMessageReceivedThreaded(string logString, string stackTrace, LogType type)
        {
            s_Sbuilder.Clear();
            s_Sbuilder.Append($"[{type.ToString().Substring(0, 3)}] [{DateTime.Now.ToString(TimeFormat)}] {logString} \n\t {stackTrace}");
            s_StreamWriter.WriteLine(s_Sbuilder.ToString());
            if ( !(type == LogType.Assert || type == LogType.Log))
            {
                s_StreamWriter.Flush();
            }
        }
    }
}