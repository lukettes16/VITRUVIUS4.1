#if UNITY_EDITOR

using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

#if VLB_URP
using UnityEngine.Rendering.Universal;
#endif

namespace VLB
{
    [HelpURL(Consts.Help.UrlConfig)]
    public class Config : ScriptableObject
    {
        public const string ClassName = "Config";

        public const string kAssetName = "VLBConfigOverride";
        public const string kAssetNameExt = ".asset";

        public bool geometryOverrideLayer = Consts.Config.GeometryOverrideLayerDefault;

        public int geometryLayerID = Consts.Config.GeometryLayerIDDefault;

        public string geometryTag = Consts.Config.GeometryTagDefault;

        public int geometryRenderQueue = (int)Consts.Config.GeometryRenderQueueDefault;
        public int geometryRenderQueueHD = (int)Consts.Config.HD.GeometryRenderQueueDefault;

        public RenderPipeline renderPipeline
        {
            get { return m_RenderPipeline; }
            set
            {
#if UNITY_EDITOR
                m_RenderPipeline = value;
#else
                
#endif
            }
        }
        [FormerlySerializedAs("renderPipeline"), FormerlySerializedAs("_RenderPipeline")]
        [SerializeField] RenderPipeline m_RenderPipeline = Consts.Config.GeometryRenderPipelineDefault;

        public RenderingMode renderingMode
        {
            get { return m_RenderingMode; }
            set
            {
#if UNITY_EDITOR
                m_RenderingMode = value;
#else
                
#endif
            }
        }
        [FormerlySerializedAs("renderingMode"), FormerlySerializedAs("_RenderingMode")]
        [SerializeField] RenderingMode m_RenderingMode = Consts.Config.GeometryRenderingModeDefault;

        public bool IsSRPBatcherSupported()
        {

            if (renderPipeline == RenderPipeline.BuiltIn) return false;

            var rp = SRPHelper.projectRenderPipeline;
            return rp == RenderPipeline.URP || rp == RenderPipeline.HDRP;
        }

        public RenderingMode GetActualRenderingMode(ShaderMode shaderMode)
        {
            if (renderingMode == RenderingMode.SRPBatcher && !IsSRPBatcherSupported()) return RenderingMode.Default;

            if (renderPipeline != RenderPipeline.BuiltIn && renderingMode == RenderingMode.MultiPass) return RenderingMode.Default;

            if (shaderMode == ShaderMode.HD && renderingMode == RenderingMode.MultiPass) return RenderingMode.Default;

            return renderingMode;
        }

        public bool SD_useSinglePassShader { get { return GetActualRenderingMode(ShaderMode.SD) != RenderingMode.MultiPass; } }

        public bool SD_requiresDoubleSidedMesh { get { return SD_useSinglePassShader; } }

        public Shader GetBeamShader(ShaderMode mode)
        {
#if UNITY_EDITOR
            var shader = GetBeamShaderInternal(mode);
            if (shader == null)
                RefreshShader(mode, RefreshShaderFlags.All);
            return shader;
#else
            return GetBeamShaderInternal(mode);
#endif
        }

        ref Shader GetBeamShaderInternal(ShaderMode mode)
        {
            if(mode == ShaderMode.SD)   return ref _BeamShader;
            else                        return ref _BeamShaderHD;
        }

        int GetRenderQueueInternal(ShaderMode mode)
        {
            if (mode == ShaderMode.SD)  return geometryRenderQueue;
            else                        return geometryRenderQueueHD;
        }

        public Material NewMaterialTransient(ShaderMode mode, bool gpuInstanced)
        {
            var material = MaterialManager.NewMaterialPersistent(GetBeamShader(mode), gpuInstanced);
            if (material)
            {
                material.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
                material.renderQueue = GetRenderQueueInternal(mode);
            }
            return material;
        }

        public float ditheringFactor = Consts.Config.DitheringFactor;

        public bool useLightColorTemperature = Consts.Config.UseLightColorTemperatureDefault;

        public int sharedMeshSides = Consts.Config.SharedMeshSidesDefault;

        public int sharedMeshSegments = Consts.Config.SharedMeshSegmentsDefault;

        public float hdBeamsCameraBlendingDistance = Consts.Config.HD.CameraBlendingDistance;

        public int urpDepthCameraScriptableRendererIndex = -1;

