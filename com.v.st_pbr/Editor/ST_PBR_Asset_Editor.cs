using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace V
{
    [CustomEditor(typeof(ST_PBR_Asset))]
    public class ST_PBR_Asset_Editor : Editor
    {
        ST_PBR_Asset asset;

        private void OnEnable()
        {
            asset = target as ST_PBR_Asset;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Create ALLs"))
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                string directoryName = System.IO.Path.GetDirectoryName(assetPath);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                string hmsPath = directoryName + "/" + fileName+"_P" + ".png";
                string normalPath = directoryName + "/" + fileName +"_ST2PBR"+ "_N" + ".png";

                asset.Create_HMS_N();

                asset.mo_a = WriteTextureToDisk(asset.mo_a, hmsPath,false);
                asset.normal = WriteTextureToDisk(asset.normal, normalPath,true);

                string materialPath = directoryName + "/" + fileName + ".mat";
                Material mat = CreateOrLoadMaterial(materialPath);
                mat.SetTexture("_BaseMap", asset.colorMap);
                mat.SetTexture("_MetallicGlossMap", asset.mo_a);
                mat.SetTexture("_BumpMap", asset.normal);
                mat.SetColor("_BaseColor", Color.white);
                mat.SetFloat("_Smoothness", 1.0f);
            }
        }

        Texture2D WriteTextureToDisk(Texture2D textureCache, string targetPath,bool ifNormal)
        {
            if (textureCache == null) { return null;}
            System.IO.File.WriteAllBytes(targetPath, ImageConversion.EncodeToPNG(textureCache));
            Object.DestroyImmediate(textureCache);

            AssetDatabase.ImportAsset(targetPath);

            if (ifNormal)
            {
                TextureImporter importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
                importer.textureType = TextureImporterType.NormalMap;
                AssetDatabase.WriteImportSettingsIfDirty(targetPath);
            }

            return AssetDatabase.LoadMainAssetAtPath(targetPath) as Texture2D;
        }


        Material CreateOrLoadMaterial(string targetPath)
        {
            Material material =  AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Lightweight Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, targetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            }
            return material;
        }
    }
}
