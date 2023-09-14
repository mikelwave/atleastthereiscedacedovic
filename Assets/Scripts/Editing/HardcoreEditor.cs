using UnityEngine;
[ExecuteInEditMode]
public class HardcoreEditor : MonoBehaviour
{
	#if UNITY_EDITOR
	Transform tr;
	public int cableLength = 2;
	[Range(0,1)]
	public float guyStartPoint = 0.5f;
	public bool facingLeft = true;
	SpriteRenderer cableRender;
	float[] ranges = new float[2];
	public float edges = 0.5f;
	// Use this for initialization
	void OnEnable ()
	{
		if(!Application.isPlaying)
		{
		tr = transform.GetChild(0).transform;
		cableRender = transform.GetChild(1).GetComponent<SpriteRenderer>();
		remapRanges();
		}

	}
	void OnDisable()
	{
		if(!Application.isPlaying)
		{
			ranges[0] = 0;
			ranges[1] = 0;
		}
	}
	void remapRanges()
	{
		float cableHeight = transform.GetChild(1).GetComponent<SpriteRenderer>().size.y;
		ranges[0] = (cableHeight/2)-edges-0.25f;
		ranges[1] = -(cableHeight/2)+edges-0.5f;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(!Application.isPlaying)
		{
		if(facingLeft)tr.localScale = new Vector3(-1,1,1);
		else tr.localScale = Vector3.one;

		if(cableRender.size.y*2!=cableLength)
		{
			cableRender.size = new Vector2(0.063f,cableLength*2);
			remapRanges();
		}

		//if(tr.localPosition.y!=guyStartPoint)
		//{
		//	tr.localPosition = new Vector3(-0.125f*tr.localScale.x,Mathf.Round(guyStartPoint * 2f) * 0.5f,tr.localPosition.z);
		//}
		tr.localPosition = new Vector3(-0.125f*tr.localScale.x,ranges[1]+ranges[0]*(guyStartPoint*2),transform.localPosition.z);

		//tr.localPosition = new Vector3(-0.125f*tr.localScale.x,Mathf.Round(tr.localPosition.y * 2f) * 0.5f,tr.localPosition.z);
		}
	}
	#endif
}
