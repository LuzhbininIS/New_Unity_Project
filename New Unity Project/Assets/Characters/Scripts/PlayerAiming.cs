using Cinemachine;
using UnityEngine;
using Mirror;
using UnityEngine.Animations.Rigging;

public class PlayerAiming : MonoBehaviour//NetworkBehaviour 
{
    //[SerializeField] private float mouseSensitivity = 100.0f;
    [SerializeField] private float turnSpeed = 0.05f;
    [SerializeField] private float aimAngle = 0f;
    [SerializeField] private CinemachineVirtualCamera standardCamera;
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private RigLayer standardLayer;
    [SerializeField] private RigLayer zoomLayer;
    
    private Camera mainCamera;
    private int aimingAngleHash = Animator.StringToHash("aimAngle");
    private Animator animator;
    private bool isZooming = false;
    public bool IsZooming
    {
        get { return isZooming; }
        set
        {
            isZooming = value;
            standardCamera.gameObject.SetActive(!isZooming);
            zoomCamera.gameObject.SetActive(isZooming);
            standardLayer.active = !IsZooming;
            zoomLayer.active = isZooming;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) { IsZooming = true; }
        if (Input.GetMouseButtonUp(1)) { IsZooming = false; }
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
