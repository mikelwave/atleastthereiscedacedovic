using System.Collections.Generic;
using UnityEngine;

public class bulletHellSpawner : MonoBehaviour
{
    GameObject bullet;
    public int pooledBullets = 10;
    public int bulletsToShoot = 5;
    public float angle = 45;
    List<bulletScript> bullets;
    public bool debug = false;
    // Start is called before the first frame update
    void Start()
    {
        bullets = new List<bulletScript>();
        bullet = transform.GetChild(0).gameObject;
        bullet.transform.SetParent(null);
        bullets.Add(bullet.GetComponent<bulletScript>());
        for(int i = 1;i<pooledBullets;i++)
        {
            GameObject obj = Instantiate(bullet,transform.position,Quaternion.identity);
            obj.transform.SetParent(null);
            bullets.Add(obj.GetComponent<bulletScript>());
        }
    }
    void Update()
    {
        if(debug)
        {
            debug = false;
            fire();
        }
    }
    public void fire()
    {
        float maxRot = angle/2, minRot = -maxRot, rotAdd = angle/(Mathf.Clamp(bulletsToShoot-1,0,bulletsToShoot));
        for(int bulletsFired = 0; bulletsFired<bulletsToShoot;bulletsFired++)
        {
            for(int i = 0; i<bullets.Count;i++)
            {
                if(!bullets[i].gameObject.activeInHierarchy)
                {
                    Transform tr = bullets[i].transform;
                    tr.position = transform.position;
                    tr.eulerAngles = new Vector3(0,0,transform.eulerAngles.z+Mathf.Clamp(minRot+(rotAdd*bulletsFired),minRot,maxRot));
                    //print(tr.eulerAngles);   
                    bullets[i].transform.GetChild(0).gameObject.SetActive(true);
                    bullets[i].Enable(true);
                    break;
                }
            }
        }
    }
    public void killAllActive()
    {
        for(int i = 0; i<bullets.Count;i++)
            {
                if(bullets[i].gameObject.activeInHierarchy)
                {
                    bullets[i].suicide();
                }
            }
    }
}
