using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class ColliderReplicaCreator
{
    private const string MenuPath = "Assets/Create Collider Replica";
    private const int MenuPriority = 30;
    
    [MenuItem(MenuPath, priority = MenuPriority)]
    private static void CreateColliderReplica()
    {
        GameObject sourcePrefab = Selection.activeObject as GameObject;
        if (sourcePrefab == null)
        {
            Debug.LogError("请选择一个预制体");
            return;
        }

        string prefabPath = AssetDatabase.GetAssetPath(sourcePrefab);
        string directory = Path.GetDirectoryName(prefabPath);
        string fileName = Path.GetFileNameWithoutExtension(prefabPath);
        string newPrefabPath = $"{directory}/{fileName}_ColliderReplica.prefab";

        // 检查是否已存在
        if (AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath) != null)
        {
            if (!EditorUtility.DisplayDialog("预制体已存在", 
                $"预制体 '{fileName}_ColliderReplica' 已存在，是否覆盖？", 
                "覆盖", "取消"))
            {
                return;
            }
        }

        // 创建新预制体根对象
        GameObject newPrefabRoot = new GameObject($"{sourcePrefab.name}_ColliderReplica");
        
        // 递归处理所有子节点（包括未激活的Collider）
        ProcessHierarchy(sourcePrefab.transform, newPrefabRoot.transform, true);
        
        // 添加同步组件
        //ColliderReplicaSync syncComponent = newPrefabRoot.AddComponent<ColliderReplicaSync>();
        //syncComponent.sourcePrefab = sourcePrefab;
        
        // 保存为预制体
        GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(newPrefabRoot, newPrefabPath);
        Object.DestroyImmediate(newPrefabRoot);
        
        // 选择新创建的预制体
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = prefabInstance;
        
        Debug.Log($"Collider副本创建成功: {newPrefabPath}");
    }

    [MenuItem(MenuPath, validate = true)]
    private static bool ValidateCreateColliderReplica()
    {
        // 仅在选中预制体时显示菜单项
        return Selection.activeObject is GameObject && 
               PrefabUtility.GetPrefabAssetType(Selection.activeObject) != PrefabAssetType.NotAPrefab;
    }

    private static void ProcessHierarchy(Transform original, Transform newParent, bool includeInactive)
    {
        // 检查是否有Collider组件（包括未激活的）
        Collider[] originalColliders = original.GetComponents<Collider>();
        bool hasColliders = originalColliders.Any(c => includeInactive || c.enabled);
        
        if (hasColliders)
        {
            // 创建新节点
            GameObject newNode = new GameObject($"{original.name}_Collider");
            newNode.transform.SetParent(newParent);
            
            // 复制Transform信息
            CopyTransform(original, newNode.transform);
            
            // 添加并配置所有Collider
            foreach (Collider collider in originalColliders)
            {
                AddColliderComponent(newNode, collider);
            }
        }

        // 递归处理子节点
        for (int i = 0; i < original.childCount; i++)
        {
            ProcessHierarchy(original.GetChild(i), newParent, includeInactive);
        }
    }

    private static void CopyTransform(Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;
    }

    private static void AddColliderComponent(GameObject obj, Collider original)
    {
        Collider newCollider = null;

        if (original is BoxCollider box)
        {
            BoxCollider bc = obj.AddComponent<BoxCollider>();
            bc.center = box.center;
            bc.size = box.size;
            newCollider = bc;
        }
        else if (original is SphereCollider sphere)
        {
            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.center = sphere.center;
            sc.radius = sphere.radius;
            newCollider = sc;
        }
        else if (original is CapsuleCollider capsule)
        {
            CapsuleCollider cc = obj.AddComponent<CapsuleCollider>();
            cc.center = capsule.center;
            cc.radius = capsule.radius;
            cc.height = capsule.height;
            cc.direction = capsule.direction;
            newCollider = cc;
        }
        else if (original is MeshCollider mesh)
        {
            MeshCollider mc = obj.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh.sharedMesh;
            mc.convex = mesh.convex;
            newCollider = mc;
        }
        else if (original is TerrainCollider terrain)
        {
            TerrainCollider tc = obj.AddComponent<TerrainCollider>();
            tc.terrainData = terrain.terrainData;
            newCollider = tc;
        }
        else if (original is WheelCollider wheel)
        {
            WheelCollider wc = obj.AddComponent<WheelCollider>();
            wc.center = wheel.center;
            wc.radius = wheel.radius;
            wc.suspensionDistance = wheel.suspensionDistance;
            wc.forceAppPointDistance = wheel.forceAppPointDistance;
            newCollider = wc;
        }
        else
        {
            // 未知Collider类型，使用通用方法复制
            newCollider = obj.AddComponent(original.GetType()) as Collider;
            if (newCollider == null)
            {
                Debug.LogWarning($"无法复制未知Collider类型: {original.GetType().Name}");
                return;
            }
        }

        // 复制通用属性
        newCollider.enabled = original.enabled;
        newCollider.isTrigger = original.isTrigger;
        newCollider.sharedMaterial = original.sharedMaterial;
        newCollider.contactOffset = original.contactOffset;
    }
}