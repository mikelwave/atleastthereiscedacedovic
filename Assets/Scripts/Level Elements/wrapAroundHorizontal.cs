using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wrapAroundHorizontal : MonoBehaviour
{
    List<int> cooldownList;
    public string[] enemyBlackList = {"cultist_enemy","alien_enemy","objectactivator","screennuke","enemy_corpse","enemy_flipped","mothman_enemy","spaceship_enemy","sprite"};
    IEnumerator addToList(int i)
    {
        //print("Added "+i+" to list.");
        cooldownList.Add(i);
        int waitFrames = 5;
        while(waitFrames>0)
        {
            waitFrames--;
            yield return 0;
        }
        cooldownList.Remove(i);
        //print("Removed "+i+" from list.");
    }
    Transform playerTr;
    PlayerScript pScript;
    public Vector2 range = new Vector2(72.5f,88);
    float lengthRange = 0;
    void Start()
    {
        cooldownList = new List<int>();
        playerTr = GameObject.Find("Player_main").transform;
        pScript = playerTr.GetComponent<PlayerScript>();
        lengthRange = range.y-range.x;
    }
    void Update()
    {
        if(!pScript.dead)
        {
            Vector3 ppos = playerTr.position;
            playerTr.position = new Vector3(Mathf.Repeat(ppos.x-range.x,lengthRange)+range.x,ppos.y,ppos.z);
        }
    }
    bool isBlackListed(string enemyName)
    {
        foreach (var e in enemyBlackList)
        {
            if(e.ToLower().Contains(enemyName))return true;
        }
        return false;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.parent==null||
        other.transform.parent!=null&&(!other.transform.parent.name.Contains("Enemy")&&!other.transform.parent.name.Contains("item")))
        {
            int id = other.transform.GetInstanceID();
            switch(other.name)
            {
                default:

                if(isBlackListed(other.name.ToLower()))
                return;

                if(!cooldownList.Contains(other.transform.GetInstanceID()))
                {
                    Vector3 o = other.transform.position;
                    ///print(other.name+" "+o);
                    o.x = o.x > ((range.x+range.y)/2) ? range.x : range.y;
                    other.transform.position = new Vector3(o.x,o.y,o.z);
                    
                    StartCoroutine(addToList(other.transform.GetInstanceID()));
                }
                break;
                case "PlayerCollider":
                if(!cooldownList.Contains(id))
                {
                    StartCoroutine(addToList(id));
                }
                break;
                case "Enemy_flipped(Clone)":
                case "Enemy_flipped":
                case "bone_sprite":
                case "ObjectActivator": break;
            }
        }
    }
}
