using UnityEngine;

public class nonMovingGroundSnap : MonoBehaviour {

	void OnCollisionEnter2D(Collision2D other)
	{
		if(other.contacts.Length!=0)
		for(int i =0;i<other.contacts.Length;i++)
		{
			//Debug.Log(other.transform.name+": "+" Contact point y: "+other.contacts[i].point.y+" Object point y: "+other.transform.position.y);
		if(Mathf.Round(other.contacts[i].point.y)<=Mathf.Round(other.transform.position.y))
		{
			if(other.transform.parent==null
			||other.transform.parent!=null&&other.transform.parent.name!="Player_main"&&other.transform.name!="floatyLady_enemy")
			{
				//Debug.Log(other.transform.name);
				other.transform.SetParent(transform.GetChild(0));
			}
			break;
		}
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		if(other.gameObject.tag == "Enemy"&&other.transform.parent.name!="Player_main")
			other.transform.SetParent(GameObject.Find("Enemies").transform);
		else
		{
			if(gameObject.activeInHierarchy&&other.transform.name=="Player_main")
			other.transform.SetParent(null);
		}
	}
}
