using UnityEngine;

// Script for applying offset to the main texture of a renderer's material that this script is attached to
public class staticDistortionOffset : MonoBehaviour
{
	Renderer rend; // Renderer
	public float scrollSpeed = 1f;
	float offset = 0f;
	public bool workInStoppedTime = false; // Whether to make the effect function while game is paused

	// Start is called before the first frame update
	void Start()
	{
        rend = GetComponent<Renderer>();
    }
	
	// Update is called once per frame
	void Update ()
	{
		if(!workInStoppedTime) offset+=0.01f*scrollSpeed*Time.timeScale;
		else offset+=0.01f*scrollSpeed;

		if(offset>=1f) offset = 0f;

		// Apply the offset to the material
		rend.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
	}
}