        public void SetURPScriptableRendererIndexToDepthCamera(Camera camera)
        {
#if VLB_URP
            if (urpDepthCameraScriptableRendererIndex < 0)
                return;

            Debug.Assert(camera);
            var cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData)
            {
                cameraData.SetRenderer(urpDepthCameraScriptableRendererIndex);
            }
#endif
        }

        [Range(Consts.Beam.NoiseScaleMin, Consts.Beam.NoiseScaleMax)]
        public float globalNoiseScale = Consts.Beam.NoiseScaleDefault;

        public Vector3 globalNoiseVelocity = Consts.Beam.NoiseVelocityDefault;

        public string fadeOutCameraTag = Consts.Config.FadeOutCameraTagDefault;

        public Transform fadeOutCameraTransform
        {
            get
            {
                if (m_CachedFadeOutCamera == null || !m_CachedFadeOutCamera.isActiveAndEnabled)
                {
                    ForceUpdateFadeOutCamera();
                }

                return m_CachedFadeOutCamera != null ? m_CachedFadeOutCamera.transform : null;
            }
        }

        public string fadeOutCameraName { get { return m_CachedFadeOutCamera != null ? m_CachedFadeOutCamera.name : "Invalid Camera"; } }

        public void ForceUpdateFadeOutCamera()
        {
            var gaos = GameObject.FindGameObjectsWithTag(fadeOutCameraTag);
            if (gaos != null)
            {
                foreach (GameObject gao in gaos)
                {
                    if (gao)
                    {
                        var cam = gao.GetComponent<Camera>();
                        if (cam && cam.isActiveAndEnabled)
                        {
                            m_CachedFadeOutCamera = cam;
                            return;
                        }
                    }
                }
            }

        }

        [HighlightNull]
        public Texture3D noiseTexture3D = null;

        [HighlightNull]
        public ParticleSystem dustParticlesPrefab = null;

        [HighlightNull]
        public Texture2D ditheringNoiseTexture = null;

        [HighlightNull]
        public Texture2D jitteringNoiseTexture = null;

        public FeatureEnabledColorGradient featureEnabledColorGradient = Consts.Config.FeatureEnabledColorGradientDefault;

        public bool featureEnabledDepthBlend = Consts.Config.FeatureEnabledDefault;

        public bool featureEnabledNoise3D = Consts.Config.FeatureEnabledDefault;

        public bool featureEnabledDynamicOcclusion = Consts.Config.FeatureEnabledDefault;

        public bool featureEnabledMeshSkewing = Consts.Config.FeatureEnabledDefault;

        public bool featureEnabledShaderAccuracyHigh = Consts.Config.FeatureEnabledDefault;

        public bool featureEnabledShadow = true;

        public bool featureEnabledCookie = true;

        [SerializeField] RaymarchingQuality[] m_RaymarchingQualities = null;

        [SerializeField] int m_DefaultRaymarchingQualityUniqueID = 0;

        public int defaultRaymarchingQualityUniqueID => m_DefaultRaymarchingQualityUniqueID;

        public RaymarchingQuality GetRaymarchingQualityForIndex(int index)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(m_RaymarchingQualities != null);
            Debug.Assert(index < m_RaymarchingQualities.Length);
            return m_RaymarchingQualities[index];
        }

        public RaymarchingQuality GetRaymarchingQualityForUniqueID(int id)
        {
            int index = GetRaymarchingQualityIndexForUniqueID(id);
            if (index >= 0)
                return GetRaymarchingQualityForIndex(index);
            return null;
        }

        public int GetRaymarchingQualityIndexForUniqueID(int id)
        {
            for (int i = 0; i < m_RaymarchingQualities.Length; ++i)
            {
                var qual = m_RaymarchingQualities[i];
                if (qual != null && qual.uniqueID == id)
                    return i;
            }

            Debug.LogErrorFormat("Failed to find RaymarchingQualityIndex for Unique ID {0}", id);
            return -1;
        }

        public bool IsRaymarchingQualityUniqueIDValid(int id) { return GetRaymarchingQualityIndexForUniqueID(id) >= 0; }

#if UNITY_EDITOR
        public void AddRaymarchingQuality(RaymarchingQuality qual)
        {
            ArrayUtility.Add(ref m_RaymarchingQualities, qual);
        }

        public void RemoveRaymarchingQualityAtIndex(int index)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < m_RaymarchingQualities.Length);
            ArrayUtility.RemoveAt(ref m_RaymarchingQualities, index);
        }
