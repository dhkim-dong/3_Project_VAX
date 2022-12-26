using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviourPunCallbacks
{
    private PhotonView PV;

	// ���Ӿ� ���� ���� UI ���� �Ŵ���
    private GameNetworkUIManager gameNetworkUIManager;
    public bool isActivate = false;       // �ٸ� ���� Controller���� �ߺ� ��� ������ ���� ���

    [SerializeField]  private Gun currentGun; // ���� ���� ���� ����

    private float currentFireRate; // ���� ���� �߻� �ӵ�

    private bool isReload; // ���� ����


    // ȿ����
    private AudioSource audioSource;                    // ����

    private RaycastHit hitInfo;                         // Ray ������ �浹 ���� �޾ƿ�

    // �ʿ��� ������Ʈ
    [SerializeField] private Camera theCam;

    [SerializeField] private GameObject hit_effect_Prefab;   // ��ź ȿ�� ���� ������Ʈ

    [SerializeField] private PlayerController playerController; // �÷��̾� �ִϸ��̼��� �������� ����

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private HandController handController;

    [SerializeField] private KeyController keyController;

    private void Start()
    {
        PV = GetComponentInParent<PhotonView>();

		// ������ GameNetworkUIManager ������Ʈ�� ã�� �Ҵ�
        gameNetworkUIManager = FindObjectOfType<GameNetworkUIManager>();
		
        audioSource = GetComponent<AudioSource>();

        weaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        weaponManager.currentWeaponAnim = currentGun.anim;
    }

    private void Update()
    {
        if (!PV.IsMine) return;

		// ���Ӿ��� UIâ ��ü�� ��Ȱ��ȭ�Ǿ�߸� ��Ƽ�� ��� ����
        if (isActivate && !Inventory.inventoryActivated && !gameNetworkUIManager.isUIActive)
        {
            GunFireRateCacl();
            TryFire();
            TryReload();
        }
    }

    private void GunFireRateCacl() // �߻縦 ���� ����
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime;
        }
    }

    private void TryFire() // �߻� �Է�
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    private void Fire() // �߻縦 ���� ����
    {
        if (isReload)
            return;

        if (currentGun.currentBulletCount > 0)
            PV.RPC("ShootRPC", RpcTarget.All);
        else
            PV.RPC("ReloadRPC", RpcTarget.All);
    }
  
    public void Shoot() // ���� �ѱ� �߻�
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
            // ������ ���� �޼��� ����
            // ź�� ����
            var clone = Instantiate(hit_effect_Prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }
    }

    private void TryReload() // ������ �õ�
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

  
    public IEnumerator ReloadCoroutine() // ������
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
            Debug.Log("�Ѿ��� �����ϴ�.");
        }
    }

    // ���� ���
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    // ���� �� ���� ��ȯ
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
