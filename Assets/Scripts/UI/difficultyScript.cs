using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class difficultyScript : MonoBehaviour
{
    Transform pointer;
    public int currentSelection = 0;
    public Sprite[] sprites = new Sprite[2];
    int previousSelection = 0;
	bool pressedDown = true;
	bool horPressedDown = false;
	bool canSelect = false;
	AxisSimulator axis;
	MenuScript menu;
    Animator anim;
    int startAnimInt = 8;
	int animInt = 0;
    List<TransformShake> shakeEvents;
    AudioSource aSource;
	float starterXpointerPos;
    int count;
    Transform menuLoc;
    public AudioClip[] sounds;
	dataShare DataS;
	public Color[] colors = new Color[2];
	int profilesOffset = 0;
	Image profileImage;
	GameObject profileMain;
	Material mat;
	Coroutine profileAppearCor;
	int tempMode = 0;
	int modeAmount = 0;
	bool playUhUnlock = false;
	int prevTempMode = 0;
	TextMeshProUGUI text;
	string[] messages = new string[7]
	{
		"Toggle whether you will\nlose a life after dying.\n1ups will give points instead.",
		"If holding a powerup,\nyou will turn big after taking\ndamage once.",
		"Taking damage will turn you\nsmall no matter what.",
		"You die in one hit.\nGo to hell and stop\nplaying our game.",
		"\"The Balkan Menace\"\nStandard jumps and decent turning.",
		"\"Dead meme\"\nHigher jumps, longer sliding,\ncertain enemies are harder.",
		"No stopping.\nNo intermissions."
	};
    void OnEnable()
	{
		if(anim!=null)
		{
			if(axis==null) axis = menu.axis;
			canSelect = true;
			currentSelection = 0;
			setPointer(false,false);
			disableArrows();
			changeProfileImage(0,false);
			tempMode = DataS.mode;
			Transform tr = menuLoc.GetChild(2);
			setText(0);
			for(int i = 1;i<tr.childCount;i++)
			{
				if(i-1==DataS.mode)
				{
					tr.GetChild(i).GetComponent<Image>().color = colors[0];
				}
				else
				{
					tr.GetChild(i).GetComponent<Image>().color = colors[1];
					tr.GetChild(i).GetChild(0).GetComponent<Image>().color = colors[1];
				}
			}
		}
		else
		{
			gameObject.SetActive(false);
		}
	}
	void setText(int ID)
	{
		text.text = messages[ID];
	}
    public void loadValues()
	{
		menuLoc = transform.GetChild(1);
        count = menuLoc.childCount-1;
		if(anim==null)
		{
			pointer = transform.GetChild(0);
			starterXpointerPos = pointer.localPosition.x;
			menu = transform.parent.GetComponent<MenuScript>();
			axis = menu.axis;
			anim = GetComponent<Animator>();
            aSource = transform.parent.GetComponent<AudioSource>();
			DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
			tempMode = DataS.mode;
			prevTempMode = tempMode;
			Transform tr = menuLoc.GetChild(2);
			text = menuLoc.GetChild(6).GetComponent<TextMeshProUGUI>();
			setText(0);
			//load modes
			if(dataShare.totalCompletedLevels>=41||DataS.mode>=1||DataS.mode==0&&PlayerPrefs.HasKey("Save"+(Mathf.Clamp(DataS.saveFileID+4,4,6))))
			{
				//enable face
				//print("Playuh mode unlocked");
				playUhUnlock = true;
				Image mode2Icon = menuLoc.GetChild(2).GetChild(2).GetComponent<Image>();
				mode2Icon.sprite = sprites[2];
				mode2Icon.transform.GetChild(0).gameObject.SetActive(true);
				if(DataS.mode==1)
				{
					profilesOffset = 4;
				}
			}
			//set highlight on the block
			modeAmount = tr.childCount-2;
			for(int i = 1;i<tr.childCount;i++)
			{
				if(i-1==DataS.mode)
				{
					tr.GetChild(i).GetComponent<Image>().color = colors[0];
				}
				else
				{
					tr.GetChild(i).GetComponent<Image>().color = colors[1];
					tr.GetChild(i).GetChild(0).GetComponent<Image>().color = colors[1];
				}
			}
			//toggle infinite lives
			if(DataS.infiniteLives==0)
			menuLoc.GetChild(0).GetChild(0).GetComponent<Image>().sprite = sprites[0];

			//set difficulty slider
			if(DataS.difficulty!=0)
			{
				Transform point = menuLoc.transform.GetChild(1).GetChild(1).GetChild(0);
				point.localPosition = new Vector3(-300f+(300*DataS.difficulty),point.localPosition.y,point.localPosition.z);
			}
			profileImage = menuLoc.GetChild(4).GetChild(0).GetComponent<Image>();
			mat = profileImage.material;
			profileImage.SetMaterialDirty();
			profileMain = profileImage.transform.parent.gameObject;
			changeProfileImage(0,false);
			profileImage.enabled = true;
		}
		shakeEvents = new List<TransformShake>();

	}
	public void disableObj()
	{
		gameObject.SetActive(false);
	}
    void enableArrows()
	{
		switch(currentSelection)
		{
			default: break;
			case 1:
			menuLoc.GetChild(currentSelection).GetChild(2).gameObject.SetActive(true);
			menuLoc.GetChild(currentSelection).GetChild(3).gameObject.SetActive(true);
			break;
		}
	}
	void disableArrows()
	{
		switch(previousSelection)
		{
			default: break;
			case 1:
			menuLoc.GetChild(previousSelection).GetChild(2).gameObject.SetActive(false);
			menuLoc.GetChild(previousSelection).GetChild(3).gameObject.SetActive(false);
			break;
		}
	}
    void setPointer(bool shake,bool sound)
	{
		pointer.localPosition = new Vector3(starterXpointerPos,menuLoc.GetChild(Mathf.Clamp(currentSelection,0,count)).localPosition.y+480,pointer.localPosition.z);
		if(sound)
		aSource.PlayOneShot(sounds[0]);
		if(shake)
		startTransformShake(menuLoc.GetChild(Mathf.Clamp(currentSelection,0,count)));
	}
    void startTransformShake(Transform tr)
	{
		//see if current selection is currently animating
		for(int i = shakeEvents.Count-1;i>=0;i--)
		{
			if(shakeEvents[i].cor==null)
			{
				shakeEvents.Remove(shakeEvents[i]);
				continue;
			}
			//if found, interrupt that element
			if(shakeEvents[i].tr==tr)
			{
				StopCoroutine(shakeEvents[i].cor);
				shakeEvents[i].reset();
				shakeEvents.Remove(shakeEvents[i]);
				break;
			}
		}
		//add a new element to the coroutine list
		Vector3 orgPos = tr.localPosition;
		shakeEvents.Add(new TransformShake(tr,orgPos,StartCoroutine(shakeTransform(tr,orgPos))));
	}
    IEnumerator shakeTransform(Transform tr,Vector3 orgPos)
	{
		animInt = startAnimInt;
		float divider = 1;
		float negator = -1;
		while(animInt >= 0)
		{
			//everySecondFrame
			if(animInt % 2==0)
			{
				//print(animInt);
				Vector3 offsetPos = new Vector3((12.5f/divider)*negator,0,0);
				tr.localPosition = orgPos+offsetPos;
				//print(tr.localPosition+" "+offsetPos+" "+negator+" "+divider);
				//everyFourthFrame
				if(animInt%4==0&&startAnimInt!=animInt)
				{
					divider+=1;
				}
				negator = -negator;
			}
			animInt--;
			yield return 0;
		}
		animInt = 0;
		tr.localPosition = orgPos;
	}
	void closeMenu()
	{
		aSource.PlayOneShot(sounds[1]);
		menu.miniSave(DataS.mode);
		anim.SetTrigger("Action");
		menu.displayingSettings = false;
		menu.canSelect = true;
	}
    // Update is called once per frame
    void Update()
    {
        if(Mathf.Abs(axis.verAxis)==1&&!pressedDown&&canSelect)
		{
			previousSelection = currentSelection;
			animInt = 0;
			if(axis.verAxis>0)
			{
				if(currentSelection>=1)
				currentSelection--;
				else currentSelection = 3;
			}
			else
			{
				if(currentSelection<=2)
				currentSelection++;
				else currentSelection = 0;
			}

			pressedDown = true;
			disableArrows();
			enableArrows();
			if(currentSelection!=2)
			setPointer(true,true);
			else setPointer(false,true);
			switch(currentSelection)
			{
				default: break;
				case 0: setText(0); break;
				case 1: setText(DataS.difficulty+1); break;
				case 2: setText(tempMode+4);  break;
			}

			//set profile
			if(currentSelection==0)
			{
				changeProfileImage(0,false);
			}
			else if(currentSelection==1&&previousSelection==0)
			{
				changeProfileImage(DataS.difficulty+1,false);
			}
		}
		if(Mathf.Abs(axis.horAxis)==1&&!pressedDown&&canSelect&&!horPressedDown)
		{
			horOptionChange(menuLoc.GetChild(Mathf.Clamp(currentSelection,0,count)),axis.horAxis);
		}
		else if(axis.horAxis==0
			  &&axis.verAxis==0
			  &&pressedDown)
		{
			pressedDown = false;
		}
		if(axis.horAxis==0&&horPressedDown)
		{
			horPressedDown = false;
		}

			if(axis.horAxis==0&&aSource.loop)
			{
				if(aSource.isPlaying)
				aSource.Stop();
				aSource.loop = false;
				aSource.clip = null;
			}

		if(SuperInput.GetKeyDown("Jump")&&canSelect)
		{
			canSelect = false;
			selectOptionMain(menuLoc.GetChild(Mathf.Clamp(currentSelection,0,count)));
		}
		if(SuperInput.GetKeyDown("Select")&&canSelect)
		{
			closeMenu();
		}
    }
	void changeProfileImage(int ID,bool bump)
	{
		if(gameObject.activeInHierarchy)
		{
			if(profileAppearCor!=null)StopCoroutine(profileAppearCor);
			profileAppearCor = StartCoroutine(profileChangeRoutine(ID,bump));
		}
	}
	IEnumerator profileChangeRoutine(int ID,bool bump)
	{
		profileMain.SetActive(false);
		if(ID==0)
		{
			if(DataS.infiniteLives==0)
			mat.SetFloat("_EffectAmount",1);
			else mat.SetFloat("_EffectAmount",0);
		}
		else mat.SetFloat("_EffectAmount",0);
		profileImage.material = mat;
		//print(profileImage.material.GetFloat("_EffectAmount"));
		profileImage.sprite = sprites[Mathf.Clamp(3+ID+profilesOffset,0,sprites.Length-1)];
		profileImage.SetNativeSize();
		profileMain.SetActive(true);
		Transform tr = profileImage.transform;
		float progress = 0;
		if(!bump)
		{
			tr.localPosition = new Vector3(1460,-68,0);
			yield return 0;
			while(progress<1)
			{
				progress+=Time.deltaTime*5;
				tr.localPosition = new Vector3(Mathf.Lerp(tr.localPosition.x,413,progress),-68,0);
				yield return 0;
			}
		}
		else
		{
			tr.localPosition = new Vector3(413,-68,0);
			yield return 0;
			while(progress<1)
			{
				progress+=Time.deltaTime*20;
				tr.localPosition = new Vector3(413,Mathf.Lerp(tr.localPosition.y,24,progress),0);
				yield return 0;
			}
			progress = 0;
			while(progress<1)
			{
				progress+=Time.deltaTime*20;
				tr.localPosition = new Vector3(413,Mathf.Lerp(24,-68,progress),0);
				yield return 0;
			}
		}
	}
	void selectOptionMain(Transform currentHighlight)
	{
		//this is for pressing A on a button, anything that activates canselect will have to enable it again, like a toggle button.
		switch(currentSelection)
		{
			//Back
			default:
			closeMenu();
			break;
			//infinite lives toggle
			case 0:
			startTransformShake(currentHighlight);
			aSource.PlayOneShot(sounds[1]);
			int d = DataS.infiniteLives;
			d++;
			if(d>1)d = 0;
			DataS.infiniteLives = d;
			if(d==1) currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[1];
			else currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[0];
			changeProfileImage(0,true);
			canSelect = true;
			break;
			case 1:
			canSelect = true;
			break;
			case 2: //confirm mode
			if(tempMode!=prevTempMode&&tempMode!=DataS.mode)
			{
				aSource.PlayOneShot(sounds[3]);
				StartCoroutine(switchModeCor(currentHighlight));
			}
			else canSelect = true;
			break;
		}
	}
	IEnumerator switchModeCor(Transform currentHighlight)
	{
		menu.displayMessage(4);
		yield return new WaitUntil(()=> menu.choiceConfirmed);
		if(menu.choice)
		{
			for(int i = 1;i<currentHighlight.childCount;i++)
			{
				if(tempMode==i-1)
				{
					aSource.PlayOneShot(sounds[2]);
					currentHighlight.GetChild(i).GetChild(0).GetComponent<Image>().color = colors[0];
				}
				else
				{
					currentHighlight.GetChild(i).GetChild(0).GetComponent<Image>().color = colors[1];
				}
			}
			menu.swapGamemode(tempMode);
		}
		else canSelect = true;
	}
	void horOptionChange(Transform currentHighlight,float axisDir)
	{
		switch(currentSelection)
		{
			default: break;
			//difficulty slider
			case 1:
			horPressedDown = true;
			int d = DataS.difficulty;
			aSource.PlayOneShot(sounds[1]);
			if(axisDir==1)
			{
				d++;
				if(d>2)d = 0;
			}
			else
			{
				d--;
				if(d<0)d = 2;
			}
			DataS.difficulty = d;
			Transform point = currentHighlight.transform.GetChild(1).GetChild(0);
			point.localPosition = new Vector3(-300f+(300*d),point.localPosition.y,point.localPosition.z);
			changeProfileImage(DataS.difficulty+1,true);
			setText(DataS.difficulty+1); 
			break;
			//mode select
			case 2:
			horPressedDown = true;
			prevTempMode = tempMode;
			if(axisDir==1)
			{
				tempMode++;
				if(tempMode>modeAmount)tempMode = 0;
			}
			else
			{
				tempMode--;
				if(tempMode<0)tempMode = modeAmount;
			}
			//check validity of input
			switch(tempMode)
			{
				default: break;
				case 1:
				if(!playUhUnlock)
					tempMode = prevTempMode;
				break;
			}
			//print("Temp mode: "+tempMode);
			if(tempMode!=prevTempMode)
			{
				for(int i = 1;i<currentHighlight.childCount;i++)
				{
					if(tempMode==i-1)
					{
						aSource.PlayOneShot(sounds[1]);
						currentHighlight.GetChild(i).GetComponent<Image>().color = colors[0];
					}
					else
					{
						currentHighlight.GetChild(i).GetComponent<Image>().color = colors[1];
					}
				}
				setText(tempMode+4);
			}
			break;
		}
	}
}
