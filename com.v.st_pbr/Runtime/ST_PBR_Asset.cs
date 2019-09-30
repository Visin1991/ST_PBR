using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V
{
    [System.Serializable]
    public class MaterialProperty
    {
        [Range(0.0f,0.0f)]
        public float _MetaMin = 0.0f;
        [Range(0.0f, 0.0f)]
        public float _MetaMax = 0.0f;

        [Range(0.0f, 1.0f)]
        public float _SmoothMin = 0.5f;
        [Range(0.0f, 1.0f)]
        public float _SmoothMax = 0.9f;
        [Range(0.0f, 1.0f)]
        public float _SmoothScale = 1.0f;
    }

    [CreateAssetMenu()]
    [ExecuteInEditMode]
    public class ST_PBR_Asset : ScriptableObject
    {
        public Texture2D colorMap;
        public Texture2D normal;
        public Texture2D mo_a;

        [Range(0.1f,1.0f)]
        public float normalStength = 0.5f;

        [Range(0,5)]
        public int NormalblurIteration = 0;


        public MaterialProperty materialProperty;


        public void Create_HMS_N()
        {
            if (colorMap == null) { return; }

            RenderTextureDescriptor rt_descriptor = new RenderTextureDescriptor(colorMap.width, colorMap.height) { sRGB = false };
            RenderTexture grayScaleRT = RenderTexture.GetTemporary(rt_descriptor);
            RGB_To_GrayScale(colorMap, grayScaleRT);

            RenderTexture normal_RT = RenderTexture.GetTemporary(rt_descriptor);
            GrayScale_To_Normal(grayScaleRT, normal_RT);
            normal = CreateTexture2D(normal_RT, true);

            RenderTexture mso_RT = RenderTexture.GetTemporary(rt_descriptor);
            GrayScale_To_MSO(grayScaleRT,mso_RT);

            mo_a = CreateTexture2D(mso_RT, true);

            RenderTexture.ReleaseTemporary(grayScaleRT);

            //hms = CreateTexture2D(grayScaleRT, true);          
        }

        public Texture2D CreateTexture2D(RenderTexture rt,bool releaseRT)
        {
            Texture2D tex2D = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
            RenderTexture.active = rt;
            tex2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex2D.Apply();
            if (releaseRT)
            {
                RenderTexture.ReleaseTemporary(rt);
            }
            return tex2D;
        }

        static Material m_RGB2BW;

        static Material RGB2BW
        {
            get
            {
                if (m_RGB2BW == null)
                {
                    m_RGB2BW = new Material(Shader.Find("Hidden/V/ST_PBR/RGB_To_HMS"));
                }
                return m_RGB2BW;
            }
        }

        void RGB_To_GrayScale(Texture source, RenderTexture heightMapRT)
        {
            Graphics.Blit(source, heightMapRT, RGB2BW);
        }

        void GrayScale_To_Normal(Texture heightMap, RenderTexture final_normalMapRT)
        {
            var rt_descriptor = new RenderTextureDescriptor(heightMap.width, heightMap.height) { sRGB = false, depthBufferBits = 0, colorFormat = RenderTextureFormat.Default };
            RenderTexture sobeNormalRT = RenderTexture.GetTemporary(rt_descriptor);
            Apply_SobeNormal(heightMap, sobeNormalRT);
            Apply_GuassianBlur(sobeNormalRT, final_normalMapRT);
            RenderTexture.ReleaseTemporary(sobeNormalRT);
        }

        private static Material grayScale_To_MO_S;
        void GrayScale_To_MSO(Texture grayScale,RenderTexture msoRT)
        {
            if (grayScale_To_MO_S == null)
            {
                grayScale_To_MO_S = new Material(Shader.Find("Hidden/V/ST_PBR/BW_2_MO_S"));
            }

            grayScale_To_MO_S.SetFloat("_MetaMin", materialProperty._MetaMin);
            grayScale_To_MO_S.SetFloat("_MetaMax", materialProperty._MetaMax);
            grayScale_To_MO_S.SetFloat("_SmoothMin", materialProperty._SmoothMin);
            grayScale_To_MO_S.SetFloat("_SmoothMax", materialProperty._SmoothMax);
            grayScale_To_MO_S.SetFloat("_SmoothScale", materialProperty._SmoothScale);

            Graphics.Blit(grayScale, msoRT, grayScale_To_MO_S);
        }


        private static Material SobNormal;
        void Apply_SobeNormal(Texture source, RenderTexture destination)
        {
            if (source == null || destination == null)
            {
                Debug.LogError("Apply_SobeNormal : Source or RT NULL");
                return;
            }

            if (SobNormal == null)
            {
                SobNormal = new Material(Shader.Find("Hidden/V/ST_PBR/SobelH2N"));
            }

            float v = normalStength * 2f - 1f;
            float z = 1f - v;
            float xy = 1f + v;

            SobNormal.SetVector("_Factor", new Vector4(xy, xy, z, 1));

            Graphics.Blit(source, destination, SobNormal);
        }

        private static Material guassianBlurMaterial;
        void Apply_GuassianBlur(Texture source,RenderTexture destination)
        {
            RenderTextureDescriptor rt_descriptor = new RenderTextureDescriptor(source.width, source.height) { sRGB = false, depthBufferBits = 0,colorFormat = RenderTextureFormat.Default };
           
            if (guassianBlurMaterial == null)
            {
                guassianBlurMaterial = new Material(Shader.Find("Hidden/V/ST_PBR/GaussianBlur"));
            }

            RenderTexture RT_V, RT_H;
            RT_V = RenderTexture.GetTemporary(rt_descriptor);
            RT_H = RenderTexture.GetTemporary(rt_descriptor);


            Graphics.Blit(source, RT_V);

            for (int i = 0; i < NormalblurIteration; i++)
            {
                Graphics.Blit(RT_V, RT_H, guassianBlurMaterial, 1);
                Graphics.Blit(RT_H, RT_V, guassianBlurMaterial, 2);
            }

            Graphics.Blit(RT_V, destination);

            RenderTexture.ReleaseTemporary(RT_V);
            RenderTexture.ReleaseTemporary(RT_H);

        }

    }

}