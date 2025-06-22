using System.Collections.Generic;
using System;

namespace E2MultiPlayer
{
    public abstract class TSingleton<T>: ISingleton  where T : TSingleton<T>
    {
        private static T g_Inst;


        private bool m_Disposed;

        public bool Disposed => m_Disposed;

        protected TSingleton()
        {
            if (g_Inst != null)
            {
#if UNITY_EDITOR
                throw new Exception(this + "Singleton Already Initialized");
#endif
            }
        }

        public static T Instance
        {
            get
            {
                if (g_Inst == null)
                {
                    if (SingletonMgr.Disposing)
                    {
                        Log.Error($" {typeof (T).Name} Has Already Been Disposed");
                        return null;
                    }
                    
                    g_Inst = Activator.CreateInstance<T>() as T;
                    g_Inst.Initialize();
                    SingletonMgr.Add(g_Inst);
                }

                g_Inst.OnUse();
                return g_Inst;
            }
        }


        public static bool DisposeInst()
        {
            if (g_Inst == null)
            {
                return true;
            }

            return g_Inst.Dispose();
        }


        public bool Dispose()
        {
            if (m_Disposed)
            {
                return false;
            }

            SingletonMgr.Remove(g_Inst);
            g_Inst     = default;
            m_Disposed = true;
            OnDispose();
            return true;
        }
        
        protected virtual void OnDispose()
        {
        }

        protected virtual void OnUse()
        {
            
        }
        
        //初始化回调
        protected virtual void Initialize()
        {
        }

        //每次使用前回调
        protected virtual void UnInitialize()
        {
        }
    }
}