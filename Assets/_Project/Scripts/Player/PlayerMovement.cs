using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class PlayerMovement : TickBehaviour
{
    [Header("Move Player")]
    [SerializeField] PlayerAnimBehavior playerAnimBehavior;
    [SerializeField] Transform main;
    [SerializeField] float speed;
    FloatingJoystick floatJoystick;
    CharacterController characterController;
    Transform mainCam => Camera.main.transform;
    Vector3 velocity;
    float gravity = -20f;
    float turnSmoothTime = 0.1f;
    float turnSmoothAngle;
    float horizontal;
    float vertical;
    float eulerAngleYCamPlayer;

    [Header("Jump")]
    [SerializeField] Transform checkHead;
    [SerializeField] Transform checkGround;
    [SerializeField] float jumpHeight;
    bool isGround;

    [Header("Landing")]
    [SerializeField] GameObject effectSmokePrefab;
    [SerializeField] float timeDisplaySmoke = 0.7f;
    float timeEffectSmoke;

    [Header("Ladder")]
    [SerializeField] float distanceLadder = 0.6f;
    bool isOnLadder;
    bool canMoveLadder;

    [Header("Jetpack")]
    [SerializeField] GameObject jetpack;
    bool isJetpack;

    [Header("Attack")]
    bool isAttack;

    private MapController mapController;
    protected override void Awake()
    {
        base.Awake();
        characterController = GetComponent<CharacterController>();
        floatJoystick = UIManager.Panel<PanelGamePlay>().FloatingJoystick;
    }
    private void Start()
    {
        StopAttack();
        mapController = FindFirstObjectByType<MapController>();
    }
    public override void OnUpdate()
    {

        base.OnUpdate();
        //if(IsPlayerDie())
        //{
        //    velocity.y += gravity * Time.deltaTime;
        //    characterController.Move(velocity * Time.deltaTime);
        //    return;
        //}
        if (GameController.Instance.GameState == GameState.Pause)
        {
            if (isJetpack) velocity.y = 0;
            if (playerAnimBehavior.PlayerState == PlayerState.Run)
            {
                playerAnimBehavior.SetPlayerState(PlayerState.Idle);
            }
            return;
        }
        if (!isJetpack)
        {
            CheckForLadder();
            isSlideDown = false;
            if (!isOnLadder) CheckSlope();
        }
        bool inputKey = false;
#if UNITY_EDITOR
        inputKey = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
#endif
        horizontal = isSlideDown ? 0 : (inputKey ? Input.GetAxis("Horizontal") : floatJoystick.Horizontal);
        vertical = isSlideDown ? 0 : (inputKey ? Input.GetAxis("Vertical") : floatJoystick.Vertical);
        Vector3 dirInput = new Vector3(horizontal, 0f, vertical);
        float inputMagnitude = dirInput.magnitude;
        Vector3 forwardCam = mainCam.forward;
        Vector3 rightCam = mainCam.right;
        dirInput.Normalize();
        eulerAngleYCamPlayer = mainCam.eulerAngles.y;

        forwardCam.y = 0f;
        rightCam.y = 0f;

        Vector3 moveDirection = forwardCam * dirInput.z + rightCam * dirInput.x;
        moveDirection.Normalize();

        float targetAngle = Mathf.Atan2(dirInput.x, dirInput.z) * Mathf.Rad2Deg + eulerAngleYCamPlayer;
        float smoothAngle = Mathf.SmoothDampAngle(main.eulerAngles.y, targetAngle, ref turnSmoothAngle, turnSmoothTime);

        if ((!isOnLadder || isJetpack) && !isAttack)
        {
            if (inputMagnitude != 0) main.rotation = Quaternion.Euler(isJetpack ? main.eulerAngles.x : 0f, smoothAngle, 0f);
        }
        forwardCam.Normalize();
        Vector3 move = (rightCam * dirInput.x + forwardCam * dirInput.y).normalized;
        move.y = 0.1f;
        float adjustedSpeed = speed * inputMagnitude;
        if (!isAttack && !isSlideDown)
        {
            if (!isOnLadder)
            {
                characterController.Move(moveDirection * adjustedSpeed * Time.deltaTime);
            }
            else
            {
                HandleLadderMovement();
            }
        }

        if (isOnLadder || isAttack)
        {
            if (isAttack && !isJetpack)
            {
                characterController.Move(velocity * Time.deltaTime);
            }
            return;
        }
        if (!isJetpack)
        {
            CheckJump(inputMagnitude);
        }
        else
        {
        }
        characterController.Move(velocity * Time.deltaTime);
    }
    #region Slide Down
    [SerializeField] float slideSpeed;
    void CheckSlope()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        float rayLength = 1f;
        int layerMask = LayerMask.GetMask(Preconsts.Layer_Ground, Preconsts.Layer_Wall);
        Vector3[] directions = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            Vector3.forward + Vector3.left,
            Vector3.forward + Vector3.right,
            Vector3.back + Vector3.left,
            Vector3.back + Vector3.right
        };
        foreach (Vector3 direction in directions)
        {
            Vector3 rayDirection = direction + Vector3.down * 0.5f;
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit1, rayLength, layerMask))
            {
                Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.blue);
                Vector3 normal = hit1.normal;
                float slopeAngle = Vector3.Angle(Vector3.up, normal);
                if (slopeAngle > characterController.slopeLimit)
                {
                    if (velocity.y < 0 && (vertical == 0 && horizontal == 0))
                    {
                        isSlideDown = true;
                        SlideDown(normal);
                    }

                }
            }
            else
            {
                Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.green);
            }
        }
    }
    bool isSlideDown;
    void SlideDown(Vector3 surfaceNormal)
    {
        Vector3 slideDirection = new Vector3(surfaceNormal.x, -surfaceNormal.y, surfaceNormal.z);
        characterController.Move(slideDirection * slideSpeed * Time.deltaTime);
    }
    #endregion
    #region Move Ladder
    public void SetMoveLadder(bool isMove)
    {
        canMoveLadder = isMove;
    }
    bool isCanOutLadder = false;
    void CheckForLadder()
    {
        int layerLadder = LayerMask.GetMask(Preconsts.Layer_Ladder);
        int layerGround = LayerMask.GetMask(Preconsts.Layer_Ground);
        Vector3 rayOrigin = isOnLadder ? main.position : main.position + Vector3.up * 0.5f;
        Ray rayOnLadder = new Ray(rayOrigin, main.forward);
        Ray rayOutLadder = new Ray(main.position + Vector3.up * 0.05f, Vector3.down * 0.1f);
        isCanOutLadder = Physics.Raycast(rayOutLadder, out RaycastHit _, 0.1f, layerGround);
        bool onLadder = Physics.Raycast(rayOnLadder, out RaycastHit _, distanceLadder, layerLadder);
        isOnLadder = isCanOutLadder ? onLadder && vertical > 0 : onLadder;
        if (isOnLadder) playerAnimBehavior.SetPlayerState(PlayerState.Ladder);
    }
    void HandleLadderMovement()
    {
        bool inputKey = false;
#if UNITY_EDITOR
        inputKey = Input.GetAxis("Vertical") != 0;
#endif
        float verticalInput = inputKey ? Input.GetAxis("Vertical") : floatJoystick.Vertical;
        Vector3 move = new Vector3(0, verticalInput * speed / 2, 0);
        characterController.Move(move * Time.deltaTime);
        velocity.y = 0;
    }
    #endregion

    #region Jetpack
    private Coroutine corJetpack;
    IEnumerator IEJetPack(float timeJetpack)
    {
        isOnLadder = false;
        playerAnimBehavior.SetPlayerState(PlayerState.Jetpack);
        isJetpack = true;
        velocity.y = 0;
        yield return new WaitForSeconds(timeJetpack);
        isJetpack = false;
    }
    void StopJetpack()
    {
        if (corJetpack != null)
        {
            StopCoroutine(corJetpack);
        }
    }
    public void Jetpack(float timeJetpack)
    {
        StopJetpack();
        corJetpack = StartCoroutine(IEJetPack(timeJetpack));
    }
    public void SetVelocJetpack(float velocJetpack)
    {
        velocity.y = velocJetpack;
    }
    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawWireSphere(checkGround.position, radiusJump);
    }
    #region Jump
    float radiusJump = 0.495f;
    void SetAnimJump()
    {
        if (playerAnimBehavior.PlayerState != PlayerState.Jump)
        {
            playerAnimBehavior.SetPlayerState(PlayerState.Jump);
        }
    }
    void CheckJump(float dirInputMagnitude)
    {
        int layerMask = LayerMask.GetMask(Preconsts.Layer_Ground);
        isGround = Physics.CheckSphere(checkGround.position, radiusJump, layerMask);
        if (!isGround)
        {
            if (Physics.Raycast(checkHead.position, Vector3.up, 0.1f) && velocity.y > 1)
            {
                velocity.y = 1;
            }
            velocity.y += gravity * Time.deltaTime;
            timeEffectSmoke += Time.deltaTime;
            if (velocity.y > 0)
            {
                SetAnimJump();
            }
            else
            {
                SetAnimJump();
            }
        }
        else
        {
            if (velocity.y > 0)
            {
                velocity.y += gravity * Time.deltaTime;
                return;
            }
            if (dirInputMagnitude >= 0.001f)
            {
                if (dirInputMagnitude > 0.5f)
                {
                    playerAnimBehavior.SetPlayerState(PlayerState.Run);
                }
                else
                {
                    playerAnimBehavior.SetPlayerState(PlayerState.Walk);
                }
            }
            else
            {
                playerAnimBehavior.SetPlayerState(PlayerState.Idle);
            }
            SmokePlayer();
            if (timeEffectSmoke != 0) { timeEffectSmoke = 0; }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
#endif
    }
    public void Jump()
    {
        if (!isGround) return;
        timeEffectSmoke = 0;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    #endregion

    #region Landing
    void SmokePlayer()
    {
        if (timeEffectSmoke <= timeDisplaySmoke) return;
        SpawnSmoke();
    }
    public void SpawnSmoke()
    {
        Vector3 posSmoke = transform.position + Vector3.up * 0.15f;
        playerAnimBehavior.SetPlayerState(PlayerState.Landing);
    }
    #endregion

    #region Attack
    void StopAttack()
    {
        isAttack = false;
        if (corPlayerAttack != null)
        {
            StopCoroutine(corPlayerAttack);
        }
    }
    public bool IsJetpack()
    {
        return isJetpack;
    }
    public void PlayerAttack(Transform target, Action actionAttack = null)
    {
        if (isAttack) return;
        StopAttack();
        corPlayerAttack = StartCoroutine(IEPlayerAttack(target, actionAttack));
    }
    Coroutine corPlayerAttack;
    IEnumerator IEPlayerAttack(Transform target, Action actionAttack = null)
    {
        isAttack = true;
        if (!isJetpack) velocity.y = -5;
        timeEffectSmoke = 0;
        Vector3 direction = (target.position - main.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        SetAnimWhenAttack();
        while (Quaternion.Angle(main.rotation, targetRotation) > 5f) // Xoay nguoi choi huong vao diem target
        {
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            main.rotation = Quaternion.Slerp(main.rotation, targetRotation, 10f * Time.deltaTime);
            yield return null;
        }
        playerAnimBehavior.SetPlayerState(PlayerState.Attack);
        yield return new WaitForSeconds(0.7f);
        actionAttack?.Invoke();
        yield return new WaitForSeconds(0.6f);
        SetAnimWhenAttack();
        isAttack = false;
    }
    void SetAnimWhenAttack()
    {
        if (isJetpack)
        {
            playerAnimBehavior.SetPlayerState(PlayerState.Jetpack);
        }
        else if (!isJetpack && playerAnimBehavior.PlayerState != PlayerState.Landing)
        {
            playerAnimBehavior.SetPlayerState(PlayerState.Idle);
        }
    }
    #endregion
    public void SetPosPlayer(Vector3 pos)
    {
        transform.position = pos;
    }
    public void ResetPlayer(float angleYPlayer)
    {
        StopJetpack();
        SetRotateAndAngleCam(angleYPlayer);
        playerAnimBehavior.SetPlayerState(PlayerState.Idle);
    }
    void SetRotateAndAngleCam(float angleYPlayer)
    {
        eulerAngleYCamPlayer = 0;
        main.rotation = Quaternion.Euler(0, angleYPlayer + 180, 0);
        angleYPlayer = (angleYPlayer + 360) % 360;
    }

    private BoxCollider jetpackTrigger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trap"))
        {
            Die();
        }

        if (other.CompareTag("Death"))
        {
            Die();
        }

        if (other.CompareTag("Jetpack")) // popup
        {
            jetpackTrigger = other.GetComponent<TriggerJetpack>().boxCollider; 
        }

    }

    void Die()
    {
        StartCoroutine(IEDie());
    }

    IEnumerator IEDie()
    {
        GameController.Instance.GameState = GameState.Pause;
        playerAnimBehavior.SetPlayerState(PlayerState.Die);
        yield return new WaitForSeconds(2f);
        SetPosPlayer(mapController.Checkpoint.position + mapController.Checkpoint.forward * 0.5f); // VI TRI
        ResetPlayer(transform.eulerAngles.y + 180); // GOC XOAY
        var cam = GetComponent<CamPlayer>(); // CAM
        cam.SetRotCamFirstGame(transform.eulerAngles.y);
        yield return new WaitForSeconds(0.2f);
        GameController.Instance.GameState = GameState.Playing;

        if (jetpackTrigger) jetpackTrigger.enabled = true;
    }
}