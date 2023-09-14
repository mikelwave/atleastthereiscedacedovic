using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {
	Transform highlight;
	Image highlightImage;
	Transform[] buttons = new Transform[2];
	bool canSelect = true;
	bool pressedDown = false;
	public Color32[] highlightColors = new Color32[2];
	int colorFrames = 0;
	int colorint = 0;
	int currentOption = 0;
	AxisSimulator axis;
	MGCameraController gameCam;
	AudioSource asc;
	public AudioClip[] clips = new AudioClip[2];
	dataShare dataS;
	// Use this for initialization
	void Start ()
	{
		for(int i = 1; i<transform.childCount;i++)
		{
			buttons[i-1]=transform.GetChild(i);
		}
		highlight = transform.GetChild(0);
		highlightImage = highlight.GetComponent<Image>();
		axis = GameObject.Find("Player_main").GetComponent<AxisSimulator>();
		gameCam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		asc = GetComponent<AudioSource>();
		dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
	}
	// Update is called once per frame
	void Update ()
	{
		if(colorFrames>0)
		{
			colorFrames--;
			if(colorFrames==0)
				colorint = 0;
			else if(colorFrames%5==0)
			{
				colorint += 1;
				if(colorint>highlightColors.Length-1)
				colorint = 0;
			}
			highlightImage.color = highlightColors[colorint];
		}
		if(Mathf.Abs(axis.horAxis)==1&&!pressedDown&&canSelect)
		{
			pressedDown = true;
			Option();
		}
		else if(axis.horAxis==0&&pressedDown)
			pressedDown = false;
		if(SuperInput.GetKeyDown("Jump")&&canSelect)
		{
			canSelect = false;
			StartCoroutine(selectOption());
		}
	}
	void Option()
	{
		currentOption++;
		if(currentOption>1) currentOption = 0;

		asc.PlayOneShot(clips[1]);
		Vector3 buttonPos = buttons[currentOption].localPosition;
		highlight.localPosition = new Vector3(buttonPos.x+10,buttonPos.y-10,buttonPos.z);
	}
	IEnumerator selectOption()
	{
		colorFrames = 60;
		asc.PlayOneShot(clips[0]);
		gameCam.fadeScreen(true);
		yield return new WaitUntil(()=>gameCam.fadeAnim>=1f);
		if(currentOption==1)
		{
			dataS.loadSceneWithLoadScreen(2);
		}
		else
		{
			dataS.resetStats();
			dataS.resetValues();
			UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		}
	}
}
