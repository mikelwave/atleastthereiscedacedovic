using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsScript : MonoBehaviour {
	Transform pointer;
	int menuType = 0; //0 - base settings, 1 - input settings
	
	public int currentSelection = 0;
	int previousSelection = 0;
	bool pressedDown = true;
	bool horPressedDown = false;
	bool canSelect = false;
	AxisSimulator axis;
	MenuScript menu;
	int startAnimInt = 8;
	int animInt = 0;
	Animator anim;
	public Sprite[] sprites;
	[Header("Settings")]
	public int musicVolume = 100;
	public int sfxVolume = 100;
	public bool musicEnabled = true;
	public bool sfxEnabled = true;
	public int HUDType = 0;	//0 = wide, 1 = close
	public int ResolutionValue = 0;
	public bool fullscreen = false;
	public bool rumble = true;
	public int inputType = 0;
	public int padLayout = 0;
	public AudioMixer Audio;
	GameObject manager;
	saveData hudtoggle;
	GamepadEffects gamepad;
	string[] resolutionsText = new string[2]{"[768x432]","[1536x864]"}; 
	string[] inputTypeText = new string[2]{"keyboard","gamepad"};
	string[] padLayoutText = new string[4]{"keyboard"," xbox","  switch pro"," ps4"};
	public float pointerMiddleOffset = 800f;
	float pointerOffset = 0f;
	float starterXpointerPos;
	InputReader reader;
	public AudioClip[] sounds;
	AudioSource aSource,music;
	Coroutine rumbleCor;
	bool musicPlaying = false;
	TextMeshProUGUI[] buttonTexts;
	TextMeshProUGUI acceptButton;
	List<TransformShake> shakeEvents;
	bool canAccept = true,rebound = false;
	IEnumerator rumbleCoroutine()
	{
		gamepad.enableTimeStopRumble();
		gamepad.setRumble(new Vector2(0.5f,0.5f));
		int rumbleFrames = 30;
		while(rumbleFrames>=0)
		{
			rumbleFrames--;
			yield return 0;
		}
		gamepad.loadSavedRumble();
	}
	public void saveData()
	{
		SettingsSaveSystem.SaveSettings(this);
	}
	public void loadValues()
	{
		if(anim==null)
		{
			manager = GameObject.Find("Rebindable Manager");
			gamepad = manager.GetComponent<GamepadEffects>();
			reader = manager.GetComponent<InputReader>();
			aSource = transform.parent.GetComponent<AudioSource>();
			music = GetComponent<AudioSource>();

			pointer = transform.GetChild(0);
			starterXpointerPos = pointer.localPosition.x;
			menu = transform.parent.GetComponent<MenuScript>();
			axis = menu.axis;
			anim = GetComponent<Animator>();
			hudtoggle = manager.GetComponent<saveData>();
		}
		shakeEvents = new List<TransformShake>();
		GameSettings settings = SettingsSaveSystem.LoadSettings();
		if(settings!=null)
		{
			musicVolume = settings.musicVolume;
			musicEnabled = settings.musicEnabled;
			sfxVolume = settings.sfxVolume;
			sfxEnabled = settings.sfxEnabled;
			HUDType = settings.HUDType;
			ResolutionValue = settings.ResolutionValue;
			fullscreen = settings.fullscreen;
			if(settings.rumble==1)
			rumble = true;
			else rumble = false;
			if(InputReader.foundController)
			{
				if(settings.inputType!=0)
				{
					inputType = settings.inputType;
					reader.controllerType = settings.padLayout;
				}
				if(reader.controllerType!=5)
				padLayout = Mathf.Clamp(reader.controllerType,1,3);
				else padLayout = 1;
				//print("Pad layout: "+reader.controllerType);
			}
			else
			{
				inputType = 0;
				padLayout = 0;
				reader.controllerType = 0;
			}
			hudtoggle.hudType = HUDType;
			hudtoggle.change();
			acceptButton = transform.GetChild(2).GetChild(10).GetComponent<TextMeshProUGUI>();
			inputType = Mathf.Clamp(inputType,0,1);

			//print("Input type: "+inputType);
		}
		//Assigning input display texts
		buttonTexts = new TextMeshProUGUI[reader.getActionsCount()];
		Transform ch = transform.GetChild(2);
		for(int i = 0;i<4;i++) buttonTexts[i] = ch.GetChild(i+2).GetChild(0).GetComponent<TextMeshProUGUI>(); //Up to Right
		for(int i = 0;i<4;i++) buttonTexts[i+4] = ch.GetChild(i+12).GetChild(0).GetComponent<TextMeshProUGUI>(); //Alt Up to Alt Right
		buttonTexts[8] = ch.GetChild(7).GetChild(0).GetComponent<TextMeshProUGUI>(); //Run
		buttonTexts[9] = ch.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>(); //Jump
		buttonTexts[10] = ch.GetChild(16).GetChild(0).GetComponent<TextMeshProUGUI>(); //Select
		buttonTexts[11] = ch.GetChild(17).GetChild(0).GetComponent<TextMeshProUGUI>(); //Start
		buttonTexts[12] = ch.GetChild(8).GetChild(0).GetComponent<TextMeshProUGUI>(); //Dash
		//updateBindsText();
		registerValueDisplays();
	}
	void OnEnable()
	{
		if(anim!=null)
		{
			if(axis==null) axis = menu.axis;
			canSelect = true;
			enableArrows();

			if(music!=null)
			{
				aSource.PlayOneShot(sounds[5]);
				if(!musicPlaying)
				music.Play();
				else music.UnPause();
			}
		}
		else
		{
			gameObject.SetActive(false);
		}
	}
	void registerValueDisplays()
	{
		Transform tr;
		//todo main menu
		tr = transform.GetChild(1);

		//music toggle
		if(musicEnabled) tr.GetChild(0).GetChild(0).GetComponent<Image>().sprite = sprites[1];
		else tr.GetChild(0).GetChild(0).GetComponent<Image>().sprite = sprites[0];

		//music volume
		Transform point = tr.GetChild(0).GetChild(1).GetChild(0);
		point.localPosition = new Vector3(-300f+(6f*musicVolume),point.localPosition.y,point.localPosition.z);
		setVolume("MusicVolume");

		//sfx toggle
		if(sfxEnabled) tr.GetChild(1).GetChild(0).GetComponent<Image>().sprite = sprites[1];
		else tr.GetChild(1).GetChild(0).GetComponent<Image>().sprite = sprites[0];

		//sfx volume
		point = tr.GetChild(1).GetChild(1).GetChild(0);
		point.localPosition = new Vector3(-300f+(6f*sfxVolume),point.localPosition.y,point.localPosition.z);
		setVolume("SFXVolume");

		//fullscreen
		if(fullscreen) tr.GetChild(2).GetChild(0).GetComponent<Image>().sprite = sprites[1];
		else tr.GetChild(2).GetChild(0).GetComponent<Image>().sprite = sprites[0];

		//HUD Type
		if(HUDType==0) tr.GetChild(3).GetChild(0).GetComponent<Image>().sprite = sprites[3];
		else tr.GetChild(3).GetChild(0).GetComponent<Image>().sprite = sprites[2];

		//Resolution
		tr.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = resolutionsText[ResolutionValue];

		//Rumble
		if(rumble)
		tr.GetChild(5).GetChild(0).GetComponent<Image>().sprite = sprites[1];
		else tr.GetChild(5).GetChild(0).GetComponent<Image>().sprite = sprites[0];
		
		//controls menu
		tr = transform.GetChild(2);

		tr.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = inputTypeText[inputType];
		tr.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = padLayoutText[padLayout];
	}
	void updateBindsText()
	{
		List<string> actions = reader.Actions;
		List<RebindableKey> rk = reader.tmpKeyInputs;
		List<DigiButton> dB = reader.tmpDigiButtons;
		/*
		Up
		Down
		Left
		Right
		Alt Up
		Alt Down
		Alt Left
		Alt Right
		Run
		Jump
		Select
		Start
		Dash
		*/
		if(!canAccept)
		evaluateAllInputs();
		for(int i = 0;i<buttonTexts.Length;i++)
		{
			bool found = false;
			//look in tmp key inputs for action with the same name as the current one
			for(int x = 0;x<rk.Count;x++)
			{
				//if found a match
				if(rk[x].inputName==actions[i])
				{
					found = true;
					textButtonSetter(rk[x].input.ToString(),buttonTexts[i]);
					//Debug.Log(actions[i]+" "+buttonTexts[i].text);
					break;
				}
			}
			//if not found look in tmp digibutton inputs for the action
			if(!found)
			for(int x = 0;x<dB.Count;x++)
			{
				if(dB[x].inputName==actions[i])
				{
					found = true;
					textButtonSetter(dB[x].DigiInput.ToString(),buttonTexts[i]);
					//Debug.Log(actions[i]+" "+buttonTexts[i].text);
					break;
				}
			}
			//else throw a not found error.
			if(!found)
			{
				Debug.LogError("Could not find a bound key to "+actions[i]);
			}
		}
	}
	void textButtonSetter(string input,TextMeshProUGUI tm)
	{
		if(input.Contains("Joystick"))
		{
			for(int l = 0; l<input.Length;l++)
			{
				if(input[l]=='B')
				{
					input = input.Remove(0,l);
					break;
				}
			}
		}
		tm.text = InputReader.joystickInterpreter(input);
		if(tm.text==""||tm.text.ToLower()=="none")
		{
			tm.color = Color.red;
			tm.text = "UNBOUND";
			acceptButton.text = "Unacceptable Inputs";
			canAccept = false;
		}
		else tm.color = Color.white;
	}
	void evaluateAllInputs()
	{
		for(int i = 0;i<reader.tmpKeyInputs.Count;i++)
		{
			RebindableKey k = reader.tmpKeyInputs[i];
			if(k.inputName!="Dummy"&&k.input==KeyCode.None)
			{
				//Debug.Log(k.inputName+" "+k.input+", can't accept");
				return;
			}
		}
		for(int i = 0;i<reader.tmpDigiButtons.Count;i++)
		{
			DigiButton d = reader.tmpDigiButtons[i];
			if(d.inputName!="Dummy"&&d.DigiInput=="")
			{
				//Debug.Log(d.inputName+" "+d.DigiInput+", can't accept");
				return;
			}
		}
		//Debug.Log("Accept avaliable");
		acceptButton.text = "Accept Inputs";
		canAccept = true;
	}
	void setPointer(bool shake)
	{
		pointer.localPosition = new Vector3(starterXpointerPos+pointerOffset,
		transform.GetChild(menuType+1).GetChild(Mathf.Clamp(currentSelection,0,transform.GetChild(menuType+1).childCount-1)).localPosition.y+490,pointer.localPosition.z);

		if(shake)
		{
		aSource.PlayOneShot(sounds[0]);
		startTransformShake(transform.GetChild(menuType+1).GetChild(Mathf.Clamp(currentSelection,0,transform.GetChild(menuType+1).childCount-1)));
		}
	}
	void closeMenu()
	{
		aSource.PlayOneShot(sounds[3]);

			if(music!=null)
			{
				aSource.PlayOneShot(sounds[5]);
			}
			reader.detectInputType();
			saveData();
			anim.SetTrigger("Action");
			menu.displayingSettings = false;
			menu.canSelect = true;
	}
	void returnToMains()
	{
		aSource.PlayOneShot(sounds[3]);
		anim.SetTrigger("swapOptions");
	}
	void selectOptionMain(Transform currentHighlight)
	{
		//main controls
		if(menuType==0)
		{
			//this is for pressing A on a button, anything that activates canselect will have to enable it again, like a toggle button.
			switch(currentSelection)
			{
				//Back
				default:
				closeMenu();
				break;
				//music toggle
				case 0:
				startTransformShake((currentHighlight));
				aSource.PlayOneShot(sounds[3]);
				musicEnabled = !musicEnabled;

				if(musicEnabled) currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[1];
				else currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[0];

				currentHighlight.GetChild(2).gameObject.SetActive(true);
				currentHighlight.GetChild(3).gameObject.SetActive(true);
				
				setVolume("MusicVolume");
				canSelect = true;
				break;
				//sfx toggle
				case 1:
				startTransformShake(currentHighlight);
				aSource.PlayOneShot(sounds[3]);
				sfxEnabled = !sfxEnabled;

				if(sfxEnabled) currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[1];
				else currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[0];

				currentHighlight.GetChild(2).gameObject.SetActive(true);
				currentHighlight.GetChild(3).gameObject.SetActive(true);
				
				setVolume("SFXVolume");
				canSelect = true;
				break;
				//fullscreen toggle
				case 2:
				startTransformShake(currentHighlight);
				aSource.PlayOneShot(sounds[3]);
				fullscreen = !fullscreen;

				if(fullscreen) currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[1];
				else currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[0];
				
				Screen.fullScreen = fullscreen;
				canSelect = true;
				break;
				
				//hud type toggle
				case 3:
				startTransformShake(currentHighlight);
				aSource.PlayOneShot(sounds[3]);
				HUDType++;
				if(HUDType>1)HUDType = 0;

				if(HUDType==0) currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[3];
				else currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[2];

				hudtoggle.hudType = HUDType;

				hudtoggle.change();
				canSelect = true;
				break;

				//resolution
				case 4:
				canSelect = true;
				break;

				//rumble
				case 5:
				startTransformShake(currentHighlight);
				aSource.PlayOneShot(sounds[3]);
				rumble = !rumble;
				gamepad.toggleRumble(rumble);
				if(rumble)
				{
					currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[1];
				}
				else currentHighlight.GetChild(0).GetComponent<Image>().sprite = sprites[0];
				if(rumbleCor!=null)StopCoroutine(rumbleCor);
				rumbleCor = StartCoroutine(rumbleCoroutine());
				canSelect = true;
				break;

				//keybinds
				case 6:
				aSource.PlayOneShot(sounds[3]);
				anim.SetTrigger("swapOptions");
				break;
			}
		}
		//bind controls
		else
		{
			int index = -1;
			switch(currentSelection)
			{
				default: canSelect = true; break;
				//Up
				case 2:
				index = 0;
				break;
				//Down
				case 3:
				index = 1;
				break;
				//Left
				case 4:
				index = 2;
				break;
				//Right
				case 5:
				index = 3;
				break;
				//Jump
				case 6:
				index = 9;
				break;
				//Run
				case 7:
				index = 8;
				break;
				//Select
				case 16:
				index = 10;
				break;
				//Start
				case 17:
				index = 11;
				break;


				//Alt Up
				case 12:
				index = 4;
				break;
				//Alt Down
				case 13:
				index = 5;
				break;
				//Alt Left
				case 14:
				index = 6;
				break;
				//Alt Right
				case 15:
				index = 7;
				break;
				//Dash
				case 8:
				index = 12;
				break;
				//Set default
				case 9:
				aSource.PlayOneShot(sounds[3]);
				reader.defaultKeys();
				acceptButton.text = "Accept Inputs";
				reader.setUsedFromTmp();
				rebound = false;
				canAccept = true;
				updateBindsText();
				startTransformShake(transform.GetChild(2).GetChild(currentSelection));
				canSelect = true;
				break;
				//Accept
				case 10:
				if(rebound&&canAccept)
				{
					aSource.PlayOneShot(sounds[7]);
					reader.setUsedFromTmp();
					rebound = false;
				}
				else if(!canAccept) aSource.PlayOneShot(sounds[6]);
				aSource.PlayOneShot(sounds[3]);
				startTransformShake(transform.GetChild(2).GetChild(currentSelection));
				canSelect = true;
				break;
				//Back
				case 11:
				returnToMains();
				break;
			}
			if(index!=-1)
			{
				StartCoroutine(rebindKey(transform.GetChild(2).GetChild(currentSelection),index));
			}
		}
	}
	IEnumerator rebindKey(Transform tr,int actionID)
	{

		string pressedKey = "Jump";
		bool returnPress=false;
		if(inputType==0&&Input.GetKey(KeyCode.Return))
		{
			returnPress = true;
			//print("Pressed return");
		}
		if(SuperInput.GetKey("Start"))
		pressedKey = "Start";
		aSource.PlayOneShot(sounds[1]);
		pressedDown = true;
		//string Skey = InputReader.GetActionFromID(actionID);
		//Debug.Log("Rebinding Action: "+InputReader.GetActionFromID(actionID)+" with ID of: "+actionID);
		startTransformShake(tr);
		//print(pressedKey+" pressed");
		TextMeshProUGUI tm = tr.GetChild(0).GetComponent<TextMeshProUGUI>();
		tm.text = "PRESS A KEY";
		tm.color = Color.white;

		if(!returnPress)
		yield return new WaitUntil(()=>SuperInput.GetKeyUp(pressedKey));
		else yield return new WaitUntil(()=>Input.GetKeyUp(KeyCode.Return));
		//print(pressedKey+" released");
		InputReader.rebindKey(actionID);
		yield return new WaitUntil(()=>!InputReader.lookingForKey);
		aSource.PlayOneShot(sounds[2]);
		//print("Key found");
		updateBindsText();
		yield return new WaitUntil(()=>SuperInput.GetKeyUp("Dummy"));
		yield return 0;

		pressedDown = false;
		canSelect = true;
		rebound = true;
	}
	IEnumerator changepadType(int ID)
	{
		yield return new WaitUntil(()=>axis.horAxis==0);
		reader.changeKeys(ID,true);
		reader.getUsedToTmp();
		updateBindsText();
		//print("Updated pad");
	}
	IEnumerator changePad(float axisDir,TextMeshProUGUI tmp)
	{
		if(InputReader.foundController)
		{
			if(axisDir==1)
			{
				padLayout++;
				if(padLayout>3)padLayout=1;
				if(padLayout==2)padLayout = 3;//switch pro drop
			}
			else
			{
				padLayout--;
				if(padLayout<1)padLayout=3;
				if(padLayout==2)padLayout = 1;//switch pro drop
			}
		}
		else padLayout = 0;
		yield return new WaitUntil(()=>axis.horAxis==0);
		if(inputType!=0)
		{
			reader.changeKeys(padLayout,true);
			reader.getUsedToTmp();
			updateBindsText();
		}
		aSource.PlayOneShot(sounds[0]);
		startTransformShake(tmp.transform.parent);
		tmp.text = padLayoutText[padLayout];
	}
	void horOptionChange(Transform currentHighlight,float axisDir)
	{
		if(menuType==0)
			{
				switch(currentSelection)
				{
					default: break;
					//music volume
					case 0:

					if(axisDir==1) musicVolume = Mathf.Clamp(musicVolume+2,0,100);
					else musicVolume = Mathf.Clamp(musicVolume-2,0,100);

					Transform point = currentHighlight.transform.GetChild(1).GetChild(0);
					point.localPosition = new Vector3(-300f+(6f*musicVolume),point.localPosition.y,point.localPosition.z);
					setVolume("MusicVolume");

					break;
					//sfx volume
					case 1:
					if(!aSource.loop)
					{
						aSource.clip = sounds[4];
						aSource.loop = true;
						aSource.Play();
					}
					if(axisDir==1) sfxVolume = Mathf.Clamp(sfxVolume+2,0,100);
					else sfxVolume = Mathf.Clamp(sfxVolume-2,0,100);

					point = currentHighlight.transform.GetChild(1).GetChild(0);
					point.localPosition = new Vector3(-300f+(6f*sfxVolume),point.localPosition.y,point.localPosition.z);
					setVolume("SFXVolume");

					break;
					//resolution
					case 4:
						startTransformShake(currentHighlight);
						if(!horPressedDown)
						{
							horPressedDown = true;
							aSource.PlayOneShot(sounds[0]);
						if(axisDir==1)
						{
							ResolutionValue++;
							if(ResolutionValue>1)
							{
								ResolutionValue=0;
								Screen.SetResolution(768,432,Screen.fullScreen);
							}
							else Screen.SetResolution(1536,864,Screen.fullScreen);
						}
						else
						{
							ResolutionValue--;
							if(ResolutionValue<0)
							{
								ResolutionValue=1;
								Screen.SetResolution(1536,864,Screen.fullScreen);
							}
							else Screen.SetResolution(768,432,Screen.fullScreen);

						}
						currentHighlight.GetChild(0).GetComponent<TextMeshProUGUI>().text = resolutionsText[ResolutionValue];
						}
					break;
				}
			}
		else
		{
			switch(currentSelection)
				{
					default: break;
					//controller type
					case 0:
					if(!horPressedDown)
					{
						horPressedDown = true;
						aSource.PlayOneShot(sounds[0]);
						if(InputReader.foundController)
						{
							if(axisDir==1)
							{
								inputType++;
								if(inputType>1)
								{
									inputType=0;
								}
							}
							else
							{
								inputType--;
								if(inputType<0)
								{
									inputType=1;
								}
							}
						}
						else {inputType = 0;}

						if(inputType==0)
						{
							StartCoroutine(changepadType(0));
						}
						else
						{
							StartCoroutine(changepadType(padLayout));
						}
						startTransformShake(currentHighlight);
						currentHighlight.GetChild(0).GetComponent<TextMeshProUGUI>().text = inputTypeText[inputType];
					}
					break;
					//pad layout
					case 1:
					if(!horPressedDown)
					{
						horPressedDown = true;
						StartCoroutine(changePad(axisDir,currentHighlight.GetChild(0).GetComponent<TextMeshProUGUI>()));
					}
					break;
					// Up to alt up switch 2 -> 12
					case 2:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=12;
						pointerOffset = pointerMiddleOffset;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					// Alt up to up switch 12 -> 2
					case 12:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=2;
						pointerOffset = 0;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					// Down to alt down switch 3 -> 13
					case 3:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=13;
						pointerOffset = pointerMiddleOffset;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					// Alt down to down switch 13 -> 3
					case 13:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=3;
						pointerOffset = 0;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					// Left to alt left switch 4 -> 14
					case 4:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=14;
						pointerOffset = pointerMiddleOffset;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					// Alt left to left switch 13 -> 4
					case 14:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=4;
						pointerOffset = 0;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					// Right to alt right switch 5 -> 15
					case 5:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=15;
						pointerOffset = pointerMiddleOffset;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					// Alt right to right switch 15 -> 5
					case 15:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=5;
						pointerOffset = 0;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					//Jump to Select switch 6 -> 16
					case 6:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=16;
						pointerOffset = pointerMiddleOffset;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					//Select to Jump switch 16 -> 6
					case 16:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=6;
						pointerOffset = 0;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					//Run to start switch 7 -> 17
					case 7:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=17;
						pointerOffset = pointerMiddleOffset;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
					//Start to run switch 17 -> 7
					case 17:
					if(!horPressedDown)
					{
						horPressedDown = true;
						//if(movementCor==null||axis.horAxis!=savedHor) resetMoveCor(true,true);
						currentSelection=7;
						pointerOffset = 0;
						disableArrows();
						enableArrows();
						setPointer(true);
					}
					break;
				}
		}
	}
	void setVolume(string s)
	{
		if(s=="SFXVolume")
		{
			if(sfxEnabled&&sfxVolume!=0)
			{
				if(sfxVolume!=100)
				Audio.SetFloat("SFXVolume",(-40f/100)*(100-sfxVolume));
				else Audio.SetFloat("SFXVolume",0f);
			}
			else Audio.SetFloat("SFXVolume",-80f);
		}
		if(s=="MusicVolume")
		{
			if(musicEnabled&&musicVolume!=0)
			{
				if(musicVolume!=100)
				Audio.SetFloat("MusicVolume",(-40f/100)*(100-musicVolume));
				else Audio.SetFloat("MusicVolume",0f);
			}
			else Audio.SetFloat("MusicVolume",-80f);
		}
	}
	void disableArrows()
	{
		if(menuType==0)
		{
			switch(previousSelection)
			{
				default: break;
				case 0:
				transform.GetChild(1).GetChild(previousSelection).GetChild(2).gameObject.SetActive(false);
				transform.GetChild(1).GetChild(previousSelection).GetChild(3).gameObject.SetActive(false);
				break;
				case 1:
				transform.GetChild(1).GetChild(previousSelection).GetChild(2).gameObject.SetActive(false);
				transform.GetChild(1).GetChild(previousSelection).GetChild(3).gameObject.SetActive(false);
				break;
				case 4:
				transform.GetChild(1).GetChild(previousSelection).GetChild(1).gameObject.SetActive(false);
				transform.GetChild(1).GetChild(previousSelection).GetChild(2).gameObject.SetActive(false);
				break;
			}
		}
		else
		{
			Transform tr = transform.GetChild(2).GetChild(previousSelection);
			switch(previousSelection)
			{
				default: break;
				case 0:
				tr.GetChild(1).gameObject.SetActive(false);
				tr.GetChild(2).gameObject.SetActive(false);
				break;
				case 1:
				tr.GetChild(1).gameObject.SetActive(false);
				tr.GetChild(2).gameObject.SetActive(false);
				break;
			}
		}
	}
	void enableArrows()
	{
		if(menuType==0)
		{
			switch(currentSelection)
			{
				default: break;
				case 0:
				case 1:
				transform.GetChild(1).GetChild(currentSelection).GetChild(2).gameObject.SetActive(true);
				transform.GetChild(1).GetChild(currentSelection).GetChild(3).gameObject.SetActive(true);
				break;
				case 4:
				transform.GetChild(1).GetChild(currentSelection).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(1).GetChild(currentSelection).GetChild(2).gameObject.SetActive(true);
				break;
			}
		}
		else
		{
			switch(currentSelection)
			{
				default: break;
				case 0:
				case 1:
				transform.GetChild(2).GetChild(currentSelection).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(2).GetChild(currentSelection).GetChild(2).gameObject.SetActive(true);
				break;
			}
		}
	}
	public void disableObj()
	{
		gameObject.SetActive(false);
	}
	public void swapSettings()
	{
		menuType++;
		if(menuType>1)menuType = 0;

		//go to main controls
		if(menuType==0)
		{
			transform.GetChild(1).gameObject.SetActive(true);
			transform.GetChild(2).gameObject.SetActive(false);
			currentSelection = 6;
			disableArrows();
			enableArrows();
			setPointer(false);
		}
		//go to bind controls
		else
		{
			transform.GetChild(1).gameObject.SetActive(false);
			transform.GetChild(2).gameObject.SetActive(true);
			currentSelection = 0;
			reader.getUsedToTmp();
			updateBindsText();
			disableArrows();
			enableArrows();
			setPointer(false);
		}
	}
	public void enableSelectingSwitch()
	{
		canSelect = true;
	}
	void OnDisable()
	{
		currentSelection = 0;
		menuType = 0;
		if(pointer!=null)
		{
			pointer.localPosition = new Vector3(pointer.localPosition.x,
			transform.GetChild(menuType+1).GetChild(Mathf.Clamp(currentSelection,0,transform.GetChild(menuType+1).childCount-1)).localPosition.y+490,pointer.localPosition.z);
		}
		if(music!=null)
		{
			music.Pause();
		}
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
	// Update is called once per frame

	Coroutine movementCor;
	float savedVer=0;
	//savedHor=0;
	IEnumerator IMovementCor(bool hor)
	{
		int frameWait = 16;
        //infinite loop
        while(true)
        {
            frameWait--;

            if(frameWait<=0)
            {
				if(!hor)
                pressedDown = false;
				else horPressedDown = false;
                frameWait = 6;
            }
            yield return 0;
        }
	}
	void resetMoveCor(bool reActivate,bool hor)
	{
		if(hor)
		{
			//savedHor = axis.horAxis;
			savedVer = 0;
		}
		else
		{
			savedVer = axis.verAxis;
			//savedHor = 0;
		}
		if(movementCor!=null)StopCoroutine(movementCor);
		if(reActivate)
		movementCor = StartCoroutine(IMovementCor(hor));
		else movementCor=null;
	}
	void Update ()
	{
		if(Mathf.Abs(axis.verAxis)==1&&!pressedDown&&canSelect)
		{
			previousSelection = currentSelection;
			animInt = 0;
			if(axis.verAxis>0)
			{
				if(menuType==0
				 ||menuType==1&&currentSelection!=12)
				currentSelection--;

				else if(menuType==1&&currentSelection==12)
				{
					pointerOffset = 0;
					currentSelection = 1;
				}
			}
			else
			{
				if(menuType==0||menuType==1&&currentSelection!=11)
				currentSelection++;
				else if(menuType==1&&currentSelection==11)
				{
					pointerOffset = 0;
					currentSelection = 0;
				}
			}

			if(menuType==0)
			{
				if(currentSelection>=transform.GetChild(1).childCount)
				currentSelection = 0;
				else if(currentSelection<0)
				currentSelection=transform.GetChild(1).childCount-1;
			}
			else
			{
				if(currentSelection<0)
				{
					currentSelection = 11;
					pointerOffset = 0;
				}
				else if(currentSelection>17)
				{
					currentSelection = 8;
					pointerOffset = 0;
				}
			}
			if(movementCor==null||axis.verAxis!=savedVer) resetMoveCor(true,false);
			pressedDown = true;
			disableArrows();
			enableArrows();
			setPointer(true);
		}
		/*if(axis.horAxis==0)
		{
			savedHor=0;
		}*/
		if(axis.verAxis==0)
		{
			savedVer=0;
		}
		//code for selecting as option horizontally (functions for volume, resolution, screenshake and keybinds)
		if(Mathf.Abs(axis.horAxis)==1&&!horPressedDown&&!pressedDown&&canSelect)
		{
			horOptionChange(transform.GetChild(menuType+1).GetChild(Mathf.Clamp(currentSelection,0,transform.GetChild(menuType+1).childCount-1)),axis.horAxis);
		}
		else if(axis.horAxis==0
			  &&axis.verAxis==0
			  &&pressedDown)
		{
			pressedDown = false;
			resetMoveCor(false,false);
		}
		if(axis.horAxis==0&&horPressedDown)
		{
			horPressedDown = false;
			//resetMoveCor(false,true);
		}

			if(axis.horAxis==0&&aSource.loop)
			{
				if(aSource.isPlaying)
				aSource.Stop();
				aSource.loop = false;
				aSource.clip = null;
			}

		if(movementCor==null&&(SuperInput.GetKeyDown("Jump")&&canSelect
		||inputType!=0&&SuperInput.GetKeyDown("Start")&&canSelect
		||inputType==0&&Input.GetKeyDown(KeyCode.Return)&&canSelect))
		{
			canSelect = false;
			selectOptionMain(transform.GetChild(menuType+1).GetChild(Mathf.Clamp(currentSelection,0,transform.GetChild(menuType+1).childCount-1)));
		}
		if(movementCor==null&&SuperInput.GetKeyDown("Select")&&canSelect)
		{
			if(menuType==1)
			{
				returnToMains();
			}
			else
			{
				closeMenu();
			}
		}
	}
}
