using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FPSController : NetworkBehaviour {

	private Transform firstPerson_View, firstPerson_Camera;

	private Vector3 firstPerson_View_Rotation = Vector3.zero;

	public float
		walkSpeed = 6.75f,
		runSpeed = 10f,
		crouchSpeed = 4f,
		jumpSpeed = 8f,
		gravity = 20f;

	private float speed;

	private bool is_Moving, is_Grounded, is_Crouching;

	private float inputX, inputY;
	private float inputX_Set, inputY_Set;
	private float inputModifyFactor;

	private bool limitDiagonalSpeed = true;

	private float antiBumpFactor = .75f;

	private CharacterController characterController;
	private Vector3 moveDirection = Vector3.zero;

	public LayerMask groundLayer;
	private float rayDistance;
	private float default_ControllerHeight;
	private Vector3 default_CamPos;
	private float camHeight;

	private FPSPlayerAnimations playerAnimations;

	[SerializeField] private WeaponManager weapon_Manager;

	private FPSWeapon current_Weapon;

	private float fireRate = 15f;
	private float nextTimeToFire = 0f;

	[SerializeField]
	private WeaponManager handsWeapon_Manager;
	private FPSHandsWeapon current_Hands_Weapon;

	public GameObject playerHolder, weaponsHolder;
	public GameObject[] weapons_FPS;
	private Camera mainCam;
	public FPSMouseLook[] mouseLook;

	void Start () {
		firstPerson_View = transform.Find("FPS View").transform;
		characterController = GetComponent<CharacterController>();
		speed = walkSpeed;
		is_Moving = false;

		rayDistance = characterController.height * .5f + characterController.radius;
		default_ControllerHeight = characterController.height;
		default_CamPos = firstPerson_View.localPosition;

		playerAnimations = GetComponent<FPSPlayerAnimations>();

		weapon_Manager.weapons[0].SetActive(true);
		current_Weapon = weapon_Manager.weapons[0].GetComponent<FPSWeapon>();

		handsWeapon_Manager.weapons[0].SetActive(true);
		current_Hands_Weapon = handsWeapon_Manager.weapons[0].GetComponent<FPSHandsWeapon>();

		mainCam = transform.Find("FPS View").Find("FPS Camera").GetComponent<Camera>();
		mainCam.gameObject.SetActive(false);


		if (isLocalPlayer) {
			playerHolder.layer = LayerMask.NameToLayer("Player");

			foreach(Transform child in playerHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer("Player");

				for (int i = 0; i < weapons_FPS.Length; i++) {
					weapons_FPS[i].layer = LayerMask.NameToLayer("Player");
				}
			}
			weaponsHolder.layer = LayerMask.NameToLayer("Enemy");

			foreach (Transform child in weaponsHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer("Enemy");
			}
		}

		if (!isLocalPlayer) {

			playerHolder.layer = LayerMask.NameToLayer("Enemy");

			foreach (Transform child in playerHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer("Enemy");

				for (int i = 0; i < weapons_FPS.Length; i++) {
					weapons_FPS[i].layer = LayerMask.NameToLayer("Enemy");
				}
			}


			weaponsHolder.layer = LayerMask.NameToLayer("Player");

			foreach (Transform child in weaponsHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer("Player");
			}

			//diable MouseLook scripts
			for(int i = 0; i < mouseLook.Length; i++) {
				mouseLook[i].enabled = false;
			}
		}
	}

	public override void OnStartLocalPlayer() {
		tag = "Player";
	}

	void Update () {
		if (isLocalPlayer) {
			//disable camera
			if (!mainCam.gameObject.activeInHierarchy) {
				mainCam.gameObject.SetActive(true);
			}
		}
		if (!isLocalPlayer) {
			return ;
		}
		PlayerMovement();
	}

	private void PlayerMovement() {
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) { 
			if (Input.GetKey(KeyCode.W))
				inputY_Set = 1f;
			else
				inputY_Set = -1f;
		}
		else
			inputY_Set = 0f;
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) { 
			if (Input.GetKey(KeyCode.A))
				inputX_Set = -1f;
			else
				inputX_Set = 1f;
		}
		else
			inputX_Set = 0f;

		inputY = Mathf.Lerp(inputY, inputY_Set, Time.deltaTime * 19f);
		inputX = Mathf.Lerp(inputX, inputX_Set, Time.deltaTime * 19f);

		inputModifyFactor = Mathf.Lerp(inputModifyFactor,
			(inputY_Set != 0 && inputX_Set != 0 && limitDiagonalSpeed) ? .75f : 1.0f,
			Time.deltaTime * 19f);

		firstPerson_View_Rotation = Vector3.Lerp(firstPerson_View_Rotation, Vector3.zero, Time.deltaTime * 5f);
		firstPerson_View.localEulerAngles = firstPerson_View_Rotation;

		if (is_Grounded){
			// call crouch and sprint
			PlayerCrouchingAndSprinting();

			moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);

			moveDirection = transform.TransformDirection(moveDirection) * speed;

			// call jump
			PlayerJump();
		}
		moveDirection.y -= gravity * Time.deltaTime; 

		is_Grounded = (characterController.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

		is_Moving = characterController.velocity.magnitude > .15f;

		HandleAnimations();
	}

	void PlayerCrouchingAndSprinting()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			if (!is_Crouching)
				is_Crouching = true;
			else
				if (CanGetUp())
					is_Crouching = false;

			StopCoroutine(MoveCameraCrouch());
			StartCoroutine(MoveCameraCrouch());
		}
		if (is_Crouching)
			speed = crouchSpeed;
		else
		{
			if (Input.GetKey(KeyCode.LeftShift))
				speed = runSpeed;
			else
				speed = walkSpeed;
		}

		playerAnimations.PlayerCrouch(is_Crouching);
	}

	IEnumerator MoveCameraCrouch()
	{
		characterController.height = is_Crouching ? default_ControllerHeight / 1.5f : default_ControllerHeight;
		characterController.center = new Vector3(0f, characterController.height / 2f, 0f);

		camHeight = is_Crouching ? default_CamPos.y / 1.5f : default_CamPos.y;

		while(Mathf.Abs(camHeight - firstPerson_View.localPosition.y) > 0.01f){

			firstPerson_View.localPosition = Vector3.Lerp(firstPerson_View.localPosition,
				new Vector3(default_CamPos.x, camHeight, default_CamPos.z),
				Time.deltaTime * 11f);

			yield return null;
		}

	}

	private bool CanGetUp() {
		Ray groundRay = new Ray(transform.position, transform.up);

		RaycastHit groundHit;
		if (Physics.SphereCast(groundRay, characterController.radius + .05f, out groundHit, rayDistance, groundLayer)) { 
			if(Vector3.Distance(transform.position, groundHit.point) < 2.3f) { 
				return false;
			}
		}
		return true;
	}

	void PlayerJump() { 
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (is_Crouching) {
				if (CanGetUp()) {
					is_Crouching = false;

					playerAnimations.PlayerCrouch(is_Crouching);

					StopCoroutine(MoveCameraCrouch());
					StartCoroutine(MoveCameraCrouch());
				}
			} else {
				moveDirection.y = jumpSpeed;
			} 
		}
	}

	public void HandleAnimations() {
		playerAnimations.Movement(characterController.velocity.magnitude);
		playerAnimations.PlayerJump(characterController.velocity.y);

		//crouching and moving
		if (is_Crouching && characterController.velocity.magnitude > 0f) {
			playerAnimations.PlayerCrouchWalk(characterController.velocity.magnitude);
		}

		//Shooting
		if (Input.GetMouseButtonDown(0) && Time.time > nextTimeToFire) {
			nextTimeToFire = Time.time + 1f / fireRate;
			
			playerAnimations.Shoot(is_Crouching);

			current_Weapon.Shoot();
			current_Hands_Weapon.Shoot();
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			playerAnimations.Reload();
			current_Hands_Weapon.Reload();

		}
	}
}
