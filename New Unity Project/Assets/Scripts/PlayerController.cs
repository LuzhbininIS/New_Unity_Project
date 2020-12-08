using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
	[SerializeField]
	private float speed = 5f;
	[SerializeField]
	private float lookSpeed = 3f;

	private PlayerMotor motor;

	// Start is called before the first frame update
	void Start()
	{
		motor = GetComponent<PlayerMotor>();
	}

	// Update is called once per frame
	void Update()
	{
		float xMove = Input.GetAxisRaw("Horizontal");
		float zMove = Input.GetAxisRaw("Vertical");

		Vector3 horMove = transform.right * xMove;
		Vector3 verMove = transform.forward * zMove;

		Vector3 movement = (horMove + verMove).normalized * speed;

		motor.Move(movement);

		float xRote = Input.GetAxisRaw("Mouse X");
		Vector3 rotation = new Vector3(0f, xRote, 0f) * lookSpeed;

		motor.Rotate(rotation);

		float yRote = Input.GetAxisRaw("Mouse Y");
		Vector3 camRotation = new Vector3(yRote, 0f, 0f) * lookSpeed;

		motor.RotateCam(camRotation);
	}
}
