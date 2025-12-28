
#if UNITY_2019_3_OR_NEWER
#define VLB_LIGHT_TEMPERATURE_SUPPORT
#endif

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace VLB
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [SelectionBase]
    [HelpURL(Consts.Help.HD.UrlBeam)]
    [AddComponentMenu(Consts.Help.HD.AddComponentMenuBeam3D)]
    public partial class VolumetricLightBeamHD : VolumetricLightBeamAbstractBase
    {
        public new const string ClassName = "VolumetricLightBeamHD";

        public bool colorFromLight
        {
            get { return m_ColorFromLight; }
            set { if (m_ColorFromLight != value) { m_ColorFromLight = value; ValidateProperties(); } }
        }

        public ColorMode colorMode
        {
            get {
                if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off) return ColorMode.Flat;
                return m_ColorMode;
            }
            set { if (m_ColorMode != value) { m_ColorMode = value; ValidateProperties(); SetPropertyDirty(DirtyProps.ColorMode); } }
        }

        public Color colorFlat
        {
            get { return m_ColorFlat; }
            set { if (m_ColorFlat != value) { m_ColorFlat = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Color); } }
        }

        public Gradient colorGradient
        {
            get { return m_ColorGradient; }
            set { if (m_ColorGradient != value) { m_ColorGradient = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Color); } }
        }

#if UNITY_EDITOR
        public override Color ComputeColorAtDepth(float depthRatio)
        {
            if (colorMode == ColorMode.Flat) return colorFlat;
            else return colorGradient.Evaluate(depthRatio);
        }
#endif

        bool useColorFromAttachedLightSpot => colorFromLight && lightSpotAttached != null;

#if VLB_LIGHT_TEMPERATURE_SUPPORT
        bool useColorTemperatureFromAttachedLightSpot => useColorFromAttachedLightSpot && lightSpotAttached.useColorTemperature && Config.Instance.useLightColorTemperature;
#else
        bool useColorTemperatureFromAttachedLightSpot => false;
