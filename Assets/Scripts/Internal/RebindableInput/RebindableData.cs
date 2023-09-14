using UnityEngine;
using System.Collections.Generic;

public class RebindableData : MonoBehaviour {
	
	public List<RebindableKey> defaultRebindableKeys;
	public List<RebindableAxis> defaultRebindableAxes;
	public List<RebindableKey> controllerDefaultRebindableKeys;
	
	List<RebindableKey> savedRebindableKeys;
	List<RebindableAxis> savedRebindableAxes;
	
	List<RebindableKey> rebindableKeys;
	List<RebindableAxis> rebindableAxes;
	
	void Awake ()
	{
		savedRebindableKeys = LoadSavedKeys ();
		savedRebindableAxes = LoadSavedAxes ();
		rebindableKeys = new List<RebindableKey>(savedRebindableKeys);
		rebindableAxes = new List<RebindableAxis>(savedRebindableAxes);
		DontDestroyOnLoad(this.gameObject);
	}
	
	public static RebindableData GetRebindableManager ()
	{
		RebindableData manage = GameObject.Find ("Rebindable Manager").GetComponent<RebindableData>();
		return (RebindableData)(manage);
	}
	public static InputReader GetInputReader ()
	{
		InputReader reader = GameObject.Find ("Rebindable Manager").GetComponent<InputReader>();
		return (InputReader)(reader);
	}
	
	List<RebindableKey> LoadSavedKeys ()
	{
		string rebindPrefs = PlayerPrefs.GetString ("RebindableKeyPrefs", "");
		
		
		if (rebindPrefs == "")
		{
			return CopyKeyList (defaultRebindableKeys);
		}
		else
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
	}
	
	List<RebindableAxis> LoadSavedAxes ()
	{
		
		string axisPrefs = PlayerPrefs.GetString ("RebindableAxisPrefs", "");
		
		if (axisPrefs == "")
		{
			return CopyAxisList (defaultRebindableAxes);
		}
		else
		{
			string[] axisPrefsSplit = axisPrefs.Split ("\n".ToCharArray ());
			
			string[] axisNames = axisPrefsSplit[0].Split ("*".ToCharArray ());
			string[] axisPoses = axisPrefsSplit[1].Split ("*".ToCharArray ());
			string[] axisNegss = axisPrefsSplit[2].Split ("*".ToCharArray ());
			
			List<RebindableAxis> axes = new List<RebindableAxis> ();
			
			for (int i = 0; i < axisNames.Length; i++)
			{
				axes.Add (new RebindableAxis(axisNames[i], (KeyCode)int.Parse (axisPoses[i]), (KeyCode)int.Parse (axisNegss[i])));
			}		
			
			return axes;
		}
	}
	
	public List<RebindableKey> GetCurrentKeys ()
	{
		return rebindableKeys;
	}
	
	public List<RebindableAxis> GetCurrentAxes ()
	{
		return rebindableAxes;
	}
	
	public void ActivateDefaultKeys ()
	{		
		rebindableKeys = CopyKeyList (defaultRebindableKeys);
	}
	
	public void ActivateDefaultAxes ()
	{		
		rebindableAxes = CopyAxisList (defaultRebindableAxes);
	}
	public void ActivateDefaultControllerKeys ()
	{		
		rebindableKeys = CopyKeyList (controllerDefaultRebindableKeys);
	}
	
	public void ActivateSavedKeys ()
	{		
		rebindableKeys = CopyKeyList (savedRebindableKeys);
	}
	
	public void ActivateSavedAxes ()
	{		
		rebindableAxes = CopyAxisList (savedRebindableAxes);
	}
	
	public void SaveKeys ()
	{
		string keyNames = "";
		string keyValues = "";
		
		savedRebindableKeys = new List<RebindableKey> (rebindableKeys);
		
		for (int i = 0; i < rebindableKeys.Count; i++)
		{
			if (i < rebindableKeys.Count - 1)
			{
				keyNames += rebindableKeys[i].inputName + "*";
				keyValues += ((int)rebindableKeys[i].input).ToString () + "*";
			}
			else
			{
				keyNames += rebindableKeys[i].inputName;
				keyValues += ((int)rebindableKeys[i].input).ToString ();
			}
		}
		
		string prefsToSave = keyNames + "\n" + keyValues;
		
		PlayerPrefs.SetString ("RebindableKeyPrefs", prefsToSave);
	}
	
	public void SaveAxes ()
	{
		string axisNames = "";
		string axisPoses = "";
		string axisNegss = "";
		
		savedRebindableAxes = new List<RebindableAxis> (rebindableAxes);
		
		for (int i = 0; i < rebindableAxes.Count; i++)
		{
			if (i < rebindableAxes.Count - 1)
			{
				axisNames += rebindableAxes[i].axisName + "*";
				axisPoses += ((int)rebindableAxes[i].axisPos).ToString () + "*";
				axisNegss += ((int)rebindableAxes[i].axisNeg).ToString () + "*";
			}
			else
			{
				axisNames += rebindableAxes[i].axisName;
				axisPoses += ((int)rebindableAxes[i].axisPos).ToString ();
				axisNegss += ((int)rebindableAxes[i].axisNeg).ToString ();
			}
		}
		
		string prefsToSave = axisNames + "\n" + axisPoses + "\n" + axisNegss;
		
		PlayerPrefs.SetString ("RebindableAxisPrefs", prefsToSave);
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
	
	List<RebindableAxis> CopyAxisList (List<RebindableAxis> listToCopy)
	{
		List<RebindableAxis> listToReturn = new List<RebindableAxis> (listToCopy.Count);
		
		foreach (RebindableAxis axis in listToCopy)
		{
			listToReturn.Add (new RebindableAxis(axis.axisName, axis.axisPos, axis.axisNeg));
		}
		
		return listToReturn;
	}
}