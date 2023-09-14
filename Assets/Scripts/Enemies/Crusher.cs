using UnityEngine;

public class Crusher : MonoBehaviour
{
    public bool canGetCrushed = true,sendContactInfo = false;
    public float collisionDetectOffset = 0.05f,crusherMaxHeight = 0,crusherDownLength = 0.3f;
    public float horCrusherLength = 0.5f;
    public LayerMask whatIsSolidGround;
    LayerMask whatIsGround;
    GameData data;
    EnemyCorpseSpawner eneCorpse;
    bool loaded = false;
    public void assignValues(LayerMask w,EnemyCorpseSpawner e,GameData d)
    {
        whatIsGround = w;
        eneCorpse = e;
        data = d;
        loaded = true;
    }

    void crusher()
	{
		RaycastHit2D rayLeft = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.right,horCrusherLength,whatIsGround);
		RaycastHit2D rayRight = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),transform.right,horCrusherLength,whatIsGround);
		RaycastHit2D rayDown = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.up,crusherDownLength,whatIsGround);
		RaycastHit2D rayUp = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5+crusherMaxHeight,0),transform.up,0.5f,whatIsSolidGround);
		if(sendContactInfo)
		{
			Vector3 startPos = transform.position+new Vector3(0,collisionDetectOffset*5+crusherMaxHeight,0);
			Debug.DrawLine(startPos,startPos+(transform.up*0.5f),Color.red,0f);
			if(rayUp.collider!=null)
			print(gameObject.name+" Up: "+rayUp.transform.name);

			startPos = transform.position+new Vector3(0,collisionDetectOffset*5,0);
			Debug.DrawLine(startPos,startPos-(transform.up*crusherDownLength),Color.blue,0f);

			if(rayDown.collider!=null)
			print(gameObject.name+" Down: "+rayDown.transform.name);

		}
		if(rayLeft.collider!=null&rayRight.collider!=null&&rayLeft.collider.transform!=rayRight.collider.transform
		||rayUp.collider!=null&rayDown.collider!=null&&rayUp.collider.transform!=rayDown.collider.transform)
		{
			
			if(sendContactInfo)
			{
				print(transform.name+" crushed");
				if(rayUp.collider!=null)
				print(gameObject.name+" Up: "+rayUp.transform.name);
				if(rayDown.collider!=null)
				print(gameObject.name+" Down: "+rayDown.transform.name);
				if(rayLeft.collider!=null)
				print(gameObject.name+" Left: "+rayLeft.transform.name);
				if(rayRight.collider!=null)
				print(gameObject.name+" Right: "+rayRight.transform.name);
			}
			if(eneCorpse!=null)
			{
				data.playSoundOverWrite(20,transform.position);
				eneCorpse.createCorpseFlipped = true;
				eneCorpse.createCorpse = false;
				eneCorpse.spawnCorpse();
			}
			else
			{
				data.playSoundOverWrite(20,transform.position);
				Destroy(gameObject);
			}
		}
	}
    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0&&loaded&&canGetCrushed)
        {
            crusher();
        }
    }
}
