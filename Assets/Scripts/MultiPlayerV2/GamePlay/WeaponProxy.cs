using System.Threading.Tasks;
using MalbersAnimations;
using MalbersAnimations.Controller;
using MalbersAnimations.Weapons;
using UnityEngine;

namespace E2MultiPlayer
{
    public class WeaponProxy
    {
        WeaponScriptableObject m_WeaponData;
        Transform holsterLeft;
        Transform holsterRight;
        Transform holsterBack1;
        Transform holsterBack2;
        Transform leftHand;
        Transform rightHand;

        public GameObject swordObj;
        public GameObject pistolObj;
        public GameObject rifleObj;
        public GameObject smgObj;
        public GameObject magicStickObj;
        public GameObject smgFlashEffect, smgBulletEffect;

        Stats playerStat;
        GameObject currentWeapon;
        private Transform m_PlayerControl;
        private E2ClientEntityScript m_EntityScript;

        public void Initialize(Transform _playerControl,E2ClientEntityScript entityScript)
        {

            m_PlayerControl = _playerControl;
            m_EntityScript = entityScript;
            InitializeVariables();
            if (m_EntityScript.IsOwner)
            {
                RegisterWeaponEvents();    
            }
        }

        void InitializeVariables()
        {
            holsterLeft = m_PlayerControl.FindGrandChild("Holster Left");
            holsterRight = m_PlayerControl.FindGrandChild("Holster Right");
            holsterBack1 = m_PlayerControl.FindGrandChild("Holster Back 1");
            holsterBack2 = m_PlayerControl.FindGrandChild("Holster Back 2");
            leftHand = m_PlayerControl.FindGrandChild("CC_Base_L_Hand");
            rightHand = m_PlayerControl.FindGrandChild("CC_Base_R_Hand");
        }

        void RegisterWeaponEvents()
        {
            MWeaponManager weaponManager = m_PlayerControl.GetComponent<MWeaponManager>();
            if (weaponManager == null)
            {
                Log.Error("WeaponProxy.RegisterWeaponEvents Invalid WeaponManager");
                return;
            }
                

            weaponManager.OnEquipWeapon.AddListener(OnEquipWeaponAsync);
            weaponManager.OnUnequipWeapon.AddListener(OnUnEquipWeaponAsync);
            weaponManager.holsters[0].OnWeaponInHolster.AddListener(OnWeaponInHolster);
            weaponManager.holsters[1].OnWeaponInHolster.AddListener(OnWeaponInHolster);
            weaponManager.holsters[2].OnWeaponInHolster.AddListener(OnWeaponInHolster);
        }

        #region Weapon Event Listeners
        public void OnEquipWeaponAsync(GameObject weapon)
        {
            Log.Info($"WeaponProxy.OnEquipWeaponAsync {m_EntityScript.OwnerId} {weapon}");
            //int weaponIndex = FindWeaponIndex(weapon.name);
            //int previousHolsterIndex = m_WeaponData.weapons[weaponIndex].holsterIndex;

            m_EntityScript.Entity.CallRPC(Consts.RPC.EQUIP_WEAPON, 0, 0);
        }

        public void OnUnEquipWeaponAsync(GameObject weapon)
        {
            Log.Info($"WeaponProxy.OnUnEquipWeaponAsync {m_EntityScript.OwnerId} {weapon}");
            //int weaponIndex = FindWeaponIndex(weapon.name);
            //int previousHolsterIndex = m_WeaponData.weapons[weaponIndex].holsterIndex;

            m_EntityScript.Entity.CallRPC(Consts.RPC.UNEQUIP_WEAPON, 0, 0);
        }

        public async void OnWeaponInHolster(MWeapon weapon)
        {
            weapon.OnPlaySound.AddListener(OnWeaponSoundPlay);

            // weaponIndex = FindWeaponIndex(weapon.name);
            //int holsterIndex = m_WeaponData.weapons[weaponIndex].holsterIndex;

            m_EntityScript.Entity.CallRPC(Consts.RPC.SPAWN_WEAPON, 0,weapon.HolsterID);
        }
        
        void OnWeaponSoundPlay(int soundID)
        {
            m_EntityScript.Entity.CallRPC(Consts.RPC.WEAPON_SOUND, soundID);
        }
        #endregion

        #region Weapon Event Callbacks
        public GameObject SpawnWeapon(int weaponIndex, int holsterIndex)
        {
            Log.Info($"WeaponProxy.SpawnWeapon {m_EntityScript.OwnerId} {weaponIndex} {holsterIndex}");
            GameObject spawnedWeapon = null;
            Transform weaponParent = null;

            switch (holsterIndex)
            {
                case 0:
                    weaponParent = holsterLeft; break;
                case 1:
                    weaponParent = holsterRight; break;
                case 2:
                    weaponParent = holsterBack1; break;
                case 3:
                    weaponParent = holsterBack2; break;
            }
            

            foreach (Transform t in weaponParent)
            {
                GameObject.Destroy(t.gameObject);
            }

            var sword = GameObject.Find("Sword Collectable");
            
            spawnedWeapon = GameObject.Instantiate(sword, weaponParent);
            //spawnedWeapon.transform.localPosition = m_WeaponData.weapons[weaponIndex].holsterOffset.position;
            //spawnedWeapon.transform.localRotation = m_WeaponData.weapons[weaponIndex].holsterOffset.rotation;
            //spawnedWeapon.transform.localScale = m_WeaponData.weapons[weaponIndex].holsterOffset.scale;
            spawnedWeapon.name = spawnedWeapon.name.Replace("(Clone)", "");

            if(spawnedWeapon.name.Contains("Sword")) swordObj = spawnedWeapon;
            else if(spawnedWeapon.name.Contains("Pistol")) pistolObj = spawnedWeapon;
            else if(spawnedWeapon.name.Contains("Rifle")) rifleObj = spawnedWeapon;
            else if (spawnedWeapon.name.Contains("SMG")) smgObj = spawnedWeapon;
            else if(spawnedWeapon.name.Contains("Magic_Staff")) magicStickObj = spawnedWeapon;

            return spawnedWeapon;
        }

