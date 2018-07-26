using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FPSPlayerAnimations : MonoBehaviour {

	private Animator anim;

	private string MOVE = "Move";
	private string VELOCITY_Y = "Velocity_Y";
	private string CROUCH = "Crouch";
	private string CROUCH_WALK = "CrouchWalk";

	private string STAND_SHOOT = "StandShoot";
	private string CROUCH_SHOOT = "CrouchShoot";
	private string RELOAD = "Reload";

	private NetworkAnimator networkAnim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		networkAnim = GetComponent<NetworkAnimator>();
	}
	
	public void Movement(float magnitude) {
		anim.SetFloat(MOVE, magnitude);
	}

	public void PlayerJump(float velocity) {
		anim.SetFloat(VELOCITY_Y, velocity);
	}

	public void PlayerCrouch(bool isCrouching) {
		anim.SetBool(CROUCH, isCrouching);
	}

	public void PlayerCrouchWalk(float magnitude) {
		anim.SetFloat(CROUCH_WALK, magnitude);
	}

	public void Shoot(bool isCrouching) {
		if (isCrouching) {
			anim.SetTrigger(CROUCH_SHOOT);
			networkAnim.SetTrigger(CROUCH_SHOOT);
		} else {
			anim.SetTrigger(STAND_SHOOT);
			networkAnim.SetTrigger(STAND_SHOOT);
		}
	}

	public void Reload() {
		anim.SetTrigger(RELOAD);
		networkAnim.SetTrigger(RELOAD); 
	}
}
 