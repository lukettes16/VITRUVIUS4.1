using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    public static class DummyMaterial
    {
#if UNITY_EDITOR
        static string GetPath(ShaderMode shaderMode, Shader shader)
        {
            string kDummyFilename = "VLBDummyMaterial" + shaderMode + ".mat";
            string kDummyPathFallback = "Assets/" + Consts.PluginFolder + "/Shaders/" + kDummyFilename;

            Debug.Assert(shader);

            var shaderPath = AssetDatabase.GetAssetPath(shader);
            if (string.IsNullOrEmpty(shaderPath))
                return kDummyPathFallback;

            var shaderFolder = System.IO.Path.GetDirectoryName(shaderPath);
            return System.IO.Path.Combine(shaderFolder, kDummyFilename);
        }

        public static Material Create(ShaderMode shaderMode, Shader shader, bool gpuInstanced)
        {
            if (shader == null)
                return null;

            string path = GetPath(shaderMode, shader);
            var dummyMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (dummyMat == null
             || dummyMat.shader != shader
             || BatchingHelper.IsGpuInstancingEnabled(dummyMat) != gpuInstanced)
            {
                dummyMat = MaterialManager.NewMaterialPersistent(shader, gpuInstanced);
                if (dummyMat)
                    AssetDatabase.CreateAsset(dummyMat, path);
            }

            return dummyMat;
        }
#endif
    }
}