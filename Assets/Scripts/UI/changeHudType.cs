using UnityEngine;

public class changeHudType : MonoBehaviour {
	Vector3[] livesPos = new [] { new Vector3(-350.0f, 191.0f, 0.0f), new Vector3(-224.0f, 191.0f, 0.0f) };
	Vector3[] teapotsPos = new [] { new Vector3(-352.0f, 153.1f, 0.0f), new Vector3(-224.0f, 155.5f, 0.0f) };
	Vector3[] SausagesPos = new [] { new Vector3(-253.8f, 186.1f, 0.0f), new Vector3(181.5f, 186.0f, 0.0f) };
	Vector3[] ScorePos = new [] { new Vector3(235.0f, 179.3f, 0.0f), new Vector3(201.6f, 157.2f, 0.0f) };
	Vector3[] timePos = new [] { new Vector3(309.5f, 186.0f, 0.0f), new Vector3(81.5f, 186.0f, 0.0f) };
	Vector3[] SMeterPos = new [] { new Vector3(-249.6f, -195.3f, 0.0f), new Vector3(-130.7f, -195.3f, 0.0f) };
	Vector3[] floppiesPos = new [] { new Vector3(-153.2f, 186.1f, 0.0f), new Vector3(-116.6f, 186.1f, 0.0f) };
	Vector3[] HealthMeterPos = new [] { new Vector3(-344.5f, -163f, 0.0f), new Vector3(-224.5f, -163f, 0.0f) };
	Vector3[] KeysPos = new [] { new Vector3(353.95f, -193.91f, 0.0f), new Vector3(223.8f, -193.91f, 0.0f) };
	public Sprite playuhLives;

	RectTransform[] HUDElements = new RectTransform[9];
	// Use this for initialization
	void Start () {
		HUDElements[0]=transform.GetChild(2).GetComponent<RectTransform>();
		HUDElements[2]=transform.GetChild(3).GetComponent<RectTransform>();
		HUDElements[1]=transform.GetChild(4).GetComponent<RectTransform>();
		HUDElements[3]=transform.GetChild(5).GetComponent<RectTransform>();
		HUDElements[4]=transform.GetChild(6).GetComponent<RectTransform>();
		HUDElements[5]=transform.GetChild(7).GetComponent<RectTransform>();
		HUDElements[6]=transform.GetChild(8).GetComponent<RectTransform>();
		HUDElements[7]=transform.GetChild(10).GetComponent<RectTransform>();
		HUDElements[8]=transform.GetChild(11).GetComponent<RectTransform>();
		dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		if(DataS!=null&&DataS.mode==1) //playuh icon
		{
			HUDElements[0].GetComponent<UnityEngine.UI.Image>().sprite = playuhLives;
		}
	}
	public void changeHud(int value)
	{
		if(HUDElements[0]==null)Start();
		HUDElements[0].localPosition = livesPos[value];
		HUDElements[1].localPosition = teapotsPos[value];
		HUDElements[2].localPosition = SausagesPos[value];
		HUDElements[3].localPosition = ScorePos[value];
		HUDElements[4].localPosition = timePos[value];
		HUDElements[5].localPosition = SMeterPos[value];
		HUDElements[6].localPosition = floppiesPos[value];
		HUDElements[7].localPosition = HealthMeterPos[value];
		HUDElements[8].localPosition = KeysPos[value];
	}
}
