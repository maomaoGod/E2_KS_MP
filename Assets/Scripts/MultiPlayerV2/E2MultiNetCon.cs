using UnityEngine;

namespace E2MultiPlayer
{
    public class E2MultiNetCon:MonoBehaviour
    {
        public void Start()
        {
            SingletonMgr.Initialize();
            NetworkManager.Instance.BeginConnect();
        }
    }
}