#endif

        public float intensity
        {
            get { return m_Intensity; }
            set { if (m_Intensity != value) { m_Intensity = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Intensity); } }
        }

        public float intensityMultiplier
        {
            get { return m_IntensityMultiplier; }
            set { if (m_IntensityMultiplier != value) { m_IntensityMultiplier = value; ValidateProperties(); } }
        }

        public bool useIntensityFromAttachedLightSpot
        {
            get { return intensityMultiplier >= 0.0f && lightSpotAttached != null; }
            set { intensityMultiplier = (value ? 1.0f : -1.0f) * Mathf.Abs(intensityMultiplier); }
        }

        public float hdrpExposureWeight
        {
            get { return m_HDRPExposureWeight; }
            set { if (m_HDRPExposureWeight != value) { m_HDRPExposureWeight = value; ValidateProperties(); SetPropertyDirty(DirtyProps.HDRPExposureWeight); } }
        }

        public BlendingMode blendingMode
        {
            get { return m_BlendingMode; }
            set { if (m_BlendingMode != value) { m_BlendingMode = value; ValidateProperties(); SetPropertyDirty(DirtyProps.BlendingMode); } }
        }

        public float spotAngle
        {
            get { return m_SpotAngle; }
            set { if (m_SpotAngle != value) { m_SpotAngle = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Cone); } }
        }

        public float spotAngleMultiplier
        {
            get { return m_SpotAngleMultiplier; }
            set { if (m_SpotAngleMultiplier != value) { m_SpotAngleMultiplier = value; ValidateProperties(); } }
        }

        public bool useSpotAngleFromAttachedLightSpot
        {
            get { return spotAngleMultiplier >= 0.0f && lightSpotAttached != null; }
            set { spotAngleMultiplier = (value ? 1.0f : -1.0f) * Mathf.Abs(spotAngleMultiplier); }
        }

        public float coneAngle { get { return Mathf.Atan2(coneRadiusEnd - coneRadiusStart, maxGeometryDistance) * Mathf.Rad2Deg * 2f; } }

        public float coneRadiusStart
        {
            get { return m_ConeRadiusStart; }
            set { if (m_ConeRadiusStart != value) { m_ConeRadiusStart = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Cone); } }
        }

        public float coneRadiusEnd {
            get { return Utils.ComputeConeRadiusEnd(maxGeometryDistance, spotAngle); }
            set { spotAngle = Utils.ComputeSpotAngle(maxGeometryDistance, value); }
        }

        public float coneVolume { get { float r1 = coneRadiusStart, r2 = coneRadiusEnd; return (Mathf.PI / 3) * (r1 * r1 + r1 * r2 + r2 * r2) * fallOffEnd; } }

        public float GetConeApexOffsetZ(bool counterApplyScaleForUnscalableBeam)
        {

            float ratioRadius = coneRadiusStart / coneRadiusEnd;
            if (ratioRadius == 1f)
                return float.MaxValue;
            else
            {
                float value = ((maxGeometryDistance * ratioRadius) / (1 - ratioRadius));
                if(counterApplyScaleForUnscalableBeam && !scalable) value /= GetLossyScale().z;
                return value;
            }
        }

        public bool scalable {
            get { return m_Scalable; }
            set { if (m_Scalable != value) { m_Scalable = value; SetPropertyDirty(DirtyProps.Attenuation); } }
        }

        public override bool IsScalable() { return scalable; }

        public AttenuationEquationHD attenuationEquation
        {
            get { return m_AttenuationEquation; }
            set { if (m_AttenuationEquation != value) { m_AttenuationEquation = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Attenuation); } }
        }

        public float fallOffStart
        {
            get { return m_FallOffStart; }
            set { if (m_FallOffStart != value) { m_FallOffStart = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Cone); } }
        }

        public float fallOffEnd
        {
            get { return m_FallOffEnd; }
            set { if (m_FallOffEnd != value) { m_FallOffEnd = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Cone); } }
        }

        public float maxGeometryDistance { get { return fallOffEnd; } }

        public float fallOffEndMultiplier
        {
            get { return m_FallOffEndMultiplier; }
            set { if (m_FallOffEndMultiplier != value) { m_FallOffEndMultiplier = value; ValidateProperties(); } }
        }

        public bool useFallOffEndFromAttachedLightSpot
        {
            get { return fallOffEndMultiplier >= 0f && lightSpotAttached != null; }
            set { fallOffEndMultiplier = (value ? 1.0f : -1.0f) * Mathf.Abs(fallOffEndMultiplier); }
        }

        public float sideSoftness
        {
            get { return m_SideSoftness; }
            set { if (m_SideSoftness != value) { m_SideSoftness = value; ValidateProperties(); SetPropertyDirty(DirtyProps.SideSoftness); } }
        }

        public float jitteringFactor
        {
            get { return m_JitteringFactor; }
            set { if (m_JitteringFactor != value) { m_JitteringFactor = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Jittering); } }
        }

        public int jitteringFrameRate
        {
            get { return m_JitteringFrameRate; }
            set { if (m_JitteringFrameRate != value) { m_JitteringFrameRate = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Jittering); } }
        }

        public MinMaxRangeFloat jitteringLerpRange
        {
            get { return m_JitteringLerpRange; }
            set { if (m_JitteringLerpRange != value) { m_JitteringLerpRange = value; ValidateProperties(); SetPropertyDirty(DirtyProps.Jittering); } }
        }

        public NoiseMode noiseMode
        {
            get { return m_NoiseMode; }
            set { if (m_NoiseMode != value) { m_NoiseMode = value; ValidateProperties(); SetPropertyDirty(DirtyProps.NoiseMode); } }
        }

        public bool isNoiseEnabled { get { return noiseMode != NoiseMode.Disabled; } }

        public float noiseIntensity
        {
            get { return m_NoiseIntensity; }
            set { if (m_NoiseIntensity != value) { m_NoiseIntensity = value; ValidateProperties(); SetPropertyDirty(DirtyProps.NoiseIntensity); } }
        }

        public bool noiseScaleUseGlobal
        {
            get { return m_NoiseScaleUseGlobal; }
            set { if (m_NoiseScaleUseGlobal != value) { m_NoiseScaleUseGlobal = value; ValidateProperties(); SetPropertyDirty(DirtyProps.NoiseVelocityAndScale); } }
        }

        public float noiseScaleLocal
        {
            get { return m_NoiseScaleLocal; }
            set { if (m_NoiseScaleLocal != value) { m_NoiseScaleLocal = value; ValidateProperties(); SetPropertyDirty(DirtyProps.NoiseVelocityAndScale); } }
        }

        public bool noiseVelocityUseGlobal
        {
            get { return m_NoiseVelocityUseGlobal; }
            set { if (m_NoiseVelocityUseGlobal != value) { m_NoiseVelocityUseGlobal = value; ValidateProperties(); SetPropertyDirty(DirtyProps.NoiseVelocityAndScale); } }
        }

        public Vector3 noiseVelocityLocal
        {
            get { return m_NoiseVelocityLocal; }
            set { if (m_NoiseVelocityLocal != value) { m_NoiseVelocityLocal = value; ValidateProperties(); SetPropertyDirty(DirtyProps.NoiseVelocityAndScale); } }
        }

        public int raymarchingQualityID
        {
            get { return m_RaymarchingQualityID; }
            set { if (m_RaymarchingQualityID != value) { m_RaymarchingQualityID = value; ValidateProperties(); SetPropertyDirty(DirtyProps.RaymarchingQuality); } }
        }

        public int raymarchingQualityIndex
        {
            get { return Config.Instance.GetRaymarchingQualityIndexForUniqueID(raymarchingQualityID); }
            set { raymarchingQualityID = Config.Instance.GetRaymarchingQualityForIndex(raymarchingQualityIndex).uniqueID; }
        }

        public override BeamGeometryAbstractBase GetBeamGeometry() { return m_BeamGeom; }
        protected override void SetBeamGeometryNull() { m_BeamGeom = null; }

        public int blendingModeAsInt { get { return Mathf.Clamp((int)blendingMode, 0, System.Enum.GetValues(typeof(BlendingMode)).Length); } }

        public Quaternion beamInternalLocalRotation { get { return GetDimensions() == Dimensions.Dim3D ? Quaternion.identity : Quaternion.LookRotation(Vector3.right, Vector3.up); } }
        public Vector3 beamLocalForward { get { return GetDimensions() == Dimensions.Dim3D ? Vector3.forward : Vector3.right; } }
        public Vector3 beamGlobalForward { get { return transform.TransformDirection(beamLocalForward); } }
        public override Vector3 GetLossyScale() { return GetDimensions() == Dimensions.Dim3D ? transform.lossyScale : new Vector3(transform.lossyScale.z, transform.lossyScale.y, transform.lossyScale.x); }

        public VolumetricCookieHD GetAdditionalComponentCookie() { return GetComponent<VolumetricCookieHD>(); }
        public VolumetricShadowHD GetAdditionalComponentShadow() { return GetComponent<VolumetricShadowHD>(); }

        public void SetPropertyDirty(DirtyProps flags)
        {
            if (m_BeamGeom) m_BeamGeom.SetPropertyDirty(flags);
        }

        public virtual Dimensions GetDimensions() { return Dimensions.Dim3D; }
        public virtual bool DoesSupportSorting2D() { return false; }
        public virtual int GetSortingLayerID() { return 0; }
        public virtual int GetSortingOrder() { return 0; }

        [SerializeField] bool m_ColorFromLight = true;
        [SerializeField] ColorMode m_ColorMode = Consts.Beam.ColorModeDefault;
        [SerializeField] Color m_ColorFlat = Consts.Beam.FlatColor;
        [SerializeField] Gradient m_ColorGradient;
        [SerializeField] BlendingMode m_BlendingMode = Consts.Beam.BlendingModeDefault;
        [SerializeField] float m_Intensity = Consts.Beam.IntensityDefault;
        [SerializeField] float m_IntensityMultiplier = Consts.Beam.MultiplierDefault;
        [SerializeField] float m_HDRPExposureWeight = Consts.Beam.HDRPExposureWeightDefault;
        [SerializeField] float m_SpotAngle = Consts.Beam.SpotAngleDefault;
        [SerializeField] float m_SpotAngleMultiplier = Consts.Beam.MultiplierDefault;
        [SerializeField] float m_ConeRadiusStart = Consts.Beam.ConeRadiusStart;
        [SerializeField] bool m_Scalable = Consts.Beam.ScalableDefault;
        [SerializeField] float m_FallOffStart = Consts.Beam.FallOffStart;
        [SerializeField] float m_FallOffEnd = Consts.Beam.FallOffEnd;
        [SerializeField] float m_FallOffEndMultiplier = Consts.Beam.MultiplierDefault;
        [SerializeField] AttenuationEquationHD m_AttenuationEquation = Consts.Beam.HD.AttenuationEquationDefault;
        [SerializeField] float m_SideSoftness = Consts.Beam.HD.SideSoftnessDefault;
        [SerializeField] int m_RaymarchingQualityID = -1;
        [SerializeField] float m_JitteringFactor = Consts.Beam.HD.JitteringFactorDefault;
        [SerializeField] int m_JitteringFrameRate = Consts.Beam.HD.JitteringFrameRateDefault;
        [MinMaxRange(0.0f, 1.0f)] [SerializeField] MinMaxRangeFloat m_JitteringLerpRange = Consts.Beam.HD.JitteringLerpRange;
        [SerializeField] NoiseMode m_NoiseMode = Consts.Beam.NoiseModeDefault;
        [SerializeField] float m_NoiseIntensity = Consts.Beam.NoiseIntensityDefault;
        [SerializeField] bool m_NoiseScaleUseGlobal = true;
        [SerializeField] float m_NoiseScaleLocal = Consts.Beam.NoiseScaleDefault;
        [SerializeField] bool m_NoiseVelocityUseGlobal = true;
        [SerializeField] Vector3 m_NoiseVelocityLocal = Consts.Beam.NoiseVelocityDefault;

        public uint _INTERNAL_InstancedMaterialGroupID { get; protected set; }

        protected BeamGeometryHD m_BeamGeom = null;