#endif

        public int raymarchingQualitiesCount { get { return Mathf.Max(1, m_RaymarchingQualities != null ? m_RaymarchingQualities.Length : 1); } }

        void CreateDefaultRaymarchingQualityPreset(bool onlyIfNeeded)
        {
            if (m_RaymarchingQualities == null || m_RaymarchingQualities.Length == 0 || !onlyIfNeeded)
            {
                m_RaymarchingQualities = new RaymarchingQuality[3];

                m_RaymarchingQualities[0] = RaymarchingQuality.New("Fast", 1, 5);
                m_RaymarchingQualities[1] = RaymarchingQuality.New("Balanced", 2, 10);
                m_RaymarchingQualities[2] = RaymarchingQuality.New("High", 3, 20);
                m_DefaultRaymarchingQualityUniqueID = m_RaymarchingQualities[1].uniqueID;
            }
        }

        public bool isHDRPExposureWeightSupported
        {
            get
            {
            #if UNITY_2021_1_OR_NEWER
                return renderPipeline == RenderPipeline.HDRP;
            #else
                return false;
            #endif
            }
        }

#pragma warning disable 0414
        [SerializeField] int pluginVersion = -1;
        [SerializeField] Material _DummyMaterial = null;
        [SerializeField] Material _DummyMaterialHD = null;
        [SerializeField] Shader _BeamShader = null;
        [SerializeField] Shader _BeamShaderHD = null;
#pragma warning restore 0414

        Camera m_CachedFadeOutCamera = null;

        public bool hasRenderPipelineMismatch { get { return (SRPHelper.projectRenderPipeline == RenderPipeline.BuiltIn) != (m_RenderPipeline == RenderPipeline.BuiltIn); } }

        [RuntimeInitializeOnLoadMethod]
        static void OnStartup()
        {
            Instance.m_CachedFadeOutCamera = null;
            Instance.RefreshGlobalShaderProperties();

#if UNITY_EDITOR
            Instance.RefreshShaders(RefreshShaderFlags.All);
#endif

            if (Instance.hasRenderPipelineMismatch) { }}

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {

            if (ms_Instance)
                ms_Instance.SetScriptingDefineSymbolsForCurrentRenderPipeline();
        }

        public void SetScriptingDefineSymbolsForCurrentRenderPipeline()
        {
            SRPHelper.SetScriptingDefineSymbolsForRenderPipeline(renderPipeline);
        }
#endif

        public void Reset()
        {
            geometryOverrideLayer = Consts.Config.GeometryOverrideLayerDefault;
            geometryLayerID = Consts.Config.GeometryLayerIDDefault;
            geometryTag = Consts.Config.GeometryTagDefault;
            geometryRenderQueue = (int)Consts.Config.GeometryRenderQueueDefault;
            geometryRenderQueueHD = (int)Consts.Config.HD.GeometryRenderQueueDefault;

            sharedMeshSides = Consts.Config.SharedMeshSidesDefault;
            sharedMeshSegments = Consts.Config.SharedMeshSegmentsDefault;

            globalNoiseScale = Consts.Beam.NoiseScaleDefault;
            globalNoiseVelocity = Consts.Beam.NoiseVelocityDefault;

            renderPipeline = Consts.Config.GeometryRenderPipelineDefault;
            renderingMode = Consts.Config.GeometryRenderingModeDefault;
            ditheringFactor = Consts.Config.DitheringFactor;
            useLightColorTemperature = Consts.Config.UseLightColorTemperatureDefault;

            fadeOutCameraTag = Consts.Config.FadeOutCameraTagDefault;

            featureEnabledColorGradient = Consts.Config.FeatureEnabledColorGradientDefault;
            featureEnabledDepthBlend = Consts.Config.FeatureEnabledDefault;
            featureEnabledNoise3D = Consts.Config.FeatureEnabledDefault;
            featureEnabledDynamicOcclusion = Consts.Config.FeatureEnabledDefault;
            featureEnabledMeshSkewing = Consts.Config.FeatureEnabledDefault;
            featureEnabledShaderAccuracyHigh = Consts.Config.FeatureEnabledDefault;

            hdBeamsCameraBlendingDistance = Consts.Config.HD.CameraBlendingDistance;
            urpDepthCameraScriptableRendererIndex = -1;

            CreateDefaultRaymarchingQualityPreset(onlyIfNeeded: false);

            ResetInternalData();

#if UNITY_EDITOR
            GlobalMeshSD.Destroy();
            Utils._EditorSetAllMeshesDirty();
#endif
        }

        void RefreshGlobalShaderProperties()
        {
            Shader.SetGlobalFloat(ShaderProperties.GlobalUsesReversedZBuffer, SystemInfo.usesReversedZBuffer ? 1.0f : 0.0f);
            Shader.SetGlobalFloat(ShaderProperties.GlobalDitheringFactor, ditheringFactor);
            Shader.SetGlobalTexture(ShaderProperties.GlobalDitheringNoiseTex, ditheringNoiseTexture);

            Shader.SetGlobalFloat(ShaderProperties.HD.GlobalCameraBlendingDistance, hdBeamsCameraBlendingDistance);
            Shader.SetGlobalTexture(ShaderProperties.HD.GlobalJitteringNoiseTex, jitteringNoiseTexture);
        }

