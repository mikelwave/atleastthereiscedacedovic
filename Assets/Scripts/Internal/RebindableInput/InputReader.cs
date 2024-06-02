using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputReader : MonoBehaviour {
	public int controllerType = 0; //0 - keyboard, 1 - xbox, 2 - switch, 3 - ps4
	public static int pluggedController = 0; //0 - none, 1 - xbox, 2 - switch, 3 - ps4
	[SerializeField]
	public List<string> Actions;
	public List <string> inputButtons;

	[Space]
	[Header("KEYBOARD DEFAULT BINDS")]
	public List <RebindableKey> keyInputsKeyboard;

	[Space]
	[Header("XBOX DEFAULT BINDS")]
	public List <RebindableKey> keyInputsXbox360;
	public List<DigiButton> digibuttonsXbox360;
	[Space]
	[Header("SWITCH PRO DEFAULT BINDS")]
	public List <RebindableKey> keyInputsSwitchPro;
	public List<DigiButton> digibuttonsSwitchPro;
	[Space]
	[Header("PS4 DEFAULT BINDS")]
	public List <RebindableKey> keyInputsPS4;
	public List<DigiButton> digibuttonsPS4;

	[Space]
	//key inputs used ingame
	List <RebindableKey> usedKeyInputs;
	List<DigiButton> usedDigiButtons;
	//lists used for rebinding
	public List <RebindableKey> tmpKeyInputs;
	public List<DigiButton> tmpDigiButtons;
	//saved key inputs
	//key inputs used ingame
	List <RebindableKey> savedKeyInputs;
	List<DigiButton> savedDigiButtons;
	static InputReader instance;
	KeyCode foundInputkey = KeyCode.None;
	string foundDigiButtonInput = "";
	//float JoyXL,JoyXR,JoyYL,JoyYR,Joy4L,Joy4R,Joy5L,Joy5R,Joy6L,Joy6R,Joy7L,Joy7R,JoyRT,JoyLT;
	public static bool lookingForKey = false;
	public static string[] inputStrings;
	string[] joystickNames;
	public static bool foundController = false;
	//Debug
	public float JoyXL,JoyYL,Joy6L,Joy7L,JoyLT,JoyXR,JoyYR,Joy6R,Joy7R,JoyRT,Joy4L_xbox,Joy4R_xbox,Joy5L_xbox,Joy5R_xbox,Joy4L_ps,Joy5L_ps,Joy4R_ps,Joy5R_ps;
	public bool debug = false;
	void Awake()
	{
		if(instance==null)
		{
			instance = this;
			if(joystickNames==null)
			loadJoysticks();
			//print(instance.gameObject+" "+instance.GetInstanceID());
		}
	}
	void loadJoysticks()
	{
		joystickNames = Input.GetJoystickNames();
		if(joystickNames.Length!=0&&joystickNames[0].Length!=0)
		foundController = true;

		//if(foundController) Debug.Log("Controller(s) detected");
	}
	public void Start()
	{
		if(usedKeyInputs==null)
		changeKeys(controllerType,true);
	}
	void loadnewKeys()
	{
		if(usedKeyInputs!=null)
		{
		usedKeyInputs.Clear();
		}
		if(usedDigiButtons!=null)
		usedDigiButtons.Clear();

		usedKeyInputs = new List<RebindableKey>(savedKeyInputs);
		
		if(savedDigiButtons!=null)
		usedDigiButtons = new List<DigiButton>(savedDigiButtons);
	}
	public void getUsedToTmp()
	{
		if(tmpDigiButtons!=null)
			tmpDigiButtons.Clear();
		if(tmpKeyInputs!=null)
			tmpKeyInputs.Clear();
		
		tmpDigiButtons = CopyDigiButtonList(usedDigiButtons);
		tmpKeyInputs = CopyKeyList(usedKeyInputs);
	}
	public void setUsedFromTmp()
	{
		if(usedDigiButtons!=null)
			usedDigiButtons.Clear();
		if(usedKeyInputs!=null)
			usedKeyInputs.Clear();
		
		usedDigiButtons = CopyDigiButtonList(tmpDigiButtons);
		usedKeyInputs = CopyKeyList(tmpKeyInputs);
		//Debug.Log("New inputs registered");
	}
	public int getActionsCount()
	{
		return Actions.Count-1;
	}
	public void changeKeys(int i, bool loadDefaults)
	{
		if(inputStrings==null) inputStrings = new string[Actions.Count-1];
		//print("Changing keys: "+i);
		if(joystickNames==null)
		{
			loadJoysticks();
		}
		bool noValidPads = true;
		for(int z = 0; z<joystickNames.Length;z++)
		{
			//print(joystickNames[z]);
			if(joystickNames[z].Length!=0)
			{
				noValidPads = false;
				controllerType = i;	
				break;
			}
		}
		if(i!=5)
		{
			if(i!=0)
			{
				if(noValidPads)
				controllerType = 0;
			}
			else controllerType = 0;
			if(loadDefaults) UpdateUsedKeys();
		}
		//print(i);
		loadnewKeys();
		validateControllerInput();
		detectInputType();
	}
	public void registerInputsFromFile(string[] arr)
	{
		savedKeyInputs = new List<RebindableKey>();
		savedDigiButtons = new List<DigiButton>();
		for(int i = 0; i<arr.Length;i++)
		{
			//print("Checking "+arr[i]+" for input type");
			//check if input resembles keycode
			string s = arr[i].ToLower();
			if(s.Contains("joystick")||!s.Contains("joy"))
			{
				//parse to a keycode
				//print(arr[i]+" is keycode type");
				RebindableKey key = new RebindableKey();
				key.input = (KeyCode) System.Enum.Parse(typeof(KeyCode), arr[i]);
				key.inputName = Actions[i];
				//print("Key: "+key.inputName+" input: "+key.input);
				savedKeyInputs.Add(key);
			}
			else
			{
				//print(arr[i]+" is digibutton type");
				DigiButton key = new DigiButton();
				key.inputName = Actions[i];
				key.DigiInput = arr[i];
				savedDigiButtons.Add(key);
			}
		}
	}
	public static void rebindKey(int actionID)
	{
		instance.StartCoroutine(instance.findKey(actionID));
	}
	public static string GetActionFromID(int id)
	{
		return instance.Actions[id];
	}
	public void defaultKeys()
	{
		switch(instance.controllerType)
		{
			default:
			savedKeyInputs = CopyKeyList(keyInputsKeyboard);
			savedDigiButtons = null;
			break;
			case 1:
			savedKeyInputs = CopyKeyList(keyInputsXbox360);
			savedDigiButtons = CopyDigiButtonList(digibuttonsXbox360);
			break;
			case 2:
			savedKeyInputs = CopyKeyList(keyInputsSwitchPro);
			savedDigiButtons = CopyDigiButtonList(digibuttonsSwitchPro);
			break;
			case 3:
			savedKeyInputs = CopyKeyList(keyInputsPS4);
			savedDigiButtons = CopyDigiButtonList(digibuttonsPS4);
			break;
		}
		loadnewKeys();
		getUsedToTmp();
		detectInputType();
	}
	void UpdateUsedKeys()
	{
		print("Updating used keys");
		savedKeyInputs = LoadDefaultKeys ();
		savedDigiButtons = LoadDefaultDigiButtons ();
	}
	public static string joystickInterpreter(string toTranslate)
	{
		//print("To translate: "+toTranslate+" controller type: "+instance.controllerType);
		string output = "";
		int index = -1;
		switch(instance.controllerType)
		{
			default: output = toTranslate; break;
			//translate xbox keys
			case 1:
			switch(toTranslate)
			{
				default:
				index = -1;
				break;
				case "Button4":
				index = 0;
				break;
				case "Button5":
				index = 1;
				break;
				case "Joy6L":
				index = 3;
				break;
				case "Button0":
				index = 4;
				break;
				case "Button2":
				index = 5;
				break;
				case "Button8":
				index = 6;
				break;
				case "Joy7R":
				index = 7;
				break;
				case "Button3":
				index = 8;
				break;
				case "Button1":
				index = 9;
				break;
				case "Button9":
				index = 10;
				break;
				case "Joy6R":
				index = 11;
				break;
				case "Button6":
				index = 12;
				break;
				case "JoyLT":
				index = 13;
				break;
				case "JoyRT":
				index = 14;
				break;
				case "Joy9L":
				index = 13;
				break;
				case "Joy10L":
				index = 14;
				break;
				case "Joy7L":
				index = 15;
				break;
				case "JoyYR":
				index = 16;
				break;
				case "JoyXR":
				index = 17;
				break;
				case "Joy5R_xbox":
				index = 18;
				break;
				case "Joy4R_xbox":
				index = 19;
				break;
				case "Joy5R_ps":
				index = 22;
				break;
				case "Joy4R_ps":
				index = 19;
				break;
				case "JoyYL":
				index = 20;
				break;
				case "JoyXL":
				index = 21;
				break;
				case "Joy5L_xbox":
				index = 22;
				break;
				case "Joy4L_xbox":
				index = 23;
				break;
				case "Joy5L_ps":
				index = 22;
				break;
				case "Joy4L_ps":
				index = 23;
				break;
				case "Button7":
				index = 24;
				break;
			}
			if(index!=-1)
			output = "<sprite=\"buttons_xbox\" index="+index+">";
			else output = toTranslate;
			break;
			//translate switch keys
			case 2:
			switch(toTranslate)
			{
				default:
				index = -1;
				break;
				case "Button0":
				index = 4;
				break;
				case "Button1":
				index = 9;
				break;
				case "Button2":
				index = 25;
				break;
				case "Button3":
				index = 5;
				break;
				case "Button4":
				index = 8;
				break;
				//Button 5
				case "Button6":
				index = 13;
				break;
				case "Button7":
				index = 14;
				break;
				case "Button8":
				index = 0;
				break;
				case "Button9":
				index = 1;
				break;
				case "Button10":
				index = 24;
				break;
				case "Button11":
				index = 12;
				break;
				case "Button13":
				index = 6;
				break;
				case "Button14":
				index = 10;
				break;

				case "JoyLT":
				index = 23;
				break;
				case "JoyRT":
				index = 19;
				break;
				case "JoyYL":
				index = 20;
				break;
				case "JoyYR":
				index = 16;
				break;
				case "JoyXL":
				index = 21;
				break;
				case "JoyXR":
				index = 17;
				break;

				case "Joy5R_xbox":
				index = 3;
				break;
				case "Joy5R_ps":
				index = 11;
				break;

				case "Joy4L_xbox":
				index = 18;
				break;
				case "Joy4R_ps":
				index = 22;
				break;

				case "Joy6R":
				index = 7;
				break;
				case "Joy6L":
				index = 15;
				break;
			}
			if(index!=-1)
			output = "<sprite=\"buttons_switch\" index="+index+">";
			else output = toTranslate;
			//print(output);
			break;
			//translate ps4 keys
			case 3:
			switch(toTranslate)
			{
				default:
				index = -1;
				break;

				case "Joy8L":
				index = 15;
				break;
				case "JoyLT":
				index = 19;
				break;
				case "JoyRT":
				index = 23;
				break;
				case "Joy8R":
				index = 7;
				break;
				case "Joy7R":
				index = 11;
				break;
				case "Joy7L":
				index = 3;
				break;
				case "JoyXL":
				index = 21;
				break;
				case "JoyXR":
				index = 17;
				break;
				case "JoyYL":
				index = 20;
				break;
				case "JoyYR":
				index = 16;
				break;
				case "Joy3L":
				index = 23;
				break;
				case "Joy3R":
				index = 19;
				break;
				case "Joy6L":
				index = 18;
				break;
				case "Joy6R":
				index = 22;
				break;
				case "Joy4R":
				index = 13;
				break;
				case "Joy4L":
				index = 13;
				break;
				case "Joy5R":
				index = 14;
				break;
				case "Joy5L":
				index = 14;
				break;
				case "Button0":
				index = 4;
				break;
				case "Button1":
				index = 8;
				break;
				case "Button2":
				index = 9;
				break;
				case "Button3":
				index = 5;
				break;
				case "Button4":
				index = 0;
				break;
				case "Button5":
				index = 1;
				break;
				case "Button6":
				index = 13;
				break;
				case "Button7":
				index = 14;
				break;
				case "Button8":
				index = 12;
				break;
				case "Button9":
				index = 24;
				break;
				case "Button10":
				index = 6;
				break;
				case "Button11":
				index = 10;
				break;
			}
			if(index!=-1)
			output = "<sprite=\"buttons_ps4\" index="+index+">";
			else output = toTranslate;
			break;
		}

		return output;
	}
	public IEnumerator findKey(int actionID)
	{
		//Debug.Log("Looking for key");
		lookingForKey = true;
		yield return new WaitUntil(()=>foundInputkey!=KeyCode.None||foundDigiButtonInput!="");
		if(foundInputkey!=KeyCode.None)
		{
			//check if a dummy key is in digibuttons, remove it
			bool found = false;
			for(int i = 0;i<usedDigiButtons.Count;i++)
			{
				if(usedDigiButtons[i].inputName=="Dummy")
				{
					found = true;
					usedDigiButtons.Remove(usedDigiButtons[i]);
					//Add a "Dummy" to used key inputs
					usedKeyInputs.Add(new RebindableKey("Dummy",foundInputkey));
					break;
				}
			}
			//if a dummy wasn't found in the digibuttons, it must be in the keys
			if(!found)
			{
				for(int i = 0;i<usedKeyInputs.Count;i++)
				{
					if(usedKeyInputs[i].inputName=="Dummy")
					{
						found = true;
						usedKeyInputs[i].input=foundInputkey;
						break;
					}
				}
			}
			if(!found)
				usedKeyInputs.Add(new RebindableKey("Dummy",foundInputkey));	
			//Debug.Log("Dummy key is: "+foundInputkey);
		}
		else if(foundDigiButtonInput!="")
		{
			bool found = false;
			//check if a dummy digibutton is in keys, remove it
			for(int i = 0;i<usedKeyInputs.Count;i++)
			{
				if(usedKeyInputs[i].inputName=="Dummy")
				{
					found = true;
					usedKeyInputs.Remove(usedKeyInputs[i]);
					//Add a "Dummy" to used key inputs
					usedDigiButtons.Add(new DigiButton("Dummy",foundDigiButtonInput));
					break;
				}
			}
			if(!found)
			{
				for(int i = 0;i<usedDigiButtons.Count;i++)
				{
					if(usedDigiButtons[i].inputName=="Dummy")
					{
						found = true;
						usedDigiButtons[i].DigiInput = foundDigiButtonInput;
						break;
					}
				}
			}
			if(!found)
				usedDigiButtons.Add(new DigiButton("Dummy",foundDigiButtonInput));
			//Debug.Log("Dummy key is: "+foundDigiButtonInput);
		}
		else Debug.LogError("No acceptable input found, this shouldn't be possible.");
		lookingForKey = false;
		//Debug.Log("Input read");
		//RebindableKey noneAction = null;
		//DigiButton noneDigibutton = null;
		//unbind current key
		bool unbound = false;
		int ignoreActionIndex = 0;
		if(actionID==1||actionID==5) ignoreActionIndex = 1;
		else if(actionID==12) ignoreActionIndex = 2;
		//index = 0; don't ignore
		//index = 1; ignore action 12
		//index = 2; ignore actions 1,5
		for(int i = 0; i<tmpKeyInputs.Count;i++)
		{
			if(tmpKeyInputs[i].inputName==Actions[actionID])
			{
				unbound = true;
				tmpKeyInputs[i].input = KeyCode.None;
				break;
			}
		}
		if(tmpDigiButtons!=null&&!unbound)
			for(int i = 0; i<tmpDigiButtons.Count;i++)
			{
				if(tmpDigiButtons[i].inputName==Actions[actionID])
				{
					tmpDigiButtons[i].DigiInput="";
					break;
				}
			}
		//if inputted an inputkey
		if(foundInputkey!=KeyCode.None)
		{
			//check if the action was already bound to the input part
			for(int i = 0; i<tmpKeyInputs.Count;i++)
			{
				if(ignoreActionIndex==0
				||ignoreActionIndex==1 && i!=12
				||ignoreActionIndex==2 && i!=1 && i!=5)
				if(tmpKeyInputs[i].input==foundInputkey)
				{
					////noneAction = tmpKeyInputs[i];
					//found the key already bound in the array, set that to unbound
					////noneAction.input = KeyCode.None;
					tmpKeyInputs[i].input = KeyCode.None;
					break;
				}
			}
			//now that an existing key for this was unbound, or one wasn't found, check if the ACTION is bound to a digibutton (if list exists),
			//as we can't have it be there either
			if(tmpDigiButtons!=null)
			for(int i = 0; i<tmpDigiButtons.Count;i++)
			{
				if(tmpDigiButtons[i].inputName==Actions[actionID])
				{
						//found the element in the list, remove it.
						//Debug.Log("Removed element "+tmpDigiButtons[i].inputName);
						tmpDigiButtons.Remove(tmpDigiButtons[i]);
					break;
				}
			}
			//element removed, now find the action in the key input array
			for(int i = 0; i< tmpKeyInputs.Count;i++)
			{
				if(tmpKeyInputs[i].inputName==Actions[actionID])
				{
					//change the input
					tmpKeyInputs[i].input = foundInputkey;
					//Debug.Log("Rebound "+Actions[actionID]+" to "+foundInputkey);
					foundInputkey = KeyCode.None;
					break;
				}
			}
			//if action wasn't found add it to the list
			if(foundInputkey!=KeyCode.None)
			{
				//print(Actions.Count);
				tmpKeyInputs.Add(new RebindableKey(Actions[actionID],foundInputkey));
				//Debug.Log("Rebound "+Actions[actionID]+" to "+foundInputkey);
				foundInputkey = KeyCode.None;
			}

		}
		else if(foundInputkey==KeyCode.None&&foundDigiButtonInput!="")
		{
			//check if the action was already bound to the input part
			for(int i = 0; i<tmpDigiButtons.Count;i++)
			{
				if(tmpDigiButtons[i].DigiInput==foundDigiButtonInput)
				{
					//found the key already bound in the array, set that to unbound
					////noneDigibutton = tmpDigiButtons[i];
					////noneDigibutton.DigiInput = "";
					if(ignoreActionIndex==0
					||ignoreActionIndex==1 && i!=12
					||ignoreActionIndex==2 && i!=1 && i!=5)
					tmpDigiButtons[i].DigiInput = "";
					break;
				}
			}
			//check if action is bound to input key
			for(int i = 0; i<tmpKeyInputs.Count;i++)
			{
				if(tmpKeyInputs[i].inputName==Actions[actionID])
				{
						//found the element in the list, remove it.
						//Debug.Log("Removed element "+tmpKeyInputs[i].inputName);
						tmpKeyInputs.Remove(tmpKeyInputs[i]);
					break;
				}
			}
			//element removed, now find the action in the digibutton array
			for(int i = 0; i< tmpDigiButtons.Count;i++)
			{
				if(tmpDigiButtons[i].inputName==Actions[actionID])
				{
					//change the input
					tmpDigiButtons[i].DigiInput = foundDigiButtonInput;
					//Debug.Log("Rebound "+Actions[actionID]+" to "+foundDigiButtonInput);
					foundDigiButtonInput = "";
					break;
				}
			}
			//if action wasn't found add it to the list
			if(foundDigiButtonInput!="")
			{
				tmpDigiButtons.Add(new DigiButton(Actions[actionID],foundDigiButtonInput));
				//Debug.Log("Rebound "+Actions[actionID]+" to "+foundDigiButtonInput);
				foundDigiButtonInput = "";
			}
		}
		else
		{
			Debug.LogError("An input was detected but wasn't applicable");
		}
	}
	List<RebindableKey> LoadDefaultKeys ()
	{
		//string rebindPrefs = PlayerPrefs.GetString ("RebindableKeyPrefs", "");
		//if (rebindPrefs == "")
		//{
			List<RebindableKey> listToReturn;
			switch(controllerType)
			{
				default:
				listToReturn = keyInputsKeyboard;
				break;
				case 1:
				listToReturn = keyInputsXbox360;
				break;
				case 2:
				listToReturn = keyInputsSwitchPro;
				break;
				case 3:
				listToReturn = keyInputsPS4;
				break;
			}
			return CopyKeyList (listToReturn);
		//}
		/* else
		{
			string[] keybindPrefsSplit = rebindPrefs.Split ("\n".ToCharArray ());
			
			string[] keyNames = keybindPrefsSplit[0].Split ("*".ToCharArray ());
			string[] keyValues = keybindPrefsSplit[1].Split ("*".ToCharArray ());
			
			List <RebindableKey> keys = new List<RebindableKey> ();
			
			for (int i = 0; i < keyNames.Length; i++)
			{
				keys.Add (new RebindableKey(keyNames[i], (KeyCode)int.Parse (keyValues[i])));
			}
			
			return keys;
		}
		*/
	}
	List<DigiButton> LoadDefaultDigiButtons()
	{
		//todo actually load saved key inputs
		List<DigiButton> listToReturn;
		switch(controllerType)
		{
			default:
			listToReturn = null;
			break;
			case 1:
			listToReturn = digibuttonsXbox360;
			break;
			case 2:
			listToReturn = digibuttonsSwitchPro;
			break;
			case 3:
			listToReturn = digibuttonsPS4;
			break;
		}
		return CopyDigiButtonList (listToReturn);
	}
	List<RebindableKey> CopyKeyList (List<RebindableKey> listToCopy)
	{
		List<RebindableKey> listToReturn = new List<RebindableKey> (listToCopy.Count);
		
		foreach (RebindableKey key in listToCopy)
		{
			listToReturn.Add (new RebindableKey(key.inputName, key.input));
		}
		
		return listToReturn;
	}
	List<DigiButton> CopyDigiButtonList (List<DigiButton> listToCopy)
	{
		List<DigiButton> listToReturn;
		if(listToCopy!=null)
		{
			listToReturn = new List<DigiButton> (listToCopy.Count);
			foreach (DigiButton button in listToCopy)
			{
				listToReturn.Add (new DigiButton(button.inputName, button.DigiInput));
			}
		}
		else
		{
			listToReturn = new List<DigiButton>();
		}
		return listToReturn;
	}
	void validateControllerInput()
	{
		if(usedDigiButtons.Count!=0&&!foundController)
		{
			Debug.Log("Controller binds detected, but no controller found. Reseting inputs to keyboard default.");
			controllerType = 0;
			UpdateUsedKeys();
			loadnewKeys();
		}
	}
	public void detectInputType()
	{
		for(int i = 0; i<Actions.Count-1; i++)
		{
			bool found = false;
			//compare names of action name to input name
			//keys first
			for(int a = 0; a<usedKeyInputs.Count;a++)
			{
				if(Actions[i].ToLower()==usedKeyInputs[a].inputName.ToLower())
				{
					if(usedKeyInputs[a].input!=KeyCode.None)
					{
					//print(Actions[i]+" is key driven and is bound to "+usedKeyInputs[a].input);
					inputStrings[i] = usedKeyInputs[a].input.ToString();
					found = true;
					break;
					}
				}
			}
			if(found)continue;
			//if not found in keys, look in digibuttons
			else
			{
				for(int a = 0; a<usedDigiButtons.Count;a++)
				{
					if(Actions[i].ToLower()==usedDigiButtons[a].inputName.ToLower())
					{
						if(usedDigiButtons[a].DigiInput!=null)
						{
						//print(Actions[i]+" is axis driven and is bound to "+usedDigiButtons[a].DigiInput);
						inputStrings[i] = usedDigiButtons[a].DigiInput;
						found = true;
						break;
						}
					}
				}
			}
			if(found)continue;
			else
			{
				Debug.LogError("Action "+Actions[i]+" is not bound to a key");
			}
		}
	}
	void digiButtonInputs(DigiButton digibutton)
	{
		if(digibutton.down)
			digibutton.down = false;
		if(!digibutton.pressed)
		{
			digibutton.down = true;
			//print(digibutton.inputName+": Pressed down");
			digibutton.pressed = true;
		}
	}
	public List<DigiButton> GetCurrentDigiButtons ()
	{
		return usedDigiButtons;
	}
	public List<RebindableKey> GetCurrentKeys ()
	{
		return usedKeyInputs;
	}
	// Update is called once per frame
	void Update ()
	{
		//Debug
		if(debug)
		{
		/*JoyXL = Mathf.Clamp(Input.GetAxis("JoyXL"),-1,0);
		JoyYL = Mathf.Clamp(Input.GetAxis("JoyYL"),-1,0);
		Joy4L = Mathf.Clamp(Input.GetAxis("Joy4L"),-1,0);
		Joy5L = Mathf.Clamp(Input.GetAxis("Joy5L"),-1,0);
		Joy6L = Mathf.Clamp(Input.GetAxis("Joy6L"),-1,0);
		Joy7L = Mathf.Clamp(Input.GetAxis("Joy7L"),-1,0);
		JoyLT = Mathf.Clamp(Input.GetAxis("JoyLT"),-1,0);

		JoyXR = Mathf.Clamp(Input.GetAxis("JoyXR"),0,1f);
		JoyYR = Mathf.Clamp(Input.GetAxis("JoyYR"),0,1f);
		Joy4R = Mathf.Clamp(Input.GetAxis("Joy4R"),0,1f);
		Joy5R = Mathf.Clamp(Input.GetAxis("Joy5R"),0,1f);
		Joy6R = Mathf.Clamp(Input.GetAxis("Joy6R"),0,1f);
		Joy7R = Mathf.Clamp(Input.GetAxis("Joy7R"),0,1f);
		JoyRT = Mathf.Clamp(Input.GetAxis("JoyRT"),0,1f); */
		JoyXL = Input.GetAxis("JoyXL");
		JoyYL = Input.GetAxis("JoyYL");
		Joy6L = Input.GetAxis("Joy6L");
		Joy7L = Input.GetAxis("Joy7L");
		JoyLT = Input.GetAxis("JoyLT");

		JoyXR = Input.GetAxis("JoyXR");
		JoyYR = Input.GetAxis("JoyYR");
		Joy6R = Input.GetAxis("Joy6R");
		Joy7R = Input.GetAxis("Joy7R");
		JoyRT = Input.GetAxis("JoyRT");

		Joy4L_xbox = Input.GetAxis("Joy4L_xbox");
		Joy5L_xbox = Input.GetAxis("Joy5L_xbox");
		Joy4R_xbox = Input.GetAxis("Joy4R_xbox");
		Joy5R_xbox = Input.GetAxis("Joy5R_xbox");

		Joy4L_ps = Input.GetAxis("Joy4L_ps");
		Joy5L_ps = Input.GetAxis("Joy5L_ps");
		Joy4R_ps = Input.GetAxis("Joy4R_ps");
		Joy5R_ps = Input.GetAxis("Joy5R_ps");
		}
		

		if(lookingForKey)
		{
		 foreach(KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
		 {
             if(Input.GetKey(vKey))
			 {
				if(controllerType==0||controllerType==5)
				if(!vKey.ToString().Contains("Joy")&&!vKey.ToString().Contains("Mouse"))
				{
           			//print(vKey.ToString());
					foundInputkey = vKey;
				}
				if(controllerType!=0&&controllerType!=5)
				if(vKey.ToString().Contains("Joystick1"))
				{
           			//print(vKey.ToString());
					foundInputkey = vKey;
				}
             }
         }
		 if(controllerType!=3)
		 {
		 	for(int i = 0; i < inputButtons.Count;i++)
		 	{
			 	if(!inputButtons[i].Contains("ps")&&inputButtons[i].Contains("R")&&Input.GetAxis(inputButtons[i])==1
				 ||!inputButtons[i].Contains("switch")&&inputButtons[i].Contains("R")&&Input.GetAxis(inputButtons[i])==1)
			 	{
					//print(inputButtons[i]);
					foundDigiButtonInput = inputButtons[i];
			 	}
			 	else if(!inputButtons[i].Contains("ps")&&inputButtons[i].Contains("L")&&Input.GetAxis(inputButtons[i])==-1
				 ||!inputButtons[i].Contains("switch")&&inputButtons[i].Contains("L")&&Input.GetAxis(inputButtons[i])==-1)
				 {
					//print(inputButtons[i]);
					foundDigiButtonInput = inputButtons[i];
			 	}
			 }
		 }
		 else
		 {
			for(int i = 0; i < inputButtons.Count;i++)
		 	{
			 	if(!inputButtons[i].Contains("xbox")&&inputButtons[i].Contains("R")&&Input.GetAxis(inputButtons[i])==1)
			 	{
					//print(inputButtons[i]);
					foundDigiButtonInput = inputButtons[i];
			 	}
			 	else if(!inputButtons[i].Contains("xbox")&&inputButtons[i].Contains("L")&&Input.GetAxis(inputButtons[i])==-1)
				 {
					//print(inputButtons[i]);
					foundDigiButtonInput = inputButtons[i];
			 	}
			 }
		 }
		}
		 if(usedDigiButtons!=null)
		 {
			 //print(usedDigiButtons[0].inputName+": "+usedDigiButtons[0].DigiInput+" "+Input.GetAxis(usedDigiButtons[0].DigiInput));
		 	for(int i = 0; i < usedDigiButtons.Count;i++)
		 	{
				 DigiButton digibutton = usedDigiButtons[i];
				 if(digibutton.DigiInput!="")
				 {
				 	if(digibutton.DigiInput.Contains("R")&&Input.GetAxis(digibutton.DigiInput)==1)
				 	{
						//print(digibutton.inputName+" "+digibutton.DigiInput+" "+Input.GetAxis(digibutton.DigiInput));
						digiButtonInputs(digibutton);
				 	}
				 	if(digibutton.DigiInput.Contains("L")&&Input.GetAxis(digibutton.DigiInput)==-1)
				 	{
						//print(digibutton.inputName+" "+digibutton.DigiInput+" "+Input.GetAxis(digibutton.DigiInput));
						digiButtonInputs(digibutton);
				 	}
				 	if(Input.GetAxis(digibutton.DigiInput)==0
					||digibutton.DigiInput.Contains("R")&&Input.GetAxis(digibutton.DigiInput)==-1
					||digibutton.DigiInput.Contains("L")&&Input.GetAxis(digibutton.DigiInput)==1)
				 	{
					 	if(digibutton.up)
					 	digibutton.up = false;
					 	if(digibutton.pressed)
					 	{
							digibutton.pressed = false;
							digibutton.up = true;
							//print(digibutton.inputName+": Released");
					 	}
				 	}
				 }
				 else
				 {
					digibutton.pressed = false;
					digibutton.down = false;
					digibutton.up = false;
				 }
			 }
		 }
	}
}
