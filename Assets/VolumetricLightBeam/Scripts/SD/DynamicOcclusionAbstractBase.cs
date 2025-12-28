using UnityEngine;
using UnityEngine.Serialization;

namespace VLB
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeamSD))]
    public abstract class DynamicOcclusionAbstractBase : MonoBehaviour
    {
        public const string ClassName = "DynamicOcclusionAbstractBase";

        public DynamicOcclusionUpdateRate updateRate = Consts.DynOcclusion.UpdateRateDefault;

        [FormerlySerializedAs("waitFrameCount")]
        public int waitXFrames = Consts.DynOcclusion.WaitFramesCountDefault;

        public void ProcessOcclusionManually() { ProcessOcclusion(ProcessOcclusionSource.User); }

        public event System.Action onOcclusionProcessed;

        public static bool _INTERNAL_ApplyRandomFrameOffset = true;

        protected enum ProcessOcclusionSource
        {
            RenderLoop,
            OnEnable,
            EditorUpdate,
            User,
        }

        protected void ProcessOcclusion(ProcessOcclusionSource source)
        {
            if (!Config.Instance.featureEnabledDynamicOcclusion)
                return;

            if (m_LastFrameRendered == Time.frameCount && Application.isPlaying && source == ProcessOcclusionSource.OnEnable)
                return;

            Debug.Assert(!Application.isPlaying || m_LastFrameRendered != Time.frameCount, "ProcessOcclusion has been called twice on the same frame, which is forbidden");
            Debug.Assert(m_Master);

            bool occlusionSuccess = OnProcessOcclusion(source);

            if(onOcclusionProcessed != null)
                onOcclusionProcessed();

            if (m_Master)
            {
                Debug.Assert(m_MaterialModifierCallbackCached != null);
                m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), occlusionSuccess ? m_MaterialModifierCallbackCached : (MaterialModifier.Callback)(null));
            }

            if (updateRate.HasFlag(DynamicOcclusionUpdateRate.OnBeamMove))
                m_TransformPacked = transform.GetWorldPacked();

            bool firstTime = m_LastFrameRendered < 0;
            m_LastFrameRendered = Time.frameCount;

            if (firstTime && _INTERNAL_ApplyRandomFrameOffset)
            {
                m_LastFrameRendered += Random.Range(0, waitXFrames);
            }
        }

        TransformUtils.Packed m_TransformPacked;
        int m_LastFrameRendered = int.MinValue;
        public int _INTERNAL_LastFrameRendered { get { return m_LastFrameRendered; } }
        protected VolumetricLightBeamSD m_Master = null;
        protected MaterialModifier.Callback m_MaterialModifierCallbackCached = null;

        protected abstract string GetShaderKeyword();
        protected abstract MaterialManager.SD.DynamicOcclusion GetDynamicOcclusionMode();

        protected abstract bool OnProcessOcclusion(ProcessOcclusionSource source);
        protected abstract void OnModifyMaterialCallback(MaterialModifier.Interface owner);
        protected abstract void OnEnablePostValidate();

        protected virtual void OnValidateProperties()
        {
            waitXFrames = Mathf.Clamp(waitXFrames, 1, 60);
        }

        protected virtual void Awake()
        {
            m_Master = GetComponent<VolumetricLightBeamSD>();
            Debug.Assert(m_Master);

            m_Master._INTERNAL_DynamicOcclusionMode = GetDynamicOcclusionMode();
        }

        protected virtual void OnDestroy()
        {
            m_Master._INTERNAL_DynamicOcclusionMode = MaterialManager.SD.DynamicOcclusion.Off;
            DisableOcclusion();
        }

        protected virtual void OnEnable()
        {

            m_MaterialModifierCallbackCached = OnModifyMaterialCallback;

            OnValidateProperties();

            OnEnablePostValidate();

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                m_Master.onWillCameraRenderThisBeam += OnWillCameraRender;

                if (!updateRate.HasFlag(DynamicOcclusionUpdateRate.Never))
                    m_Master.RegisterOnBeamGeometryInitializedCallback(() => ProcessOcclusion(ProcessOcclusionSource.OnEnable));
            }
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                m_Master.onWillCameraRenderThisBeam -= OnWillCameraRender;
            }

            DisableOcclusion();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            OnValidateProperties();
        }
#endif

        void OnWillCameraRender(Camera cam)
        {
            Debug.Assert(Application.isPlaying);

            if (cam != null && cam.enabled
                && Time.frameCount != m_LastFrameRendered)
            {
                bool shouldUpdate = false;

                if (!shouldUpdate && updateRate.HasFlag(DynamicOcclusionUpdateRate.OnBeamMove))
                {
                    if (!m_TransformPacked.IsSame(transform))
                        shouldUpdate = true;
                }

                if (!shouldUpdate && updateRate.HasFlag(DynamicOcclusionUpdateRate.EveryXFrames))
                {
                    if (Time.frameCount >= m_LastFrameRendered + waitXFrames)
                        shouldUpdate = true;
                }

                if (shouldUpdate)
                    ProcessOcclusion(ProcessOcclusionSource.RenderLoop);
            }
        }

        void DisableOcclusion()
        {
            m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), null);
        }
    }
}