#if UNITY_EDITOR
        public BeamGeometryHD _EDITOR_GetBeamGeometry() { return m_BeamGeom; }
        public override int _EDITOR_GetInstancedMaterialID() { return m_BeamGeom ? m_BeamGeom._EDITOR_InstancedMaterialID : int.MinValue; }

        static VolumetricLightBeamHD[] _EditorFindAllInstances()
        {
            return Resources.FindObjectsOfTypeAll<VolumetricLightBeamHD>();
        }

        public static void _EditorSetAllMeshesDirty()
        {
            foreach (var instance in _EditorFindAllInstances())
                instance._EditorSetMeshDirty();
        }

        public static void _EditorSetAllBeamGeomDirty()
        {
            foreach (var instance in _EditorFindAllInstances())
                instance.m_EditorDirtyFlags |= EditorDirtyFlags.FullBeamGeomGAO;
        }
#endif

        public float GetInsideBeamFactor(Vector3 posWS) { return GetInsideBeamFactorFromObjectSpacePos(transform.InverseTransformPoint(posWS)); }

        public float GetInsideBeamFactorFromObjectSpacePos(Vector3 posOS)
        {
            if(GetDimensions() == Dimensions.Dim2D)
            {
                posOS = new Vector3(posOS.z, posOS.y, posOS.x);
            }

            if (posOS.z < 0f) return -1f;

            Vector2 posOSXY = posOS.xy();

            var triangle2D = new Vector2(posOSXY.magnitude, posOS.z + GetConeApexOffsetZ(true)).normalized;
            const float maxRadiansDiff = 0.1f;
            float slopeRad = (coneAngle * Mathf.Deg2Rad) / 2;

            return Mathf.Clamp((Mathf.Abs(Mathf.Sin(slopeRad)) - Mathf.Abs(triangle2D.x)) / maxRadiansDiff, -1, 1);
        }

        public override void GenerateGeometry()
        {
            if(pluginVersion == -1)
            {

                raymarchingQualityID = Config.Instance.defaultRaymarchingQualityUniqueID;
            }

            if (!Config.Instance.IsRaymarchingQualityUniqueIDValid(raymarchingQualityID))
            {
                Debug.LogErrorFormat(gameObject, "HD Beam '{0}': fallback to default quality '{1}'"
                    , name
                    , Config.Instance.GetRaymarchingQualityForUniqueID(Config.Instance.defaultRaymarchingQualityUniqueID).name
                    );
                raymarchingQualityID = Config.Instance.defaultRaymarchingQualityUniqueID;
                Utils.MarkCurrentSceneDirty();
            }

            HandleBackwardCompatibility(pluginVersion, Version.Current);
            pluginVersion = Version.Current;

            ValidateProperties();

            if (m_BeamGeom == null)
            {
                m_BeamGeom = Utils.NewWithComponent<BeamGeometryHD>("Beam Geometry");
                m_BeamGeom.Initialize(this);
            }

            m_BeamGeom.RegenerateMesh();
            m_BeamGeom.visible = enabled;

            base.GenerateGeometry();
        }

        public virtual void UpdateAfterManualPropertyChange()
        {
            ValidateProperties();
            SetPropertyDirty(DirtyProps.All);
        }

