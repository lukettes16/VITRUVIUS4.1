using UnityEngine;

namespace VLB
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeamHD))]
    [HelpURL(Consts.Help.HD.UrlShadow)]
    [AddComponentMenu(Consts.Help.HD.AddComponentMenuShadow)]
    public class VolumetricShadowHD : MonoBehaviour
    {
        public const string ClassName = "VolumetricShadowHD";

        public float strength
        {
            get { return m_Strength; }
            set { if (m_Strength != value) { m_Strength = value; SetDirty(); } }
        }

        public ShadowUpdateRate updateRate
        {
            get { return m_UpdateRate; }
            set { m_UpdateRate = value; }
        }

        public int waitXFrames
        {
            get { return m_WaitXFrames; }
            set { m_WaitXFrames = value; }
        }

        public LayerMask layerMask
        {
            get { return m_LayerMask; }
            set
            {
                m_LayerMask = value;
                UpdateDepthCameraProperties();
            }
        }

        public bool useOcclusionCulling
        {
            get { return m_UseOcclusionCulling; }
            set
            {
                m_UseOcclusionCulling = value;
                UpdateDepthCameraProperties();
            }
        }

        public int depthMapResolution
        {
            get { return m_DepthMapResolution; }
            set
            {
                if(m_DepthCamera != null && Application.isPlaying) { Debug.LogErrorFormat(Consts.Shadow.GetErrorChangeRuntimeDepthMapResolution(this)); }
                m_DepthMapResolution = value;
            }
        }

        public int depthMapDepth
        {
            get { return m_DepthMapDepth; }
            set
            {
                if (m_DepthCamera != null && Application.isPlaying) { Debug.LogErrorFormat(Consts.Shadow.GetErrorChangeRuntimeDepthMapDepth(this)); }
                m_DepthMapDepth = value;
            }
        }

        public void ProcessOcclusionManually() { ProcessOcclusion(ProcessOcclusionSource.User); }

        [SerializeField] float m_Strength = Consts.Shadow.StrengthDefault;
        [SerializeField] ShadowUpdateRate m_UpdateRate = Consts.Shadow.UpdateRateDefault;
        [SerializeField] int m_WaitXFrames = Consts.Shadow.WaitFramesCountDefault;
        [SerializeField] LayerMask m_LayerMask = Consts.Shadow.LayerMaskDefault;
        [SerializeField] bool m_UseOcclusionCulling = Consts.Shadow.OcclusionCullingDefault;
        [SerializeField] int m_DepthMapResolution = Consts.Shadow.DepthMapResolutionDefault;
        [SerializeField] int m_DepthMapDepth = Consts.Shadow.DepthMapDepthDefault;

        public void UpdateDepthCameraProperties()
        {
            if (m_DepthCamera)
            {
                m_DepthCamera.cullingMask = layerMask;
                m_DepthCamera.useOcclusionCulling = useOcclusionCulling;
            }
        }

        enum ProcessOcclusionSource
        {
            RenderLoop,
            OnEnable,
            EditorUpdate,
            User,
        }

        void ProcessOcclusion(ProcessOcclusionSource source)
        {
            if (!Config.Instance.featureEnabledShadow)
                return;

            if(m_LastFrameRendered == Time.frameCount && Application.isPlaying && source == ProcessOcclusionSource.OnEnable)
                return;

            Debug.Assert(!Application.isPlaying || m_LastFrameRendered != Time.frameCount, "ProcessOcclusion has been called twice on the same frame, which is forbidden");
            Debug.Assert(m_Master && m_DepthCamera);

            if (SRPHelper.IsUsingCustomRenderPipeline())
                m_NeedToUpdateOcclusionNextFrame = true;
            else
                ProcessOcclusionInternal();

            SetDirty();

            if (updateRate.HasFlag(ShadowUpdateRate.OnBeamMove))
                m_TransformPacked = transform.GetWorldPacked();

            bool firstTime = m_LastFrameRendered < 0;
            m_LastFrameRendered = Time.frameCount;

            if (firstTime && _INTERNAL_ApplyRandomFrameOffset)
            {
                m_LastFrameRendered += Random.Range(0, waitXFrames);
            }
        }

        public static void ApplyMaterialProperties(VolumetricShadowHD instance, BeamGeometryHD geom)
        {
            Debug.Assert(geom != null);

            if (instance && instance.enabled)
            {
                Debug.Assert(instance.m_DepthCamera);
                geom.SetMaterialProp(ShaderProperties.HD.ShadowDepthTexture, instance.m_DepthCamera.targetTexture);

                var scale = instance.m_Master.scalable ? instance.m_Master.GetLossyScale() : Vector3.one;
                geom.SetMaterialProp(ShaderProperties.HD.ShadowProps, new Vector4(Mathf.Sign(scale.x) * Mathf.Sign(scale.z), Mathf.Sign(scale.y), instance.m_Strength, instance.m_DepthCamera.orthographic ? 0f : 1f));
            }
            else
            {
                geom.SetMaterialProp(ShaderProperties.HD.ShadowDepthTexture, BeamGeometryHD.InvalidTexture.NoDepth);
            }
        }

        void Awake()
        {
            m_Master = GetComponent<VolumetricLightBeamHD>();
            Debug.Assert(m_Master);

#if UNITY_EDITOR
            MarkMaterialAsDirty();
#endif
        }

        void OnEnable()
        {
            OnValidateProperties();
            InstantiateOrActivateDepthCamera();
            OnBeamEnabled();
        }

        void OnDisable()
        {
            if (m_DepthCamera) m_DepthCamera.gameObject.SetActive(false);
            SetDirty();
        }

        void OnDestroy()
        {
            DestroyDepthCamera();

#if UNITY_EDITOR
            MarkMaterialAsDirty();
#endif
        }

        void ProcessOcclusionInternal()
        {
            UpdateDepthCameraPropertiesAccordingToBeam();
            m_DepthCamera.Render();
        }

        void OnBeamEnabled()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) { return; }
