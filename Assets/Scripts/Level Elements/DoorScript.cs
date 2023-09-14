using System;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class DoorScript : MonoBehaviour
{
	#region doorscript
	[Header("Editing")]
	public char id = '0';
	public bool keyDoor = false;
	public bool eventLocked = false;
	public int keyID = 0;
	public Color32 doorColor = new Color32(97,159,89,255);
	public Color32[] keyDoorColors = new Color32[3];
	public Sprite[] doorSprites;
	public bool twoSided = true;
	public bool leadsToSubArea = false,keepAudioEffects = false;
	public bool camStayInBounds = true;
	[Space]
	[Header("Camera positions")]
	public bool recordEntranceCameraPosition = false;
	public bool recordExitCameraPosition = false;
	[Space]
	public Vector3 entranceCameraPosition;
	public Vector3 exitCameraPosition;
	SpriteRenderer[] doors = new SpriteRenderer[2];
	public bool touchToWarp = false;
	public bool freePlayerOnExit = true;
	public bool alwaysUseDoorEvent = true;
	public bool keepCamLocked = false;
	#if UNITY_EDITOR
	char lastChar = '~';
	#endif
	// Use this for initialization
	void Start ()
	{
		if(!keyDoor)
		transform.name = "Door "+id;
		else transform.name = "KeyDoor "+keyID;
		doors[0] = transform.GetChild(0).GetComponent<SpriteRenderer>();
		doors[1] = transform.GetChild(1).GetComponent<SpriteRenderer>();
	}
	#if UNITY_EDITOR
	// Update is called once per frame
	void Update ()
	{
		if(!Application.isPlaying)
		{
			if(lastChar!=id)
			{
				lastChar = id;
			}
			if(!keyDoor)
			{
				if(doors[0].sprite!=doorSprites[0]||doors[1].sprite!=doorSprites[0])
				{
					doors[0].sprite = doorSprites[0];
					doors[1].sprite = doorSprites[0];
				}
				transform.name = "Door "+id;
			}
			if(keyDoor)
			{
				keyID = Mathf.Clamp(keyID,0,2);
				doors[0].color = keyDoorColors[keyID];
				doors[1].color = keyDoorColors[keyID];

				if(doors[0].sprite!=doorSprites[1]||doors[1].sprite!=doorSprites[1])
				{
					doors[0].sprite = doorSprites[1];
					doors[1].sprite = doorSprites[1];
				}
				transform.name = "KeyDoor "+keyID;
			}
		}
		//if(twoSided) doors[1].GetComponent<BoxCollider2D>().enabled =  true;
		//else if(!twoSided) doors[1].GetComponent<BoxCollider2D>().enabled = false;

		if(recordEntranceCameraPosition)
		{
			recordEntranceCameraPosition = false;
			entranceCameraPosition = GameObject.Find("Main Camera").transform.position;
		}
		if(recordExitCameraPosition)
		{
			recordExitCameraPosition = false;
			exitCameraPosition = GameObject.Find("Main Camera").transform.position;
		}
		if(!keyDoor&&doors[0].color!=doorColor)
		{
			doors[0].color = doorColor;
			doors[1].color = doorColor;
		}
	}
	#endif
	#endregion
	//my event
    [Serializable]
    public class DoorTransitionEvent : UnityEvent { }
 
    [SerializeField]
    private DoorTransitionEvent doorEvent = new DoorTransitionEvent();
	[SerializeField]
	private DoorTransitionEvent exitEvent = new DoorTransitionEvent();
	[SerializeField]
    private DoorTransitionEvent startEvent = new DoorTransitionEvent();
	public DoorTransitionEvent onStartEvent { get { return startEvent; } set { startEvent = value; } }
    public DoorTransitionEvent onDoorEvent { get { return doorEvent; } set { doorEvent = value; } }
	public DoorTransitionEvent onExitEvent { get { return exitEvent; } set { exitEvent = value; } }
 
	public void StartEventTriggered()
    {
        onStartEvent.Invoke();
    }
    public void DoorEventTriggered()
    {
        onDoorEvent.Invoke();
    }
	public void ExitEventTriggered()
    {
        onExitEvent.Invoke();
    }
}