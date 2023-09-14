using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Color Correction (Ramp)")]
    public class ColorCorrectionRamp : ImageEffectBase {
        public Texture  textureRamp;
        [Range(-1.0f,1.0f)]
        public float    rampOffsetR;
        [Range(-1.0f,1.0f)]
        public float    rampOffsetG;
        [Range(-1.0f,1.0f)]
        public float    rampOffsetB;

        // Called by camera to apply image effect
        void OnRenderImage (RenderTexture source, RenderTexture destination) {
            material.SetTexture ("_RampTex", textureRamp);
            material.SetFloat("_RampOffsetR", rampOffsetR);
            material.SetFloat("_RampOffsetG", rampOffsetG);
            material.SetFloat("_RampOffsetB", rampOffsetB);
            Graphics.Blit (source, destination, material);
        }
    }
}
