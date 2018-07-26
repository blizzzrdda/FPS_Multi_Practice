﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSWeapon : MonoBehaviour {

	private GameObject muzzleFlash;

	private void Awake() {
		muzzleFlash = transform.Find("Muzzle Flash").gameObject;
		muzzleFlash.SetActive(false);
	}

	public void Shoot() {
		StartCoroutine(TurnOnMuzzleFlash());
	}

	IEnumerator TurnOnMuzzleFlash() {
		muzzleFlash.SetActive(true);
		yield return new WaitForSeconds(.1f);
		muzzleFlash.SetActive(false);
	}
}