#if UNITY_EDITOR
        public void _EditorSetRenderingModeAndRefreshShader(RenderingMode mode)
        {
            renderingMode = mode;
            RefreshShaders(RefreshShaderFlags.All);
        }

        void OnValidate()
        {
            sharedMeshSides = Mathf.Clamp(sharedMeshSides, Consts.Config.SharedMeshSidesMin, Consts.Config.SharedMeshSidesMax);
            sharedMeshSegments = Mathf.Clamp(sharedMeshSegments, Consts.Config.SharedMeshSegmentsMin, Consts.Config.SharedMeshSegmentsMax);

            ditheringFactor = Mathf.Clamp01(ditheringFactor);

            hdBeamsCameraBlendingDistance = Mathf.Max(hdBeamsCameraBlendingDistance, 0f);
        }

        void AutoSelectRenderPipeline()
        {
            var newPipeline = SRPHelper.projectRenderPipeline;
            if (newPipeline != renderPipeline)
            {
                renderPipeline = newPipeline;
                EditorUtility.SetDirty(this);
                RefreshShaders(RefreshShaderFlags.All);
                SetScriptingDefineSymbolsForCurrentRenderPipeline();
            }
        }

        public static void EditorSelectInstance()
        {
            Selection.activeObject = Instance;
            if (Selection.activeObject == null) { }}

        ref Material GetDummyMaterial(ShaderMode shaderMode)
        {
            if (shaderMode == ShaderMode.SD)    return ref _DummyMaterial;
            else                                return ref _DummyMaterialHD;
        }

        [System.Flags]
        public enum RefreshShaderFlags
        {
            Reference = 1 << 1,
            Dummy = 1 << 2,
            All = Reference | Dummy,
        }

        public void RefreshShaders(RefreshShaderFlags flags)
        {
            foreach (ShaderMode shaderMode in System.Enum.GetValues(typeof(ShaderMode)))
                RefreshShader(shaderMode, flags);
        }

        public void RefreshShader(ShaderMode shaderMode, RefreshShaderFlags flags)
        {
            ref Shader shader = ref GetBeamShaderInternal(shaderMode);

            if (flags.HasFlag(RefreshShaderFlags.Reference))
            {
                var prevShader = shader;

                var configProps = new ShaderGenerator.ConfigProps
                {
                    renderPipeline = m_RenderPipeline,
                    renderingMode = GetActualRenderingMode(shaderMode),
                    dithering = ditheringFactor > 0.0f,
                    noise3D = featureEnabledNoise3D,
                    colorGradient = featureEnabledColorGradient,
                    depthBlend = featureEnabledDepthBlend,
                    dynamicOcclusion = featureEnabledDynamicOcclusion,
                    meshSkewing = featureEnabledMeshSkewing,
                    shaderAccuracyHigh = featureEnabledShaderAccuracyHigh,
                    cookie = featureEnabledCookie,
                    shadow = featureEnabledShadow,
                    raymarchingQualities = m_RaymarchingQualities
                };

                shader = ShaderGenerator.Generate(shaderMode, configProps);

                if (shader != prevShader)
                {
                    EditorUtility.SetDirty(this);
                }
            }

            if (flags.HasFlag(RefreshShaderFlags.Dummy) && shader != null)
            {
                bool gpuInstanced = GetActualRenderingMode(shaderMode) == RenderingMode.GPUInstancing;
                ref var dummyMat = ref GetDummyMaterial(shaderMode);
                dummyMat = DummyMaterial.Create(shaderMode, shader, gpuInstanced);
            }

            if (GetDummyMaterial(shaderMode) == null)
            {
                Debug.LogErrorFormat(this, "No dummy material referenced to VLB config for ShaderMode {0}, please try to reset this asset.", shaderMode);
            }

            RefreshGlobalShaderProperties();
        }

        static void DeleteAsset<T>(ref T assetObject) where T : UnityEngine.Object
        {
            if (assetObject)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(assetObject);
                AssetDatabase.DeleteAsset(path);
                assetObject = null;
            }
        }

        public static void CleanGeneratedAssets()
        {
            var instance = Instance;
            if (instance)
            {
                DeleteAsset(ref instance._DummyMaterial);
                DeleteAsset(ref instance._DummyMaterialHD);
                DeleteAsset(ref instance._BeamShader);
                DeleteAsset(ref instance._BeamShaderHD);
                DeleteAsset(ref instance);
            }
        }
