//RealToon - DeNorSob Outline Effect (URP - Post Processing)
//MJQStudioWorks
//�2025

Shader  "Hidden/URP/RealToon/Effects/DeNorSobOutline"
{

    Properties
    {
        _OutlineWidth("Outline Width", Float) = 1.0

        _DepthThreshold("Depth Threshold", Float) = 900.0

        _NormalThreshold("Normal Threshold", Float) = 1.3
        _NormalMin("Normal Min", Float) = 1.0
        _NormalMax("Normal Max", Float) = 1.0

        _SobOutSel("Sobel Outline", Float) = 0.0
        _SobelOutlineThreshold(" Sobel Outline Threshold", Float) = 300.0
        _WhiThres("Black Threshold", Float) = 0.0
        _BlaThres("White Threshold", Float) = 0.0

        _OutlineColor("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _OutlineColorIntensity("Outline Color Intensity", Float) = 0.0
        _ColOutMiSel("Mix Full Screen Color", Float) = 0.0

        _OutOnSel("Show Outline Only", Float) = 0.0
        _MixDeNorSob("Mix Deph Normal And Sobel Outline", Float) = 0.0 
    }

    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

    #pragma shader_feature_local RENDER_OUTLINE_ALL
    #pragma shader_feature_local MIX_DENOR_SOB

    TEXTURE2D(_CameraColorTexture);
    SAMPLER(sampler_CameraColorTexture);

    float _OutlineWidth;

    float _DepthThreshold;
    float _NormalThreshold;
    float _NormalMin;
    float _NormalMax;

    float _SobOutSel;
    float _SobelOutlineThreshold;
    float _WhiThres;
    float _BlaThres;

    float3 _OutlineColor;
    float _OutlineColorIntensity;
    float _ColOutMiSel;

    float _OutOnSel;
    float _MixDeNorSob;

    float SamDep(float2 uv)
    {
        float output = (float)min(max(SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, uv), _BlaThres), _WhiThres);

        return output;
    }

    float sob_fil(float CX, float2 uv)
    {
        float2 d = float2(CX, CX);

        float hr = 0;
        float vt = 0;

        hr += SamDep(uv + float2(-1.0, -1.0) * d) * 1.0;
        hr += SamDep(uv + float2(1.0, -1.0) * d) * -1.0;
        hr += SamDep(uv + float2(-1.0, 0.0) * d) * 2.0;
        hr += SamDep(uv + float2(1.0, 0.0) * d) * -2.0;
        hr += SamDep(uv + float2(-1.0, 1.0) * d) * 1.0;
        hr += SamDep(uv + float2(1.0, 1.0) * d) * -1.0;

        vt += SamDep(uv + float2(-1.0, -1.0) * d) * 1.0;
        vt += SamDep(uv + float2(0.0, -1.0) * d) * 2.0;
        vt += SamDep(uv + float2(1.0, -1.0) * d) * 1.0;
        vt += SamDep(uv + float2(-1.0, 1.0) * d) * -1.0;
        vt += SamDep(uv + float2(0.0, 1.0) * d) * -2.0;
        vt += SamDep(uv + float2(1.0, 1.0) * d) * -1.0;

        return sqrt(dot(hr , hr) + dot(vt , vt));

    }

    float3 SampSceNorm(float2 uv)
    {
        float3 normal = SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_CameraNormalsTexture, UnityStereoTransformScreenSpaceTex(uv)).xyz;

        #if defined(_GBUFFER_NORMALS_OCT)
            float2 remappedOctNormalWS = Unpack888ToFloat2(normal);
            float2 octNormalWS = remappedOctNormalWS.xy * 2.0 - 1.0;
            normal = UnpackNormalOctQuadEncode(octNormalWS);
        #endif

        return normal;
    }

    float EdgeDetect(float2 uv, float4 input_pos_cs)
    {

        float2 _ScreenSize = (_OutlineWidth) / float2(1920, 1080); 
        //float2(_ScreenParams.r, _ScreenParams.g);

        float obj_only = (float)SampleSceneDepth(uv) != UNITY_RAW_FAR_CLIP_VALUE;

        float halfScaleFloor = floor(_OutlineWidth * 0.5);
        float halfScaleCeil = ceil(_OutlineWidth * 0.5);

        float2 bottomLeftUV = uv - float2(_ScreenSize.x, _ScreenSize.y) * halfScaleFloor;
        float2 topRightUV = uv + float2(_ScreenSize.x, _ScreenSize.y) * halfScaleCeil;
        float2 bottomRightUV = uv + float2(_ScreenSize.x * halfScaleCeil, -_ScreenSize.y * halfScaleFloor);
        float2 topLeftUV = uv + float2(-_ScreenSize.x * halfScaleFloor, _ScreenSize.y * halfScaleCeil);

        float depth0 = SampleSceneDepth(bottomLeftUV);
        float depth1 = SampleSceneDepth(topRightUV);
        float depth2 = SampleSceneDepth(bottomRightUV);
        float depth3 = SampleSceneDepth(topLeftUV);

        float depthDerivative0 = depth1 - depth0;
        float depthDerivative1 = depth3 - depth2;

        float edgeDepth = sqrt(pow(depthDerivative0, 2.0) + pow(depthDerivative1, 2.0)) * 100;
        edgeDepth = edgeDepth > (depth0 * (_DepthThreshold * 0.01)) ? 1 : 0;

        float3 normalData0 = SampSceNorm(bottomLeftUV);
        float3 normalData1 = SampSceNorm(topRightUV);
        float3 normalData2 = SampSceNorm(bottomRightUV);
        float3 normalData3 = SampSceNorm(topLeftUV);

        float3 normalFiniteDifference0 = (normalData1 - normalData0);
        float3 normalFiniteDifference1 = (normalData3 - normalData2);

        float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
        edgeNormal = smoothstep(_NormalMin, _NormalMax, edgeNormal * _NormalThreshold) * obj_only;

        float edgeSob = sob_fil(_OutlineWidth / float2(_ScreenParams.r, _ScreenParams.g), uv) > ((_SobelOutlineThreshold * 0.01) * SamDep(uv)) ? 1 : 0;

        #ifndef MIX_DENOR_SOB

            #ifdef RENDER_OUTLINE_ALL
                return edgeSob;
            #else
                return max(edgeDepth, edgeNormal);
            #endif

        #else

            #ifdef RENDER_OUTLINE_ALL
                float edgeSob_Mix = edgeSob;
            #else
                float edgeSob_Mix = 0.0;
            #endif

                return max(edgeDepth, max(edgeNormal, edgeSob_Mix));

        #endif

    }

    half4 Frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

        float3 ful_scr_so = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv).rgb;

        float denorsobOut = EdgeDetect(uv, input.positionCS);

        float3 coloutmix = lerp(_OutlineColor * _OutlineColorIntensity, lerp(ful_scr_so * ful_scr_so, ful_scr_so , _OutlineColorIntensity) * _OutlineColor, _ColOutMiSel);
        return float4( lerp( coloutmix, lerp( ful_scr_so , 1.0, _OutOnSel ) , (1.0 - denorsobOut) ) ,1.0);

    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
            LOD 100
            ZTest Always ZWrite Off Cull Off

            Pass
        {
            Name "DeNorSob_Outline"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }

}
