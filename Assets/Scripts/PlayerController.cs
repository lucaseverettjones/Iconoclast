using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

enum PlayerState
{
    Grounded, Jumping, Falling, Dashing
}
public class PlayerController : MonoBehaviour
{
    private PlayerState state;
    #region Movement Fields
    [SerializeField] private Transform playerCamera, cameraHolder;
    [SerializeField] private float defaultMouseSensitivity = 2f, dashDistance, dashTime, jumpHeight, speed = 3, defaultFOV = 60, dashingFOV = 45;
    [SerializeField][Range(0f, 0.5f)] private float moveSmoothTime = 0.3f;
    [SerializeField][Range(0f, 0.5f)] private float mouseSmoothTime = 0.03f;
    [SerializeField][Range(0, 0.1f)] private float headBobAmplitude = 0.015f;
    [SerializeField][Range(0, 30)] private float headBobFrequency = 10;
    private float cameraPitch = 0, jumpForceMultiplier, mouseSensitivity;
    private Vector2 currentDir = Vector2.zero, currentDirVelocity = Vector2.zero, currentMouseDelta = Vector2.zero, currentMouseDeltaVelocity = Vector2.zero;
    private Vector3 movement, cameraStartPosition;
    #endregion
    private bool lockCursor = true, interactibleInRange = false, shouldHeadBob = false, canMidairDash = true, crouching = false, onMovingPlatform = false, movingWithPlatform;
    private GameObject currentPlatform;
    private Collider targetedInteractible;
    private CharacterController characterController;
    [SerializeField] private CinemachineImpulseSource headBobImpulseSource, shakeImpulseSource;
    private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AnimationCurve jumpFallOff;
    private Animator animator;
    private UIController uIController;
    private SimonController simonController;
    private RuleContainer ruleContainer;
    #region SFX Fields
    private AudioSource[] mySources;
    [SerializeField] private AudioClip jumpSound, landSound, crouchSound, collideSound, breakSound, doorBreakSound, doorOpenSound, doorCloseSound;
    [SerializeField] private AudioClip[] dashSounds, midairDashSounds;
    [SerializeField] private bool playFallSound, playWalkSound, playDashSound, playJumpSound, playLandSound;
    private bool soundsStopped = false;
    #endregion
    private VFXController vFXController;
    [SerializeField] private Mesh doorMesh;
    private List<Collider> lastCollided, collided;
    public bool freeze;
    public Transform lastCheckpoint;
    private Coroutine fovCoroutine;
    private void Start()
    {
        collided = new List<Collider>();
        lastCollided= new List<Collider>();
        playWalkSound = true;
        playLandSound = true;
        playDashSound = true;
        playJumpSound = true;
        movement = Vector3.zero;
        mySources = GetComponents<AudioSource>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraStartPosition = playerCamera.localPosition;
        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        virtualCamera.m_Lens.FieldOfView = defaultFOV;
        try
        {
            ruleContainer = GameObject.Find("RuleContainer").GetComponent<RuleContainer>();
            uIController = GameObject.Find("Canvas").GetComponent<UIController>();
            vFXController = GameObject.Find("VFXController").GetComponent<VFXController>();
        }
        catch { Debug.Log("no canvas, rulecontainer, or vfxcontroller"); }
        jumpForceMultiplier = CalculateJumpForceMultiplier();
        
        if (uIController != null && uIController.mouseSensitivity != 0)
        {
            UpdateMouseSensitivity();
        }
        else
        {
            mouseSensitivity = defaultMouseSensitivity;
        }
        StartCoroutine("EnableFallSound");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (freeze)
            {
                freeze = false;
            }
            else
            {
                freeze = true;
                movement = Vector3.zero;
                characterController.Move(Vector3.zero);
            }
            mySources[0].Stop();
            uIController.ToggleMenu();
        }
        if (!freeze)
        {
            UpdateMouseLook();
            UpdateMovement();
            CheckForInteractibles();
            soundsStopped = false;
        }
        else if(!soundsStopped)
        {
            foreach(AudioSource sound in mySources)
            {
                sound.Stop();
            }
            soundsStopped = true;
        }
    }

    public void UpdateMouseSensitivity()
    {
        if (uIController.mouseSensitivity != 0)
        {
            mouseSensitivity = defaultMouseSensitivity * uIController.mouseSensitivity;
        }
    }

    public void UpdateShouldHeadBob()
    {
        if(uIController.headBob == 0)
        {
            shouldHeadBob = false;
        }
        else
        {
            shouldHeadBob = true;
        }
    }

    #region Motion
    private void UpdateMovement()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Dash();
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            ToggleCrouch();
        }
        if (characterController.isGrounded)
        {
            if (state != PlayerState.Dashing && state != PlayerState.Grounded)
            {
                state = PlayerState.Grounded;
                playWalkSound = true;
            }
            if(!canMidairDash)
            {
                canMidairDash = true;
            }
            playLandSound = false;
        }
        else if(state == PlayerState.Grounded)
        {
            state = PlayerState.Falling;
            playLandSound = true;
        }
        if (state != PlayerState.Dashing)
        {
            Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if(!mySources[0].isPlaying && targetDir.magnitude != 0 && state == PlayerState.Grounded && playWalkSound)
            {
                //Debug.Log("playing walk sound");
                mySources[0].volume = uIController.sfxVolume;
                mySources[0].Play();
            } else if (mySources[0].isPlaying && (targetDir.magnitude == 0 || !playWalkSound))
            {
                mySources[0].Stop();
            }
            targetDir.Normalize();
            if(state != PlayerState.Grounded)
            {
                currentDir.x *= 0.5f;
            }
            if(targetDir.x > 0 || targetDir.y > 0)
            {
                ResetCameraPosition();
            }
            currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);
            Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x);
            velocity *= speed;
            movement.x = velocity.x;
            movement.z = velocity.z;
        }
        if (state == PlayerState.Dashing)
        {
            movement.y  = 0;
        }
        else
        {
            ApplyGravity();
        }
        Move();
    }
    #region Jump functions

    public void Jump()
    {
        if (state != PlayerState.Jumping && characterController.isGrounded && ruleContainer.canJump)
        {
            state = PlayerState.Jumping;
            movement.y = 0;
            playWalkSound = false;
            StartCoroutine("JumpEvent");
        }
    }

    private IEnumerator JumpEvent()
    {
        if(SFXController.instance != null && playJumpSound)
        {
            SFXController.instance.PlaySound(mySources[1], jumpSound);
        }
        characterController.slopeLimit = 90f;
        float timeInAir = 0;
        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            characterController.Move(Vector3.up * jumpForce * jumpForceMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;
            if(jumpForce <= 0)
            {
                state = PlayerState.Falling;
            }
            yield return null;
        } while (!characterController.isGrounded && characterController.collisionFlags != CollisionFlags.Above);
        characterController.slopeLimit = 45f;
    }
    #endregion

    #region Dash functions

    private void Dash()
    {
        if (state != PlayerState.Dashing && ruleContainer.canDash)
        {
            if (characterController.isGrounded)
            {
                state = PlayerState.Dashing;
                playWalkSound = false;
                fovCoroutine = StartCoroutine(FOVChange(dashingFOV, 0.1f));
                StartCoroutine("DashEvent");
                playLandSound = false;
            }
            else if (canMidairDash)
            {
                state = PlayerState.Dashing;
                canMidairDash = false;
                fovCoroutine = StartCoroutine(FOVChange(dashingFOV, 0.1f));
                StartCoroutine("DashEvent");
            }
        }
    }

    private IEnumerator DashEvent()
    {
        if(SFXController.instance != null && playDashSound)
        {
            if (characterController.isGrounded)
            {
                int soundIndex = Random.Range(0, dashSounds.Length);
                SFXController.instance.PlaySound(mySources[1], dashSounds[soundIndex]);
                StartCoroutine("EnableWalkSound", dashSounds[soundIndex].length);
            }
            else
            {
                int soundIndex = Random.Range(0, midairDashSounds.Length);
                SFXController.instance.PlaySound(mySources[1], midairDashSounds[soundIndex]);
            }
        }
        movement = transform.forward * (dashDistance / dashTime);
        yield return new WaitForSeconds(dashTime);
        StopDash();
    }

    private void StopDash()
    {
        movement = Vector3.zero;
        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
        }
        fovCoroutine = StartCoroutine(FOVChange(defaultFOV, 0.125f));
        if (characterController.isGrounded)
        {
            state = PlayerState.Grounded;
            Debug.Log(state);
        }
        else
        {
            state = PlayerState.Falling;
            Debug.Log(state);
        }
        playLandSound = false;
    }
    #endregion

    #region Crouch functions

    private void ToggleCrouch()
    {
        if(crouching)
        {
            StandUp();
            crouching = false;
        }
        else
        {
            Crouch();
            crouching = true;
        }
    }

    private void Crouch()
    {
        if(state == PlayerState.Grounded && ruleContainer.canCrouch)
        {
            if(SFXController.instance != null)
            {
                SFXController.instance.PlaySound(mySources[1], crouchSound);
            }
            animator.SetBool("isCrouching", true);
        }
    }

    private void StandUp()
    {
        if(characterController.isGrounded)
        {
            state = PlayerState.Grounded;
        }
        else
        {
            state = PlayerState.Falling;
        }
        animator.SetBool("isCrouching", false);
    }
    #endregion

    private void Move()
    {
        if (characterController.isGrounded && (Mathf.Abs(movement.x) >= 0.5 || Mathf.Abs(movement.z) >= 0.5) && state != PlayerState.Dashing)
        {
            PlayMotion(FootstepMotion());
        }
        if (!(onMovingPlatform && movingWithPlatform))
        {
            characterController.Move(movement * Time.deltaTime);
        }
        if (onMovingPlatform)
        {
            if (Mathf.Abs(movement.x) <= 0.1 && Mathf.Abs(movement.z) <= 0.1)
            {
                PlatformController platformController = currentPlatform.GetComponentInParent<PlatformController>();
                if (!movingWithPlatform)
                {
                    transform.parent = platformController.transform;
                    movingWithPlatform = true;
                }
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
            else
            {
                transform.parent = null;
                movingWithPlatform = false;
            }
        }
        
        DetectCollisions();
    }
    #endregion

    #region Camera Functions
    private void UpdateMouseLook()
    {
        playerCamera.LookAt(FocusTarget());
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
        cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90, 90);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(Vector3.up, currentMouseDelta.x * mouseSensitivity);
    }

    private Vector3 FootstepMotion()
    {
        Vector3 position = Vector3.zero;
        if (shouldHeadBob)
        {
            position.y += Mathf.Sin(Time.time * headBobFrequency) * headBobAmplitude;
            position.x += Mathf.Cos(Time.time * headBobFrequency / 2) * headBobAmplitude * 2;
        }
        return position;
    }

    private void PlayMotion(Vector3 motion)
    {
        playerCamera.localPosition += motion;
    }

    private void ResetCameraPosition()
    {
        if(playerCamera.localPosition != cameraStartPosition)
        {
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, cameraStartPosition, Time.deltaTime);
        }
    }

    private Vector3 FocusTarget()
    {
        Vector3 position = new Vector3(transform.position.x, transform.position.y + cameraHolder.localPosition.y, transform.position.z);
        position += cameraHolder.forward * 15;
        return position;
    }

    private IEnumerator FOVChange(float newFOV, float transitionTime)
    {
        float timeElapsed = 0f;
        while (virtualCamera.m_Lens.FieldOfView != newFOV)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, newFOV, timeElapsed / transitionTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region Physics
    private void ApplyGravity()
    {
        if(!characterController.isGrounded && state != PlayerState.Dashing)
        {
            movement.y -= 9.8f * Time.deltaTime;
        }
    }

    private float CalculateJumpForceMultiplier()
    {
        float multiplier = (jumpHeight + 0.4814f) / 0.3056f;
        return multiplier;
    }

    #endregion

    #region Interaction

    private void CheckForInteractibles()
    {
        if (!freeze)
        {
            float interactRange = 1.5f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                if ((collider.gameObject.CompareTag("Interactible") || collider.gameObject.CompareTag("Ladder") || collider.gameObject.CompareTag("TeleportPoint") || collider.gameObject.CompareTag("Door") || collider.gameObject.CompareTag("BlockedDoor") && !freeze))
                {
                    if (!interactibleInRange)
                    {
                        interactibleInRange = true;
                        uIController.ToggleInteract();
                    }
                    targetedInteractible = collider;
                    break;
                }
                else
                {
                    if (interactibleInRange)
                    {
                        interactibleInRange = false;
                        uIController.ToggleInteract();
                    }
                }
            }
        }
    }

    private void Interact()
    {
        if (interactibleInRange) 
        {
            if (targetedInteractible.TryGetComponent(out NPCInteraction npcInteraction))
            {
                npcInteraction.Interact();
            }
            else if (targetedInteractible.TryGetComponent(out PillarController pillarController))
            {
                pillarController.Interact();
            }
            else if (targetedInteractible.TryGetComponent(out ButtonController buttonController))
            {
                buttonController.Interact();
            }
            else if (targetedInteractible.TryGetComponent(out TabletController tabletController))
            {
                tabletController.FallOver();
            }
            else
            {
                try
                {
                    tabletController = targetedInteractible.GetComponentInParent<TabletController>();
                    tabletController.FallOver();
                }
                catch{}
            }
            if (targetedInteractible.TryGetComponent(out SimonController newSimonController))
            {
                newSimonController.GlowSequence();
            }
            else if(targetedInteractible.TryGetComponent(out FountainController fountainController))
            {
                if (fountainController.puzzleComplete)
                {
                    uIController.Speak("", "The magic seal has been broken; the door will now open to you.", false, gameObject);
                }
                else
                {
                    uIController.Speak("", "Use the pillars in the correct order to open the door and escape.", false, gameObject);
                }
            }
            else if (targetedInteractible.gameObject.CompareTag("Ladder"))
            {
                GameObject position1 = targetedInteractible.gameObject.transform.GetChild(0).gameObject;
                Vector3 pos1 = position1.transform.position;
                Vector3 distance1 = pos1 - transform.position;
                Vector3 distance2 = Vector3.zero;
                Vector3 pos2 = Vector3.zero;
                try
                {
                    GameObject position2 = targetedInteractible.gameObject.transform.GetChild(1).gameObject;
                    Debug.Log(position1.name + "," + position2.name);
                    pos2 = position2.transform.position;
                    distance2 = pos2 - transform.position;
                }
                catch
                {
                    Debug.Log("One-Way Ladder");
                }
                if (distance1.magnitude > distance2.magnitude)
                {
                    characterController.enabled = false;
                    characterController.transform.position = new Vector3(pos1.x, pos1.y, pos1.z);
                    characterController.enabled = true;
                }
                else
                {
                    characterController.enabled = false;
                    characterController.transform.position = new Vector3(pos2.x, pos2.y, pos2.z);
                    characterController.enabled = true;
                }
            }
            else if(targetedInteractible.gameObject.CompareTag("TeleportPoint"))
            {
                freeze = true;
                try
                {
                    AudioSource doorAudio = targetedInteractible.GetComponent<AudioSource>();
                    if (SFXController.instance != null)
                    {
                        SFXController.instance.PlaySound(doorAudio, doorOpenSound);
                    }
                }
                catch
                {
                    Debug.Log("No door audio source found");
                }
                uIController.LoadScene("Next");
            }
            else if(targetedInteractible.gameObject.CompareTag("BlockedDoor"))
            {
                uIController.Speak("", "The door is blocked by some mechanism.", false);
            }
            else if(targetedInteractible.gameObject.name.Contains("Simon"))
            {
                if(simonController == null)
                {
                    simonController = GameObject.Find("SimonController").GetComponent<SimonController>();
                }
                if(targetedInteractible.gameObject.name.Contains("1"))
                {
                    simonController.Check(1);
                    Debug.Log("Simon1");
                }
                else if (targetedInteractible.gameObject.name.Contains("2"))
                {
                    simonController.Check(2);
                    Debug.Log("Simon2");
                }
                else if (targetedInteractible.gameObject.name.Contains("3"))
                {
                    simonController.Check(3);
                    Debug.Log("Simon3");
                }
            }
        }
    }

    public void GoToCheckpoint(Animator sender)
    {
        transform.position = lastCheckpoint.position;
        freeze = false;
        if(state == PlayerState.Dashing)
        {
            StopDash();
        }
        sender.SetBool("isTalking", false);
    }
    #endregion

    #region Collision
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Breakable") && state == PlayerState.Dashing)
        {
            if (hit.gameObject.TryGetComponent(out ShatterScript shatterScript))
            {
                if (hit.gameObject.name.ToLower().Contains("door"))
                {
                    shatterScript.Explode();
                    if (SFXController.instance != null)
                    {
                        SFXController.instance.PlaySound(mySources[1], doorBreakSound);
                    }
                }
                else
                {
                    shatterScript.Explode();
                    if (SFXController.instance != null)
                    {
                        SFXController.instance.PlaySound(mySources[1], breakSound);
                    }
                }
            }
            else
            {
                Destroy(hit.gameObject);
            }
            shakeImpulseSource.GenerateImpulse();
            vFXController.SpawnParticle(hit.gameObject);
            StopCoroutine("DashEvent");
            StopDash();
        }
        else if (hit.gameObject.CompareTag("BreakableFloor"))
        {
            if (hit.gameObject.TryGetComponent(out ShatterScript shatterScript))
            {
                shatterScript.Explode();
            }

        }
        else if(hit.gameObject.CompareTag("Fire"))
        {
            freeze = true;
            uIController.LoadScene("Level1");
        }
    }

    private void DetectCollisions()
    {
        float yCoord1;
        float yCoord2;
        if(crouching)
        {
            yCoord1 = 0.75f;
            yCoord2 = 0.75f;
        }
        else
        {
            yCoord1 = 0.5f;
            yCoord2 = 1.0f;
        }
        Collider[] colliders = Physics.OverlapCapsule(transform.TransformPoint(0, yCoord1, 0), transform.TransformPoint(0, yCoord2, 0), 0.6f);
        lastCollided = colliders.ToList();
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        for(int i = 0; i < collided.Count; i++)
        {
            if (lastCollided.Contains(collided[i]))
            {
                OnStay(collided[i].gameObject);
            }
            else if (collided[i] != null)
            {
                OnExit(collided[i].gameObject);
                collided.Remove(collided[i]);
            }
        }
        for(int i = 0; i < lastCollided.Count; i++)
        {
            if (!collided.Contains(lastCollided[i]))
            {
                OnInitialHit(lastCollided[i].gameObject);
                collided.Add(lastCollided[i]);
            }
            lastCollided.Remove(lastCollided[i]);
        }
    }

    private void OnInitialHit(GameObject hitObject)
    {
        if ((!hitObject.CompareTag("Floor")) && (!hitObject.CompareTag("BreakableFloor")) && ((characterController.collisionFlags & CollisionFlags.Sides) != 0) && SFXController.instance != null)
        {
            SFXController.instance.PlaySound(mySources[1], collideSound);
            Debug.Log("Played Collision Sound");
        }
        if(hitObject.CompareTag("MovingPlatform"))
        {
            onMovingPlatform = true;
            currentPlatform = hitObject;
        }
        if(hitObject.CompareTag("BreakableFloor"))
        {
            SFXController.instance.PlaySound(mySources[1], breakSound);
        }
        if(hitObject.CompareTag("Checkpoint"))
        {
            lastCheckpoint = hitObject.transform;
        }
        if (hitObject.TryGetComponent(out ThroneRoomGuardsController throneRoom))
        {
            throneRoom.Interact();
        }
    }

    private void OnStay(GameObject hitObject)
    {

    }

    private void OnExit(GameObject hitObject)
    {
        if (hitObject.CompareTag("MovingPlatform"))
        {
            onMovingPlatform = false;
            transform.parent = null;
        }
    }
    IEnumerator DestroyDelayed(GameObject toDestroy)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(toDestroy);
    }
    #endregion

    #region SFX Coroutines
    private IEnumerator EnableFallSound()
    {
        yield return new WaitForSeconds(1f);
        playFallSound = true;
    }

    IEnumerator EnableWalkSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        playWalkSound = true;
    }
    IEnumerator PlayCloseSound(AudioSource doorAudio, float animLength)
    {
        yield return new WaitForSeconds(animLength);
        SFXController.instance.PlaySound(doorAudio, doorCloseSound);
    }
    #endregion
}
