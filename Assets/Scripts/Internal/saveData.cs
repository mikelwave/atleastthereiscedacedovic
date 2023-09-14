using UnityEngine;
public class saveData : MonoBehaviour {
	public int hudType = 0;
	changeHudType changeHud;
	public void change()
	{
		if(changeHud==null)
		{
			if(GameObject.Find("HUD_Canvas")!=null)
			changeHud = GameObject.Find("HUD_Canvas").GetComponent<changeHudType>();
		}
		if(changeHud!=null)
		{
			//Debug.Log("HUD altered to "+hudType);
			changeHud.changeHud(hudType);
		}
	}
}
