using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour {

	[SyncVar]
	public float health = 100f;

	public void TakeDamage(float damage) {
		//Running as a server
		if (!isServer) {
			return;
		}

		health -= damage;
		print("Damage Taken");
		if(health <= 0) {

		}
	}
}
