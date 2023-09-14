using System.Collections;
using UnityEngine;

public class AutoScrollArea : MonoBehaviour {
	MGCameraController cam;
	public Vector2 scrollSpeed = new Vector2(0.05f,0);
	public GameObject toToggle,enableOnExit,destroyOnStart;
	public bool lockCameraOnDisable = true;
	public bool disableBlockerOnEnd = true;
	bool active = false;
	public float newCamVerticalOffset = 0;
	float oldCamVerticalOffset = 0;
	public bool disableWithOutSettings = false;
	void Start()
	{
		GetComponent<BoxCollider2D>().enabled = false;
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		oldCamVerticalOffset = cam.verticalOffset;
		StartCoroutine(activate());

	}
	IEnumerator activate()
	{
		yield return new WaitForSeconds(0.1f);
		GetComponent<BoxCollider2D>().enabled = true;
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&!active)
		{
			//print(gameObject.name+" on");
			active = true;
			if(destroyOnStart!=null)
				Destroy(destroyOnStart);
			if(enableOnExit!=null)
			{
			enableOnExit.GetComponent<AutoScrollArea>().disableWithOutSettings = true;
			enableOnExit.SetActive(false);
			}
			cam.autoscrollDir = scrollSpeed;
			cam.verticalOffset = newCamVerticalOffset;
			if(toToggle!=null)
			{
				toToggle.SetActive(true);
				//print(toToggle +" enabled");
			}
			cam.setAutoScrollLimits(true);
		}
	}
	void OnDisable()
	{
		if(Application.isPlaying)
		{
			//print(gameObject.name+" disabled");
			if(!disableWithOutSettings)
			turnOff();
			else
			{
				disableWithOutSettings = false;
				if(toToggle!=null)toToggle.SetActive(false);
				//print(toToggle +" disabled");
			}
		}
	}
	void turnOff()
	{
		//print("Auto scroll off");
		active = false;
		cam.autoscrollDir = Vector2.zero;
		if(lockCameraOnDisable)
		cam.lockCamera = true;
		cam.verticalOffset = oldCamVerticalOffset;
		if(toToggle!=null)
		{
			toToggle.SetActive(false);
			//print(toToggle +" disabled");
		}
		if(disableBlockerOnEnd)
		cam.setAutoScrollLimits(true);
		else cam.setAutoScrollLimits(false);

		if(enableOnExit!=null)
		{
		enableOnExit.GetComponent<AutoScrollArea>().disableWithOutSettings = false;
		enableOnExit.SetActive(true);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			turnOff();
		}
	}
}
