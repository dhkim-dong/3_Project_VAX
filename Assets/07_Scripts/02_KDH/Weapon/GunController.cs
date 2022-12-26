using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviourPunCallbacks
{
    private PhotonView PV;

	// 게임씬 포톤 서버 UI 연동 매니저
    private GameNetworkUIManager gameNetworkUIManager;
    public bool isActivate = false;       // 다른 무기 Controller와의 중복 사용 방지를 위해 사용

    [SerializeField]  private Gun currentGun; // 현재 총의 무기 정보

    private float currentFireRate; // 현재 총의 발사 속도

    private bool isReload; // 상태 변수


    // 효과음
    private AudioSource audioSource;                    // 음향

    private RaycastHit hitInfo;                         // Ray 레이저 충돌 정보 받아옴

    // 필요한 컴포넌트
    [SerializeField] private Camera theCam;

    [SerializeField] private GameObject hit_effect_Prefab;   // 착탄 효과 게임 오브젝트

    [SerializeField] private PlayerController playerController; // 플레이어 애니메이션을 가져오기 위함

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private HandController handController;

    [SerializeField] private KeyController keyController;

    private void Start()
    {
        PV = GetComponentInParent<PhotonView>();

		// 씬에서 GameNetworkUIManager 오브젝트를 찾아 할당
        gameNetworkUIManager = FindObjectOfType<GameNetworkUIManager>();
		
        audioSource = GetComponent<AudioSource>();

        weaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        weaponManager.currentWeaponAnim = currentGun.anim;
    }

    private void Update()
    {
        if (!PV.IsMine) return;

		// 게임씬의 UI창 전체가 비활성화되어야만 액티브 기능 실행
        if (isActivate && !Inventory.inventoryActivated && !gameNetworkUIManager.isUIActive)
        {
            GunFireRateCacl();
            TryFire();
            TryReload();
        }
    }

    private void GunFireRateCacl() // 발사를 위한 연산
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime;
        }
    }

    private void TryFire() // 발사 입력
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    private void Fire() // 발사를 위한 과정
    {
        if (isReload)
            return;

        if (currentGun.currentBulletCount > 0)
            PV.RPC("ShootRPC", RpcTarget.All);
        else
            PV.RPC("ReloadRPC", RpcTarget.All);
    }
  
    public void Shoot() // 실제 총기 발사
    {
        currentGun.anim.CrossFade("Shoot", 0.2f);
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate;
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();

        Hit();
    }

    private void Hit()
    {
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward, out hitInfo, currentGun.range))
        {
            if (hitInfo.transform.GetComponent<BossEnemy>())
            {
                hitInfo.transform.GetComponent<BossEnemy>().OnDamagedProcess(currentGun.damage);
            }
            // 데미지 적용 메서드 참조
            // 탄피 생성
            var clone = Instantiate(hit_effect_Prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }
    }

    private void TryReload() // 재장전 시도
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            PV.RPC("ReloadRPC", RpcTarget.All);
        }
    }

    public void CancelReload()
    {
        if (isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }

  
    public IEnumerator ReloadCoroutine() // 재장전
    {
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;
            //SoundManager.instance.PlaySE("Reload");
            weaponManager.currentWeaponAnim.CrossFade("Reload", 0.2f);

            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;

            yield return new WaitForSeconds(currentGun.reloadTime);

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("총알이 없습니다.");
        }
    }

    // 사운드 재생
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    // 현재 총 정보 반환
    public Gun GetGun()
    {
        return currentGun;
    }

    public void GunChange(Gun _gun)
    {
        if (weaponManager.currentWeapon != null)
            weaponManager.currentWeapon.gameObject.SetActive(false);

        currentGun = _gun;
        weaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        weaponManager.currentWeaponAnim = currentGun.anim;       

        //currentGun.transform.localPosition = Vector3.zero;
        currentGun.gameObject.SetActive(true);
        playerController.anim.runtimeAnimatorController = gameObject.GetComponentInChildren<Animator>().runtimeAnimatorController;
        handController.isActivate = false;
        keyController.isActivate = false;
        isActivate = true;
    }
}
