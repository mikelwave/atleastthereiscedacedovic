using UnityEngine;

// Script for a plane background effect for the spartaman level
public class Plane : MonoBehaviour
{
    bool active = false;
    public int spawnChance = 1;
    public float speed = 10;
    int frame = 0;
    Transform cam; // Game camera transform

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        transform.position = new Vector3(-500,-500,60);
    }

    // Spawn function
    void spawn()
    {
        active = true;
        transform.position = new Vector3(cam.position.x+15,cam.position.y+Random.Range(-2,7.1f),60);
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0) // Only update position if time isn't paused
        {
            if(active)
            {
                // Move until outside camera view
                transform.position+=Vector3.left*Time.deltaTime*speed;
                if(transform.position.x<=cam.position.x-13)
                {
                    active = false;
                    transform.position = new Vector3(-500,-500,60);
                }
            }
            else // Every 60 frames have a chance to spawn
            {
                frame++;
                if(frame==60)
                {
                    if((int)Random.Range(0,spawnChance)==0)
                    {
                        spawn();
                    }
                    frame = 0;
                }
            }
        }
    }
}
