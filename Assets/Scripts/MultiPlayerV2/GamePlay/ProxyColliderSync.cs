using System.Collections.Generic;
using KS.Reactor.Client.Unity;
using UnityEngine;

namespace E2MultiPlayer
{
    public class ProxyColliderSync
    {
        private Dictionary<Transform, Transform> m_TransMap = new();

        private GameObject m_SourcePrefab = null;
        private GameObject m_SyncPrefab = null;
        public void Initialize(GameObject sourcePrefab, GameObject syncPrefab)
        {
            if (null == sourcePrefab)
            {
                Log.Error("ProxyColliderSync::Initialize: sourcePrefab is null");
                return;
            }

            if (null == syncPrefab)
            {
                Log.Error("ProxyColliderSync::Initialize: syncPrefab is null");
                return;
            }

            var needSyncComs = syncPrefab.GetComponentsInChildren<ksEntityComponent>();
            foreach (var needSyncCom in needSyncComs)
            {
                var col = needSyncCom.GetComponent<Collider>();
                if (null == col)
                {
                    continue;
                }
                
                var sourceTm = sourcePrefab.transform.Find(col.gameObject.name);
                if (null == sourceTm)
                {
                    Log.Error($"ProxyColliderSync::Initialize: sourceTm is null for node:{col}");
                    continue;
                }
                
                m_TransMap[sourceTm] = col.transform;
            }
        }

        public void OnUpdate(float dtTime)
        {
            if (null == m_TransMap || m_TransMap.Count == 0)
            {
                return;
            }

            foreach (var kv in m_TransMap)
            {
                kv.Value.position = kv.Key.position;
                kv.Value.rotation = kv.Key.rotation;
                kv.Value.localScale = Vector3.Scale(
                    kv.Key.lossyScale, 
                    new Vector3(
                        1f / kv.Key.parent.lossyScale.x, 
                        1f / kv.Key.parent.lossyScale.y, 
                        1f / kv.Key.parent.lossyScale.z
                    )
                );
            }
        }
        
        //Sync transform information to ks-server
        public void OnLateUpdate(float dtTime)
        {
            
        }
    }
}