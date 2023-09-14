using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurphyDetonator : MonoBehaviour
{
    GameData data;
    public List<Vector2> floppySpawnPoints;
    public GameObject yellowFloppy;
    public bool debugActivate = false;
    [HideInInspector]
    public bool activated = false;
    List <GameObject> floppies;
    public AudioClip spawnDisk;

    // Start is called before the first frame update
    void Start()
    {
        data = GameObject.Find("_GM").GetComponent<GameData>();
        if(floppySpawnPoints==null)floppySpawnPoints = new List<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if(debugActivate)
        {
            debugActivate = false;
            activate();
        }
    }
    IEnumerator countDown()
    {
        floppies = new List<GameObject>();
        activated = true;
        int toSpawn = floppySpawnPoints.Count,spawned = 0;
        while(spawned<toSpawn)
        {
            for(int i = 0; i<floppySpawnPoints.Count;i++)
            {
                if((Random.value>=0.75f||i==floppySpawnPoints.Count-1)) //have a chance to not spawn but always spawn at least one
                {
                    GameObject obj = Instantiate(yellowFloppy,new Vector3(floppySpawnPoints[i].x+0.5f,floppySpawnPoints[i].y+0.5f,transform.position.z),Quaternion.identity);
                    floppies.Add(obj);
                    floppySpawnPoints.RemoveAt(i);
                    spawned++;
                    data.playUnlistedSoundPoint(spawnDisk,obj.transform.position);
                    yield return new WaitForSeconds(0.1f);
                    break;
                }
            }
            yield return 0;
        }
        yield return new WaitForSeconds(2f-(0.1f*toSpawn));
        data.playSoundStatic(69);
        yield return new WaitForSeconds(2f);
        data.createExplosions();
        data.playSoundStatic(68);
        foreach(GameObject floppy in floppies)
        {
            Destroy(floppy);
        }
        floppies.Clear();
        activated = false;
    }
    public void activate()
    {
        if(!activated)
        StartCoroutine(countDown());
    }
}
