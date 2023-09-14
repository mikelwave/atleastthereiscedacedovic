using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class WarpScript : MonoBehaviour {
[Header("General")]
public char WarpID = '~';
public bool leadsToSubArea = true;
public bool twoSided = false;
public bool camStayInBounds = true;
public Sprite[] ArrowSprites;
[Space]	
[Header("Entrance")]
public int directionEntrance = 0;
public Vector3 entranceCameraPosition;
SpriteRenderer EntrRender;
TextMeshPro EntrID;

[Space]
[Header("Exit")]
public int directionExit = 0;
public Vector3 exitCameraPosition;
SpriteRenderer ExitRender;
TextMeshPro ExitID;
[Space]
[Header("Debug")]
public bool executeStartMethod = false;
public bool showArrowsDuringPlay = false;

#if UNITY_EDITOR
int lastDirectionEntrance = -1;
int lastDirectionExit = -1;
char lastChar;
#endif

string prefix = null;
string dir1 = null;
string dir2 = null;
	// Use this for initialization
	void Start () {
		EntrRender = transform.GetChild(0).GetComponent<SpriteRenderer>();
		ExitRender = transform.GetChild(1).GetComponent<SpriteRenderer>();
		EntrID = EntrRender.transform.GetChild(0).GetComponent<TextMeshPro>();
		ExitID = ExitRender.transform.GetChild(0).GetComponent<TextMeshPro>();
		#if UNITY_EDITOR
		if(!Application.isPlaying && WarpID == '~')
		{
			transform.parent = GameObject.Find("Special").transform;
			string name = (transform.parent.childCount-1).ToString();
			WarpID = name[0];
			lastChar = WarpID;
			transform.name = "Warp "+WarpID;
			EntrID.SetText(WarpID.ToString());
			ExitID.SetText(WarpID.ToString());
		}
		#endif
		if(Application.isPlaying)
		{
			if(!showArrowsDuringPlay)
			{
			EntrRender.enabled = false;
			ExitRender.enabled = false;
			EntrID.gameObject.SetActive(false);
			ExitID.gameObject.SetActive(false);
			}

			if(exitCameraPosition == Vector3.zero)
			{
				exitCameraPosition = transform.GetChild(1).position;
			}
			if(entranceCameraPosition == Vector3.zero)
			{
				entranceCameraPosition = transform.GetChild(0).position;
			}
		}
	}
	void setNames()
	{
		EntrRender.transform.name = prefix+"Entrance"+" "+dir1;
		ExitRender.transform.name = prefix+"Exit"+" "+dir2;
	}
	#if UNITY_EDITOR
	void Update ()
	{
		if(!Application.isPlaying)
		{
			if(EntrRender==null||executeStartMethod)
			{
				executeStartMethod = false;
				//Debug.Log("Updated "+transform.name+" variables.");
				Start();
			}

			if(twoSided&&EntrRender.sprite!=ArrowSprites[1])
			{
				EntrRender.sprite = ArrowSprites[1];
				ExitRender.sprite = ArrowSprites[3];
				prefix = "Two Sided ";
				ExitRender.GetComponent<BoxCollider2D>().enabled = true;
				setNames();
			}

			else if(!twoSided&&EntrRender.sprite!=ArrowSprites[0])
			{
				EntrRender.sprite = ArrowSprites[0];
				ExitRender.sprite = ArrowSprites[2];
				prefix = "";
				ExitRender.GetComponent<BoxCollider2D>().enabled = false;
				setNames();
			}

			if(lastChar!=WarpID)
			{
				lastChar = WarpID;
				EntrID.SetText(WarpID.ToString());
				ExitID.SetText(WarpID.ToString());
				transform.name = "Warp "+WarpID;
			}

			if(directionEntrance!=lastDirectionEntrance)
			{
				lastDirectionEntrance = directionEntrance;
				switch(directionEntrance)
				{
					//Down
					default:
					directionEntrance = 0;
					lastDirectionEntrance = directionEntrance;
					EntrRender.transform.localEulerAngles = Vector3.zero;
					EntrRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,-0.55f);
					EntrID.transform.localEulerAngles = Vector3.zero;
					EntrID.transform.localPosition = new Vector3(0,0.1f,0);
					dir1 = "Down";
					setNames();
					break;
					//Up
					case 2:
					EntrRender.transform.localEulerAngles = new Vector3(0,0,180f);
					EntrRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,-0.55f);
					EntrID.transform.localEulerAngles = new Vector3(0,0,180f);
					EntrID.transform.localPosition = new Vector3(0,0.4f,0);
					dir1 = "Up";
					setNames();
					break;
					//Right
					case 3:
					EntrRender.transform.localEulerAngles = new Vector3(0,0,90f);
					//EntrRender.GetComponent<BoxCollider2D>().offset = new Vector2(-0.4f,-0.55f);
					EntrRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,-0.55f);
					EntrID.transform.localEulerAngles = new Vector3(0,0,-90f);
					EntrID.transform.localPosition = new Vector3(-0.15f,0.25f,0);
					dir1 = "Right";
					setNames();
					break;
					//Left
					case 1:
					EntrRender.transform.localEulerAngles = new Vector3(0,0,-90f);
					//EntrRender.GetComponent<BoxCollider2D>().offset = new Vector2(0.4f,-0.55f);
					EntrRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,-0.55f);
					EntrID.transform.localEulerAngles = new Vector3(0,0,90f);
					EntrID.transform.localPosition = new Vector3(0.15f,0.25f,0);
					dir1 = "Left";
					setNames();
					break;
				}
			}

			if(directionExit!=lastDirectionExit)
			{
				lastDirectionExit = directionExit;
				switch(directionExit)
				{
					//Down
					default:
					directionExit = 0;
					lastDirectionExit = directionExit;
					ExitRender.transform.localEulerAngles = Vector3.zero;
					ExitRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,0.55f);
					ExitID.transform.localEulerAngles = Vector3.zero;
					ExitID.transform.localPosition = new Vector3(0,0.1f,0);
					dir2 = "Down";
					setNames();
					break;
					//Up
					case 2:
					ExitRender.transform.localEulerAngles = new Vector3(0,0,180f);
					ExitRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,0.55f);
					ExitID.transform.localEulerAngles = new Vector3(0,0,180f);
					ExitID.transform.localPosition = new Vector3(0,0.4f,0);
					dir2 = "Up";
					setNames();
					break;
					//Right
					case 3:
					ExitRender.transform.localEulerAngles = new Vector3(0,0,90f);
					//ExitRender.GetComponent<BoxCollider2D>().offset = new Vector2(-0.4f,-0.55f);
					ExitRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,0.55f);
					ExitID.transform.localEulerAngles = new Vector3(0,0,-90f);
					ExitID.transform.localPosition = new Vector3(-0.15f,0.25f,0);
					dir2 = "Right";
					setNames();
					break;
					//Left
					case 1:
					ExitRender.transform.localEulerAngles = new Vector3(0,0,-90f);
					//ExitRender.GetComponent<BoxCollider2D>().offset = new Vector2(0.4f,-0.55f);
					ExitRender.GetComponent<BoxCollider2D>().offset = new Vector2(0f,0.55f);
					ExitID.transform.localEulerAngles = new Vector3(0,0,90f);
					ExitID.transform.localPosition = new Vector3(0.15f,0.25f,0);
					dir2 = "Left";
					setNames();
					break;
				}
			}
		}
	}
	#endif
}