#endif
            if (!enabled) { return; }

            if (!updateRate.HasFlag(ShadowUpdateRate.Never))
                ProcessOcclusion(ProcessOcclusionSource.OnEnable);
        }

        public void OnWillCameraRenderThisBeam(Camera cam, BeamGeometryHD beamGeom)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) { return; }
#endif
            if (!enabled) { return; }

            if(cam != null
            && cam.enabled
            && Time.frameCount != m_LastFrameRendered
            && updateRate != ShadowUpdateRate.Never)
            {
                bool shouldUpdate = false;

                if (!shouldUpdate && updateRate.HasFlag(ShadowUpdateRate.OnBeamMove))
                {
                    if (!m_TransformPacked.IsSame(transform))
                        shouldUpdate = true;
                }

                if (!shouldUpdate && updateRate.HasFlag(ShadowUpdateRate.EveryXFrames))
                {
                    if (Time.frameCount >= m_LastFrameRendered + waitXFrames)
                        shouldUpdate = true;
                }

                if (shouldUpdate)
                    ProcessOcclusion(ProcessOcclusionSource.RenderLoop);
            }
        }

        void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (m_NeedToReinstantiateDepthCamera)
                {
                    DestroyDepthCamera();
                    InstantiateOrActivateDepthCamera();
                    m_NeedToReinstantiateDepthCamera = false;
                }

                if (m_Master && m_Master.enabled)
                    ProcessOcclusion(ProcessOcclusionSource.EditorUpdate);
            }
