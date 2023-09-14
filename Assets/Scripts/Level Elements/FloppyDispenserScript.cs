using System.Collections;
using UnityEngine;

public class FloppyDispenserScript : MonoBehaviour
{
    public GameObject toEject;
    Transform player;
    //bool visible = false;
    Coroutine cor;
    GameObject boundObject;
    GameData data;
    public Sprite[] normalSprites,closeSprites;
    SimpleAnim2 anim2;
    // Start is called before the first frame update
    void Start()
    {
       player = GameObject.Find("Player_main").transform;
       data = GameObject.Find("_GM").GetComponent<GameData>();
       anim2 = transform.parent.GetComponent<SimpleAnim2>();
    }

    // Update is called once per frame
    IEnumerator spawnCooldown()
    {
        yield return new WaitUntil(()=>boundObject==null);
        bool tooClose = true;
        while(tooClose)
        {
            //check for distance
            if(Mathf.Abs(transform.position.x-player.position.x)>5)
            {
                if(anim2.sprites[0]!=normalSprites[0])
                {
                    for(int i = 0; i<normalSprites.Length;i++)
                    anim2.sprites[i]=normalSprites[i];

                }
                //print("check 1 success");
                yield return new WaitForSeconds(1f);
                //print("check 2...");
                if(Mathf.Abs(transform.position.x-player.position.x)>5)
                {
                //print("success");
                tooClose = false;
                }
            }
            else if(anim2.sprites[0]!=closeSprites[0])
            {
                for(int i = 0; i<closeSprites.Length;i++)
                anim2.sprites[i]=closeSprites[i];

            }
            yield return 0;
        }
        Eject();
        yield return new WaitUntil(()=>transform.childCount==0||transform.GetChild(0).name.Contains("Explosive"));
        if(transform.childCount!=0)
        {
            boundObject = transform.GetChild(0).gameObject;
            boundObject.transform.GetChild(0).tag = "blockHoldable";
        }
        else Debug.LogError("No bound object could be found");
        cor = StartCoroutine(spawnCooldown());
    }
    void Eject()
    {
        data.playSound(70,transform.position);
        GameObject obj = Instantiate(toEject,transform.position+transform.up,Quaternion.identity);
        obj.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        obj.transform.tag = "Untagged";
        obj.transform.SetParent(transform);
    }
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			//visible = true;
            if(cor==null)
            {
                cor = StartCoroutine(spawnCooldown());
            //print(gameObject.name+" activated");
            }
		}
    }
    //void OnTriggerExit2D(Collider2D other)
	//{
		//if(other.name == "ObjectActivator")
		//{
		//	visible = false;
		//}
    //}
}
