using UnityEngine;
using System.Collections.Generic;
public class RebindableInput : MonoBehaviour {
	static RebindableData rebindableManager;
	bool loaded = false;
	
	// Use this for initialization
	void Awake () {
		if(!loaded)
		{
		loaded = true;
		rebindableManager = RebindableData.GetRebindableManager ();
		}
	}
	
	public static bool GetKey (string inputName)
	{
		List<RebindableKey> keyDatabase = rebindableManager.GetCurrentKeys ();
		
		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				return Input.GetKey (key.input);
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	public static string GetKeyName (string inputName)
	{
		List<RebindableKey> keyDatabase = rebindableManager.GetCurrentKeys ();
		
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
		List<RebindableKey> keyDatabase = rebindableManager.GetCurrentKeys ();
		
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
		List<RebindableKey> keyDatabase = rebindableManager.GetCurrentKeys ();
		
		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				return Input.GetKeyUp (key.input);
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	
	public static int GetAxis (string axisName)
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
	}
	
	public static KeyCode GetKeyFromBinding (string inputName)
	{
		List<RebindableKey> keyDatabase = rebindableManager.GetCurrentKeys ();
		
		foreach (RebindableKey key in keyDatabase)
		{
			if (key.inputName == inputName)
			{
				return key.input;
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	
	public static KeyCode GetPositiveFromAxis (string axisName)
	{
		List<RebindableAxis> axisDatabase = rebindableManager.GetCurrentAxes ();
		
		foreach (RebindableAxis axis in axisDatabase)
		{
			if (axis.axisName == axisName)
			{
				return axis.axisPos;
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable axis '" + axisName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
	
	public static KeyCode GetNegativeFromAxis (string axisName)
	{
		List<RebindableAxis> axisDatabase = rebindableManager.GetCurrentAxes ();
		
		foreach (RebindableAxis axis in axisDatabase)
		{
			if (axis.axisName == axisName)
			{
				return axis.axisNeg;
			}
		}
		
		throw new RebindableNotFoundException ("The rebindable axis '" + axisName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
	}
}