#endif

            if (m_NeedToUpdateOcclusionNextFrame && m_Master && m_DepthCamera
                && Time.frameCount > 1)
            {
                ProcessOcclusionInternal();
                m_NeedToUpdateOcclusionNextFrame = false;
            }
        }

        void UpdateDepthCameraPropertiesAccordingToBeam()
        {
            Debug.Assert(m_Master);

            Utils.SetupDepthCamera(m_DepthCamera
                , m_Master.GetConeApexOffsetZ(true), m_Master.maxGeometryDistance, m_Master.coneRadiusStart, m_Master.coneRadiusEnd
                , m_Master.beamLocalForward, m_Master.GetLossyScale(), m_Master.scalable, m_Master.beamInternalLocalRotation
                , false);
        }

        void InstantiateOrActivateDepthCamera()
        {
            if (m_DepthCamera != null)
            {
                m_DepthCamera.gameObject.SetActive(true);
            }
            else
            {

                gameObject.ForeachComponentsInDirectChildrenOnly<Camera>(cam => DestroyImmediate(cam.gameObject), true);

                m_DepthCamera = Utils.NewWithComponent<Camera>("Depth Camera");

                if (m_DepthCamera && m_Master)
                {
                    m_DepthCamera.enabled = false;

                    UpdateDepthCameraProperties();
                    m_DepthCamera.clearFlags = CameraClearFlags.Depth;
                    m_DepthCamera.depthTextureMode = DepthTextureMode.Depth;
                    m_DepthCamera.renderingPath = RenderingPath.Forward;
                    m_DepthCamera.gameObject.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
                    m_DepthCamera.transform.SetParent(transform, false);
                    Config.Instance.SetURPScriptableRendererIndexToDepthCamera(m_DepthCamera);

                    var rt = new RenderTexture(depthMapResolution, depthMapResolution, depthMapDepth, RenderTextureFormat.Depth);
                    m_DepthCamera.targetTexture = rt;

                    UpdateDepthCameraPropertiesAccordingToBeam();

#if UNITY_EDITOR
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(m_DepthCamera.gameObject, m_Master.GetStaticEditorFlagsForSubObjects());
                    m_DepthCamera.gameObject.SetSameSceneVisibilityStatesThan(m_Master.gameObject);
#endif
                }
            }
        }

        void DestroyDepthCamera()
        {
            if (m_DepthCamera)
            {
                if (m_DepthCamera.targetTexture)
                {
                    m_DepthCamera.targetTexture.Release();
                    DestroyImmediate(m_DepthCamera.targetTexture);
                    m_DepthCamera.targetTexture = null;
                }

                DestroyImmediate(m_DepthCamera.gameObject);
                m_DepthCamera = null;
            }
        }

        void OnValidateProperties()
        {
            m_WaitXFrames = Mathf.Clamp(m_WaitXFrames, 1, 60);
            m_DepthMapResolution = Mathf.Clamp(Mathf.NextPowerOfTwo(m_DepthMapResolution), 8, 2048);
            m_DepthMapDepth = (Mathf.Clamp(m_DepthMapDepth, 16, 32) / 8) * 8;
        }

        void SetDirty()
        {
            if (m_Master)
                m_Master.SetPropertyDirty(DirtyProps.ShadowProps);
        }

        VolumetricLightBeamHD m_Master = null;
        TransformUtils.Packed m_TransformPacked;
        int m_LastFrameRendered = int.MinValue;
        public int _INTERNAL_LastFrameRendered { get { return m_LastFrameRendered; } }

        Camera m_DepthCamera = null;
        bool m_NeedToUpdateOcclusionNextFrame = false;

        static bool _INTERNAL_ApplyRandomFrameOffset = true;

        public static bool INTERNAL_GetApplyRandomFrameOffset() { return _INTERNAL_ApplyRandomFrameOffset; }

        public static void INTERNAL_EnableApplyRandomFrameOffset()
        {
            _INTERNAL_ApplyRandomFrameOffset = true;
        }

        public void INTERNAL_DisableApplyRandomFrameOffset()
        {
            _INTERNAL_ApplyRandomFrameOffset = false;
            m_LastFrameRendered = int.MinValue;
        }

#if UNITY_EDITOR
        bool m_NeedToReinstantiateDepthCamera = false;

        public void ForceReinstantiateDepthCamera()
        {
            m_NeedToReinstantiateDepthCamera = true;
        }

        void MarkMaterialAsDirty()
        {

            if (!Application.isPlaying)
                m_Master._EditorSetBeamGeomDirty();
        }

        void OnValidate()
        {
            OnValidateProperties();
            m_NeedToReinstantiateDepthCamera = true;
        }

        public bool HasLayerMaskIssues()
        {
            if (Config.Instance.geometryOverrideLayer)
            {
                int layerBit = 1 << Config.Instance.geometryLayerID;
                return ((layerMask.value & layerBit) == layerBit);
            }
            return false;
        }
#endif
    }
}