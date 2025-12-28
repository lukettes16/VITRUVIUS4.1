using UnityEngine;

#pragma warning disable 0429, 0162

namespace VLB
{
    public static class Noise3D
    {

        public static bool isSupported {
            get {
                if (!ms_IsSupportedChecked)
                {
                    ms_IsSupported = SystemInfo.graphicsShaderLevel >= kMinShaderLevel;
                    if (!ms_IsSupported)
                        
                    ms_IsSupportedChecked = true;
                }
                return ms_IsSupported;
            }
        }

        public static bool isProperlyLoaded { get { return ms_NoiseTexture != null; } }

        public static string isNotSupportedString { get {
                var str = string.Format("3D Noise requires higher shader capabilities (Shader Model 3.5 / OpenGL ES 3.0), which are not available on the current platform: graphicsShaderLevel (current/required) = {0} / {1}",
                    SystemInfo.graphicsShaderLevel,
                    kMinShaderLevel);
#if UNITY_EDITOR
                str += "\nPlease change the editor's graphics emulation for a more capable one via \"Edit/Graphics Emulation\" and press Play to force the light beams to be recomputed.";
#endif
                return str;
            }
        }

        static bool ms_IsSupportedChecked = false;
        static bool ms_IsSupported = false;
        static Texture3D ms_NoiseTexture = null;

        const int kMinShaderLevel = 35;

        [RuntimeInitializeOnLoadMethod]
        static void OnStartUp()
        {
            LoadIfNeeded();
        }

#if UNITY_EDITOR
        public static void _EditorForceReloadData()
        {
            ms_NoiseTexture = null;
            LoadIfNeeded();
        }
#endif

        public static void LoadIfNeeded()
        {
            if (!isSupported) return;

            if (ms_NoiseTexture == null)
            {
                ms_NoiseTexture = Config.Instance.noiseTexture3D;

                Shader.SetGlobalTexture(ShaderProperties.GlobalNoiseTex3D, ms_NoiseTexture);
                Shader.SetGlobalFloat(ShaderProperties.GlobalNoiseCustomTime, -1.0f);
            }
        }
    }
}