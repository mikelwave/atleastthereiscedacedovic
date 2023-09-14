using UnityEngine;

// Script for calling the gmae master to display score popups for the received score
public class showAddedScore : MonoBehaviour
{
	// Called manually
	public void showScore()
	{
		GameObject.Find("_GM").GetComponent<GameData>().ScorePopUp(transform.position,"+10",new Color32(255,255,255,255));
	}
}
