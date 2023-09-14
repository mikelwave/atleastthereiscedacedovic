using UnityEngine;

public class murphyTrigger : MonoBehaviour
{
    MurphyBossScript mBoss;
    // Start is called before the first frame update
    void Start()
    {
        mBoss = GameObject.Find("Boss_murphy").GetComponent<MurphyBossScript>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name=="PlayerCollider"||other.name.Contains("FloppyExplosive"))
        {
            mBoss.trigger();
            Destroy(gameObject);
        }
    }
}
