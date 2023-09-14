using UnityEngine;

public class deadBlockSender : MonoBehaviour {
	void goDead()
	{
		transform.parent.GetComponent<HitBlockScript>().slashTurnOff();
	}
}
