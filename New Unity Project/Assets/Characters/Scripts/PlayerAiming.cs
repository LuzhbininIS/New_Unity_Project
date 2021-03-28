using UnityEngine;

public class PlayerAiming : MonoBehaviour
{   
    [SerializeField] private float mouseSensitivity = 100.0f;
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private float aimAngle = 0f;
    private Camera mainCamera;

    private int aimingAngleHash = Animator.StringToHash("aimAngle");
    private Animator animator;

    private void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        aimAngle = mainCamera.transform.rotation.eulerAngles.x;
        if (aimAngle > 300) { aimAngle -= 360; } 
        animator.SetFloat(aimingAngleHash, -aimAngle);

        float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), turnSpeed * Time.fixedDeltaTime);
    }
}
