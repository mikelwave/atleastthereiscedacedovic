using UnityEngine;

[System.Serializable]
public class RebindableAxis {

	public string axisName = "";
	public string axisPosName = "";
	public string axisNegName = "";
	
	public KeyCode axisPos = KeyCode.W;
	public KeyCode axisNeg = KeyCode.S;
	
	public RebindableAxis ()
	{
		
	}
	
	public RebindableAxis (string name, KeyCode positive, KeyCode negative)
	{
		axisName = name;
		axisPos = positive;
		axisNeg = negative;
	}
}
