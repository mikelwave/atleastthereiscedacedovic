Shader "Metkis/2DSprite Outline Diffuse" 
  {
       Properties 
       {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineOffSet ("Outline OffSet", Float) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _Color("Outline Color", Color) = (0,0,0,0)
        _Color2 ("Tint Inner", Color) = (1,1,1,1)
        _Color3 ("Tint Overall", Color) = (1,1,1,1)


       }
       SubShader
       {
       	Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
        Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

        Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			uniform float4 _MainTex_TexelSize;

       	    float4 _Color2;
       	    float4 _Color3;
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color3;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			          float _OutlineOffSet;

			float _AlphaSplitEnabled;


			fixed4 SampleSpriteTexture (float2 uv)
			{
			
			   fixed4 TempColor = tex2D(_MainTex, uv +float2(-_MainTex_TexelSize.x * _OutlineOffSet ,0.0)) + tex2D(_MainTex, uv -float2(-_MainTex_TexelSize.x * _OutlineOffSet ,0.0));
               TempColor = TempColor + tex2D(_MainTex, uv  + float2(0.0,-_MainTex_TexelSize.y * _OutlineOffSet )) + tex2D(_MainTex, uv  - float2(0.0,-_MainTex_TexelSize.y * _OutlineOffSet ));
               if(TempColor.a >= 0.01){
                   TempColor.a = _Color.a;
               }
               fixed4 AlphaColor = fixed4(TempColor.a,TempColor.a,TempColor.a,TempColor.a);
               
               fixed4 mainColor = AlphaColor  * _Color.rgba;
               fixed4 addcolor = tex2D(_MainTex, uv) * _Color2.rgba;
    
               if(addcolor.a > 0){
                   mainColor = addcolor;
               }
     
  
               
				fixed4 Albedo = mainColor.rgba * _Color3.rgba;
				fixed4 Alpha = mainColor.a * _Color3.a;
				Albedo.a = Alpha;
				
				
				if (_AlphaSplitEnabled)
					Albedo.a = tex2D (_AlphaTex, uv).r;

				return Albedo;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
           CGPROGRAM
           #pragma surface surf Lambert alpha nofog
    
           struct Input 
           {
               float2 uv_MainTex;
               float4 _MainTex_TexelSize ;
               fixed4 color : COLOR;
           };
    
          sampler2D _MainTex;
          float _OutlineOffSet;
          float4 _Color;
       	  float4 _Color2;
       	  float4 _Color3;

       	  uniform float4 _MainTex_TexelSize;
       	  
           void surf(Input IN, inout SurfaceOutput o)
           {
               fixed4 TempColor = tex2D(_MainTex, IN.uv_MainTex+float2(-_MainTex_TexelSize.x * _OutlineOffSet ,0.0)) + tex2D(_MainTex, IN.uv_MainTex-float2(-_MainTex_TexelSize.x * _OutlineOffSet ,0.0));
               TempColor = TempColor + tex2D(_MainTex, IN.uv_MainTex + float2(0.0,-_MainTex_TexelSize.y * _OutlineOffSet )) + tex2D(_MainTex, IN.uv_MainTex - float2(0.0,-_MainTex_TexelSize.y * _OutlineOffSet ));
               if(TempColor.a > 0.1){
                   TempColor.a = 1;
               }
               fixed4 AlphaColor = fixed4(TempColor.r,TempColor.g,TempColor.b,TempColor.a);
               fixed4 mainColor = AlphaColor  * _Color.rgba;
               fixed4 addcolor = tex2D(_MainTex, IN.uv_MainTex) * IN.color * _Color2.rgba;
    
               if(addcolor.a > 0){
                   mainColor = addcolor;
               }

    
               o.Albedo = mainColor.rgb * _Color3.rgb;
               o.Alpha = mainColor.a * _Color3.a;

           }
           ENDCG       
       }
          SubShader 
      {
         Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
          ZWrite Off Blend One OneMinusSrcAlpha Cull Off Fog { Mode Off }
          LOD 100
          Pass {
              Tags {"LightMode" = "Vertex"}
              ColorMaterial AmbientAndDiffuse
              Lighting off
              SetTexture [_MainTex] 
              {
                  Combine texture * primary double, texture * primary
              }
          }
      }
       
       
     Fallback Off

   }
