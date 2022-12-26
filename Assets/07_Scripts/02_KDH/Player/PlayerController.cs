using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

	// ���Ӿ� ���� ���� UI ���� �Ŵ���
    private GameNetworkUIManager gameNetworkUIManager;
	
    [Header("MoveStat")] 
    [SerializeField] private float walkSpeed = 2f;    // �ȱ� �ӵ�
    [SerializeField] private float runSpeed  = 5f;    // �޸��� �ӵ�
    
    private float applySpeed;                         // ���� �ӵ��� �޾ƿͼ� ����� ���� �ӵ�

    [SerializeField] private float jumpForce;         // ���� ũ��

    [Header("Rotation")]
    [SerializeField] private float rotSpeed;          // ȸ�� �ӵ�
    [SerializeField] float cameraRotationLimit;       // ī�޶� ȸ�� ����
    private float currentCameraRotationX = 0f;

    [SerializeField] private float zoomCamRotY = 0f;  

    [Header("Ground Check Values")] 
    [SerializeField] float groundCheckRadius = 0.2f;  // �� Ȯ�ο� ��ü�� ������ ����
    [SerializeField] Vector3 groundCheckOffset;       // ĳ���� �ٴ� ��ġ ������ ��ġ ��
    [SerializeField] LayerMask groundLayer;           // physics Ȯ�ο� layer

    [SerializeField] GameObject GO_canvas;            // �� �̿��� UI ���ſ� ������Ʈ

    // ���� ����
    private bool isRun = false; 			// �޸��� ���� ���� ���� ���� ����
    private bool isGrounded;    			// ĳ���� ���� Ȯ�� ����
    private bool hasControl = true; 		// ���� �׼� �� �̵� ������ ���� ����

    // �̵��� ���� �ӽ� ����
    private float bodyAngle;    			// ĳ���� ȸ�� ������ ������ ����

    private Vector3 dir;        			// ĳ���� �̵� ����
    private Vector3 velocity;   			// ĳ���� �̵� �ӵ�
    private float ySpeed;       			// ĳ���� �������� �� ����
    private float locoMotionAmount; 		// ĳ���� �̵� ����Ʈ�� �� ���� 
    private float staminaTime;      		// ���׹̳� ���� ��� �ð� üũ ����

    private Quaternion targetRotation; 		// ���� ��� ���� ����?

    // �ڿ������� �ִϸ��̼� ����ȭ�� ���� �ӽ� ����
    private float tempAni = 1;

    // �ʿ� ������Ʈ
    private Camera camera;	// �÷��̾� ī�޶�
    private CharacterController cc;
    public Animator anim;
    private EnvironmentScanner environmentScanner;
    private StatusController statusController;

    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }

    // ������Ƽ
    public float RotSpeed => rotSpeed;
    public bool IsGrounded => isGrounded;
    public bool HasControl => hasControl;

    private void Awake()
    {
        if (PV.IsMine) 
        {
            gameObject.name = "Player";
            gameObject.layer = LayerMask.NameToLayer("Player");
            camera = GetComponentInChildren<Camera>();
            
        }
        else
        {
            gameObject.name = "OtherPlayer";
            gameObject.layer = LayerMask.NameToLayer("OtherPlayer");
            camera = GetComponentInChildren<Camera>();
            var audio = camera.GetComponent<AudioListener>();
            audio.enabled = false;
            camera.enabled = false;
            GO_canvas.SetActive(false);
        }
    }

    private void Start()
    {

        // ������ GameNetworkUIManager ������Ʈ�� ã�� �Ҵ�
        gameNetworkUIManager = FindObjectOfType<GameNetworkUIManager>();
		
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        statusController = GetComponent<StatusController>();

        applySpeed = walkSpeed;                                          // ó�� �ӵ��� �ȴ� �ӵ���

        Cursor.lockState = CursorLockMode.Locked;                        // ������ �� Ŀ�� �����
    }


    private void Update()
    {
        if (!PV.IsMine) return;

        AnimatorAvatar();

        // UIâ�� ��� ������ ���� Ŀ���� �����ǵ��� ����
        // IŰ�� ������ �� ���ѽ�Ű�� ���� �͵� �ֱ�
        if (!Inventory.inventoryActivated && !gameNetworkUIManager.isUIActive)                               
        {        
            CamRotation();
            CharacterRotation();
            Cursor.lockState = CursorLockMode.Locked;	// ��Ȱ��ȭ ���϶��� ��װ�
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;		// Ȱ��ȭ ���϶��� �����Ѵ�.
        }

		// ���� ���߿��� �̵� �Ұ�, ī�޶� �̵��� ���� ������ ���󰡰� ���� ����
        if (!hasControl) return;	

		// �÷��̾� Ű���� ��ȣ�ۿ��� UI�� ��Ȱ��ȭ �϶��� ����
        if(gameNetworkUIManager.isUIActive == false)
        {
			GroundCheck();                       	// ĳ������Ʈ�ѷ��� isGround�� ������ ���ٰ� �Ͽ� ���� üũ�ϴ� �Լ� ����
			anim.SetBool("isGrounded", isGrounded); // �ϰ� �ִϸ��̼� ���϶� ���� ���¿� ���ϰ� �ϱ� ���� ����

			TryRun();	// LeftShiftŰ�� �������� �ȴ������� üũ
			Move();		// �ݵ�� TryRun �ڿ��� ����Ǿ���. ĳ���� �̵� ����
		}

        /*if(transform.position.y < 5f)
        {
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        }*/
    }

    private void AnimatorAvatar()
    {
        if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.7f)
        {
            if (tempAni >= 0)
            {
                tempAni -= Time.deltaTime;
            }
            anim.SetLayerWeight(1, tempAni);
        }
        tempAni = 1;
    }

    private void CharacterRotation()              // ī�޶� ĳ���� ���ο� �־� �¿� �̵� �� ���� ȭ�� �¿찡 ���� �����δ�.
    {
        float _yRotation = Input.GetAxis("Mouse X");
        bodyAngle += _yRotation * rotSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0, bodyAngle, 0);
    }

    private void CamRotation()                    // ȭ�� ���� �̵��� ī�޶� �����̰�, ������ �� �ִ� �ִ� ������ ���� �ص�
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * rotSpeed * Time.deltaTime;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); // ī�޶� Limit -,+���� ������, �ٸ��� ���� ������ ��� ����

        camera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
    }

    private void TryRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && statusController.CurSp >= 10)  // ���׹̳ʸ� ����Ͽ� �ٴ� ���� ������
        {
            Running();
        }
        else if (statusController.CurSp < 5)  // ���׹̳ʰ� ������ �ڵ����� �ȵ��� ����
            RunningCancle();

        if (Input.GetKeyUp(KeyCode.LeftShift)) // LeftShitŰ�� ���� �ȴ´�.
        {
            RunningCancle();
        }
    }

    private void Running()
    {       
        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancle()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    private void Move()
    {
        float _moveDirx = Input.GetAxis("Horizontal");
        float _moveDirz = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(_moveDirx) + Mathf.Abs(_moveDirz)); // 0~1���� �޾ƿ���, �ش� ������ BlendTree���� ���,�ȱ�,�޸��� ���¸� üũ�Ѵ�.

        dir = new Vector3(_moveDirx, 0, _moveDirz).normalized;

        dir = camera.transform.TransformDirection(dir);            //ī�޶� ���� �������� �ȱ� ����

        velocity = Vector3.zero; // dir�� �޾ƿ� ���� �̵��� ���� ����

        if (isGrounded) // �� �������� �̵��� �� �ֵ���
        {
            if(isRun)
            statusController.DecreaseSP(1);

            ySpeed = -0.5f;
            velocity = dir * applySpeed;

            #region Ledge // Ledge���¶�� dir, Vector3�� zero�� �����Ͽ� �������̰� �Ѵ�.
            IsOnLedge = environmentScanner.LedgeCheck(dir, out LedgeData ledgeData);
            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }
            #endregion

            if (isRun)
                locoMotionAmount = velocity.magnitude / applySpeed;         // locoMotionAmount���� ���� �ڿ������� �ִϸ��̼��� ��µȴ�.
            else
                locoMotionAmount = Mathf.Clamp(velocity.magnitude / applySpeed, 0f, 0.5f); // �ȴ� ������ ��쿡�� 0 ~ 0.5 ���̿����� ���� ��ȯ�ǵ��� �Ѵ�.

            anim.SetFloat("moveAmount", locoMotionAmount, 0.2f, Time.deltaTime); // �ִϸ����Ϳ��� ����
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;     // Physics�� ��ϵ� �߷°����� ĳ���Ͱ� �ϰ���.

            dir = transform.forward * applySpeed / 2;         
        }
        velocity.y = ySpeed;
        
        cc.Move(velocity * Time.deltaTime);
    }

    private void GroundCheck()               // �� ������ ���
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    void LedgeMovement()                     // Ledge������ �� �̵��� ���ѽ�Ű�� �޼���
    {
        float angle = Vector3.Angle(LedgeData.surfaceHit.normal, dir);

        if (angle < 90)
        {
            velocity = Vector3.zero;
            dir = Vector3.zero;
        }
    }

    public void SetControl(bool _hasControl)  // �ܺο��� �����Ͽ� �����׼� �Ǵ� �ൿ���̶�� �˸��� �޼���, SetCotrol(false)��� �ൿ���� ����
    {
        this.hasControl = _hasControl;
        cc.enabled = hasControl;

        if (!hasControl)
        {
            anim.SetFloat("moveAmount", 0f);
        }
    }

    private void OnDrawGizmos()               // ������ �ƴ��� üũ�ϰ� �ִ� ���� ������ ���� ����
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
