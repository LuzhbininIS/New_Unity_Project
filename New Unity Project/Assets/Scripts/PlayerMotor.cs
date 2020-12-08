using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Rigidbody rb;

    private Vector3 movement = Vector3.zero;

    private Vector3 rotation = Vector3.zero;

    private Vector3 rotationCamera = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 _movement)
	{
        movement = _movement;
	}

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void RotateCam(Vector3 _camRotation)
    {
        rotationCamera = _camRotation;
    }

    private void FixedUpdate()
	{
        PerformMove();
        PerformRotation();
    }

    void PerformMove()
	{
        if (movement != Vector3.zero)
            rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
	}

    void PerformRotation()
	{
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
            cam.transform.Rotate(rotationCamera);
	}
}
