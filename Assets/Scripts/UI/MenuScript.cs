using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class MenuScript : MonoBehaviour {
	public bool debugAutoPause =false;
	public bool debugPlayuhCheck = false;
	public bool isPauseMenu = false;
	bool pressedDown = false;
	public int amountOfOptions = 5;
	int currentOption = 1;
	[HideInInspector]
	public AxisSimulator axis;
	Transform highlight;
	Image highlightImage;
	List<Transform> buttons;
	Transform[] optionButtons = new Transform[2];
	public bool canSelect = false;
	bool active = false;
	HubCamera cam;
	MGCameraController gameCam;
	dataShare dataS;
	public AudioClip[] sounds = new AudioClip[3];
	public AudioClip[] PauseSounds = new AudioClip[2];
	AudioSource asc;
	float musicVolume = 1;
	Coroutine cor;
	public Color32[] highlightColors = new Color32[2];
	int colorFrames = 0;
	int colorint = 0;
	GameData data;
	GameObject HUD;
	Animator anim;
	SettingsScript settings;
	difficultyScript difficulty;
	public bool playerDead = false;
	//bool unlocked = false;
	public AudioMixerSnapshot[] snapshots;
	bool paused = false;
	Coroutine pauseCor;
	public AudioSource music;
	//alts
	public AudioClip altMusic;
	public Sprite altLogo;
	public Sprite[] altChar = new Sprite[2];
	GameObject unlockPanel;
	bool musicPaused = false,bookOpen = false;
	IEnumerator pauseCoolDown()
	{
		yield return 0;
		yield return new WaitForSeconds(0.05f);
		paused = false;
		pauseCor = null;
	}
	string[] messages = new string[]
	{
		"Are you sure you want to quit?"+'\n'+"Your unsaved progress will be lost.",
		"Are you sure you want to return to the map?"+'\n'+"Your level progress will be lost.",
		"Are you sure you want to return to main menu?",
		"Return to file select?"+'\n'+"Your unsaved progress will be lost.",
		"Switch game mode?"+'\n'+"Your current progress will be saved.",
		"Would you like to view the cutscene now?"+'\n'+"It can later be played in the Gallery.",
		"Would you like to play the song now?"+'\n'+"It can later be played by selecting it.",
		"Restart from last checkpoint?"
	};

	Image highlightMessageImage;
	TextMeshProUGUI text;
	public bool choice,choiceConfirmed,displayingMessage = false;
	public bool displayingSettings = false;
	public bool pauseLock = false;
	bool unlockedLorebook = true;
	Coroutine fCor;
	Coroutine pauseStep;
	PlayerScript pScript;
	IEnumerator fadeMusicCor(bool fadeIn)
	{
		if(music!=null)
		{
			if(musicPaused&&fadeIn)
			{
				musicPaused = false;
				music.UnPause();
			}
			float TargetVolume = 0,curVolume = music.volume,progress = 0;
			if(fadeIn)TargetVolume = musicVolume;

			while(progress<=1)
			{
				progress+=Time.deltaTime*2f;
				music.volume = Mathf.Lerp(curVolume,TargetVolume,progress);
				yield return 0;
			}
			if(!fadeIn)
			{
				musicPaused=true;
				music.Pause();
			}
		}
	}
	IEnumerator showUnlockScreen()
	{
		dataS.clearedUnbeatenLevel = false;
		cam.fadeScreen(false);
		yield return 0;
		yield return new WaitUntil(()=>cam.fadeAnim<=0);
		Time.timeScale = 1;
		yield return new WaitForSeconds(4f);
		int waitFrames = 180;
		while(!SuperInput.GetKey("Jump")&&waitFrames>0)
		{
			waitFrames--;
			yield return 0;
		}
		if(SuperInput.GetKey("Jump"))asc.PlayOneShot(sounds[1]);
		cam.fadeScreen(true);
		yield return 0;
		yield return new WaitUntil(()=>cam.fadeAnim>=1);
		unlockPanel.SetActive(false);
		anim.enabled = true;
		Start();
		yield return new WaitUntil(()=>Time.timeScale!=0);
		StartCoroutine(dataS.saveData(true));

	}
	bool unfocusPass = false;
	IEnumerator waitForUnFocus()
	{
		yield return 0;
		yield return 0;
		if(data.sources[0].clip!=null)
		yield return new WaitUntil(()=>data.sources[0].isPlaying);
		yield return 0;
		unfocusPass = true;
	}
	void showCodeText()
	{
		bool playsound = true;
		bool cheatInit = false;
		switch(dataShare.codeIDBuffer)
		{
			default: playsound = false; break;
			case 0: cheatInit = true; dataS.setTextPopup("Random sounds: "+(dataShare.randSounds?"ON":"OFF")); break;
			case 1: dataS.setTextPopup("Buddha mode: "+(dataShare.godMode?"ON":"OFF")); break;
			case 2:
				int val = Mathf.Clamp(dataShare.powerBufferInt,0,data.powerups.Length-1);
				dataS.setTextPopup("Given item #"+val);
			break;
			case 3: dataS.StartCoroutine(dataS.saveData(true));playsound = false; break;
			case 4: cheatInit = true; dataS.setTextPopup("Cheats reset."); break;
			case 5: dataS.setTextPopup("Added teapots: "+dataShare.powerBufferInt); break;
			case 6: dataS.setTextPopup("Added floppies: "+dataShare.powerBufferInt); break;
			case 7: dataS.setTextPopup("Maxed out teapots"); break;
			case 8: dataS.setTextPopup("Maxed out floppies"); break;
			case 9: dataS.setTextPopup("Added lives: "+dataShare.powerBufferInt); break;
			case 10: dataS.setTextPopup("Maxed out lives"); break;
			case 11: cheatInit = true; dataS.setTextPopup("Debug mode: "+(dataShare.debug?"ON":"OFF")); break;
			case 12:
				int keyVal = Mathf.Clamp(dataShare.powerBufferInt,0,2);
				string keyname = "Red";
				if(keyVal!=0)keyname = (keyVal==1?"Blue":"Yellow");
				dataS.setTextPopup("Got key: "+keyname);
			break;
			case 13: dataS.setTextPopup("Got all keys"); break;
			case 14:
				int checkVal = Mathf.Clamp(dataShare.powerBufferInt,0,data.checkpointCount);
				dataS.setTextPopup("Checkpoint set: "+checkVal); break;
			case 15: dataS.setTextPopup("Removed all teapots"); break;
			case 16: dataS.setTextPopup("Removed all floppies"); break;
			case 17: dataS.setTextPopup("Removed all lives"); break;
			case 18: dataS.setTextPopup("Added score: "+dataShare.powerBufferInt); break;
			case 19: dataS.setTextPopup("Removed all score");break;
			case 20:
				int diffVal = Mathf.Clamp(dataShare.powerBufferInt,0,2);
				dataS.setTextPopup("Difficulty set: "+diffVal); break;
			case 21: dataS.setTextPopup("Maxed out score"); break;
		}
		if(!data.cheated)data.cheated = !cheatInit;
		//print("Cheated: "+data.cheated);
		if(playsound)asc.PlayOneShot(dataS.cheatSound);
	}
	// Use this for initialization
	void Start () 
	{
		asc = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		settings = transform.GetChild(3).GetComponent<SettingsScript>();
		if(dataS==null)
		{
			dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
			cam = GameObject.Find("Main Camera").GetComponent<HubCamera>();
		}
		if(isPauseMenu)
		{
			axis = GameObject.Find("Player_main").GetComponent<AxisSimulator>();
			HUD = GameObject.Find("HUD_Canvas").gameObject;
			data = GameObject.Find("_GM").GetComponent<GameData>();
			gameCam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
			StartCoroutine(waitForUnFocus());
		}
		else
		{
			axis = GetComponent<AxisSimulator>();
			//display playuh unlock screen
			if(dataShare.totalCompletedLevels==41&&dataS.clearedUnbeatenLevel)
			{
				if(dataS.mode!=1)
				{
					dataS.clearedUnbeatenLevel = false;
					if(debugPlayuhCheck||!PlayerPrefs.HasKey("Save"+(Mathf.Clamp(dataS.saveFileID+4,4,6))))
					{
						anim.enabled = false;
						unlockPanel = transform.GetChild(6).gameObject;
						unlockPanel.SetActive(true);
						StartCoroutine(showUnlockScreen());
						return;
					}
					else StartCoroutine(dataS.saveData(true));
				}
				else StartCoroutine(dataS.saveData(true));
			}
			SpriteRenderer icon = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
			if(dataS.mode==0)
			{
				//check for blood god
				if(dataShare.totalCompletedLevels>=42)
				icon.sprite = altChar[2];
				if(!isPauseMenu&&dataS.levelProgress[0][0]=='N')
				{
					unlockedLorebook = false;
					transform.GetChild(1).GetChild(4).GetComponent<Image>().color = new Color(0.35f,0,0,1);
				}
				if(dataShare.totalCompletedLevels>=41&&!PlayerPrefs.HasKey("Save"+(Mathf.Clamp(dataS.saveFileID+4,4,6))))
				{
					transform.GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(true);
				}
			}
			else if(dataS.mode==1)
			{
				//check for blood god
				if(dataShare.totalCompletedLevels>=42)
				{
					bool found = false;
					//check if all extras unlocked
					for(int i = 0;i<29;i++)
					{
						if(!dataS.cellData[i])
						{
							found = true;
							break;
						}
					}
					icon.sprite = altChar[found ? 0 : 1];
				}
				else icon.sprite = altChar[0];
				transform.GetChild(0).GetChild(5).GetComponent<Image>().sprite = altLogo;
				music.clip = altMusic;
			}
			musicVolume = music.volume;
			music.gameObject.SetActive(true);
			difficulty = transform.GetChild(4).GetComponent<difficultyScript>();
			if(difficulty.gameObject.activeInHierarchy)
			difficulty.gameObject.SetActive(false);
			difficulty.loadValues();
			Time.timeScale = 1;
			cam.fadeScreen(false);
			snapshots[0].TransitionTo(0.0f);
			if(dataS.lives<=0)
			{
				dataS.lives = 5;
				dataS.score = 0;
				dataS.coins = 0;
			}
			dataS.updateBook(false);
		}
		buttons = new List<Transform>();
		if(!isPauseMenu)
		{
			dataS.resetValues();
		}
		highlight = transform.GetChild(1).GetChild(0);
		highlightImage = highlight.GetComponent<Image>();
		highlightMessageImage = transform.GetChild(2).GetChild(1).GetComponent<Image>();
		text = transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
		for(int i = 2; i<transform.GetChild(2).childCount;i++)
		{
			optionButtons[i-2]=transform.GetChild(2).GetChild(i);
		}
		for(int i = 1; i<transform.GetChild(1).childCount;i++)
		{
			buttons.Add(transform.GetChild(1).GetChild(i));
		}
		if(!isPauseMenu)
		{
			Transform t = transform.GetChild(2);
			t.SetAsLastSibling();
		}
		if(settings.gameObject.activeInHierarchy)
		settings.gameObject.SetActive(false);
		settings.loadValues();
		Canvas C = GetComponent<Canvas>();
		if(C.worldCamera!=null)
		{
			dataS.setBookCam(C.worldCamera,C.planeDistance+0.01f);
		}
		else dataS.setBookCam(GameObject.Find("Main Camera").GetComponent<Camera>(),3);
		dataS.forceBookNotifs();
	}
	int failInt = 0;
	// Update is called once per frame
	void Update ()
	{
		if(active)
		{
			if(dataShare.codeIDBuffer!=-1)
			{
				if(!isPauseMenu)
				{
					dataShare.codeIDBuffer = -1;
					dataShare.powerBufferInt = -1;
					return;
				}
				if(data==null)
				{
					data = GameObject.Find("_GM").GetComponent<GameData>();
				}
				if(data!=null)
				{
					if(dataShare.debug)
					print("Code buffer: "+dataShare.codeIDBuffer);
					if(data.inHub)//blacklist cheats
					{
						switch(dataShare.codeIDBuffer)
						{
							default: break;
							case 2:
							case 12:
							case 13:
							case 14:
							dataShare.codeIDBuffer = -1;
							dataShare.powerBufferInt = -1;
							break;
						}
					}
					switch(dataShare.codeIDBuffer)
					{
						default: break;
						case 2: data.givePowerup(dataShare.powerBufferInt); break;
						case 5: data.addCoin(dataShare.powerBufferInt,false); break;
						case 6: data.addFloppy(dataShare.powerBufferInt,false); break;
						case 7: data.addCoin(999,false); break;
						case 8: data.addFloppy(99,false); break;
						case 9: data.addLivesSilent(dataShare.powerBufferInt); break;
						case 10: data.addLivesSilent(99); break;
						case 12:
							int keyVal = Mathf.Clamp(dataShare.powerBufferInt,0,2);
							data.updateKeys(keyVal, false);
						break;
						case 13: data.updateKeys(0, false); data.updateKeys(1, false); data.updateKeys(2, false); break;
						case 14:
						int checkVal = Mathf.Clamp(dataShare.powerBufferInt,0,data.checkpointCount);
						data.setPoint(checkVal);
						break;
						case 15:
						data.nullCoin();
						break;
						case 16:
						data.nullFloppy();
						break;
						case 17:
						data.nullLives();
						break;
						case 18: data.addScore(dataShare.powerBufferInt); data.saveScore(); break;
						case 19: data.nullScore(); break;
						case 20:
						int diffVal = Mathf.Clamp(dataShare.powerBufferInt,0,2);
						if(pScript==null&&isPauseMenu)pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
						
						if(pScript!=null)pScript.dmgMode = diffVal;
						dataS.difficulty = diffVal; break;
						case 21: data.addScore(99999999); data.saveScore(); break;
					}
					showCodeText();
					dataShare.powerBufferInt = -1;
					dataShare.codeIDBuffer = -1;
				}
			}
			if(axis==null||settings==null)
			{
				print("Error: Axis: "+(axis==null?true:false)+" Settings: "+(settings==null?true:false));
				if(failInt<3)
				{
				Start();
				failInt++;
				}
				else Application.Quit(13);
			}
			//unlock all levels
			/*if(!isPauseMenu)
			{
				if(Input.GetKeyDown(KeyCode.Tab)&&!unlocked)
				{
					unlocked = true;
					Debug.Log("Unlocked all levels");
					asc.PlayOneShot(sounds[1]);
					dataShare.totalCompletedLevels = 10;
					dataS.currentWorld = 0;
					dataS.lastLoadedLevel = 1;
					dataS.lastLoadedWorld = 0;
					for(int i = 0; i<8;i++)
					{
						string currentLevelProgress = dataS.levelProgress[i];
						currentLevelProgress = currentLevelProgress.Remove(0,1);
						currentLevelProgress = currentLevelProgress.Insert(0,"F");
						dataS.levelProgress[i] = currentLevelProgress;
					}
					dataS.worldProgression = 2;
				}
			}*/
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
				if(!displayingMessage)
					highlightImage.color = highlightColors[colorint];
				else
				{
					if(highlightImage.color !=highlightColors[0])
					highlightImage.color = highlightColors[0];

					highlightMessageImage.color = highlightColors[colorint];
				}
			}
			//menu navigation
			if(!displayingMessage&&!displayingSettings)
			{
				if(isPauseMenu&&canSelect&&SuperInput.GetKeyDown("Start")&&axis.verAxis==0)
				{
					unPause();
				}
				if(Mathf.Abs(axis.verAxis)==1&&!pressedDown&&canSelect)
				{
					pressedDown = true;
					changeOption(axis.verAxis);
				}
				else if(axis.verAxis==0&&pressedDown)
				{
					pressedDown = false;
				}
				if(SuperInput.GetKeyDown("Jump")&&canSelect||settings.inputType%5==0&&Input.GetKeyDown(KeyCode.Return)&&canSelect)
				{
					if(!isPauseMenu)
					{	
						if(cor!=null)
						StopCoroutine(cor);
						cor = StartCoroutine(selectOptionMainMenu());
					}
					else
					{
						if(cor!=null)
						StopCoroutine(cor);
						cor = StartCoroutine(selectOptionPause());
					}
				}
			}
			else
			{
				if(displayingMessage)
				{
					if(Mathf.Abs(axis.horAxis)==1&&!pressedDown&&canSelect)
					{
						pressedDown = true;
						messageOption();
					}
					else if(axis.horAxis==0&&pressedDown)
					{
						pressedDown = false;
					}
					if(SuperInput.GetKeyDown("Jump")&&canSelect||settings.inputType%5==0&&Input.GetKeyDown(KeyCode.Return)&&canSelect)
					{
						StartCoroutine(closeMessageBox());
					}
				}
			}
			if(!bookOpen)
			dataShare.codeInput();
		}
		if((isPauseMenu&&!playerDead&&!pauseLock&&!paused)&&((!active&&!transform.GetChild(0).gameObject.activeInHierarchy&&Time.timeScale!=0)&&
		((SuperInput.GetKeyDown("Start")&&unfocusPass)
		||(!Application.isFocused&&!Application.isEditor&&unfocusPass))
		||debugAutoPause&&unfocusPass))
		{
			debugAutoPause = false;
			paused = true;
			anim.SetBool("Paused",true);
			if(pauseStep!=null)StopCoroutine(pauseStep);
			pauseStep = StartCoroutine(IPauseStep());
			asc.PlayOneShot(PauseSounds[0]);
			HUD.SetActive(false);
			
			enableMenu();
		}

	}
	IEnumerator IPauseStep()
	{
		while(anim.GetBool("Paused"))
		{
			Time.timeScale = 0;
			data.pauseMusic(true);
			yield return new WaitForSeconds(0.01f);
			yield return 0;
		}
		print("pause stopped");
	}
	void changeOption(float dir)
	{
		if(dir<0) {currentOption++;if(currentOption>amountOfOptions)currentOption=1;}
		else if (dir>0) {currentOption--;if(currentOption<1)currentOption=amountOfOptions;}
		asc.PlayOneShot(sounds[0]);
		Vector3 buttonPos = buttons[currentOption-1].localPosition;
		highlight.localPosition = new Vector3(buttonPos.x+10,buttonPos.y-10,buttonPos.z);
	}
	void messageOption()
	{
		choice = !choice;
		int i = 0;
		asc.PlayOneShot(sounds[0]);
		if(!choice) i = 1;
			Vector3 buttonPos = optionButtons[i].localPosition;
		highlightMessageImage.transform.localPosition = new Vector3(buttonPos.x+10,buttonPos.y-10,buttonPos.z);
	}
	IEnumerator selectOptionMainMenu()
	{
		asc.PlayOneShot(sounds[1]);
		canSelect = false;
		colorFrames=20;
		if(currentOption==1)
		{
			colorFrames+=40;
			yield return new WaitForSeconds(0.5f);
			cam.fadeScreen(true);
			if(fCor!=null)StopCoroutine(fCor);
			fCor = StartCoroutine(fadeMusicCor(false));
			yield return new WaitForSeconds(1f);
		}
		else yield return new WaitUntil(()=> colorFrames==0);
		switch(currentOption)
		{
			default: Debug.LogError("Invalid Option: "+currentOption);canSelect = true; asc.PlayOneShot(sounds[2]); break;
			case 1: startGame(); break;
			case 2:
			//print("difficulty");
			displayingSettings = true;
			difficulty.gameObject.SetActive(true);
			break;
			case 3:
			yield return new WaitUntil(()=> colorFrames==0);
			displayMessage(3);
			yield return new WaitUntil(()=> choiceConfirmed);
			if(choice)
			{
				colorFrames=60;
				cam.fadeScreen(true);
				if(fCor!=null)StopCoroutine(fCor);
				fCor = StartCoroutine(fadeMusicCor(false));
				yield return new WaitUntil(()=> colorFrames==0);
				saveSelect();
			}
			else canSelect = true; 
			break;
			case 4:
			if(!unlockedLorebook)
			{
				canSelect = true;
				asc.PlayOneShot(sounds[2]);
				yield break;
			}
			if(fCor!=null)StopCoroutine(fCor);
			fCor = StartCoroutine(fadeMusicCor(false));
			bookOpen = true;
			dataS.showLoreBook();
			//wait until book is closed
			yield return 0;
			yield return new WaitUntil(()=>dataS.getBookDisableFlag());
			if(fCor!=null)StopCoroutine(fCor);
			fCor = StartCoroutine(fadeMusicCor(true));
			yield return new WaitUntil(()=>!dataS.bookActive());
			miniSave(dataS.mode);
			canSelect = true;
			bookOpen = false;
			break;
			case 6:
			//print("options");
			displayingSettings = true;
			settings.gameObject.SetActive(true);
			break;
			case 5:
			colorFrames=60;
			cam.fadeScreen(true);
			if(fCor!=null)StopCoroutine(fCor);
			fCor = StartCoroutine(fadeMusicCor(false));
			yield return new WaitUntil(()=> colorFrames==0);
			dataS.playCutscene = false;
			dataS.specialData = 0;
			dataS.loadSceneWithoutLoadScreen(54);
			break;
			case 7: 
			yield return new WaitUntil(()=> colorFrames==0);
			displayMessage(0);
			yield return new WaitUntil(()=> choiceConfirmed);
			if(choice)
			{
				colorFrames=60;
				cam.fadeScreen(true);
				if(fCor!=null)StopCoroutine(fCor);
			fCor = StartCoroutine(fadeMusicCor(false));
				yield return new WaitUntil(()=> colorFrames==0);
				quitGame();
			}
			else canSelect = true;
			break;
		}
	}
	IEnumerator selectOptionPause()
	{
		asc.PlayOneShot(sounds[1]);
		canSelect = false;
		colorFrames = 20;
		while(colorFrames>0)
		{
			yield return 0;
		}
		if(amountOfOptions==6)
		{
			//Debug.Log("Picking option from a level pause menu");
			switch(currentOption)
			{
				default: Debug.LogError("Invalid Option: "+currentOption); canSelect = true; asc.PlayOneShot(sounds[2]); break;
				case 1: unPause(); break;
				case 2:
				yield return new WaitUntil(()=> colorFrames==0);
				colorFrames=10;
				displayMessage(1);
				yield return new WaitUntil(()=> choiceConfirmed);
				if(choice)
				{
					colorFrames+=60;
					gameCam.fadeScreen(true);
					while(colorFrames>0)
						yield return 0; 
					goToMap();
				}
				else canSelect = true;
				break;

				case 3:
				bookOpen = true;
				dataS.showLoreBook();
				//wait until book is closed
				yield return 0;
				yield return new WaitUntil(()=>dataS.getBookDisableFlag());
				yield return new WaitUntil(()=>!dataS.bookActive());
				miniSave(dataS.mode);
				canSelect = true;
				bookOpen = false;
				break;

				case 4:
				//print("options");
				displayingSettings = true;
				settings.gameObject.SetActive(true);
				break;
				case 5:
				colorFrames=10;
				if(dataS.checkpointValue!=0)
				{
					displayMessage(7);
					yield return new WaitUntil(()=> choiceConfirmed);
					if(!choice)
					{
						dataS.resetCheckData();
					}
					colorFrames+=60;
				}
					unPause();
					playerDead = true;
					data.killPlayer();
				break;
				case 6:
				yield return new WaitUntil(()=> colorFrames==0);
				colorFrames=10;
				displayMessage(2);
				yield return new WaitUntil(()=> choiceConfirmed);
				if(choice)
				{
					colorFrames+=60; 
					gameCam.fadeScreen(true); 
					while(colorFrames>0)
						yield return 0; 
					dataS.loadSceneWithLoadScreen(2);
				}
				else canSelect = true;
				break;
			}
		}
		else if(amountOfOptions==4)
		{
			//Debug.Log("Picking option from a world pause menu");
			switch(currentOption)
			{
				default: Debug.LogError("Invalid Option: "+currentOption); canSelect = true; asc.PlayOneShot(sounds[2]); break;
				case 1: unPause(); break;

				case 2:
				bookOpen = true;
				dataS.showLoreBook();
				//wait until book is closed
				yield return 0;
				yield return new WaitUntil(()=>dataS.getBookDisableFlag());
				yield return new WaitUntil(()=>!dataS.bookActive());
				miniSave(dataS.mode);
				canSelect = true;
				bookOpen = false;
				break;

				case 3:
				//print("options");
				displayingSettings = true;
				settings.gameObject.SetActive(true);
				break;
				
				case 4:
				yield return new WaitUntil(()=> colorFrames==0);
				colorFrames=10;
				displayMessage(2);
				yield return new WaitUntil(()=> choiceConfirmed);
				if(choice)
				{
					colorFrames+=60; 
					if(gameCam!=null)
					gameCam.fadeScreen(true); 
					else GameObject.Find("Main Camera").GetComponent<HubCamera>().fadeScreen(true);
					while(colorFrames>0)
						yield return 0; 
					dataS.loadSceneWithLoadScreen(2);
				}
				else canSelect = true;
				break;
			}
		}
	}
	IEnumerator closeMessageBox()
	{
		asc.PlayOneShot(sounds[1]);
		canSelect = false;
		colorFrames=20;
		yield return new WaitUntil(()=> colorFrames==0);
		choiceConfirmed = true;
		displayingMessage = false;
		anim.SetBool("Message",false);
		if(anim.GetBool("Paused"))
		{
			if(!choice)
			canSelect = true;
		}
	}
	Coroutine waitForSelect;
	IEnumerator IWaitForSelect()
	{
		int i = 10;
		while(i>0)
		{
			i--;
			yield return 0;
		}
		canSelect = true;
	}
	public void enableMenu()
	{
		active = true;
		if(waitForSelect!=null)StopCoroutine(waitForSelect);
		waitForSelect = StartCoroutine(IWaitForSelect());
		if(isPauseMenu)
		{
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(true);
		}
	}
	public void partEnableDisable(bool toggle)
	{
		this.enabled = toggle;
		active = toggle;
		canSelect = toggle;
	}
	IEnumerator disableMenu()
	{
		active = false;
		canSelect = false;
		int unPauseFrames = 25;
		while(unPauseFrames>0)
		{
			unPauseFrames--;
			yield return 0;
		}
		if(isPauseMenu)
		{
			if(pauseStep!=null)StopCoroutine(pauseStep);

			if(!playerDead)
			data.pauseMusic(false);
			HUD.SetActive(true);
			Time.timeScale = 1;
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
		}
	}
	IEnumerator altModeIntro()
	{
		StartCoroutine(dataS.saveData(true));
		yield return 0;
		yield return new WaitUntil(()=>!dataS.saving);
		dataS.loadWorldWithLoadScreen(1);
	}
	void startGame()
	{
		if(dataS.mode==1)
		{
			if(dataS.levelProgress[0][0]!='F')
			{
				dataS.lastLoadedWorld = 1;
				dataS.levelProgress[0] = dataS.levelProgress[0].Remove(0,1);
				dataS.levelProgress[0] = dataS.levelProgress[0].Insert(0,"F");
				dataShare.totalCompletedLevels++;
				StartCoroutine(altModeIntro());
			}
			else
			{
				//print("Last loaded world: "+dataS.lastLoadedWorld);
				if(dataS.lastLoadedWorld!=0)
				dataS.loadWorldWithLoadScreen(dataS.lastLoadedWorld);
				else dataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex+1);
			}
		}
		else
		{
			//print("Last loaded world: "+dataS.lastLoadedWorld);
			if(dataS.lastLoadedWorld!=0)
			dataS.loadWorldWithLoadScreen(dataS.lastLoadedWorld);
			else dataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex+1);
		}
	}
	void saveSelect()
	{
		dataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex-1);
	}
	void quitGame()
	{
		Application.Quit();
	}
	void unPause()
	{
		asc.PlayOneShot(PauseSounds[1]);
		anim.SetBool("Paused",false);
		if(pauseCor!=null)StopCoroutine(pauseCor);
		pauseCor = StartCoroutine(pauseCoolDown());
		StartCoroutine(disableMenu());
	}
	void goToMap()
	{
		dataS.loadWorldWithLoadScreen(dataS.currentWorld);
	}
	public void displayMessage(int disp)
	{
		//print("Displaying: "+disp);
		choiceConfirmed = false;
		canSelect = true;
		choice = true;
		Vector3 buttonPos = optionButtons[0].localPosition;
		highlightMessageImage.transform.localPosition = new Vector3(buttonPos.x+10,buttonPos.y-10,buttonPos.z);
		if(disp<messages.Length)
		{
			anim.enabled = true;
			colorFrames = 0;
			displayingMessage = true;
			text.text = messages[disp];
			anim.SetBool("Message",true);
		}
	}
	public void swapGamemode(int mode)
	{
		StartCoroutine(swapGamemodeCor(mode,false));
	}
	public void miniSave(int mode)
	{
		StartCoroutine(swapGamemodeCor(mode,true));
	}
	IEnumerator swapGamemodeCor(int mode,bool miniSave)
	{
		string targetSaveName = "Save";
		if(!miniSave)
		{
			int root = dataS.saveFileID-(3*dataS.mode);
			dataS.resetValues();
			cam.fadeScreen(true);
			if(fCor!=null)StopCoroutine(fCor);
			fCor = StartCoroutine(fadeMusicCor(false));
			yield return new WaitUntil(()=>cam.fadeAnim>=1);
			StartCoroutine(dataS.saveData(true));
			yield return new WaitUntil(()=>!dataS.saving);
			//switch to another save file here
			print("Swapping save to mode "+mode+" File ID: "+dataS.saveFileID);
			int offset = 0,targOffset = 0;
			//get offset of current mode
			switch(dataS.mode)
			{
				default: break;
				case 1: offset = 3; break;
				case 2: offset = 6; break;
			}
			int mainID = dataS.saveFileID-offset;
			//get offset of target mode
			switch(mode)
			{
				default: break;
				case 1: targOffset = 3; break;
				case 2: targOffset = 6; break;
			}
			dataS.saveFileID = targOffset+mainID;
			targetSaveName+=(targOffset+mainID+1).ToString();
			//print("Target save name: "+targetSaveName);
			//print("Root save ID: "+root+" Save file ID: "+dataS.saveFileID+" Mode: "+dataS.mode);
			string l = "";
			string lastUsedOld = "000";
			if(PlayerPrefs.HasKey("LastUsedSaves"))
			lastUsedOld = PlayerPrefs.GetString("LastUsedSaves");
			for(int i = 0;i<lastUsedOld.Length;i++)
			{
				if(i!=root)
				l+=lastUsedOld[i];
				else l+=mode;
			}
			PlayerPrefs.SetString("LastUsedSaves",l);
			//print("Last used: "+l);
		}
		else
		{
			targetSaveName+=(dataS.saveFileID+1).ToString();
			//print("Current save name: "+targetSaveName);
		}
		List<string> textAsLines = new List<string>();
		List<int> saveArr = new List<int>();
		List<string> levelArr = new List<string>();
		string celStr = "";
		string s = "";
		bool newFile = false;

		//create new save
		if(!PlayerPrefs.HasKey(targetSaveName))
		{
			//print("Creating save");
			TextAsset emptyFile = (TextAsset)Resources.Load("Text/dummysavefile",typeof (TextAsset));
			s = emptyFile.ToString();
			newFile = true;

		}
		else
		{
			//print("Reading save");
			s = PlayerPrefs.GetString(targetSaveName);
			//print(s);
		}
		//read string
		int fileProgression=0,saveProgress=0,levelProgress=0;
		textAsLines.AddRange(s.Split("\n"[0]));
		string curLine = "";
		for(int allLines = 0;allLines<textAsLines.Count;allLines++)
		{
			string currentTextLine = textAsLines[allLines];
			//Debug.Log(currentTextLine);
			if(currentTextLine.StartsWith(";1")&&fileProgression==0)
			fileProgression = 1;
			else if(currentTextLine.StartsWith(";2")&&fileProgression==1)
			{
				fileProgression = 2;
				continue;
			}
			else if(currentTextLine.StartsWith(";3")&&fileProgression==2)
			{
				fileProgression = 3;
				continue;
			}
			//read current line
			for(int i = 0; i<currentTextLine.Length;i++)
			{
				//Debug.Log(currentTextLine[i]+", "+System.Convert.ToInt32(currentTextLine[i]));
				//characters that end line reading
				if(System.Convert.ToInt32(currentTextLine[i])==13
				||System.Convert.ToInt32(currentTextLine[i])==61
				||System.Convert.ToInt32(currentTextLine[i])==47)
				{
					//Debug.Log(fileProgression+": "+curLine);
					switch(fileProgression)
					{
						default: //Debug.Log(fileProgression+": "+curLine);
						break;
						case 1:
						if(curLine!="")
						{
							int val = 0;
							int.TryParse(curLine, out val);
							saveArr.Add(val);
							saveProgress++;
							curLine = "";
						}
						break;
						case 2:
						levelProgress++;
						if(curLine!="")
						{
							//Debug.Log(curLine);
							levelArr.Add(curLine);
							curLine = "";
						}
						break;
						case 3:
							celStr = curLine;
							curLine = "";
						break;
					}
					break;
				}
				else 
				{
					curLine+=currentTextLine[i];
				}
			}
		}
		//print("Cel str: "+celStr);
		//write to dataShare
		if(!miniSave)
		{
			try
			{
				dataS.currentWorld = saveArr[1];
				dataShare.totalCompletedLevels = saveArr[2];
				dataS.score = saveArr[3];
				dataS.lives = saveArr[4];
				dataS.coins = saveArr[5];
				dataS.sausages = saveArr[6];
				dataS.floppies = saveArr[7];
				dataS.playerState = saveArr[8];
				dataS.storedItem = saveArr[9];
				dataS.lastLoadedLevel = saveArr[10];
				dataS.lastLoadedWorld = saveArr[11];
				dataS.worldProgression = saveArr[12];
				dataS.AndreMissionProgress = saveArr[13];
				dataS.DjoleMissionProgress = saveArr[14];
				dataS.MiroslavMissionProgress = saveArr[15];
				dataS.mode = mode;
				dataS.infiniteLives = saveArr[17];
				dataS.difficulty = saveArr[18];
				for(int i = 0; i<levelArr.Count;i++)
				{
					if(i<dataS.levelProgress.Length)
					dataS.levelProgress[i] = levelArr[i];
				}
				//load cell data
				dataS.cellData = new bool[62];
				if(celStr!="")
				{
					for(int i = 0; i<celStr.Length;i++)
					{
						if(celStr[i]=='1')
						dataS.cellData[i]=true;
					}
				}
			}
			catch(System.IndexOutOfRangeException ex)
			{
				Debug.LogError(ex+ " file corrupt");
				saveArr = new List<int>();
				dataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
			}
			if(newFile)
			{
				StartCoroutine(dataS.saveData(true));
				yield return new WaitUntil(()=>!dataS.saving);
			}
			dataShare.allClear = false;
			dataS.resetValues();
			dataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		}
		else
		{
			//print("Mini saving");
			if(saveArr.Count==16)
			{
				saveArr.Add(0);
				saveArr.Add(0);
				saveArr.Add(0);
			}
			saveArr[16] = dataS.mode;
			saveArr[17] = dataS.infiniteLives;
			saveArr[18] = dataS.difficulty;

			//mini save data
			string save = "";
			save = ";1"+'/'+'\n';
			for(int i = 1;i<saveArr.Count;i++)
			{
				//print(saveArr[i].ToString());
				save+=saveArr[i].ToString()+'/'+'\n';
			}
			save+="="+'\n'+";2"+'/'+'\n';
			for(int i = 0;i<levelArr.Count;i++)
			{
				save+=levelArr[i]+'/'+'\n';
			}

			save+="="+'\n'+";3"+'/'+'\n'; //add cell data
			for(int i = 0;i<dataS.cellData.Length;i++)
			{
				if(dataS.cellData[i]==true)
				save+='1';
				else save+='0';
			}
			save+='\n'+"="+'/'+'\n'+"END OF SAVE FILE";
			//Debug.Log(save);
			PlayerPrefs.SetString(targetSaveName,save);
		}
	}
}
