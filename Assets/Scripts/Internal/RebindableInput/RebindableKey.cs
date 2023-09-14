using UnityEngine;

[System.Serializable]
public class RebindableKey {

	public string inputName = "";
	public KeyCode input = KeyCode.A;
	
	public RebindableKey ()
	{
		
	}
	
	public RebindableKey (string name, KeyCode key)
	{
		inputName = name;
		input = key;
	}
}
