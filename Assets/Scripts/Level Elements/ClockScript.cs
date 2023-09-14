using UnityEngine;
using System;
using TMPro;

public class ClockScript : MonoBehaviour {
	TextMeshProUGUI text;
	// Use this for initialization
	void Start () {
		text = GetComponent<TextMeshProUGUI>();
	}
	
	// Update is called once per frame
	void Update () {
		text.text = DateTime.Now.ToShortTimeString();
	}
}
