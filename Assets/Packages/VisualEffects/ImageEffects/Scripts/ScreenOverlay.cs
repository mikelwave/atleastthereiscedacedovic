using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu ("Image Effects/Other/Screen Overlay")]
    public class ScreenOverlay : PostEffectsBase
	{   
        public bool grab = false;
	    public enum OverlayBlendMode
		{
            Additive = 0,
            ScreenBlend = 1,
            Multiply = 2,
            Overlay = 3,
            AlphaBlend = 4,
        }
        public OverlayBlendMode blendMode = OverlayBlendMode.Overlay;
        public RawImage screencap;
        public float intensity = 1.0f;
        public Texture2D texture = null;
        public Material screencapMaterial = null;

        public Shader overlayShader = null;
        private Material overlayMaterial = null;



        public override bool CheckResources ()
		{
            CheckSupport (false);

            overlayMaterial = CheckShaderAndCreateMaterial (overlayShader, overlayMaterial);

            if	(!isSupported)
                ReportAutoDisable ();
            return isSupported;
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
            if (CheckResources() == false)
			{
                Graphics.Blit (source, destination);
                return;
            }

            Vector4 UV_Transform = new  Vector4(1, 0, 0, 1);

			#if UNITY_WP8
	    	// WP8 has no OS support for rotating screen with device orientation,
	    	// so we do those transformations ourselves.
			if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
				UV_Transform = new Vector4(0, -1, 1, 0);
			}
			if (Screen.orientation == ScreenOrientation.LandscapeRight) {
				UV_Transform = new Vector4(0, 1, -1, 0);
			}
			if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
				UV_Transform = new Vector4(-1, 0, 0, -1);
			}
			#endif

            overlayMaterial.SetVector("_UV_Transform", UV_Transform);
            overlayMaterial.SetFloat ("_Intensity", intensity);
            overlayMaterial.SetTexture ("_Overlay", texture);
            Graphics.Blit (source, destination, overlayMaterial, (int) blendMode);
        }
        private void OnPostRender()
        {
            if (grab)
            {
                grab = false;
                Transform HUD = GameObject.Find("HUD_Canvas").transform;
                GameObject im = new GameObject();
                im.name = "GameOverScreencap";
                im.transform.SetParent(HUD);
                im.transform.localScale = Vector3.one;
                im.transform.localPosition = Vector3.zero;
                im.AddComponent<RawImage>();
                im.GetComponent<RectTransform>().sizeDelta = new Vector2(768,432);
                screencap = im.GetComponent<RawImage>();
                Texture2D textureScreencap = new Texture2D(768, 432, TextureFormat.RGB24, false);
                textureScreencap.ReadPixels(new Rect(0, 0, 768,432), 0, 0, false);
                textureScreencap.Apply();
                im.GetComponent<RawImage>().texture = textureScreencap;
                im.GetComponent<RawImage>().material = screencapMaterial;
            }
        }
    }
}
