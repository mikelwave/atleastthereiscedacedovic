/*
[Typewriter textboxes] VERSION 1.1
Made by MGZone, don't distribute without permission.
*/
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;

public class TextBox : MonoBehaviour
{
	public TextAsset TextFile;
	List<string> textAsLines;
	public int startLine = 0;
	int startLineTrue;
	string textAsString;
	int profileDataProgress = 0;
	string line,charName="";
	public bool restoreDefaultSpeeds = false;
	float letterPause = 0.026f;
	float comaPause = 0.15f;
	float dotPause = 0.3f;
	[Header("Text speeds [SLOW, NORMAL, FAST]")]
	public Vector3 letterSpeeds = new Vector3(0.15f,0.026f,0f);
	public Vector3 comaSpeeds = new Vector3(0.15f,0.15f,0.01f);
	public Vector3 dotSpeeds = new Vector3(0.3f,0.3f,0.026f);
	//Don't modify these.
	Vector3 defLetterSpeeds = new Vector3(0.15f,0.026f,0f);
	Vector3 defComaSpeeds = new Vector3(0.15f,0.15f,0.01f);
	Vector3 defDotSpeeds = new Vector3(0.3f,0.3f,0.026f);

	bool arrowActive = false;
	public bool canSkip = true;
	bool skipLine = false;
	bool displayAllText = false;
	bool musicPausedFromtext = false;
	TextMeshProUGUI uitext,nameText;
	PlayerScript pScript;
	AxisSimulator axis;
	string curLetter;
	public int eventInt = 0;
	//for options
	public bool optionsMode = false;
	TextMeshProUGUI[] optionStrings = new TextMeshProUGUI[4];
	GameObject options, OptionArrows;
	public int currentOption = 0;
	public bool optionSelectionConfirmed = false;
	public bool confirmOptionforOutside = false;
	public bool canPickOpton = false;
	public int option1StartLine,option2StartLine,option3StartLine,option4StartLine;
	int maxOption = 0;
	//end options
	int profileDataInt = 0;
	public GameData data;
	public dataShare dataS;
	Transform nameIndicator;
	GameObject nextArrow;
	bool buttonDown = false;
	public bool protagonistOnLeft = false;
	public bool inHub = false;
	MGCameraController cam;
	Animator anim;
	
	Coroutine cor;
	AudioClip usedSound;