#if !UNITY_EDITOR
        void Start()
        {
            InitLightSpotAttachedCached();

            GenerateGeometry();
        }
#else
        void Start()
        {
            if (Application.isPlaying)
            {
                InitLightSpotAttachedCached();
                GenerateGeometry();
                m_EditorDirtyFlags = EditorDirtyFlags.Clean;
            }
            else
            {

                m_EditorDirtyFlags = EditorDirtyFlags.Everything;
            }
        }

        void OnValidate()
        {
            m_EditorDirtyFlags |= EditorDirtyFlags.Props;
        }

        void Update()
        {
            EditorHandleLightPropertiesUpdate();

            if (m_EditorDirtyFlags == EditorDirtyFlags.Clean)
            {
                if (Application.isPlaying)
                {
                    return;
                }
            }
            else
            {
                if (m_EditorDirtyFlags.HasFlag(EditorDirtyFlags.Mesh))
                {
                    if (m_EditorDirtyFlags.HasFlag(EditorDirtyFlags.BeamGeomGAO))
                        DestroyBeam();

                    GenerateGeometry();
                }
                else if (m_EditorDirtyFlags.HasFlag(EditorDirtyFlags.Props))
                {
                    ValidateProperties();
                }
            }

            UpdateAfterManualPropertyChange();

            m_EditorDirtyFlags = EditorDirtyFlags.Clean;
        }

        public override void CopyPropsFrom(VolumetricLightBeamAbstractBase beamSrc, BeamProps beamProps)
        {
            base.CopyPropsFrom(beamSrc, beamProps);

            if (beamSrc is VolumetricLightBeamSD)
            {
                var beamSD = beamSrc as VolumetricLightBeamSD;
                if (beamProps.HasFlag(BeamProps.Color))         { colorMode = beamSD.colorMode; colorFlat = beamSD.color; colorGradient = beamSD.colorGradient; }
                if (beamProps.HasFlag(BeamProps.BlendingMode))  { blendingMode = beamSD.blendingMode; }
                if (beamProps.HasFlag(BeamProps.Intensity))     { intensity = beamSD.intensityGlobal; intensityMultiplier = beamSD.intensityMultiplier; }
                if (beamProps.HasFlag(BeamProps.FallOffAttenuation))
                {
                    attenuationEquation = UtilsBeamProps.ConvertAttenuation(beamSD.attenuationEquation);
                    fallOffStart = beamSD.fallOffStart;
                }
                if (beamProps.HasFlag(BeamProps.SpotShape))
                {
                    spotAngle = beamSD.spotAngle; spotAngleMultiplier = beamSD.spotAngleMultiplier;
                    coneRadiusStart = beamSD.coneRadiusStart;
                    fallOffEnd = beamSD.fallOffEnd; fallOffEndMultiplier = beamSD.fallOffEndMultiplier;
                }
                if (beamProps.HasFlag(BeamProps.Noise3D))
                {
                    noiseMode = beamSD.noiseMode; noiseIntensity = beamSD.noiseIntensity;
                    noiseScaleUseGlobal = beamSD.noiseScaleUseGlobal; noiseScaleLocal = beamSD.noiseScaleLocal;
                    noiseVelocityUseGlobal = beamSD.noiseVelocityUseGlobal; noiseVelocityLocal = beamSD.noiseVelocityLocal;
                }
            }
            else if (beamSrc is VolumetricLightBeamHD)
            {
                var beamHD = beamSrc as VolumetricLightBeamHD;
                if (beamProps.HasFlag(BeamProps.Color))         { colorMode = beamHD.colorMode; colorFlat = beamHD.colorFlat; colorGradient = beamHD.colorGradient; }
                if (beamProps.HasFlag(BeamProps.BlendingMode))  { blendingMode = beamHD.blendingMode; }
                if (beamProps.HasFlag(BeamProps.Intensity))     { intensity = beamHD.intensity; intensityMultiplier = beamHD.intensityMultiplier; }
                if (beamProps.HasFlag(BeamProps.FallOffAttenuation))
                {
                    attenuationEquation = beamHD.attenuationEquation;
                    fallOffStart = beamHD.fallOffStart;
                }
                if (beamProps.HasFlag(BeamProps.SpotShape))
                {
                    spotAngle = beamHD.spotAngle; spotAngleMultiplier = beamHD.spotAngleMultiplier;
                    coneRadiusStart = beamHD.coneRadiusStart;
                    fallOffEnd = beamHD.fallOffEnd; fallOffEndMultiplier = beamHD.fallOffEndMultiplier;
                    scalable = beamHD.scalable;
                }
                if (beamProps.HasFlag(BeamProps.Noise3D))
                {
                    noiseMode = beamHD.noiseMode; noiseIntensity = beamHD.noiseIntensity;
                    noiseScaleUseGlobal = beamHD.noiseScaleUseGlobal; noiseScaleLocal = beamHD.noiseScaleLocal;
                    noiseVelocityUseGlobal = beamHD.noiseVelocityUseGlobal; noiseVelocityLocal = beamHD.noiseVelocityLocal;
                }
            }
        }

        public virtual void Reset()
        {
            m_ColorMode = Consts.Beam.ColorModeDefault;
            m_ColorFlat = Consts.Beam.FlatColor;
            m_ColorFromLight = true;

            m_Intensity = Consts.Beam.IntensityDefault;
            m_IntensityMultiplier = Consts.Beam.MultiplierDefault;

            m_HDRPExposureWeight = Consts.Beam.HDRPExposureWeightDefault;

            m_BlendingMode = Consts.Beam.BlendingModeDefault;

            m_SpotAngle = Consts.Beam.SpotAngleDefault;
            m_SpotAngleMultiplier = Consts.Beam.MultiplierDefault;

            m_ConeRadiusStart = Consts.Beam.ConeRadiusStart;
            m_Scalable = Consts.Beam.ScalableDefault;

            m_AttenuationEquation = Consts.Beam.HD.AttenuationEquationDefault;

            m_FallOffStart = Consts.Beam.FallOffStart;
            m_FallOffEnd = Consts.Beam.FallOffEnd;
            m_FallOffEndMultiplier = Consts.Beam.MultiplierDefault;

            m_SideSoftness = Consts.Beam.HD.SideSoftnessDefault;
            m_JitteringFactor = Consts.Beam.HD.JitteringFactorDefault;
            m_JitteringFrameRate = Consts.Beam.HD.JitteringFrameRateDefault;
            m_JitteringLerpRange = Consts.Beam.HD.JitteringLerpRange;

            m_NoiseMode = Consts.Beam.NoiseModeDefault;
            m_NoiseIntensity = Consts.Beam.NoiseIntensityDefault;
            m_NoiseScaleUseGlobal = true;
            m_NoiseScaleLocal = Consts.Beam.NoiseScaleDefault;
            m_NoiseVelocityUseGlobal = true;
            m_NoiseVelocityLocal = Consts.Beam.NoiseVelocityDefault;

            m_EditorDirtyFlags = EditorDirtyFlags.Everything;
        }