#endif

        public void ResetInternalData()
        {
            noiseTexture3D = Resources.Load("Noise3D_64x64x64") as Texture3D;

            dustParticlesPrefab = Resources.Load("DustParticles", typeof(ParticleSystem)) as ParticleSystem;

            ditheringNoiseTexture = Resources.Load("VLBDitheringNoise", typeof(Texture2D)) as Texture2D;
            jitteringNoiseTexture = Resources.Load("VLBBlueNoise", typeof(Texture2D)) as Texture2D;

#if UNITY_EDITOR
            RefreshShaders(RefreshShaderFlags.All);
#endif
        }

        public ParticleSystem NewVolumetricDustParticles()
        {
            if (!dustParticlesPrefab)
            {
                if (Application.isPlaying)
                {
                    
                }
                return null;
            }

            var instance = Instantiate(dustParticlesPrefab);
            instance.useAutoRandomSeed = false;
            instance.name = "Dust Particles";
            instance.gameObject.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
            instance.gameObject.SetActive(true);
            return instance;
        }

        void OnEnable()
        {
            CreateDefaultRaymarchingQualityPreset(onlyIfNeeded:true);

            HandleBackwardCompatibility(pluginVersion, Version.Current);
            pluginVersion = Version.Current;
        }

        void HandleBackwardCompatibility(int serializedVersion, int newVersion)
        {
#if UNITY_EDITOR
            if (serializedVersion == -1) return;
            if (serializedVersion == newVersion) return;

            if (serializedVersion < 1830)
            {
                AutoSelectRenderPipeline();
            }

            if (serializedVersion < 1950)
            {
                ResetInternalData();
                EditorUtility.SetDirty(this);
            }

            if (serializedVersion < 1980)
            {
                useLightColorTemperature = false;
                EditorUtility.SetDirty(this);
            }

            if (serializedVersion < 20000)
            {
                ResetInternalData();
                EditorUtility.SetDirty(this);
            }

            if (serializedVersion < 20002)
            {
                SetScriptingDefineSymbolsForCurrentRenderPipeline();
            }

            if (newVersion > serializedVersion)
            {

                RefreshShaders(RefreshShaderFlags.All);
            }
#endif
        }

        static Config ms_Instance = null;
        public static Config Instance { get { return GetInstance(true); } }

#if UNITY_EDITOR && VLB_DEBUG
        public struct Guard : System.IDisposable {
            public Guard(bool assert) {
                if (m_IsAccessing && assert) 
                m_IsAccessing = true;
            }

            public void Dispose() { m_IsAccessing = false; }
            static bool m_IsAccessing = false;
        }
#endif

#if UNITY_EDITOR
        static bool ms_ShouldInvalidateCache = false;
        public Config()
        {
            ms_ShouldInvalidateCache = true;
        }
