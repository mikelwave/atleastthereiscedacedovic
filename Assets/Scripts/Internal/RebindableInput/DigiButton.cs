using UnityEngine;

[System.Serializable]
public class DigiButton {

	public string inputName = "";
	public string DigiInput = "";
	[HideInInspector]
	public bool pressed,down,up = false;
	
	public DigiButton ()
	{
		
	}
	
	public DigiButton (string name, string key)
	{
		inputName = name;
		DigiInput = key;
	}
}
