using UnityEngine;

public class enemyReplacer : MonoBehaviour
{
	EnemyCorpseSpawner eneC;
	public GameObject replacement;
	GameObject spawnedReplacement;
	// Use this for initialization
	void Start ()
	{
		eneC = GetComponent<EnemyCorpseSpawner>();
		if(replacement!=null)
		{
			spawnedReplacement = Instantiate(replacement,transform.position,Quaternion.identity);
			spawnedReplacement.transform.localScale = transform.localScale;
			spawnedReplacement.transform.SetParent(transform.parent);
			spawnedReplacement.SetActive(false);
		}
	}
	void spawnReplacement()
	{
		spawnedReplacement.transform.position = transform.position;
		SpriteRenderer r = GetComponent<SpriteRenderer>(),
		r2 = null;
		if(spawnedReplacement.transform.childCount!=0)r2 = spawnedReplacement.transform.GetChild(0).GetComponent<SpriteRenderer>();
		if(r!=null)
		r2.color = r.color;
		spawnedReplacement.transform.localScale = transform.localScale;
		spawnedReplacement.transform.eulerAngles = transform.eulerAngles;
		spawnedReplacement.SetActive(true);
		Destroy(gameObject);
	}
	// Update is called once per frame
	void FixedUpdate ()
	{
		if(eneC.stompFlag)
			{
				eneC.stompFlag = false;
				if(replacement!=null)
					spawnReplacement();
			}
	}
}