        public void EquipWeapon(int previousHolsterIndex, int weaponIndex)
        {

            Transform spawnedWeapon = null;

            switch (previousHolsterIndex)
            {
                case 0:
                    spawnedWeapon = holsterLeft.GetChild(0); break;
                case 1:
                    spawnedWeapon = holsterRight.GetChild(0); break;
                case 2:
                    spawnedWeapon = holsterBack1.GetChild(0); break;
                case 3:
                    spawnedWeapon = holsterBack2.GetChild(0); break;
            }

            if (m_WeaponData.weapons[weaponIndex].leftHand)
            {
                spawnedWeapon.transform.parent = leftHand;
            }
            else
            {
                spawnedWeapon.transform.parent = rightHand;
            }

            spawnedWeapon.localPosition = m_WeaponData.weapons[weaponIndex].handOffset.position;
            spawnedWeapon.localRotation = m_WeaponData.weapons[weaponIndex].handOffset.rotation;
            spawnedWeapon.localScale = m_WeaponData.weapons[weaponIndex].handOffset.scale;
            currentWeapon = spawnedWeapon.gameObject;
            smgFlashEffect = currentWeapon.transform.FindGrandChild("fx_avartar_smg_muzzleflash")?.gameObject;
            smgBulletEffect = currentWeapon.transform.Find("fx_avartar_smg_bulletshells")?.gameObject;
        }

        public void EquipWeapon(int weaponIndex)
        {
            SpawnWeapon(weaponIndex, m_WeaponData.weapons[weaponIndex].holsterIndex);
            EquipWeapon(m_WeaponData.weapons[weaponIndex].holsterIndex, weaponIndex);
        }

        public void UnequipWeapon(int previousHolsterIndex, int weaponIndex)
        {
            Log.Info($"WeaponProxy.UnEquipWeapon {previousHolsterIndex} {weaponIndex}");
            
            Transform spawnedWeapon = null;

            if (m_WeaponData.weapons[weaponIndex].leftHand)
            {
                spawnedWeapon = leftHand.GetChild(5);
            }
            else
            {
                spawnedWeapon = rightHand.GetChild(5);
            }
            switch (previousHolsterIndex)
            {
                case -1:
                    GameObject.Destroy(spawnedWeapon.gameObject); break;
                case 0:
                    spawnedWeapon.transform.parent = holsterLeft; break;
                case 1:
                    spawnedWeapon.transform.parent = holsterRight; break;
                case 2:
                    spawnedWeapon.transform.parent = holsterBack1; break;
                case 3:
                    spawnedWeapon.transform.parent = holsterBack2; break;
            }
            spawnedWeapon.transform.localPosition = m_WeaponData.weapons[weaponIndex].holsterOffset.position;
            spawnedWeapon.transform.localRotation = m_WeaponData.weapons[weaponIndex].holsterOffset.rotation;
            spawnedWeapon.transform.localScale = m_WeaponData.weapons[weaponIndex].holsterOffset.scale;
        }

        public void PlayWeaponSound(int soundID)
        {

            if (soundID == 2)
            {
                if(smgFlashEffect != null) smgFlashEffect.SetActive(true);
                if(smgBulletEffect != null) smgBulletEffect.SetActive(true);
            }

            int weaponIndex = FindWeaponIndex(currentWeapon.name);
            AudioSource audioSource = currentWeapon.GetComponent<AudioSource>();
            if (audioSource == null) return;
            audioSource.clip = m_WeaponData.weapons[weaponIndex].Sounds[soundID];
            audioSource.Play();
        }
        #endregion

        int FindHolsterIndex(string weapon)
        {
            if (holsterLeft.Find(weapon) != null) return 0;
            if (holsterRight.Find(weapon) != null) return 1;
            if (holsterBack1.Find(weapon) != null) return 2;
            if (holsterBack2.Find(weapon) != null) return 3;
            return -1;
        }

        int FindHandIndex(string weapon)
        {
            if (leftHand.Find(weapon) != null) return 4;
            if (rightHand.Find(weapon) != null) return 5;
            return -1;
        }

        int FindWeaponIndex(string weapon)
        {
            for (int i = 0; i < m_WeaponData.weapons.Length; i++)
            {
                if (m_WeaponData.weapons[i].weaponObject.name == weapon)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}