	public Color32[] colors = new Color32[2];
	[Header("Sounds")]
	public AudioClip letter_type,letter_type_other,letter_type_angry,selection,special_sound,shake_Sound,text_type;
	public AudioClip[] poor4Voices = new AudioClip[2];
	AudioSource aSource;
	public Animator[] usedAnimators = new Animator[4];
	string colortag = "<color=#FFFFFF>";
	string wordInsert = "";
	public bool resumeInCutscene = false;
	bool oneShotMode = false;
	MenuScript pauseMenu;
	Coroutine waitDisplay;
	IEnumerator conversation()
	{
		pauseMenu.enabled = false;
		uitext.text = "";
		charName = "";
		nameText.text = "";
		data.timerGoesDown = false;
		anim.SetInteger("Value",0);
		if(Time.timeScale!=0)
			yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length+anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
		else
		{
			//working in stopped time;
			int frameWait = 0;
			while(frameWait<30)
			{
				frameWait++;
				yield return 0;
			}
		}
		string tag = "";
		if(displayAllText)displayAllText=false;
		for(int allLines = startLineTrue;allLines<textAsLines.Count;allLines++)
		{
			uitext.text = "";
			//read the line as it goes.
			profileDataProgress = 0;
			line = textAsLines[allLines];

			//if line has no characters, we assume it ended, so we stop here.
			if(line.Length==0||System.Convert.ToInt32(line[0])==13)
			{
				break;
			}
			for(int i = 0;i<line.Length;i++)
			{
				if(line[0]=='[' && profileDataProgress <= 0)
				{
					profileDataProgress = 1;
					charName="";
					nameText.text = "";
				}
				//if there's NO assigned profile data, just keep them as they are.
				else if (line[0]!='[' && profileDataProgress <= 3)
				{
					profileDataProgress = 5; uitext.text = "";

					aSource.PlayOneShot(usedSound);
				}
				else if(profileDataProgress == 1 && line[i]!=',')
				{
					switch(char.ToUpper(line[i]))
					{
						case '?': profileDataInt=-1; usedSound = null;
						break;
						case 'C':profileDataInt=0; usedSound = letter_type;
						break;
						default: profileDataInt=1; usedSound = letter_type_other;
						break;
						case '2': profileDataInt=2; usedSound = poor4Voices[0];
						break;
						case '3': profileDataInt=3; usedSound = poor4Voices[1];
						break;
						case '!': profileDataInt=4; usedSound = letter_type_other;
						break;
					}
					if(usedSound!=null)
						aSource.PlayOneShot(usedSound);
					if(profileDataInt!=-1) nameIndicator.gameObject.SetActive(true);
					else nameIndicator.gameObject.SetActive(false);

					profileDataProgress++;
					//skip the next coma.
					i++;
				}
				else if(profileDataProgress == 2)
				{
					if(line[i]!=']')
					{
						charName+=line[i];
					}
					else if(line[i]==']')
					{
						nameText.text = charName;
						profileDataProgress=3;
						i++;
						uitext.text = "";
					}
				}
				//get rid of spaces before the first letter.
				else if(profileDataProgress == 3)
				{
					if(line[i]!=' ')
					{
						profileDataProgress=5;
					}
				}
				//ended profile info reading. Read text now.
				if(profileDataProgress == 5)
				{
					if(wordInsert!="")
						{
							//line.Insert(0,wordInsert);
							//Debug.Log(line);
							uitext.text += colortag+wordInsert+"</color>";
							wordInsert = "";
						}
					switch(line[i])
					{
						default:
							curLetter = colortag+line[i]+"</color>";
							uitext.text+=curLetter;
							int letterASCII = System.Convert.ToInt32(line[i]);
							if(!displayAllText && letterASCII != 13)
							{
								if((int)line[i]!=13&&!skipLine)
								{
									if(!oneShotMode)
										aSource.Play();
									else aSource.PlayOneShot(aSource.clip);
								}
								if(profileDataInt!=-1)
								{
									if(profileDataInt!=4)
									usedAnimators[profileDataInt].SetBool("Talk",true);
								}
								//Debug.Log("Played sound "+"["+letterASCII+"]");
								if(Time.timeScale!=0)
								yield return new WaitForSeconds(letterPause);
								else
								{
									//working in stopped time;
									int frameWait = 0;
									int timeToWait = Mathf.RoundToInt(60*letterPause);
									while(frameWait<timeToWait)
									{
										frameWait++;
										yield return 0;
									}
								}
							}
						break;
						case ' ':
							curLetter = colortag+line[i]+"</color>";
							uitext.text+=curLetter;
							if(!displayAllText)
							{
								if(profileDataInt!=-1)
								{
									if(profileDataInt!=4)
									usedAnimators[profileDataInt].SetBool("Talk",false);
								}
							}
						break;
						case '?':
							curLetter = colortag+line[i]+"</color>";
							uitext.text+=curLetter;
							if(!displayAllText)
							{
								if(!skipLine)
								{
									if(!oneShotMode)
										aSource.Play();
									else aSource.PlayOneShot(aSource.clip);
								}
								if(profileDataInt!=-1)
								{
									if(profileDataInt!=4)
									usedAnimators[profileDataInt].SetBool("Talk",false);
								}
								if(Time.timeScale!=0)
								yield return new WaitForSeconds(dotPause);
								else
								{
									//working in stopped time;
									int frameWait = 0;
									int timeToWait = Mathf.RoundToInt(60*dotPause);
									while(frameWait<timeToWait)
									{
										frameWait++;
										yield return 0;
									}
								}
							}
						break;
					
						case '.':
							curLetter = colortag+line[i]+"</color>";
							uitext.text+=curLetter;
							if(!displayAllText)
							{
								if(!skipLine)
								{
									if(!oneShotMode)
										aSource.Play();
									else aSource.PlayOneShot(aSource.clip);
								}
								if(profileDataInt!=-1)
								{
									if(profileDataInt!=4)
									usedAnimators[profileDataInt].SetBool("Talk",false);
								}
								if(Time.timeScale!=0)
								yield return new WaitForSeconds(dotPause);
								else
								{
									//working in stopped time;
									int frameWait = 0;
									int timeToWait = Mathf.RoundToInt(60*dotPause);
									while(frameWait<timeToWait)
									{
										frameWait++;
										yield return 0;
									}
								}
							}
						break;

						case '!':
							curLetter = colortag+line[i]+"</color>";
							uitext.text+=curLetter;
							if(!displayAllText)
							{
								if(!skipLine)
								{
									if(!oneShotMode)
										aSource.Play();
									else aSource.PlayOneShot(aSource.clip);
								}
								if(profileDataInt!=-1)
								{
									if(profileDataInt!=4)
									usedAnimators[profileDataInt].SetBool("Talk",false);
								}
								if(Time.timeScale!=0)
								yield return new WaitForSeconds(dotPause);
								else
								{
									//working in stopped time;
									int frameWait = 0;
									int timeToWait = Mathf.RoundToInt(60*dotPause);
									while(frameWait<timeToWait)
									{
										frameWait++;
										yield return 0;
									}
								}
							}
						break;
						case ',':
							curLetter = colortag+line[i]+"</color>";
							uitext.text+=curLetter;
							if(!displayAllText)
							{
								if(!skipLine)
								{
									if(!oneShotMode)
										aSource.Play();
									else aSource.PlayOneShot(aSource.clip);
								}
								if(profileDataInt!=-1)
								{
									if(profileDataInt!=4)
									usedAnimators[profileDataInt].SetBool("Talk",false);
								}
								if(Time.timeScale!=0)
								yield return new WaitForSeconds(comaPause);
								else
								{
									//working in stopped time;
									int frameWait = 0;
									int timeToWait = Mathf.RoundToInt(60*comaPause);
									while(frameWait<timeToWait)
									{
										frameWait++;
										yield return 0;
									}
								}
							}
						break;

						case '<': profileDataProgress = 6; tag = "";
						break;	
					}
				}
				//tag mode.
				if(profileDataProgress == 6)
				{
					if(line[i] != '>' && line[i] != '<')
					tag+=line[i];
					else if(line[i] == '>')
					{
						//Debug.Log(tag.ToLower());
						StartCoroutine(readTag(tag));
						tag = "";
						yield return new WaitUntil(() => profileDataProgress == 5);
					}
				}
			}
			if(!optionsMode)
			{
				if(canSkip)
				{
					arrowActive = true;
					if(!skipLine)
						nextArrow.SetActive(arrowActive);
				}
				displayAllText = true;
				if(profileDataInt>=0)
				{
					if(profileDataInt!=4)
					usedAnimators[profileDataInt].SetBool("Talk",false);
				}
				yield return new WaitUntil(() => buttonDown || skipLine);
				//if(skipLine)
				//	Debug.Log("Line was skipped automatically");
				buttonDown = false;
				displayAllText = false;
				skipLine = false;
				arrowActive = false;
				if(waitDisplay!=null)StopCoroutine(waitDisplay);
				nextArrow.SetActive(arrowActive);
			}
			else if (optionsMode)
			{
				if(profileDataInt>=0)
				{
					if(profileDataInt!=4)
					usedAnimators[profileDataInt].SetBool("Talk",false);
				}
				buttonDown = false;
				displayAllText = false;
				skipLine = false;
				arrowActive = false;
				displayOptions(allLines+1);
				yield return new WaitUntil(() => !optionsMode);
				optionSelectionConfirmed = false;
				canPickOpton = false;
				switch(currentOption)
				{
					default: allLines=option1StartLine-2; break;
					case 1: allLines=option2StartLine-2; break;
					case 2: allLines=option3StartLine-2; break;
					case 3: allLines=option4StartLine-2; break;
				}
				for(int s = 0; s<maxOption;s++)
				{
					optionStrings[s].transform.parent.gameObject.SetActive(false);
				}
				options.SetActive(false);
				buttonDown = false;
				displayAllText = false;
				skipLine = false;
				arrowActive = false;
				nextArrow.SetActive(arrowActive);
			}
			//Go to next line
		}
		anim.SetInteger("Value",1);
		if(Time.timeScale!=0)
			yield return new WaitForSeconds(0.5f);
		else
		{
			//working in stopped time;
			int frameWait = 0;
			while(frameWait<30)
			{
				frameWait++;
				yield return 0;
			}
		}
		if(inHub)
		{
			Collider2D col = usedAnimators[1].transform.GetComponent<BoxCollider2D>();
			col.enabled = false;
			yield return 0;
			col.enabled = true;
		}
		gameObject.SetActive(false);
		NPCScript npc = usedAnimators[1].transform.GetComponent<NPCScript>();
		if(npc!=null)
		{
			npc.talking = false;
			npc.active = false;
		}
		if(Time.timeScale==0)
		{
			Time.timeScale=1;
			anim.updateMode = AnimatorUpdateMode.Normal;
		}
		if(!resumeInCutscene)
		{
			data.timerGoesDown = true;
			
			axis.acceptXInputs = true;
			axis.acceptYInputs = true;
			if(pScript!=null)
			{
				pScript.controllable = true;
				pScript.inCutscene = false;
			}
		}
		if(musicPausedFromtext)
		{
			musicPausedFromtext = false;
			data.pauseMusic(false);
		}
		//Debug.Log("Conversation ended.");
		pauseMenu.enabled = true;
	}
	void displayOptions(int Optionsline)
	{
		string line = textAsLines[Optionsline];
		string tempOption = "";
		int currentOptionFulfil = 0;
		for(int i = 0; i<line.Length;i++)
		{
			if(line[i]=='|'||i==line.Length-1)
			{
				optionStrings[currentOptionFulfil].text=tempOption;
				currentOptionFulfil++;
				tempOption="";
			}
			else
			{
				tempOption+=line[i];
			}
		}

		for(int s = 0; s<currentOptionFulfil;s++)
		{
			optionStrings[s].transform.parent.gameObject.SetActive(true);
		}
		for(int s = currentOptionFulfil; s<optionStrings.Length;s++)
		{
			optionStrings[s].transform.parent.gameObject.SetActive(false);
		}
		options.SetActive(true);
		maxOption = currentOptionFulfil-1;
		currentOption = 0;
		OptionArrows.transform.position = new Vector3(OptionArrows.transform.position.x,optionStrings[0].transform.parent.position.y,OptionArrows.transform.position.z);
		optionStrings[currentOption].color = colors[0];
		OptionArrows.SetActive(true);
		canPickOpton = true;
	}
	void Start()
	{
		options = transform.GetChild(3).gameObject;
		OptionArrows = options.transform.GetChild(4).gameObject;
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		for(int i = 0; i<optionStrings.Length;i++)
		{
			optionStrings[i] = options.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
		}
	}
	void Awake()
	{
		nextArrow = transform.GetChild(1).gameObject;
		pauseMenu = GameObject.Find("PauseCanvas").GetComponent<MenuScript>();
		uitext = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		anim = GetComponent<Animator>();
		usedSound = letter_type;
		aSource = GetComponent<AudioSource>();
		aSource.clip = text_type;
		pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		if(pScript!=null)
			axis = pScript.transform.GetComponent<AxisSimulator>();
		else axis = GameObject.Find("Player_main").GetComponent<AxisSimulator>();
		nameIndicator = transform.GetChild(2);
		nameText = nameIndicator.GetChild(0).GetComponent<TextMeshProUGUI>();
		nextArrow.SetActive(false);
	}
	void OnEnable()
	{
		if(Time.timeScale==0)
		{
			anim.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		if(pScript!=null)
		{
			pScript.controllable = false;
			pScript.inCutscene = true;
			pScript.rb.velocity = new Vector2(0,pScript.rb.velocity.y);
		}
		nameIndicator.gameObject.SetActive(true);
		resumeInCutscene = false;
		axis.acceptXInputs = false;
		axis.acceptYInputs = false;
		textAsString = TextFile.text;
		textAsLines = new List<string>();
		textAsLines.AddRange(textAsString.Split("\n"[0]));
		nextArrow.SetActive(false);
		displayAllText = false;
		skipLine = false;
		arrowActive = false;
		if(startLine>0)startLineTrue = startLine-1;
		else if(startLine<0)startLineTrue = 0;
		else startLineTrue = startLine;
		usedSound = letter_type;
		aSource.volume = 1f;
		anim.SetInteger("Value",0);
		if(cor !=null)
		StopCoroutine(cor);
		cor = StartCoroutine(conversation());
	}
	public void disableName()
	{
		nameIndicator.gameObject.SetActive(false);
	}
	public void skip()
	{
		skipLine = true;
	}
	void OnDisable()
	{
		eventInt = 0;
		optionSelectionConfirmed = false;
		optionsMode = false;
		canSkip = true;
	}
	void Update()
	{
		if(optionsMode && canPickOpton)
		{
			if(SuperInput.GetKeyDown("Jump") && !buttonDown)
			{
				//print("Option confirmed");
				buttonDown = true;
				optionSelectionConfirmed = true;
				confirmOptionforOutside = true;
				optionsMode=false;
				optionStrings[currentOption].color = colors[1];
			}
			if(axis.verAxis==0&&buttonDown)
			{
				if(!SuperInput.GetKey("Jump"))
				buttonDown = false;
			}
			else if(axis.verAxis==-1 && !buttonDown&&!optionSelectionConfirmed)
			{
				aSource.PlayOneShot(selection,1);
				optionStrings[currentOption].color = colors[1];
				buttonDown = true;
				if(currentOption>=maxOption)
				{
					currentOption = 0;
				}
				else currentOption++;
				optionStrings[currentOption].color = colors[0];
				OptionArrows.transform.position = new Vector3(OptionArrows.transform.position.x,optionStrings[currentOption].transform.parent.position.y,OptionArrows.transform.position.z);
			}
			else if(axis.verAxis==1 && !buttonDown&&!optionSelectionConfirmed)
			{
				aSource.PlayOneShot(selection,1);
				optionStrings[currentOption].color = colors[1];
				buttonDown = true;
				if(currentOption<=0)
				{
					currentOption = maxOption;
				}
				else currentOption--;
				optionStrings[currentOption].color = colors[0];
				OptionArrows.transform.position = new Vector3(OptionArrows.transform.position.x,optionStrings[currentOption].transform.parent.position.y,OptionArrows.transform.position.z);
			}
		}
		if(SuperInput.GetKeyDown("Jump") && !buttonDown && !optionsMode && canSkip)
		{
			buttonDown = true;
			if(!displayAllText)
			{
				if(waitDisplay==null)
				waitDisplay = StartCoroutine(waitForDisplayAll());
			}
		}
		if(SuperInput.GetKeyUp("Jump") && buttonDown)
		{
			buttonDown = false;
		}
		if(restoreDefaultSpeeds)
		{
			restoreDefaultSpeeds = false;
			restoreDefaults();
		}
	}
	[ExecuteInEditMode]
	void restoreDefaults()
	{
		letterSpeeds = defLetterSpeeds;
		comaSpeeds = defComaSpeeds;
		dotSpeeds = defDotSpeeds;
	}
	IEnumerator waitForDisplayAll()
	{
		yield return new WaitUntil(()=>profileDataProgress != 6);
		//print("displaying all text");
		buttonDown = false;
		displayAllText = true;
		waitDisplay = null;
	}
	public void resetShake()
	{
		anim.SetInteger("Value",-1);
	}
	void setSound()
	{
		if(aSource.volume == 1f)
		aSource.volume = 0f;
		else aSource.volume = 1f;
	}
	IEnumerator readTag(string tag)
	{
		bool commandUsed = false;
		//Wait for the same time as if a letter was being typed out.
		if((tag.ToLower()).Contains("fake type"))
		{
			if(!displayAllText)
			{
				//print(tag.Length);
				int typedChars = 1;
				if(tag.Length>10)
				{
					typedChars = int.Parse(tag.Substring(10));
					//print(tag.Substring(10));
				}
				if(Time.timeScale!=0)
				{
					while(typedChars!=0)
					{
						yield return new WaitForSeconds(letterPause);
						typedChars--;
					}
				}
				else
				{
					//working in stopped time;
					int frameWait = 0;
					int timeToWait = Mathf.RoundToInt(60*letterPause*typedChars);
					while(frameWait<timeToWait)
					{
						frameWait++;
						yield return 0;
					}
				}
			}
			commandUsed = true;
		}
		//structure: <shakecam 0.5 0.3>
		if((tag.ToLower()).Contains("shakecam"))
		{
			if(!displayAllText)
			{
				float mag = 0, shaketime = 0;
				if(tag.Length>9)
				{
					string values = tag.Substring(9),tmp = "";
					for(int i = 0;i<values.Length;i++)
					{
						if(values[i]==' ')
						{
							tmp = tmp.ToString(CultureInfo.InvariantCulture);
							//print(tmp+" Length: "+tmp.Length);
							mag = float.Parse(tmp,CultureInfo.InvariantCulture);
							//mag = float.Parse(tmp);
							tmp = "";
						}
						else
						{
							tmp+=values[i];
						}
					}
					tmp = tmp.ToString(CultureInfo.InvariantCulture);
					//print(tmp+" Length: "+tmp.Length);
					shaketime = float.Parse(tmp,CultureInfo.InvariantCulture);
					//print(mag+" "+shaketime);
					cam.shakeCamera(mag,shaketime);
				}
			}
			commandUsed = true;
		}
		if(!commandUsed)
		switch(tag.ToLower())
				{
				default: break;
					//Go into options mode.
					case "options":
						//Debug.Log("Displaying options.");
						optionsMode = true;
						break;
					//pause music
					case "pause music":
					musicPausedFromtext = true;
						data.pauseMusic(true);
						break;
					//unpause music
					case "unpause music":
					musicPausedFromtext = false;
						data.pauseMusic(false);
						break;
					//Shake the text box.
					case "shake":
						if(!displayAllText)
						{
							aSource.PlayOneShot(shake_Sound,1);
							anim.SetInteger("Value",2);
						}
						break;
					
					//Set text color to red.
					case "color red":
						colortag = "<color=#DD4B3E>";
						break;

					//Set text color to blue.
					case "color blue":
						colortag = "<color=#00B8FF>";
						break;

					//Set text color to white.
					case "color white":
						colortag = "<color=#FFFFFF>";
						break;

					//Add +1 to eventInt.
					case "event":
						eventInt++;
						break;

					//Fast Text
					case "fast text":
						letterPause = letterSpeeds.z;
						dotPause = dotSpeeds.z;
						comaPause = comaSpeeds.z;
						break;

					//Slow Text
					case "slow text":
						letterPause = letterSpeeds.x;
						dotPause = dotSpeeds.x;
						comaPause = comaSpeeds.x;
						break;

					//Normal Text
					case "normal text":
						letterPause = letterSpeeds.y;
						dotPause = dotSpeeds.y;
						comaPause = comaSpeeds.y;
						break;

					//Go to the next line without player input.
					case "skip line":
						skipLine = true;
						break;

					//Set the letter sound to be the normal sound.
					case "normal sound":
					//if(profileDataInt==0)
						aSource.clip = text_type;
					//if(profileDataInt==1)
						//usedSound = letter_type_other;
						break;
					//Set the letter sound to be the alt sound.
					case "alt sound":
					//print("alt sound");
						aSource.clip = letter_type_angry;
						break;
					//Set the letter sound to be the null.
					case "no sound":
						aSource.clip = null;
						break;
					//Play a sound.
					case "play special sound":
						if(!displayAllText)
						aSource.PlayOneShot(special_sound,1);
						break;
					//Add a new line in textbox.
					case "new line":
						uitext.text+='\n';
						break;
					//Toggle audio output
					case "toggle sound":
						setSound();
						break;
					case "cant skip":
						canSkip = false;
						if(waitDisplay!=null)StopCoroutine(waitDisplay);
						skipLine = false;
						displayAllText=false;
					break;
					case "can skip":
						canSkip = true;
					break;
					case "run key":
						wordInsert = SuperInput.GetKeyName("Run");
						if(wordInsert.Contains("Joy"))
						changeWord();
					break;
					case "jump key":
						wordInsert = SuperInput.GetKeyName("Jump");
						if(wordInsert.Contains("Joy"))
						changeWord();
					break;
					case "select key":
						wordInsert = SuperInput.GetKeyName("Select");
						if(wordInsert.Contains("Joy"))
						changeWord();
					break;
					case "get last level":
						wordInsert = "???";
						getLatestLevel(0);
					break;
					case "get last level -1":
						wordInsert = "???";
						getLatestLevel(1);
					break;
					case "skips left":
						wordInsert = "???";
						getSkips();
					break;
					case "hide box":
						anim.SetInteger("Value",3);
					break;
					case "show box":
						anim.SetInteger("Value",-1);
					break;
					case "keep cutscene":
						resumeInCutscene = true;
					break;
					//enable one shot sound
					case "one shot sound on":
					oneShotMode = true;
					break;
					//disable one shot sound
					case "one shot sound off":
					oneShotMode = false;
					break;
			}
		profileDataProgress = 5;
	}
	void changeWord()
	{
		for(int i = 0; i<wordInsert.Length;i++)
		{
			if(wordInsert[i]=='B')
			{
				wordInsert = wordInsert.Substring(i);
				//print(wordInsert);
				break;
			}
		}
		string oldWord = wordInsert;
		wordInsert = InputReader.joystickInterpreter(wordInsert);
		if(wordInsert==oldWord)
		{
			Debug.Log("Couldn't find icon...");
			for(int i = 0; i<wordInsert.Length-1;i++)
			{
				if(char.IsDigit(wordInsert[i+1]))
				{
					wordInsert = wordInsert.Substring(i+1);
					break;
				}
			}
			wordInsert = "Btn"+wordInsert;
			//print(wordInsert);
		}
	}
	void getLatestLevel(int sub)
	{
		int level = dataShare.totalCompletedLevels,worldID = 1,levelID = 1;
		//print("Latest level: "+level);
		//world 7
		if(level>=35)
		{
			worldID = 7;
			levelID = level-34;
		}
		//world 6
		else if(level>=29)
		{
			worldID = 6;
			levelID = level-28;
		}
		//world 5
		else if(level>=23)
		{
			worldID = 5;
			levelID = level-22;
		}
		//world 4
		else if(level>=17)
		{
			worldID = 4;
			levelID = level-16;
		}
		//world 3
		else if(level>=11)
		{
			worldID = 3;
			levelID = level-10;
		}
		//world 2
		else if(level>=5)
		{
			worldID = 2;
			levelID = level-4;
		}
		//world 1
		else levelID = level;
		wordInsert = worldID+"-"+(levelID-sub);
		//print("Translated: "+wordInsert);
	}
	void getSkips()
	{
		wordInsert = dataS.skipsRemaining().ToString();
	}
}