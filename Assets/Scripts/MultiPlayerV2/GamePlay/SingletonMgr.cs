using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace E2MultiPlayer
{
        public static class SingletonMgr
    {
        private static List<ISingleton> g_Singles = new List<ISingleton>();
        public static bool Disposing { get; private set; } = true;

        public static int  Count      => g_Singles.Count;
        
        public static bool IsQuitting { get; private set; }

        static SingletonMgr()
        {
            Application.quitting -= OnQuitting;
            Application.quitting += OnQuitting;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
        }

#if UNITY_EDITOR
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                OnQuitting();
            }
        }
#endif        
        private static void OnQuitting()
        {
            Log.Warning("SingletonMgr.OnQuitting");
            
            if (IsQuitting)
            {
                return;
            }
            
            IsQuitting = true;
            Dispose();

            Disposing = false;
        }

        public static void Dispose()
        {
            Disposing = true;

            Log.Warning("SingletonMgr.Dispose");
            var singles = g_Singles.ToArray();
            for (int i = 0; i < singles.Length; i++)
            {
                var inst = singles[i];

                if (inst == null || inst.Disposed) continue;

                inst.Dispose();
                singles[i] = null;
            }

            g_Singles.Clear();

            IsQuitting = false;
        }

        //初始化
        public static void Initialize()
        {
            if (IsQuitting)
            {
                Debug.Log("Application is Quitting");
                return;
            }

            Disposing = false;
        }

        internal static void Add(ISingleton single)
        {
            
            g_Singles.Add(single);
        }

        internal static void Remove(ISingleton single)
        {
            g_Singles.Remove(single);
            
        }
    }
}