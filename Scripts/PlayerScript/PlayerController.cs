using Assets.Scripts.PlayerScript;
using MLAPI;
using System.Collections;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    CharacterController characterController;
    PlayerAudioController playerAudioController;
    [SerializeField] Animator anim;
    [SerializeField] Collider[] collidersToDisable;

    //Walk & run
    [SerializeField] private float basespeedMultiplier = 15f;
    [SerializeField] private float speedMultiplier = 15f;
    [SerializeField] float walkSpeed = 15f;
    [SerializeField] float runSpeed = 20f;
    private float penalizedSpeedMultiplier;
    bool running;
    Vector3 normalizedInput;


    //Gravity & jump
    [SerializeField] float jumpForce = 1.5f;
    float verticalForce;[SerializeField]
    float gravity = -5.81f;
    bool wasGroundedLastFrame = true;
    bool lastChangeInUpwardVelocityWasCauseByJumping = false;
    bool jumpPenaltyIsActive = false;
    [SerializeField] float wallJumpVectorHeightFactor = 1.2f;
    [SerializeField] private float wallJumpVectorDistanceFactor = 0.8f;
    bool characterIsGrounded;
    [SerializeField] Transform legSphereCastOrigin;
    [SerializeField] LayerMask playerMasks;

    //EnergiaAbilityt
    PlayerEnergyController playerEnergyController;
    //Vektorit ja kulmat
    //Inputtiin, painovoimaan ja ulkoisiin voimiin perustuva liikesuunta
    Vector3 moveDir;
    Vector3 lastPosition;
    Vector3 velocity = Vector3.zero;
    //Hahmolle mm. sein�hypyist� lis�tty voima
    private Vector3 addedForce = Vector3.zero;
    private Vector3 externalForce = Vector3.zero;
    [SerializeField] LayerMask wallLayer;
    float playerToGroundAngle;

    //Threadit
    Coroutine wallJumpLessenRoutine;
    Coroutine jumpPenaltyRoutine;

    public Vector3 Velocity { get => velocity; set => velocity = value; }

    private void Start()
    {
        playerEnergyController = GetComponent<PlayerEnergyController>();
        playerAudioController = GetComponent<PlayerAudioController>();
    }
    private void OnEnable()
    {
        characterController = GetComponent<CharacterController>();
        DisableCharacterShootColliders();
        speedMultiplier = basespeedMultiplier;
    }

    private void DisableCharacterShootColliders()
    {
        if (!IsLocalPlayer) return;
        for (int i = 0; i < collidersToDisable.Length; i++)
        {
            collidersToDisable[i].GetComponent<BoxCollider>().enabled = false;
        }
    }
    private void Update()
    {

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!IsLocalPlayer) return;
        if (!characterController.enabled) return;
        if (PlayerGlobals.isNotInMenu)
        {
            GetMoveInput();
            JumpInput();
            getJumpsFromWalls(transform.TransformDirection(normalizedInput));
            RunToggler();
            AnimateMovement();
            //if (Input.GetKeyDown(KeyCode.N))
            //{
            //    StartCoroutine(moveAuto());
            //}
        }
        else
        {
            normalizedInput = Vector3.zero;
        }
        if (wasGroundedLastFrame == false)
        {
            if (characterIsGrounded && lastChangeInUpwardVelocityWasCauseByJumping)
            {
                if (jumpPenaltyRoutine != null)
                {
                    StopCoroutine(jumpPenaltyRoutine);
                }
                jumpPenaltyRoutine = StartCoroutine(PenalizeJumping());

                lastChangeInUpwardVelocityWasCauseByJumping = false;

            }
        }
        wasGroundedLastFrame = checkIfCharacterIsGrounded();

    }

    private IEnumerator moveAuto()
    {
        Vector3 moved = transform.TransformDirection(Vector3.right) / 12;
        float timer = 0;
        while (true)
        {
            timer += Time.fixedDeltaTime;
            characterController.Move(moved);
            if (timer >= 2)
            {
                timer = 0;
                moved = -moved;
            }
            yield return null;
        }
    }

    IEnumerator PenalizeJumping()
    {
        //Pelaaja ei voi juosta tapahtuman aikana, inputtia ohjaa tämä bool
        jumpPenaltyIsActive = true;
        float newMult = basespeedMultiplier / 2.3f;
        float refMult = basespeedMultiplier;
        float timer = 0;
        while (timer < 1)
        {

            penalizedSpeedMultiplier = Mathf.Lerp(newMult, refMult, timer / 1);
            timer += Time.fixedDeltaTime;
            yield return null;
        }
        penalizedSpeedMultiplier = basespeedMultiplier;
        jumpPenaltyIsActive = false;
    }
    private void StopOnCollisions()
    {
        if (transform.position == lastPosition)
        {
            if (moveDir.magnitude > 0)
            {
                verticalForce = 0;
                moveDir = Vector3.zero;

            }
            if (externalForce.magnitude > 0)
            {
                StopAllCoroutines();
                externalForce = Vector3.zero;

            }
        }
    }

    private void GetMoveInput()
    {
        normalizedInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //normalisoidaan inputti, jotta W + A , W + D ym "diagonal" kombot ei tuottaisi isompia vauhteja.
        if (normalizedInput.magnitude > 1)
        {
            normalizedInput = normalizedInput.normalized;
        }
    }



    private void FixedUpdate()
    {
        if (!IsLocalPlayer) return;
        if (!characterController.enabled) return;

        characterIsGrounded = checkIfCharacterIsGrounded();

        applyGravity();
        Move();

        StopOnCollisions();

    }
    void applyGravity()
    {
        //Jos pelaaja ei ole maassa, lis�� painovoimaa.
        if (!characterIsGrounded)
        {

            verticalForce += gravity * Time.deltaTime;

        }
    }
    public void ResetCharacterMovement()
    {
        characterIsGrounded = true;
        moveDir = Vector3.zero;
    }


    private void JumpInput()
    {
        if (characterIsGrounded)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                lastChangeInUpwardVelocityWasCauseByJumping = true;
                verticalForce = jumpForce;
            }
            else
            {
                verticalForce = 0f;
            }

        }
    }

    void Move()
    {
        //Luodaan pohja movement vektori painovoimalla ja inputilla
        moveDir = new Vector3(normalizedInput.x, verticalForce, normalizedInput.z) * Time.deltaTime;
        //Pohjaliike pitää kertoa movespeedilla
        moveDir = new Vector3(moveDir.x * speedMultiplier, verticalForce, moveDir.z * speedMultiplier);
        Velocity = transform.TransformDirection(moveDir) + addedForce + externalForce;
        characterController.Move(Velocity);
    }
    public bool IsMovingWithInput()
    {
        return (normalizedInput.magnitude != 0);

    }
    public Vector3 getInputMovementVector()
    {
        return new Vector3(normalizedInput.x, 0, normalizedInput.z);
    }
    public void setExternalForce(Vector3 force)
    {
        externalForce = force;
    }
    public Vector3 getExternalForce()
    {
        return externalForce;
    }
    private void RunToggler()
    {

        if (Input.GetKey(KeyCode.LeftShift) && playerEnergyController.hasEnoughEnergy(2f) && !jumpPenaltyIsActive)
        {
            speedMultiplier = runSpeed;
            if (running == false)
            {
                //Pelaaja saa ilman tätä periaatteessa juossa alle 1s ilman energiankulutusta
                playerEnergyController.ChangeEnergyServerRpc(-2);
                playerEnergyController.drainEnergyServerRpc(2, 1);
                running = true;
            }

        }
        else
        {
            if (running == true)
            {
                running = false;
                playerEnergyController.stopDrainingEnergyServerRpc();
            }
            //Kattoo myös että hidastetaanko pelaajaa hypyn perusteella
            speedMultiplier = getWalkSpeed();
        }




    }

    IEnumerator addAndLessenExternalForce(Vector3 force)
    {
        Vector3 referenceForce = force;
        Vector3 oldExternalForce = externalForce;
        float timer = 0;

        while (timer < 2)
        {
            externalForce = Vector3.Lerp(referenceForce + oldExternalForce, Vector3.zero, timer / 2);
            timer += Time.deltaTime;

            yield return null;
        }


    }
    IEnumerator waitForLanding(Coroutine callbackRoutine)
    {
        if (characterIsGrounded)
        {
            while (characterIsGrounded)
            {
                //Tiedetään että ollaan maassa, odotetaan
                yield return new WaitForEndOfFrame();
            }

        }
        while (!characterIsGrounded)
        {
            //Päästiin pois maasta, odotetaan että ollaan taas grounded.
            yield return new WaitForEndOfFrame();
        }
        externalForce = Vector3.zero;
        lastChangeInUpwardVelocityWasCauseByJumping = false;
        StopCoroutine(callbackRoutine);
        yield return null;


    }
    public void addForceToCharacterController(Vector3 force, bool playerJumpBased = false)
    {
        lastChangeInUpwardVelocityWasCauseByJumping = playerJumpBased;
        Coroutine routine = StartCoroutine(addAndLessenExternalForce(force));
        StartCoroutine(waitForLanding(routine));
    }
    private void getJumpsFromWalls(Vector3 input)
    {
        Debug.DrawRay(transform.position, input);

        if (!characterIsGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position, input, out hit, 1f, wallLayer))
                {
                    Debug.Log(hit.transform.gameObject.name);
                    //Luodaan ensiksi vektorin hahmon sijainnista osumapisteeseen seinästä
                    Vector3 jumpPosition = hit.point - transform.position;
                    //Se t�ytyy peilata hyppyefekti� varten.
                    Vector3 reflectedBounce = Vector3.Reflect(jumpPosition, hit.normal).normalized;
                    //Mutta vaatii my�s hyppyvoiman.
                    reflectedBounce.y = jumpForce;
                    reflectedBounce.x *= wallJumpVectorDistanceFactor;
                    reflectedBounce.z *= wallJumpVectorDistanceFactor;
                    Vector3 addedWallForce = reflectedBounce / wallJumpVectorHeightFactor;
                    verticalForce = 0;
                    Debug.DrawRay(hit.point, reflectedBounce, Color.red, 5f);
                    if (wallJumpLessenRoutine != null)
                    {
                        StopCoroutine(wallJumpLessenRoutine);
                    }

                    addForceToCharacterController(addedWallForce, true);
                    playerAudioController.PlayAudio(PlayerAudioType.Walljump);
                }
            }
        }

    }


    private void AnimateMovement()
    {
        anim.SetFloat("YSpeed", Input.GetAxis("Vertical"));
        anim.SetFloat("XSpeed", Input.GetAxis("Horizontal"));
    }
    bool checkIfCharacterIsGrounded()
    {
        RaycastHit hit;

        if (Physics.Raycast(legSphereCastOrigin.position, Vector3.down, 0.15f, ~playerMasks))
        {


            //Mahdollistaa sein�hypyt slightly vinoista seinist�
            if (playerToGroundAngle > 0.4)
            {
                return false;
            }

            return true;
        }
        else
        {
            return false;
        }

    }
    float getWalkSpeed()
    {
        return (jumpPenaltyIsActive) ? penalizedSpeedMultiplier : walkSpeed;
    }
    public void EnableMovement() => characterController.enabled = true;
    public void DisableMovement() => characterController.enabled = false;




}