#endif

        static Config LoadAssetInternal(string assetName)
        {
        #if PROFILE_INSTANCE_LOADING
            var startTime = EditorApplication.timeSinceStartup;
        #endif
            var instance = Resources.Load<Config>(assetName);
        #if PROFILE_INSTANCE_LOADING
            var totalTime = EditorApplication.timeSinceStartup - startTime;
            
        #endif
            return instance;
        }

        private static Config GetInstance(bool assertIfNotFound)
        {
        #if UNITY_EDITOR && VLB_DEBUG
            using (new Guard(true))
        #endif
            {
                bool updateInstance = ms_Instance == null;
            #if UNITY_EDITOR
                updateInstance |= ms_ShouldInvalidateCache;
            #endif
                if (updateInstance)
                {
                #if UNITY_EDITOR
                    if (ms_IsCreatingInstance)
                    {
                        
                        return null;
                    }
                #endif

                    {
                        var newInstance = LoadAssetInternal(kAssetName + PlatformHelper.GetCurrentPlatformSuffix());
                        if (newInstance == null) newInstance = LoadAssetInternal(kAssetName);

                    #if UNITY_EDITOR
                        if (newInstance && newInstance != ms_Instance)
                        {
                            ms_Instance = newInstance;
                            newInstance.RefreshGlobalShaderProperties();
                            newInstance.SetScriptingDefineSymbolsForCurrentRenderPipeline();
                        }
                        ms_ShouldInvalidateCache = false;
                    #endif

                        ms_Instance = newInstance;
                    }

                    if (ms_Instance == null)
                    {
                    #if UNITY_EDITOR
                        ms_IsCreatingInstance = true;
                        ms_Instance = CreateInstanceAsset();
                        ms_IsCreatingInstance = false;

                        ms_Instance.AutoSelectRenderPipeline();
                        ms_Instance.SetScriptingDefineSymbolsForCurrentRenderPipeline();

                        if (Application.isPlaying)
                            ms_Instance.Reset();
                    #endif
                        Debug.Assert(!(assertIfNotFound && ms_Instance == null), string.Format("Can't find any resource of type '{0}'. Make sure you have a ScriptableObject of this type in a 'Resources' folder.", typeof(Config)));
                    }
                }
            }
            return ms_Instance;
        }

    #if UNITY_EDITOR
        static bool ms_IsCreatingInstance = false;

        public bool IsCurrentlyUsedInstance() { return Instance == this; }

        public bool HasValidAssetName()
        {
            if (name.IndexOf(kAssetName) != 0)
                return false;

            return PlatformHelper.IsValidPlatformSuffix(GetAssetSuffix());
        }

        public string GetAssetSuffix()
        {
            var fullname = name;
            var strToFind = kAssetName;
            if (fullname.IndexOf(strToFind) == 0) return fullname.Substring(strToFind.Length);
            else return "";
        }

        static void CreateFolderAndAsset(Object obj, string folderParent, string folderResources, string assetName)
        {
            if (!AssetDatabase.IsValidFolder(string.Format("{0}/{1}", folderParent, folderResources)))
                AssetDatabase.CreateFolder(folderParent, folderResources);

            CreateAsset(obj, string.Format("{0}/{1}/{2}", folderParent, folderResources, assetName));
        }

        public static void CreateAsset(Object obj, string fullPath)
        {
            AssetDatabase.CreateAsset(obj, fullPath);
            AssetDatabase.SaveAssets();
        }

        static Config CreateInstanceAsset()
        {
            var asset = CreateInstance<Config>();
            Debug.Assert(asset != null);
            CreateFolderAndAsset(asset, "Assets", "Resources", kAssetName + kAssetNameExt);
            return asset;
        }

        public string GetDebugInfo()
        {
#if UNITY_2021_2_OR_NEWER
            string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
#else
            string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif

            return "Unity version: " + Application.unityVersion
            + "\nVLB version: " + Version.Current
            + "\nPlatform: " + Application.platform
            + "\nOS: " + SystemInfo.operatingSystem
            + "\nShader Level: " + SystemInfo.graphicsShaderLevel
            + "\nGraphics API: " + SystemInfo.graphicsDeviceType
            + "\nUses Reversed ZBuffer: " + SystemInfo.usesReversedZBuffer
            + "\nScripting Define Symbols: " + scriptingDefineSymbols
            + "\nRender Pipeline Asset: " + (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null ? UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.ToString() : "none")
            + "\nRender Pipeline Enum: " + SRPHelper.projectRenderPipeline
            + "\nRender Pipeline Selected: " + renderPipeline
            + "\nRender Pipeline Symbol: " + SRPHelper.renderPipelineScriptingDefineSymbolAsString
            + "\nRendering Mode SD: " + GetActualRenderingMode(ShaderMode.SD)
            + "\nRendering Mode HD: " + GetActualRenderingMode(ShaderMode.HD)
            + "\nRendering Path: " + (Camera.main != null ? Camera.main.actualRenderingPath.ToString() : "no main camera")
            + "\nColor Space: " + QualitySettings.activeColorSpace
            ;
        }
#endif
    }
}