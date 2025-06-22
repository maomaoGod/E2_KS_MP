using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;

namespace E2MultiPlayer
{
    public class DynamicNavigationMgr : TSingleton<DynamicNavigationMgr>
    {
        private NavMeshSurface m_NavSurface;
        //private TerrainColliders m_TerrainCollider;
        //public TerrainColliders TerrainColliders => m_TerrainCollider;
        private float m_UpdateInterval = 0.5f;
        private bool m_AutoUpdate = true;
        
        private bool m_UseAsyncBaking = true;

        private float updateTimer;
        public bool CanBake = false;
        public Action OnBakeNaveMeshFinished;
        

        public float m_RaycastHeight = 100f;
        public float m_RaycastDistance = 8000000f;
        private bool m_bMeshBaked = false;

        public bool MeshBaked
        {
            get
            {
                return m_bMeshBaked;
            }
            set
            {
                m_bMeshBaked = value;
            }
        }

        
        protected override void Initialize()
        {
            base.Initialize();
            
            Log.Info("DynamicNavigationMgr.Initialize");
            
            // if (null == m_NavSurface)
            // {
            //     var navGo = GameObject.FindGameObjectWithTag("TerrainColliderMicro");
            //     if (null == navGo)
            //     {
            //         Log.Error("DynamicNavigationMgr.Initialize Can't find TerrainColliderMicro");
            //         return;
            //     }
            //     
            //     m_NavSurface = navGo.GetComponent<NavMeshSurface>();
            //     //m_TerrainCollider = navGo.GetComponent<TerrainColliders>();
            // }
            //
            // if (null == m_NavSurface)
            // {
            //     Log.Error("DynamicNavigationMgr.Initialize Invalid NavMeshSurface Prefab");
            // }
            //m_TerrainCollider.CollidersCreated += OnBakeStateChange;
            CanBake = false;
            m_bMeshBaked = false;
        }


        protected override void UnInitialize()
        {
            base.UnInitialize();
            //m_TerrainCollider.CollidersCreated -= OnBakeStateChange;
        }

        public void ResetWorldCollider()
        {
            //m_TerrainCollider.enabled = false;
            //m_TerrainCollider.enabled = true;
        }
        
        private void OnBakeStateChange(bool ableToBake)
        {
            Log.Info($"DynamicNavigationMgr.OnBakeStateChange {ableToBake}");
            CanBake = ableToBake;
        }
        
        public void Update(float dtTime)
        {
            if (!CanBake) 
                return;


            if (null != m_NavSurface && null == m_NavSurface.navMeshData)
            {
                m_NavSurface.BuildNavMesh();
                Log.Info("DynamicNavigationMgr.OnUpdate BuildNavMesh");
                CanBake = false;
                m_bMeshBaked = true;
                OnBakeNaveMeshFinished?.Invoke();
                return;
            }
            
            UpdateNavMesh();
            Log.Info("DynamicNavigationMgr.OnUpdate UpdateNavMesh");
            updateTimer = 0;
            CanBake = false;
            if (!m_bMeshBaked)
            {
                m_bMeshBaked = true;
                OnBakeNaveMeshFinished?.Invoke();
                
            }
            
        }


        
        // 增量更新导航网格
        public void UpdateNavMesh()
        {
            if (null == m_NavSurface)
            {
                return;
            }
            
            if ( m_NavSurface.navMeshData == null)
            {
                m_NavSurface.BuildNavMesh();
                return;
            }
            
            m_NavSurface.UpdateNavMesh(m_NavSurface.navMeshData);
         
        }
        
        
    public Vector3 GenerateBornPoint()
    {
       
        Collider hitCollider = null;
        
        if (Physics.Raycast(
                new Ray(CameraManager.Instance.MainCamera.transform.position + (Vector3.up * 100), Vector3.down),
                out RaycastHit hit, 60000,
                LayerMask.GetMask("TerrainColliders")
            ))
        {
            Log.Info($"DynamicNavigationMgr.GenerateBornPoint Hit Collider:{hit.collider.name}");
            hitCollider = hit.collider;
        }

        if (null == hitCollider)
        {
            Log.Error("DynamicNavigationMgr.GenerateBornPoint Hit Collider == null");
            return Vector3.zero;
        }
        
        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 50f, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        else
        {
            Log.Error($"DynamicNavigationMgr.GenerateBornPoint Hit Collider:{hit.collider.name} SamplePosition Failed");
        }
        
        // 获取碰撞器变换矩阵
        Transform colliderTransform = hitCollider.transform;
        Matrix4x4 localToWorld = colliderTransform.localToWorldMatrix;
            
        // 获取碰撞器在局部空间的包围盒
        Bounds localBounds = GetLocalBounds(hitCollider);
            
        // 在局部空间生成随机点
        Vector3 localRandomPoint = new Vector3(
            UnityEngine.Random.Range(localBounds.min.x, localBounds.max.x),
            localBounds.max.y, // Y值将在世界空间调整
            UnityEngine.Random.Range(localBounds.min.z, localBounds.max.z)
        );
                
        // 转换为世界空间
        Vector3 worldRandomPoint = localToWorld.MultiplyPoint3x4(localRandomPoint);
                
        // 向上偏移确保在碰撞体上方
        worldRandomPoint.y+= m_RaycastHeight;
                
        //Log.Info($"DynamicNavigationMgr.GenerateBornPoint {worldRandomPoint} {hitCollider.transform.position} {}");
        // 向下发射射线获取精确表面点
        Vector3 surfacePoint = GetSurfacePoint(worldRandomPoint,hitCollider);
        
        return surfacePoint;
    }

    // 获取碰撞器在局部空间的包围盒
    Bounds GetLocalBounds(Collider collider)
    {
        if (collider is TerrainCollider terrainCollider)
        {
            Terrain terrain = terrainCollider.GetComponent<Terrain>();
            TerrainData terrainData = terrain.terrainData;
            return new Bounds(
                terrainData.size * 0.5f,
                terrainData.size
            );
        }
        else if (collider is MeshCollider meshCollider)
        {
            return meshCollider.sharedMesh.bounds;
        }
        
        return collider.bounds;
    }

    Vector3 GetSurfacePoint(Vector3 rayStart, Collider targetCollider)
    {
        Ray ray = new Ray(rayStart, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit,m_RaycastDistance,LayerMask.GetMask("TerrainColliders")))
        {
            // 确保命中目标碰撞器
            // 验证点在NavMesh上
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 50f, NavMesh.AllAreas))
            {
                return navHit.position;
            }
            else
            {
                Log.Error($"DynamicNavigationMgr.GetSurfacePoint Hit Collider:{hit.collider.name} SamplePosition Failed");
            }
        }
        else
        {
            Log.Error("DynamicNavigationMgr.GetSurfacePoint Hit Collider == null");
        }
        return Vector3.zero;
    }
        
    }
}