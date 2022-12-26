using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

	// 게임씬 포톤 서버 UI 연동 매니저
    private GameNetworkUIManager gameNetworkUIManager;
	
    [Header("MoveStat")] 
    [SerializeField] private float walkSpeed = 2f;    // 걷기 속도
    [SerializeField] private float runSpeed  = 5f;    // 달리기 속도
    
    private float applySpeed;                         // 위에 속도를 받아와서 적용될 실제 속도

    [SerializeField] private float jumpForce;         // 점프 크기

    [Header("Rotation")]
    [SerializeField] private float rotSpeed;          // 회전 속도
    [SerializeField] float cameraRotationLimit;       // 카메라 회전 제한
    private float currentCameraRotationX = 0f;

    [SerializeField] private float zoomCamRotY = 0f;  

    [Header("Ground Check Values")] 
    [SerializeField] float groundCheckRadius = 0.2f;  // 땅 확인용 구체의 반지름 길이
    [SerializeField] Vector3 groundCheckOffset;       // 캐릭터 바닥 위치 조정용 위치 값
    [SerializeField] LayerMask groundLayer;           // physics 확인용 layer

    [SerializeField] GameObject GO_canvas;            // 나 이외의 UI 제거용 오브젝트

    // 상태 변수
    private bool isRun = false; 			// 달리는 도중 로직 막기 위한 변수
    private bool isGrounded;    			// 캐릭터 점프 확인 변수
    private bool hasControl = true; 		// 파쿠르 액션 중 이동 제한을 위한 변수

    // 이동을 위한 임시 변수
    private float bodyAngle;    			// 캐릭터 회전 각도를 저장할 변수

    private Vector3 dir;        			// 캐릭터 이동 방향
    private Vector3 velocity;   			// 캐릭터 이동 속도
    private float ySpeed;       			// 캐릭터 떨어지는 값 저장
    private float locoMotionAmount; 		// 캐릭터 이동 블랜드트리 값 저장 
    private float staminaTime;      		// 스테미나 감소 사용 시간 체크 변수

    private Quaternion targetRotation; 		// 파쿠르 대상 각도 저장?

    // 자연스러운 애니메이션 동기화를 위한 임시 변수
    private float tempAni = 1;

    // 필요 컴포넌트
    private Camera camera;	// 플레이어 카메라
    private CharacterController cc;
    public Animator anim;
    private EnvironmentScanner environmentScanner;
    private StatusController statusController;

    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }

    // 프로퍼티
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

        // 씬에서 GameNetworkUIManager 오브젝트를 찾아 할당
        gameNetworkUIManager = FindObjectOfType<GameNetworkUIManager>();
		
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        statusController = GetComponent<StatusController>();

        applySpeed = walkSpeed;                                          // 처음 속도는 걷는 속도로

        Cursor.lockState = CursorLockMode.Locked;                        // 시작할 때 커서 숨기기
    }


    private void Update()
    {
        if (!PV.IsMine) return;

        AnimatorAvatar();

        // UI창이 모두 닫혔을 때만 커서가 고정되도록 설정
        // I키를 눌렀을 때 제한시키고 싶은 것들 넣기
        if (!Inventory.inventoryActivated && !gameNetworkUIManager.isUIActive)                               
        {        
            CamRotation();
            CharacterRotation();
            Cursor.lockState = CursorLockMode.Locked;	// 비활성화 중일때는 잠그고
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;		// 활성화 중일때는 고정한다.
        }

		// 파쿠르 도중에는 이동 불가, 카메라 이동은 파쿠르 움직을 따라가게 먼저 실행
        if (!hasControl) return;	

		// 플레이어 키보드 상호작용은 UI가 비활성화 일때만 실행
        if(gameNetworkUIManager.isUIActive == false)
        {
			GroundCheck();                       	// 캐릭터컨트롤러의 isGround에 오류가 많다고 하여 땅을 체크하는 함수 구현
			anim.SetBool("isGrounded", isGrounded); // 하강 애니메이션 중일때 착지 상태에 변하게 하기 위해 구현

			TryRun();	// LeftShift키를 눌렀는지 안눌렀는지 체크
			Move();		// 반드시 TryRun 뒤에서 실행되야함. 캐릭터 이동 로직
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

    private void CharacterRotation()              // 카메라를 캐릭터 내부에 넣어 좌우 이동 및 게임 화면 좌우가 같이 움직인다.
    {
        float _yRotation = Input.GetAxis("Mouse X");
        bodyAngle += _yRotation * rotSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0, bodyAngle, 0);
    }

    private void CamRotation()                    // 화면 상하 이동은 카메라만 움직이고, 움직일 수 있는 최대 각도를 제한 해둠
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * rotSpeed * Time.deltaTime;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); // 카메라 Limit -,+같게 했지만, 다르게 개별 변수도 사용 가능

        camera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
    }

    private void TryRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && statusController.CurSp >= 10)  // 스테미너를 사용하여 뛰는 것을 제한함
        {
            Running();
        }
        else if (statusController.CurSp < 5)  // 스테미너가 없으면 자동으로 걷도록 변경
            RunningCancle();

        if (Input.GetKeyUp(KeyCode.LeftShift)) // LeftShit키를 땔때 걷는다.
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

        float moveAmount = Mathf.Clamp01(Mathf.Abs(_moveDirx) + Mathf.Abs(_moveDirz)); // 0~1값을 받아오며, 해당 값으로 BlendTree에서 대기,걷기,달리기 상태를 체크한다.

        dir = new Vector3(_moveDirx, 0, _moveDirz).normalized;

        dir = camera.transform.TransformDirection(dir);            //카메라가 보는 방향으로 걷기 위해

        velocity = Vector3.zero; // dir을 받아와 실제 이동에 사용될 변수

        if (isGrounded) // 땅 위에서만 이동할 수 있도록
        {
            if(isRun)
            statusController.DecreaseSP(1);

            ySpeed = -0.5f;
            velocity = dir * applySpeed;

            #region Ledge // Ledge상태라면 dir, Vector3를 zero로 변경하여 못움직이게 한다.
            IsOnLedge = environmentScanner.LedgeCheck(dir, out LedgeData ledgeData);
            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }
            #endregion

            if (isRun)
                locoMotionAmount = velocity.magnitude / applySpeed;         // locoMotionAmount값에 따라 자연스럽게 애니메이션이 출력된다.
            else
                locoMotionAmount = Mathf.Clamp(velocity.magnitude / applySpeed, 0f, 0.5f); // 걷는 상태일 경우에는 0 ~ 0.5 사이에서만 값이 변환되도록 한다.

            anim.SetFloat("moveAmount", locoMotionAmount, 0.2f, Time.deltaTime); // 애니메이터에서 실행
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;     // Physics에 등록된 중력값으로 캐릭터가 하강함.

            dir = transform.forward * applySpeed / 2;         
        }
        velocity.y = ySpeed;
        
        cc.Move(velocity * Time.deltaTime);
    }

    private void GroundCheck()               // 땅 감지한 경우
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    void LedgeMovement()                     // Ledge상태일 때 이동을 제한시키는 메서드
    {
        float angle = Vector3.Angle(LedgeData.surfaceHit.normal, dir);

        if (angle < 90)
        {
            velocity = Vector3.zero;
            dir = Vector3.zero;
        }
    }

    public void SetControl(bool _hasControl)  // 외부에서 간섭하여 파쿠르액션 또는 행동중이라고 알리는 메서드, SetCotrol(false)라면 행동하지 못함
    {
        this.hasControl = _hasControl;
        cc.enabled = hasControl;

        if (!hasControl)
        {
            anim.SetFloat("moveAmount", 0f);
        }
    }

    private void OnDrawGizmos()               // 땅인지 아닌지 체크하고 있는 것을 눈으로 보기 위함
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
