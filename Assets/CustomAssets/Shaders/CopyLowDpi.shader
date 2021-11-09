Shader "Hidden/Universal Render Pipeline/Custom/CopyLowDpi"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "CopyLowDpi"
            ZTest Always
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Fragment
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D_X(_LowDpi);
            SAMPLER(sampler_LowDpi);
            TEXTURE2D_X(_LowDpiDepth);
            SAMPLER(sampler_LowDpiDepth);

            struct output
            {
                half4 color : SV_Target;
                float depth : SV_Depth;
            };

            output Fragment(Varyings input)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 col = SAMPLE_TEXTURE2D_X(_LowDpi, sampler_LowDpi, input.uv);
                float depth = SAMPLE_DEPTH_TEXTURE(_LowDpiDepth, sampler_LowDpiDepth, input.uv);

                #ifdef _LINEAR_TO_SRGB_CONVERSION
                col = LinearToSRGB(col);
                #endif

                output o;
                o.color = col;
                o.depth = depth;
                return o;
            }
            ENDHLSL
        }
    }
}