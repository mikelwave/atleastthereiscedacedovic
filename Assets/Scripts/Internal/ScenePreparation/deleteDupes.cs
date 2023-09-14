using UnityEngine;
using System.Collections.Generic;

public class deleteDupes : MonoBehaviour {
	List <GameObject> duplicates;
	// Use this for initialization
	void OnEnable () {
		foreach (GameObject dup in GameObject.FindGameObjectsWithTag ("Rebindable Manager")) {
			if (dup.Equals(this.gameObject))
				continue;
			//gameObject.SetActive (false);
			Destroy(gameObject);
			return;
		}
			DontDestroyOnLoad (transform.gameObject);
	}
}
