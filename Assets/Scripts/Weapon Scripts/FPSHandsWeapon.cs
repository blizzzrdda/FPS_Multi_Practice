using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSHandsWeapon : MonoBehaviour {

	private GameObject muzzleFlash;
	public AudioClip shootClip, reloadClip;
	private AudioSource audioManager;
	private Animator anim;

	private string SHOOT = "Shoot";
	private string RELOAD = "Reload";

	private void Awake() {
		muzzleFlash = transform.Find("Muzzle Flash").gameObject;
		muzzleFlash.SetActive(false);

		audioManager = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
	}

	public void Shoot() {
		anim.SetTrigger(SHOOT);
		StartCoroutine(TurnOnMuzzleFlash());
	}

	IEnumerator TurnOnMuzzleFlash() {
		audioManager.clip = shootClip;
		audioManager.Play();

		muzzleFlash.SetActive(true);
		yield return new WaitForSeconds(.05f);
		muzzleFlash.SetActive(false);
	}

	public void Reload() {
		StartCoroutine(PlayReloadSound());
		anim.SetTrigger(RELOAD);
	}

	IEnumerator PlayReloadSound() {
		yield return new WaitForSeconds(.8f);
		
		audioManager.clip = reloadClip;

		audioManager.Play();
	}
}