#endif

        void OnEnable()
        {
            if (m_BeamGeom) m_BeamGeom.visible = true;
        }

        void OnDisable()
        {
            if (m_BeamGeom) m_BeamGeom.visible = false;
        }

        void OnDidApplyAnimationProperties()
        {
            AssignPropertiesFromAttachedSpotLight();
            UpdateAfterManualPropertyChange();
        }

        public void AssignPropertiesFromAttachedSpotLight()
        {
            var lightSpot = lightSpotAttached;
            if (lightSpot)
            {
                Debug.AssertFormat(lightSpot.type == LightType.Spot, "Light attached to {0} '{1}' must be a Spot", ClassName, name);
                if (useIntensityFromAttachedLightSpot) intensity = SpotLightHelper.GetIntensity(lightSpot) * intensityMultiplier;
                if (useFallOffEndFromAttachedLightSpot) fallOffEnd = SpotLightHelper.GetFallOffEnd(lightSpot) * fallOffEndMultiplier;
                if (useSpotAngleFromAttachedLightSpot) spotAngle = Mathf.Clamp(SpotLightHelper.GetSpotAngle(lightSpot) * spotAngleMultiplier, Consts.Beam.SpotAngleMin, Consts.Beam.SpotAngleMax);
                if (m_ColorFromLight)
                {
                    colorMode = ColorMode.Flat;

#if VLB_LIGHT_TEMPERATURE_SUPPORT
                    if (useColorTemperatureFromAttachedLightSpot)
                    {
                        Color colorFromTemp = Mathf.CorrelatedColorTemperatureToRGB(lightSpot.colorTemperature);
                        var finalColor = lightSpot.color.linear * colorFromTemp;
                        colorFlat = finalColor.gamma;
                    }
                    else
#endif
                    {
                        colorFlat = lightSpot.color;
                    }
                }
            }
        }

        void ClampProperties()
        {
            m_Intensity = Mathf.Max(m_Intensity, Consts.Beam.IntensityMin);

            m_FallOffEnd = Mathf.Max(Consts.Beam.FallOffDistancesMinThreshold, m_FallOffEnd);
            m_FallOffStart = Mathf.Clamp(m_FallOffStart, 0f, m_FallOffEnd - Consts.Beam.FallOffDistancesMinThreshold);

            m_SpotAngle = Mathf.Clamp(m_SpotAngle, Consts.Beam.SpotAngleMin, Consts.Beam.SpotAngleMax);
            m_ConeRadiusStart = Mathf.Max(m_ConeRadiusStart, 0f);

            m_SideSoftness = Mathf.Clamp(m_SideSoftness, Consts.Beam.HD.SideSoftnessMin, Consts.Beam.HD.SideSoftnessMax);
            m_JitteringFactor = Mathf.Max(m_JitteringFactor, Consts.Beam.HD.JitteringFactorMin);
            m_JitteringFrameRate = Mathf.Clamp(m_JitteringFrameRate, Consts.Beam.HD.JitteringFrameRateMin, Consts.Beam.HD.JitteringFrameRateMax);

            m_NoiseIntensity = Mathf.Clamp(m_NoiseIntensity, Consts.Beam.NoiseIntensityMin, Consts.Beam.NoiseIntensityMax);

        }

        void ValidateProperties()
        {
            AssignPropertiesFromAttachedSpotLight();
            ClampProperties();
        }

        void HandleBackwardCompatibility(int serializedVersion, int newVersion)
        {
            if (serializedVersion == -1) return;
            if (serializedVersion == newVersion) return;

        }

#if UNITY_EDITOR && DEBUG_SHOW_APEX
        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(0, 0, -GetConeApexOffsetZ(true)), 0.25f);
        }
#endif
    }
}