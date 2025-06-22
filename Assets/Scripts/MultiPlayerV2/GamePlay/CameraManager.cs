
using UnityEngine;

namespace E2MultiPlayer
{
    public class CameraManager:TSingleton<CameraManager>
    {
        private Camera m_MainCamera;
        public Camera MainCamera => m_MainCamera;
        private Transform m_CameraTransform;
        private Transform m_TargetTransform;
        
        private float _distanceToTarget = 30.0f;
        private float _followSpeed = 3.0f;
        
        public float DistanceToTarget
        {
            get { return _distanceToTarget; }
            set { _distanceToTarget = Mathf.Max(0.0f, value); }
        }

        public float FollowSpeed
        {
            get { return _followSpeed; }
            set { _followSpeed = Mathf.Max(0.0f, value); }
        }

        public Transform TargetTransform
        {
            set { m_TargetTransform = value; }
        }


        private ThirdPersonCam m_ThirdPersonCam = null;

        public ThirdPersonCam ThirdPersonCam
        {
            get
            {
                if (null == m_ThirdPersonCam)
                {
                    m_ThirdPersonCam = GameObject.FindFirstObjectByType<ThirdPersonCam>();
                }
                
                return m_ThirdPersonCam;
            }
        }
        
        protected override void Initialize()
        {
            base.Initialize();

            if (null == m_MainCamera)
            {
                m_MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                m_CameraTransform = m_MainCamera.transform;
            }
      
        }

        
        public void LateUpdate(float deltaTime)
        {
            if (null != ThirdPersonCam)
            {
                ThirdPersonCam.OnLateUpdate(deltaTime);
            }
        }


        public void SwitchToPlayerMode()
        {
           
            
        }

        public void StartFollowTarget()
        {
           
        }
    }
}