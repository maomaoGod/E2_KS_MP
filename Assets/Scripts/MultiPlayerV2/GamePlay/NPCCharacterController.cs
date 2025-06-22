
using UnityEngine;

namespace E2MultiPlayer
{
    public class NPCCharacterController:BaseCharacterController
    {
         #region EDITOR EXPOSED FIELDS
         

        [Tooltip("The character's walk speed.")]
        [SerializeField]
        private float _walkSpeed = 0.5f;

        [Tooltip("The character's run speed.")]
        [SerializeField]
        private float _runSpeed = 2.0f;

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
            m_InputData = new PlayerInputData();
            m_Animator = GetComponentInChildren<Animator>();
        }

        private float GetTargetSpeed()
        {
            return walk ? walkSpeed : runSpeed;
        }

      
    }
}