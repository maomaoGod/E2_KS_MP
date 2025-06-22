using System;
using System.Collections;
using System.Collections.Generic;
using E2MultiPlayer;

using KS.Reactor.Client;
using KS.Reactor.Client.Unity;

using UnityEngine;

public class E2MultiPlayerEntry : MonoBehaviour
{
    private GameObject m_goCoreFeature = null;
    private GameObject m_goLegacyFeature = null;
    private GameObject m_goCanvas = null;
    private GameObject m_goCamera = null;
    public bool m_IsAlone = false;
    //private Earth2_RuntimeNavMesh m_RuntimeNavMesh;
    private enum EntryStage
    {
        E_Stage_None,
        E_Stage_TelCam,
        E_Stage_StartBakeMesh,
        E_Stage_BakingMesh,
        E_Stage_StartNet,
        E_Stage_Conning,
        E_Stage_Play
    }
    
    private EntryStage m_Stage = EntryStage.E_Stage_None;
    private bool m_Stopped = false;

    void OnEnable()
    {
#if UNITY_EDITOR
        Common.s_IsAlone = m_IsAlone;
#else
        Common.s_IsAlone = false;
#endif
        SingletonMgr.Initialize();
        m_Stopped =  false;
    }

    private void OnApplicationQuit()
    {
        GameObject.Destroy(m_goCoreFeature);
        Common.m_bStarted = false;
        m_Stopped = true;
    }

    // Start is called before the first frame update
    // Load Ground Mesh
    // Connect Network
    // Spawn a player
    void Start()
    {
        
        // var coreFeature = Resources.Load<GameObject>("Core_Features");
        // if (null != coreFeature)
        // {
        //     m_goCoreFeature = GameObject.Instantiate(coreFeature);
        //     GameObject.DontDestroyOnLoad(m_goCoreFeature);
        // }
        //
        // var goCam = Resources.Load<GameObject>("Main Camera");
        // if (null != goCam)
        // {
        //     m_goCamera = GameObject.Instantiate(goCam);
        //     GameObject.DontDestroyOnLoad(m_goCamera);
        // }
        //
        // var legacyFeature = Resources.Load<GameObject>("Legacy_Features");
        // if (null != legacyFeature)
        // {
        //     m_goLegacyFeature = GameObject.Instantiate(legacyFeature);
        //     GameObject.DontDestroyOnLoad(m_goLegacyFeature);
        // }
        //
        // var cavans = Resources.Load<GameObject>("Canvas");
        // if (null != cavans)
        // {
        //     m_goCanvas = GameObject.Instantiate(cavans);
        //     GameObject.DontDestroyOnLoad(m_goCanvas);
        // }
        //
        if (Common.s_IsAlone)
        {
            // Load ground
            var ground = Resources.Load<GameObject>("World");
            var groundInst = GameObject.Instantiate(ground, Vector3.zero, Quaternion.identity);
        
            // Load Player
            var pl = Resources.Load<GameObject>("ECM_Character");
            var plInst = GameObject.Instantiate(pl, Vector3.zero, Quaternion.identity);
            ActorManager.Instance.OnPlayerJoin(new ksPlayer(1));
            var actor = ActorManager.Instance.GetActor(1);
            actor.Bind(plInst,null);

            CameraManager.Instance.TargetTransform = plInst.transform;
            
            var npc = Resources.Load<GameObject>("Local_NPC_RockGolem02");
            var npcInst = GameObject.Instantiate(npc, Vector3.zero, Quaternion.identity);
            var ac = new AIAgent();
            ac.Bind(npcInst,null);
            ActorManager.Instance.AddActor(2,ac);
        }
        else
        {
            
            
        }

        m_Stage = EntryStage.E_Stage_StartNet;
    }

