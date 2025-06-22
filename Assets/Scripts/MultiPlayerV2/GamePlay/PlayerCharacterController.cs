
using UnityEngine;

namespace E2MultiPlayer
{
    public class PlayerCharacterController:BaseCharacterController
    {
        #region EDITOR EXPOSED FIELDS

        [Header("CUSTOM CONTROLLER")]
        [Tooltip("The character's follow camera.")]
        public Transform playerCamera;

        [Tooltip("The character's walk speed.")]
        [SerializeField]
        private float _walkSpeed = 2.5f;

        [Tooltip("The character's run speed.")]
        [SerializeField]
        private float _runSpeed = 5.0f;

        #endregion

        #region PROPERTIES
        public float walkSpeed
        {
            get { return _walkSpeed; }
            set { _walkSpeed = Mathf.Max(0.0f, value); }
        }

        public float runSpeed
        {
            get { return _runSpeed; }
            set { _runSpeed = Mathf.Max(0.0f, value); }
        }
        
        public bool walk { get; private set; }

        private PlayerInputData m_InputData;

        public PlayerInputData InputData
        {
            get { return m_InputData; }
            set { m_InputData = value; }
        }
        
        #endregion
        
        public override void Initialize()
        {
            base.Initialize();
            playerCamera = Camera.main.transform;
            m_InputData = new PlayerInputData();
        }

        private float GetTargetSpeed()
        {
            return walk ? walkSpeed : runSpeed;
        }

        
    }
}