using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace E2MultiPlayer
{
    public class UIManager:TSingleton<UIManager>
    {
        private Text m_FPS;
        private TextMeshProUGUI m_Ping;
        private Text m_HP;
        private Slider m_Slider;
        public float m_fUpdateInterval = 0.5f; // 更新间隔（秒）
        private float m_fAccum = 0f;
        private int m_Frames = 0;
        private float m_TimeLeft;

        private Actor m_MainActor = null;
        
        protected override void Initialize()
        {
            base.Initialize();
            //m_FPS = GameObject.Find("FPS").GetComponent<Text>();
            //m_Ping = GameObject.Find("ServerStatus").GetComponent<TextMeshProUGUI>();
            //m_HP = GameObject.Find("HP").GetComponent<Text>();
            //m_Slider = GameObject.Find("Slider").GetComponent<Slider>();
            //m_TimeLeft = m_fUpdateInterval;
        }

        public void LateUpdate(float dtTime)
        {
            //if(!Common.s_IsAlone && null != NetworkManager.Instance.ConnectRoom)
             //   m_Ping.text = $"Ping:{NetworkManager.Instance.ConnectRoom.Latency.ToString()}" ;
            
            // m_TimeLeft -= dtTime;
            // m_fAccum += Time.timeScale / dtTime;
            // m_Frames++;
            //
            // if ( m_TimeLeft  <= 0f)
            // {
            //     float fps = m_fAccum / m_Frames;
            //     m_FPS.text = string.Format("FPS:{0:F0}", fps);
            //
            //     m_TimeLeft = m_fUpdateInterval;
            //     m_fAccum = 0f;
            //     m_Frames = 0;
            // }

            // if (null == m_MainActor)
            // {
            //     m_MainActor = ActorManager.Instance.GetActor(ActorManager.Instance.LocalPlayerId);
            // }
            //
            // if (null != m_MainActor)
            // {
            //     var hp = m_MainActor.GetHp();
            //     m_HP.text = hp.ToString();
            //
            //     m_Slider.value = hp * 1.0f / 100.0f;
            // }
        }
    }
}

