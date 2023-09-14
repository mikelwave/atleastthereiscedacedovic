using UnityEngine;
using System.Collections.Generic;
public class SuperInput : MonoBehaviour {
	static InputReader inputReader;
	bool loaded = false;
	
	// Use this for initialization
	void Awake () {
		if(!loaded)
		{
		inputReader = RebindableData.GetInputReader();
		loaded = true;
		}
	}
	
	public static bool GetKey (string inputName)
	{
		List<RebindableKey> keyDatabase = inputReader.GetCurrentKeys ();
		List<DigiButton> usedDigiButtons = inputReader.GetCurrentDigiButtons();
		//search for key in digibuttons
		foreach (DigiButton button in usedDigiButtons)
		{
			if(button.inputName == inputName)
			{
				//if(button.pressed) print("Digibutton "+button.inputName+" pressed");
				if(button.DigiInput!="")
				return button.pressed;
				else return false;
			}
		}
		//print("button not found in digibuttons, searching in rebindable keys");
		//search for key in normal key inputs
		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				//if(Input.GetKey (key.input)) print("Key "+key.inputName+" pressed");
				return Input.GetKey (key.input);
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	public static string GetKeyName (string inputName)
	{
		List<RebindableKey> keyDatabase = inputReader.GetCurrentKeys ();
		List<DigiButton> usedDigiButtons = inputReader.GetCurrentDigiButtons();
		//search for key in digibuttons
		foreach (DigiButton button in usedDigiButtons)
		{
			if(button.inputName == inputName)
			{
				if(button.DigiInput!="")
				return button.DigiInput.ToString();
				else return "None";
			}
		}
		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				return key.input.ToString();
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	
	public static bool GetKeyDown (string inputName)
	{
		List<RebindableKey> keyDatabase = inputReader.GetCurrentKeys ();
		List<DigiButton> usedDigiButtons = inputReader.GetCurrentDigiButtons();
		//search for key in digibuttons
		foreach (DigiButton button in usedDigiButtons)
		{
			if(button.inputName == inputName)
			{
				if(button.DigiInput!="")
				return button.down;
				else return false;
			}
		}
		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				return Input.GetKeyDown (key.input);
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	
	public static bool GetKeyUp (string inputName)
	{
		List<RebindableKey> keyDatabase = inputReader.GetCurrentKeys ();
		List<DigiButton> usedDigiButtons = inputReader.GetCurrentDigiButtons();
		//search for key in digibuttons
		foreach (DigiButton button in usedDigiButtons)
		{
			if(button.inputName == inputName)
			{
				if(button.DigiInput!="")
				return button.up;
				else return false;
			}
		}
		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				return Input.GetKeyUp (key.input);
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	
	/*public static int GetAxis (string axisName)
	{
		List<RebindableAxis> axisDatabase = rebindableManager.GetCurrentAxes ();
		
		foreach (RebindableAxis axis in axisDatabase)
		{
			if (axis.axisName == axisName)
			{
				bool posPressed = Input.GetKey (axis.axisPos);
				bool negPressed = Input.GetKey (axis.axisNeg);
				
				return 0 + (posPressed ? 1 : 0) - (negPressed ? 1 : 0);
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable axis '" + axisName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	} */
	
	public static KeyCode GetKeyFromBinding (string inputName)
	{
		List<RebindableKey> keyDatabase = inputReader.GetCurrentKeys ();

		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				return key.input;
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
}
