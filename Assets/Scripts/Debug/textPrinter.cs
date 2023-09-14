using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(AxisSimulator))]
public class textPrinter : MonoBehaviour {
	public Text text;
	// Update is called once per frame
	void Update () {
		text.text = GetComponent<AxisSimulator>().axisPosX.ToString("0.0");
	}
}
