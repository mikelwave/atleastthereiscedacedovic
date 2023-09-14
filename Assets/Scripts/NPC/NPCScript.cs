using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;

public class NPCScript : MonoBehaviour {
	#region main
	public bool isBlock = false;
	public bool playInfoblockSound = true;
	public TextAsset usedText;
	public AudioClip npcVoice;
	[HideInInspector]
	public GameData data;
	public Sprite[] npcSprites = new Sprite[2];
	public int startLine = 0,altStartLine = 541;
	public int option1StartLine,option2StartLine,option3StartLine,option4StartLine;
	Transform player;
	Transform bubble;
	bool facingLeft = true;
	bool visible = false;
	public bool talkedTo = false;
	bool canTalk = false;
	public bool talking = false;
	public int spriteInt = 0;
	Transform textboxTransform;
	[HideInInspector]
	public TextBox TBScript;
	SpriteRenderer render;
	AxisSimulator axis;
	PlayerScript pScript;
	Animator pAnim;
	public NPCLineRouter nline;
	public bool turnsToPlayer = true;
	public bool inHub = false;
	public bool active = false;
	public bool startTalkWithoutInput = false;
	public bool destroyTriggerOnTalk = false;
	public bool alwaysVisible = false;
	public bool enableAnimationOnTalk = false;
	//playuh
	public Sprite[] linkolnSprites = new Sprite[2];
	public AudioClip linkolnClip;
	public int[] linkolnLines = new int[4];
	// Use this for initialization
	void Start () {
		if(player==null)
		{
			player = GameObject.Find("Player_main").transform;
			if(inHub)pAnim = player.GetComponent<Animator>();
			if(!inHub)pScript = player.GetComponent<PlayerScript>();
			data = GameObject.Find("_GM").GetComponent<GameData>();
			axis = player.GetComponent<AxisSimulator>();
			if(!isBlock)
			render = GetComponent<SpriteRenderer>();
			else render = transform.GetChild(0).GetComponent<SpriteRenderer>();
			if(!isBlock)
			bubble = transform.GetChild(0);
			textboxTransform = GameObject.Find("Textbox_Canvas").transform.GetChild(0);
			TBScript = textboxTransform.GetComponent<TextBox>();
			if(data.mode == 1)
			{
				if(isBlock&&altStartLine==541)
				{
					Destroy(gameObject);
				}
				startLine = altStartLine;
				if(startLine==700)
				{
					npcSprites = linkolnSprites;
					npcVoice = linkolnClip;
					if(inHub)
					{
					option1StartLine = linkolnLines[0];
					option2StartLine = linkolnLines[1];
					option3StartLine = linkolnLines[2];
					option4StartLine = linkolnLines[3];
					}
				}
			}
			if(startLine==541&&!isBlock)
			{
				turnsToPlayer = false;
				talkedTo = true;
			}

			if(!isBlock)
				bubble.gameObject.SetActive(false);
			if(isBlock)
			canTalk=true;

			if(inHub)
			{
				visible = true;
				render.enabled = true;
				if(!talkedTo&&!isBlock)
				bubble.gameObject.SetActive(true);
			}
			if(alwaysVisible)visible = true;
		}
	}
	void LateUpdate()
	{
		if(inHub)
		{
			transform.localScale = new Vector3(-1,transform.localScale.y,transform.localScale.z);
		}
	}
	// Update is called once per frame
	void Update () {
		if(visible)
		{
			if(player.position.x>=transform.position.x&&facingLeft&&!isBlock&&turnsToPlayer)
			{
				facingLeft = false;
				transform.localScale = new Vector3(-1,1,1);
			}
			else if(player.position.x<transform.position.x&&!facingLeft&&!isBlock&&turnsToPlayer)
			{
				facingLeft = true;
				transform.localScale = new Vector3(1,1,1);
			}
			//canTalk, !isblock, Time.timeScale!=0
			if((canTalk&&!isBlock&&Time.timeScale!=0)
			&&(!inHub&&!pScript.holdingObject&&pScript.diveCor==null&&axis.acceptYInputs&&axis.verAxis==1&&axis.horAxis==0
			||(inHub&&axis.horAxis==0)&&(SuperInput.GetKeyDown("Jump")||(axis.acceptYInputs&&axis.verAxis==1))
			||startTalkWithoutInput))
			{
				if(nline!=null)nline.checkStatus();
				canTalk = false;
				talkedTo = true;
				active = true;
				if(!isBlock)
				bubble.gameObject.SetActive(false);
				TBScript.option1StartLine = option1StartLine;
				TBScript.option2StartLine = option2StartLine;
				TBScript.option3StartLine = option3StartLine;
				TBScript.option4StartLine = option4StartLine;
				TBScript.TextFile = usedText;
				if(!inHub)
				{
					TBScript.usedAnimators[0] = player.GetChild(0).GetComponent<Animator>();
					if(!isBlock)
					pScript.axis.axisAdder = 0.5f;
				}
				else TBScript.usedAnimators[0] = player.GetComponent<Animator>();
				TBScript.usedAnimators[1] = GetComponent<Animator>();
				TBScript.startLine = startLine;
				TBScript.letter_type_other = npcVoice;
				TBScript.inHub = inHub;
				TBScript.gameObject.SetActive(true);
				talking = true;
				if(startLine==541)
				{
					TBScript.disableName();
				}
				TBScript.protagonistOnLeft = facingLeft;
				if(destroyTriggerOnTalk)
				{
					GetComponent<BoxCollider2D>().enabled = false;
				}
				if(enableAnimationOnTalk)GetComponent<SimpleAnim2>().StartPlaying();
				ActivateEventTriggered();
			}
			if(spriteInt==0&&render.sprite!=npcSprites[0])
			{
				render.sprite = npcSprites[0];
			}
			else if(spriteInt==1&&render.sprite!=npcSprites[1])
			{
				render.sprite = npcSprites[1];
			}
			//getting apple piece
			if(nline!=null&&talking)
			{
				if(TBScript.confirmOptionforOutside)
				{
					TBScript.confirmOptionforOutside = false;
					switch(TBScript.currentOption)
					{
						default: break;
						case 0:
						if(nline.who!="babushka")
						{
							if(data.updateSausageDisplay(-15))
							{
								nline.givePiece();
								data.playSoundStatic(95);
							}
							else data.playSoundStatic(96);
						}
						break;
					}
				}
			}
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&!visible&&!inHub)
		{
			if(player==null)Start();

			visible = true;
			if(!talkedTo&&!isBlock)
			bubble.gameObject.SetActive(true);
		}
	}
	void OnTriggerStay2D(Collider2D other)
	{
		if((other.name=="PlayerCollider"||other.name=="Player")&&!canTalk&&!talking&&!isBlock)
		{
			//Debug.Log("Player in collision");
			if(!inHub||pAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="player_hub_idle")
			{
				canTalk = true;
				if(talkedTo&&!isBlock)
				bubble.gameObject.SetActive(true);
			}
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&visible&&!inHub)
		{
			if(!destroyTriggerOnTalk&&!alwaysVisible)
			visible = false;
			if(!isBlock)
			bubble.gameObject.SetActive(false);
		}
		if((other.name=="PlayerCollider"||(inHub&&other.name=="Player"))&&!isBlock)
		{
			canTalk = false;
			if(talkedTo&&!isBlock)
			bubble.gameObject.SetActive(false);
		}
	}
	public void infoBlockHit()
	{
		//Debug.Log(other.gameObject.name);

		if(isBlock)
		{
		StartCoroutine(startTalk());
		}
	}
	IEnumerator startTalk()
	{
		data.playSoundStatic(1);
		if(transform.eulerAngles.z<170) //normal
		{
			if(player.position.y<transform.position.y)
			{
				transform.GetChild(0).GetComponent<Animation>().Play("Block_BounceNoEventanim");
			}
			else transform.GetChild(0).GetComponent<Animation>().Play("Block_BounceNoEventInvert");
		}
		else //upside down
		{
			if(player.position.y<transform.position.y)
			{
				transform.GetChild(0).GetComponent<Animation>().Play("Block_BounceNoEventInvert");
			}
			else transform.GetChild(0).GetComponent<Animation>().Play("Block_BounceNoEventanim");
		}
		if(playInfoblockSound)
		{
			yield return new WaitForSeconds(0.05f);
			data.playSoundStatic(22);
			yield return new WaitForSeconds(0.15f);
		}
		else
		{
			yield return new WaitForSeconds(0.2f);
		}
		canTalk = true;

		canTalk = false;
		talkedTo = true;
		if(playInfoblockSound)
		Time.timeScale = 0;
		TBScript.TextFile = usedText;
		TBScript.usedAnimators[0] = player.GetChild(0).GetComponent<Animator>();
		TBScript.usedAnimators[1] = GetComponent<Animator>();
		TBScript.startLine = startLine;
		TBScript.letter_type_other = npcVoice;
		TBScript.gameObject.SetActive(true);
		talking = true;
		TBScript.protagonistOnLeft = facingLeft;
	}
	#endregion
	//my event
     [Serializable]
     public class Event : UnityEvent { }
 
     [SerializeField]
     private Event activateEvent = new Event(),disableEvent = new Event();
     public Event onActivateEvent { get { return activateEvent; } set { activateEvent = value; } }
	 public Event onDisableEvent { get { return disableEvent; } set { disableEvent = value; } }
 
     public void ActivateEventTriggered()
     {
         onActivateEvent.Invoke();
     }
	 public void DisableEventTriggered()
     {
         onDisableEvent.Invoke();
     }
}
