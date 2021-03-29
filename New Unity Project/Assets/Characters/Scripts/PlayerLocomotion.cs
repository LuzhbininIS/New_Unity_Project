using Mirror;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour//NetworkBehaviour
{
    private Animator animator;
    private NetworkAnimator networkAnimator;
    private CharacterController controller;
    
    [Header("Settings")]
    [SerializeField] private float runningSpeed;
    [SerializeField] private float crouchingSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private LayerMask groundCollisionMask;
    [SerializeField] private float checkSphereRadius;

    private bool isGrounded = true;
    private bool isCrouching = false;
    private bool isJumping = false;

    private float minimumDownVelocity = -0.1f;
    private float verticalVelocity = 0.0f;

    private int speedXHash = Animator.StringToHash("speedX");
    private int speedYHash = Animator.StringToHash("speedY");
    private int isJumpingHash = Animator.StringToHash("isJumping");
    private int isCrouchingHash = Animator.StringToHash("isCrouching");

    private float crouchColliderHeight = 1.18f;
    private float runColliderHeight = 1.6f;
    private Vector3 crouchColliderCenter = new Vector3(0.0f, 0.6f, 0.0f);
    private Vector3 runColliderCenter = new Vector3(0.0f, 0.89f, 0.0f);

    private void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        networkAnimator = GetComponent<NetworkAnimator>();
        verticalVelocity = minimumDownVelocity;
        controller.center = runColliderCenter;
        controller.height = runColliderHeight;
    }

    private bool IsGrounded()
    {
        isGrounded = Physics.CheckSphere(transform.position, checkSphereRadius, groundCollisionMask);
        if (isGrounded && isJumping)
        {
            isJumping = false;
            animator.SetBool(isJumpingHash, false);
        }
        return isGrounded;
    }

    private void Update()
    {
        isGrounded = IsGrounded();
        //Debug.Log(isGrounded);

        // Setting vertical acceleration
        if (isGrounded)
        {
            verticalVelocity = minimumDownVelocity;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Jumping
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = jumpSpeed * Time.deltaTime;
            networkAnimator.SetTrigger(isJumpingHash);
            isJumping = true;
            animator.SetBool(isJumpingHash, isJumping);
        }

        // Crouching
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            animator.SetBool(isCrouchingHash, isCrouching);

            if (isCrouching)
            {
                controller.center = crouchColliderCenter;
                controller.height = crouchColliderHeight;
            }
            else
            {
                controller.center = runColliderCenter;
                controller.height = runColliderHeight;
            }
        }

        float speedX = Input.GetAxis("Horizontal");
        float speedY = Input.GetAxis("Depth");
        float actualSpeed = (isCrouching) ? crouchingSpeed : runningSpeed;
        CmdMove(speedX, speedY, actualSpeed);
    }

    /*[Command]*/ private void CmdMove(float x, float y, float speed)
    {
        Vector3 movement = new Vector3(x, 0, y) * Time.deltaTime * speed;
        movement = transform.TransformDirection(movement);
        movement.y = verticalVelocity;

        animator.SetFloat(speedXHash, x);
        animator.SetFloat(speedYHash, y);
        controller.Move(movement);
        controller.Move(movement);
    }
}