    // set the long titude and latitude ,as long as active
    private float m_fLerpTime = 0.0f;
    public void TeleportCamera()
    {
        // var multiGo = Resources.Load<GameObject>("Multiplayer_Features");
        // if (null == multiGo)
        // {
        //     Log.Error("E2MultiPlayerEntry: Multiplayer_Features not found");
        //     return;
        // }
        //
        // var mutiInst =  GameObject.Instantiate(multiGo, Vector3.zero, Quaternion.identity);
        // GameObject.DontDestroyOnLoad(mutiInst);
        
        string longti = "10.259026098490498";
        string lati = "44.8979277996076";
        
        //CameraController.Instance.TeleportCameraGoogle($"{longti}, {lati}");
        //CameraController.Instance.active = false;
        
        m_Stage = EntryStage.E_Stage_TelCam;
    }


    private void OnNavMeshBakeFinished()
    {
        Log.Info("E2MultiPlayerEntry: OnNavMeshBakeFinished");
        //Earth2_RuntimeNavMesh.OnBakeNaveMeshFinished -= OnNavMeshBakeFinished;
        DynamicNavigationMgr.Instance.OnBakeNaveMeshFinished -= OnNavMeshBakeFinished;

        var camInst = CameraManager.Instance;
        camInst.SwitchToPlayerMode();
        
        m_Stage = EntryStage.E_Stage_StartNet;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (m_Stopped)
        {
            return;
        }
        
        if (m_Stage == EntryStage.E_Stage_TelCam)
        {
            m_fLerpTime += Time.deltaTime;
  
        }else if (m_Stage == EntryStage.E_Stage_StartBakeMesh)
        {
            Log.Info("E2MultiPlayerEntry: Enter Bake Mesh");
            // prepare navmesh
            var terrainCollider = GameObject.FindGameObjectWithTag("TerrainColliderMicro");
            if (null == terrainCollider)
            {
                Log.Error("E2MultiPlayerEntry: TerrainColliderMicro not found");
                return;
            }

           

            // var terrain = GameObject.FindGameObjectWithTag("TerrainColliderMicro");
            // m_RuntimeNavMesh.terrain_colliders = new GameObject[1]{terrain};
            // m_RuntimeNavMesh.CenterPos = CameraController.Instance.transform.position;
            
         

            //Earth2_RuntimeNavMesh.OnBakeNaveMeshFinished += OnNavMeshBakeFinished;
            DynamicNavigationMgr.Instance.OnBakeNaveMeshFinished += OnNavMeshBakeFinished;
            
            m_Stage = EntryStage.E_Stage_BakingMesh;
        }
        else if (m_Stage == EntryStage.E_Stage_StartNet)
        {
            NetworkManager.Instance.BeginConnect();
            m_Stage = EntryStage.E_Stage_Conning;
            
        }else if (m_Stage == EntryStage.E_Stage_Conning)
        {
            if (NetworkManager.Instance.ConnectResult == ksBaseRoom.ConnectStatus.SUCCESS)
            {
                m_Stage = EntryStage.E_Stage_Play;
                var inst = UIManager.Instance;
            }
        }
        else if(m_Stage == EntryStage.E_Stage_Play)
        {
            if (!Common.m_bStarted)
            {
                Common.m_bStarted = true;
                CameraManager.Instance.StartFollowTarget();
            }
            
        }
        
        
        
        if(Common.m_bStarted)
            ActorManager.Instance.Update(Time.deltaTime);
        if(m_Stage == EntryStage.E_Stage_BakingMesh || 
           m_Stage == EntryStage.E_Stage_Play)
            DynamicNavigationMgr.Instance.Update(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if(Common.m_bStarted)
            ActorManager.Instance.FixedUpdate(Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        if (Common.m_bStarted)
        {
            ActorManager.Instance.LateUpdate(Time.deltaTime);
            CameraManager.Instance.LateUpdate(Time.deltaTime);
            UIManager.Instance.LateUpdate(Time.deltaTime);
        }
            
        //UIManager.Instance.LateUpdate(Time.deltaTime);
    }
}
