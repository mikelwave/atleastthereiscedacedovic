using UnityEngine;

public class VerticalAutoScrollDataChanger : MonoBehaviour {
	MGCameraController cam;
	public int BPM = 120,prevBPM;
	public AudioClip newMusic,prevMusic;
	public bool lockScroll = false,hardlockleft = false,hardlockright = false,hardlockdown = false;
	bool active = false;
	public bool hardLockVerticalOnEnd = true;
	public bool freezeTimeOnEnd = true;
	public bool freezeTimeOnEnter = false;
	GameData data;
	GameObject deathZone;
	public bool DestroyParallax = true;
	public bool disableDeathZoneOnEnd = true;
	// Use this for initialization
	void Start ()
	{
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		if(transform.childCount!=0)
		deathZone = transform.GetChild(0).gameObject;
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&!active)
		{
			if(prevMusic==null)
			prevMusic = data.sources[0].clip;
			prevBPM = GameObject.Find("LevelGrid").GetComponent<storeLevelData>().BPM;
			active = true;
			cam.explicitStayInsideBounds = true;
			cam.hardLockLeft = hardlockleft;
			cam.lockLeft = hardlockleft;
			cam.hardLockRight = hardlockright;
			cam.lockRight = hardlockright;
			cam.lockDown = hardlockdown;
			cam.hardLockDown = hardlockdown;
			if(cam.hardLockRight)cam.lockRight = true;
			cam.lockscroll = lockScroll;
			cam.scrollingRight = false;
			cam.alwaysFollowVertically = true;
			if(newMusic!=null)
			data.changeMusicInSubArea(newMusic,BPM);
			if(freezeTimeOnEnter)
			data.timeFrozen = true;
			else
			{
				data.timeFrozen = false;
			}
			if(deathZone!=null)
			{
			deathZone.transform.SetParent(cam.transform);
			deathZone.transform.localPosition = Vector3.zero;
			deathZone.SetActive(true);
			}
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&active)
		{
			cam.lockDown = !hardlockdown;
			if(hardLockVerticalOnEnd)
			{
				cam.hardLockDown = true;
				cam.hardLockUp = true;
				cam.lockDown = true;
			}
			else cam.hardLockDown = false;
			active = false;
			cam.explicitStayInsideBounds = false;
			cam.lockRight=false;
			cam.lockLeft=false;
			cam.hardLockLeft = !hardlockleft;
			cam.hardLockRight = !hardlockright;
			cam.lockscroll = !lockScroll;
			cam.scrollingRight = false;
			cam.alwaysFollowVertically = false;
			if(newMusic!=null)
			data.changeMusicInSubArea(prevMusic,prevBPM);

			if(DestroyParallax)
			{
				GameObject mainParallax = GameObject.Find("MainAreaParallax");
				if(mainParallax!=null)
				{
					Transform tr = mainParallax.transform;
					for(int i = tr.transform.childCount-1; i>=0;i--)
					{
						GameObject.Destroy(tr.GetChild(i).gameObject);
					}
				}
			}
			if(freezeTimeOnEnd)
			data.timeFrozen = true;
			else data.timeFrozen = false;
			if(deathZone!=null&&disableDeathZoneOnEnd)
			deathZone.SetActive(false);
		}
	}
}
