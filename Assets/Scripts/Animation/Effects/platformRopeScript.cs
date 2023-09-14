using UnityEngine;

// Script for a retractable rope effect on moving platforms
public class platformRopeScript : MonoBehaviour
{
	movingPlatformScript pScript; // Main script
	SpriteRenderer render; // Sprite renderer
	Transform platform; // Platform transform
	Vector3 startLocalPos; // Start local start position
	Vector3 startPlatformlocalPos; // Start local platform position
	Vector2 startSize; // Start local rope size

	float platformOffset = 0;

	// Start is called before the first frame update
	void Start ()
	{
		pScript = transform.parent.parent.GetComponent<movingPlatformScript>();
		platform = transform.parent;
		render = GetComponent<SpriteRenderer>();
		startPlatformlocalPos = platform.localPosition;
		startLocalPos = transform.localPosition;
		startSize = render.size;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(pScript.moving)
		{
			// Calculate platform offset from main point;
			platformOffset = platform.localPosition.y-startPlatformlocalPos.y;

			// Divide it by 2 to set the size of spriterenderer
			render.size = new Vector2(startSize.x,startSize.y-platformOffset);

			// Set the transform local position
			transform.localPosition = new Vector3(startLocalPos.x,startLocalPos.y-platformOffset/2,startLocalPos.z);
		}
	}
}
