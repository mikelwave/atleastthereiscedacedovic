using UnityEngine;
using System.Collections;
public class killOnEnd : MonoBehaviour {
	public GameObject prefab;
	public Color32 color = new Color32(255,255,255,255);
	public bool assignToParent = false;
	public float zOffset = 0;
	public string nameToSet = "";
	public void kill () {
	StartCoroutine(destroy());
	}
	public void killNSpawn ()
	{
	//print("Prefab name: "+prefab.name);
	GameObject obj = Instantiate(prefab,transform.position+new Vector3(0,0,zOffset),Quaternion.Euler(transform.eulerAngles));
	if(nameToSet!="")
	{
		obj.name=nameToSet;
	}
	if(assignToParent)
	obj.transform.SetParent(transform.parent.parent);
	SpriteRenderer rd = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
	if(rd!=null)
	{
		rd.color = color;
		rd.enabled = true;
	}
	StartCoroutine(destroy());	
	}
	IEnumerator destroy()
	{
		yield return 0;
		yield return 0;
		Destroy(transform.parent.gameObject);
	}
}
