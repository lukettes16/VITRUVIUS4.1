

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
    [HelpURL(Consts.Help.SD.UrlBeam)]
    [AddComponentMenu(Consts.Help.SD.AddComponentMenuBeam)]
    public partial class VolumetricLightBeamSD : VolumetricLightBeamAbstractBase
    {
        public new const string ClassName = "VolumetricLightBeamSD";

        public bool colorFromLight = true;

        public ColorMode colorMode = Consts.Beam.ColorModeDefault;

        public ColorMode usedColorMode
        {
            get
            {
                if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off) return ColorMode.Flat;
                return colorMode;
            }
        }

#if UNITY_2018_1_OR_NEWER
        [ColorUsageAttribute(false, true)]
#else
        [ColorUsageAttribute(false, true, 0f, 8f, 0.125f, 3f)]
#endif
        [FormerlySerializedAs("colorValue")]
        public Color color = Consts.Beam.FlatColor;

        public Gradient colorGradient;

#if UNITY_EDITOR
        public override Color ComputeColorAtDepth(float depthRatio)
        {
            if (usedColorMode == ColorMode.Flat) return color;
            else return colorGradient.Evaluate(depthRatio);
        }
#endif

        bool useColorFromAttachedLightSpot { get { return colorFromLight && lightSpotAttached != null; } }

        bool useColorTemperatureFromAttachedLightSpot
        {
            get
            {
#if VLB_LIGHT_TEMPERATURE_SUPPORT
                return useColorFromAttachedLightSpot && lightSpotAttached.useColorTemperature && Config.Instance.useLightColorTemperature;
#else
                return false;
#endif
            }
        }

        public bool intensityFromLight = true;

        public bool intensityModeAdvanced = false;

        [FormerlySerializedAs("alphaInside")]
        [Min(Consts.Beam.IntensityMin)]
        public float intensityInside = Consts.Beam.IntensityDefault;

        [System.Obsolete("Use 'intensityGlobal' or 'intensityInside' instead")]
        public float alphaInside { get { return intensityInside; } set { intensityInside = value; } }

        [FormerlySerializedAs("alphaOutside"), FormerlySerializedAs("alpha")]
        [Min(Consts.Beam.IntensityMin)]
        public float intensityOutside = Consts.Beam.IntensityDefault;

        [System.Obsolete("Use 'intensityGlobal' or 'intensityOutside' instead")]
        public float alphaOutside { get { return intensityOutside; } set { intensityOutside = value; } }

        public float intensityGlobal { get { return intensityOutside; } set { intensityInside = value; intensityOutside = value; } }

        [Min(Consts.Beam.MultiplierMin)]
        public float intensityMultiplier = Consts.Beam.MultiplierDefault;

        public bool useIntensityFromAttachedLightSpot { get { return intensityFromLight && lightSpotAttached != null; } }

        public void GetInsideAndOutsideIntensity(out float inside, out float outside)
        {
            if(intensityModeAdvanced)
            {
                inside = intensityInside;
                outside = intensityOutside;
            }
            else
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    Debug.Assert(Utils.Approximately(intensityInside, intensityOutside), "The beam is not using advanced intensity mode, but its inside and outside have not the same value.", gameObject);
                }
                inside = outside = intensityOutside;
            }
        }

        [Range(Consts.Beam.HDRPExposureWeightMin, Consts.Beam.HDRPExposureWeightMax)]
        public float hdrpExposureWeight = Consts.Beam.HDRPExposureWeightDefault;

        public BlendingMode blendingMode = Consts.Beam.BlendingModeDefault;

        [FormerlySerializedAs("angleFromLight")]
        public bool spotAngleFromLight = true;

        public bool useSpotAngleFromAttachedLightSpot { get { return spotAngleFromLight && lightSpotAttached != null; } }

        [Range(Consts.Beam.SpotAngleMin, Consts.Beam.SpotAngleMax)]
        public float spotAngle = Consts.Beam.SpotAngleDefault;

        [Min(Consts.Beam.MultiplierMin)]
        public float spotAngleMultiplier = Consts.Beam.MultiplierDefault;

        public float coneAngle { get { return Mathf.Atan2(coneRadiusEnd - coneRadiusStart, maxGeometryDistance) * Mathf.Rad2Deg * 2f; } }

        [FormerlySerializedAs("radiusStart")]
        public float coneRadiusStart = Consts.Beam.ConeRadiusStart;

        public float coneRadiusEnd {
            get { return Utils.ComputeConeRadiusEnd(maxGeometryDistance, spotAngle); }
            set { spotAngle = Utils.ComputeSpotAngle(maxGeometryDistance, value); }
        }

        public float coneVolume { get { float r1 = coneRadiusStart, r2 = coneRadiusEnd; return (Mathf.PI / 3) * (r1 * r1 + r1 * r2 + r2 * r2) * fallOffEnd; } }

        public float coneApexOffsetZ {
            get {
                float ratioRadius = coneRadiusStart / coneRadiusEnd;
                return ratioRadius == 1f ? float.MaxValue : ((maxGeometryDistance * ratioRadius) / (1 - ratioRadius));
            }
        }

        public Vector3 coneApexPositionLocal { get { return new Vector3(0, 0, -coneApexOffsetZ); } }
        public Vector3 coneApexPositionGlobal { get { return transform.localToWorldMatrix.MultiplyPoint(coneApexPositionLocal); } }

        public override bool IsScalable() { return true; }

        public ShaderAccuracy shaderAccuracy = Consts.Beam.ShaderAccuracyDefault;

        public MeshType geomMeshType = Consts.Beam.GeomMeshType;

        [FormerlySerializedAs("geomSides")]
        public int geomCustomSides = Consts.Beam.GeomSidesDefault;

        public int geomSides
        {
            get { return geomMeshType == MeshType.Custom ? geomCustomSides : Config.Instance.sharedMeshSides; }
            set { geomCustomSides = value; Debug.LogWarningFormat("The setter VLB.{0}.geomSides is OBSOLETE and has been renamed to geomCustomSides.", ClassName); }
        }

        public int geomCustomSegments = Consts.Beam.GeomSegmentsDefault;

        public int geomSegments
        {
            get { return geomMeshType == MeshType.Custom ? geomCustomSegments : Config.Instance.sharedMeshSegments; }
            set { geomCustomSegments = value; Debug.LogWarningFormat("The setter VLB.{0}.geomSegments is OBSOLETE and has been renamed to geomCustomSegments.", ClassName); }
        }

        public Vector3 skewingLocalForwardDirection = Consts.Beam.SD.SkewingLocalForwardDirectionDefault;

        public Vector3 skewingLocalForwardDirectionNormalized
        {
            get
            {
                if (Mathf.Approximately(skewingLocalForwardDirection.z, 0.0f))
                {
                    Debug.LogErrorFormat("Beam {0} has a skewingLocalForwardDirection with a null Z, which is forbidden", name);
                    return Vector3.forward;
                }
                else return skewingLocalForwardDirection.normalized;
            }
        }

        public bool canHaveMeshSkewing { get { return geomMeshType == MeshType.Custom; } }

        public bool hasMeshSkewing
        {
            get
            {
                if (!Config.Instance.featureEnabledMeshSkewing) return false;
                if (!canHaveMeshSkewing) return false;
                var dotForward = Vector3.Dot(skewingLocalForwardDirectionNormalized, Vector3.forward);
                if (Mathf.Approximately(dotForward, 1.0f)) return false;
                return true;
            }
        }

        public Transform clippingPlaneTransform = Consts.Beam.SD.ClippingPlaneTransformDefault;

        public Vector4 additionalClippingPlane { get { return clippingPlaneTransform == null ? Vector4.zero : Utils.PlaneEquation(clippingPlaneTransform.forward, clippingPlaneTransform.position); } }

        public bool geomCap = Consts.Beam.GeomCap;

        public AttenuationEquation attenuationEquation = Consts.Beam.AttenuationEquationDefault;

        [Range(Consts.Beam.AttenuationCustomBlendingMin, Consts.Beam.AttenuationCustomBlendingMax)]
        public float attenuationCustomBlending = Consts.Beam.AttenuationCustomBlendingDefault;

        public float attenuationLerpLinearQuad {
            get {
                if (attenuationEquation == AttenuationEquation.Linear) return 0f;
                else if (attenuationEquation == AttenuationEquation.Quadratic) return 1f;
                return attenuationCustomBlending;
            }
        }

        [FormerlySerializedAs("fadeStart")]
        public float fallOffStart = Consts.Beam.FallOffStart;

        [System.Obsolete("Use 'fallOffStart' instead")]
        public float fadeStart { get { return fallOffStart; } set { fallOffStart = value; } }

        [FormerlySerializedAs("fadeEnd")]
        public float fallOffEnd = Consts.Beam.FallOffEnd;

        [System.Obsolete("Use 'fallOffEnd' instead")]
        public float fadeEnd { get { return fallOffEnd; } set { fallOffEnd = value; } }

        [FormerlySerializedAs("fadeEndFromLight")]
        public bool fallOffEndFromLight = true;

        [System.Obsolete("Use 'fallOffEndFromLight' instead")]
        public bool fadeEndFromLight { get { return fallOffEndFromLight; } set { fallOffEndFromLight = value; } }

        public bool useFallOffEndFromAttachedLightSpot { get { return fallOffEndFromLight && lightSpotAttached != null; } }

        [Min(Consts.Beam.MultiplierMin)]
        public float fallOffEndMultiplier = Consts.Beam.MultiplierDefault;

        public float maxGeometryDistance { get { return fallOffEnd + Mathf.Max(Mathf.Abs(tiltFactor.x), Mathf.Abs(tiltFactor.y)); } }

        public float depthBlendDistance = Consts.Beam.DepthBlendDistance;

        public float cameraClippingDistance = Consts.Beam.CameraClippingDistance;

        [Range(Consts.Beam.SD.GlareMin, Consts.Beam.SD.GlareMax)]
        public float glareFrontal = Consts.Beam.SD.GlareFrontalDefault;

        [Range(Consts.Beam.SD.GlareMin, Consts.Beam.SD.GlareMax)]
        public float glareBehind = Consts.Beam.SD.GlareBehindDefault;

        [FormerlySerializedAs("fresnelPowOutside")]
        public float fresnelPow = Consts.Beam.SD.FresnelPow;

        public NoiseMode noiseMode = Consts.Beam.NoiseModeDefault;

        public bool isNoiseEnabled { get { return noiseMode != NoiseMode.Disabled; } }

        [System.Obsolete("Use 'noiseMode' instead")]
        public bool noiseEnabled { get { return isNoiseEnabled; } set { noiseMode = value ? NoiseMode.WorldSpace : NoiseMode.Disabled; } }

        [Range(Consts.Beam.NoiseIntensityMin, Consts.Beam.NoiseIntensityMax)]
        public float noiseIntensity = Consts.Beam.NoiseIntensityDefault;

        public bool noiseScaleUseGlobal = true;

        [Range(Consts.Beam.NoiseScaleMin, Consts.Beam.NoiseScaleMax)]
        public float noiseScaleLocal = Consts.Beam.NoiseScaleDefault;

        public bool noiseVelocityUseGlobal = true;

        public Vector3 noiseVelocityLocal = Consts.Beam.NoiseVelocityDefault;

        public float fadeOutBegin
        {
            get { return _FadeOutBegin; }
            set { SetFadeOutValue(ref _FadeOutBegin, value); }
        }

        public float fadeOutEnd
        {
            get { return _FadeOutEnd; }
            set { SetFadeOutValue(ref _FadeOutEnd, value); }
        }

        public bool isFadeOutEnabled { get { return _FadeOutBegin >= 0 && _FadeOutEnd >= 0; } }

        public Dimensions dimensions = Consts.Beam.DimensionsDefault;

        public Vector2 tiltFactor = Consts.Beam.SD.TiltDefault;

        public bool isTilted { get { return !tiltFactor.Approximately(Vector2.zero); } }

        public int sortingLayerID
        {
            get { return _SortingLayerID; }
            set {
                _SortingLayerID = value;
                if (m_BeamGeom) m_BeamGeom.sortingLayerID = value;
            }
        }

        public string sortingLayerName
        {
            get { return SortingLayer.IDToName(sortingLayerID); }
            set { sortingLayerID = SortingLayer.NameToID(value); }
        }

        public int sortingOrder
        {
            get { return _SortingOrder; }
            set
            {
                _SortingOrder = value;
                if (m_BeamGeom) m_BeamGeom.sortingOrder = value;
            }
        }

        public bool trackChangesDuringPlaytime
        {
            get { return _TrackChangesDuringPlaytime; }
            set { _TrackChangesDuringPlaytime = value; StartPlaytimeUpdateIfNeeded(); }
        }

        public bool isCurrentlyTrackingChanges { get { return m_CoPlaytimeUpdate != null; } }

        public override BeamGeometryAbstractBase GetBeamGeometry() { return m_BeamGeom; }
        protected override void SetBeamGeometryNull() { m_BeamGeom = null; }

        public int blendingModeAsInt { get { return Mathf.Clamp((int)blendingMode, 0, System.Enum.GetValues(typeof(BlendingMode)).Length); } }

        public Quaternion   beamInternalLocalRotation   { get { return dimensions == Dimensions.Dim3D ? Quaternion.identity : Quaternion.LookRotation(Vector3.right, Vector3.up); } }
        public Vector3      beamLocalForward            { get { return dimensions == Dimensions.Dim3D ? Vector3.forward : Vector3.right; } }
        public Vector3      beamGlobalForward           { get { return transform.TransformDirection(beamLocalForward); } }
        public override Vector3 GetLossyScale()         { return dimensions == Dimensions.Dim3D ? transform.lossyScale : new Vector3(transform.lossyScale.z, transform.lossyScale.y, transform.lossyScale.x); }

        public float raycastDistance {
            get {
                if (!hasMeshSkewing) return maxGeometryDistance;
                else
                {
                    var skewingZ = skewingLocalForwardDirectionNormalized.z;
                    return Mathf.Approximately(skewingZ, 0.0f) ? maxGeometryDistance : (maxGeometryDistance / skewingZ);
                }
            }
        }

        Vector3 ComputeRaycastGlobalVector(Vector3 localVec)
        {
            var rot = transform.rotation * beamInternalLocalRotation;
            return rot * localVec;
        }

        public Vector3 raycastGlobalForward { get { return ComputeRaycastGlobalVector(hasMeshSkewing ? skewingLocalForwardDirectionNormalized : Vector3.forward); } }
        public Vector3 raycastGlobalUp { get { return ComputeRaycastGlobalVector(Vector3.up); } }
        public Vector3 raycastGlobalRight { get { return ComputeRaycastGlobalVector(Vector3.right); } }

        public MaterialManager.SD.DynamicOcclusion _INTERNAL_DynamicOcclusionMode
        {
            get { return Config.Instance.featureEnabledDynamicOcclusion ? m_INTERNAL_DynamicOcclusionMode : MaterialManager.SD.DynamicOcclusion.Off; }
            set { m_INTERNAL_DynamicOcclusionMode = value; }
        }

        public MaterialManager.SD.DynamicOcclusion _INTERNAL_DynamicOcclusionMode_Runtime { get { return m_INTERNAL_DynamicOcclusionMode_Runtime ? _INTERNAL_DynamicOcclusionMode : MaterialManager.SD.DynamicOcclusion.Off; } }

        MaterialManager.SD.DynamicOcclusion m_INTERNAL_DynamicOcclusionMode = MaterialManager.SD.DynamicOcclusion.Off;
        bool m_INTERNAL_DynamicOcclusionMode_Runtime = false;

        public void _INTERNAL_SetDynamicOcclusionCallback(string shaderKeyword, MaterialModifier.Callback cb)
        {
            m_INTERNAL_DynamicOcclusionMode_Runtime = cb != null;

            if (m_BeamGeom)
                m_BeamGeom.SetDynamicOcclusionCallback(shaderKeyword, cb);
        }

        public delegate void OnWillCameraRenderCB(Camera cam);
        public event OnWillCameraRenderCB onWillCameraRenderThisBeam;

        public void _INTERNAL_OnWillCameraRenderThisBeam(Camera cam)
        {
            if (onWillCameraRenderThisBeam != null)
                onWillCameraRenderThisBeam(cam);
        }

        public delegate void OnBeamGeometryInitialized();
        private OnBeamGeometryInitialized m_OnBeamGeometryInitialized;

        public void RegisterOnBeamGeometryInitializedCallback(OnBeamGeometryInitialized cb)
        {
            m_OnBeamGeometryInitialized += cb;

            if(m_BeamGeom)
            {
                CallOnBeamGeometryInitializedCallback();
            }
        }

        void CallOnBeamGeometryInitializedCallback()
        {
            if (m_OnBeamGeometryInitialized != null)
            {
                m_OnBeamGeometryInitialized();
                m_OnBeamGeometryInitialized = null;
            }
        }

        [FormerlySerializedAs("trackChangesDuringPlaytime")]
        [SerializeField] bool _TrackChangesDuringPlaytime = false;

        [SerializeField] int _SortingLayerID = 0;
        [SerializeField] int _SortingOrder = 0;

        [FormerlySerializedAs("fadeOutBegin")]
        [SerializeField] float _FadeOutBegin = Consts.Beam.FadeOutBeginDefault;
        [FormerlySerializedAs("fadeOutEnd")]
        [SerializeField] float _FadeOutEnd = Consts.Beam.FadeOutEndDefault;

        void SetFadeOutValue(ref float propToChange, float value)
        {
            bool wasEnabled = isFadeOutEnabled;
            propToChange = value;

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                if (isFadeOutEnabled != wasEnabled)
                    OnFadeOutStateChanged();
            }
        }

        void OnFadeOutStateChanged()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {

                if (isFadeOutEnabled && m_BeamGeom) m_BeamGeom.RestartFadeOutCoroutine();
            }
        }

        public uint _INTERNAL_InstancedMaterialGroupID { get; protected set; }

        BeamGeometrySD m_BeamGeom = null;
        Coroutine m_CoPlaytimeUpdate = null;

#if UNITY_EDITOR
        public override int _EDITOR_GetInstancedMaterialID() { return m_BeamGeom ? m_BeamGeom._EDITOR_InstancedMaterialID : int.MinValue; }

        static VolumetricLightBeamSD[] _EditorFindAllInstances()
        {
            return Resources.FindObjectsOfTypeAll<VolumetricLightBeamSD>();
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

        public string meshStats
        {
            get
            {
                Mesh mesh = m_BeamGeom ? m_BeamGeom.coneMesh : null;
                if (mesh) return string.Format("Cone angle: {0:0.0} degrees\nMesh: {1} vertices, {2} triangles", coneAngle, mesh.vertexCount, mesh.triangles.Length / 3);
                else return "no mesh available";
            }
        }

        public int meshVerticesCount { get { return (m_BeamGeom && m_BeamGeom.coneMesh) ? m_BeamGeom.coneMesh.vertexCount : 0; } }
        public int meshTrianglesCount { get { return (m_BeamGeom && m_BeamGeom.coneMesh) ? m_BeamGeom.coneMesh.triangles.Length / 3 : 0; } }

        public float GetInsideBeamFactor(Vector3 posWS) { return GetInsideBeamFactorFromObjectSpacePos(transform.InverseTransformPoint(posWS)); }

        public float GetInsideBeamFactorFromObjectSpacePos(Vector3 posOS)
        {
            if(dimensions == Dimensions.Dim2D)
            {
                posOS = new Vector3(posOS.z, posOS.y, posOS.x);
            }

            if (posOS.z < 0f) return -1f;

            Vector2 posOSXY = posOS.xy();

            if (hasMeshSkewing)
            {
                Vector3 localForwardDirN = skewingLocalForwardDirectionNormalized;
                posOSXY -= localForwardDirN.xy() * (posOS.z / localForwardDirN.z);
            }

            // Compute a factor to know how far inside the beam cone the camera is
            var triangle2D = new Vector2(posOSXY.magnitude, posOS.z + coneApexOffsetZ).normalized;
            const float maxRadiansDiff = 0.1f;
            float slopeRad = (coneAngle * Mathf.Deg2Rad) / 2;

            return Mathf.Clamp((Mathf.Abs(Mathf.Sin(slopeRad)) - Mathf.Abs(triangle2D.x)) / maxRadiansDiff, -1, 1);
        }

        [System.Obsolete("Use 'GenerateGeometry()' instead")]
        public void Generate() { GenerateGeometry(); }

        public override void GenerateGeometry()
        {
            HandleBackwardCompatibility(pluginVersion, Version.Current);
            pluginVersion = Version.Current;

            ValidateProperties();

            if (m_BeamGeom == null)
            {
                m_BeamGeom = Utils.NewWithComponent<BeamGeometrySD>("Beam Geometry");
                m_BeamGeom.Initialize(this);
                CallOnBeamGeometryInitializedCallback();
            }

            m_BeamGeom.RegenerateMesh(enabled);

            base.GenerateGeometry();
        }

        public virtual void UpdateAfterManualPropertyChange()
        {
            ValidateProperties();
            if (m_BeamGeom) m_BeamGeom.UpdateMaterialAndBounds();
        }

#if !UNITY_EDITOR
        void Start()
        {
            InitLightSpotAttachedCached();

            // In standalone builds, simply generate the geometry once in Start
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
                // In Editor, creating geometry from Start and/or OnValidate generates warning in Unity 2017.
                // So we do it from Update
                m_EditorDirtyFlags = EditorDirtyFlags.Everything;
            }

            StartPlaytimeUpdateIfNeeded();
        }

        void OnValidate()
        {
            m_EditorDirtyFlags |= EditorDirtyFlags.Props; // Props have been modified from Editor
        }

        void Update() // EDITOR ONLY
        {
            EditorHandleLightPropertiesUpdate();    // Handle edition of light properties in Editor

            if (m_EditorDirtyFlags == EditorDirtyFlags.Clean)
            {
                if (Application.isPlaying)
                {
                    if (!trackChangesDuringPlaytime) // during Playtime, realtime changes are handled by CoUpdateDuringPlaytime
                        return;
                }
            }
            else
            {
                if (m_EditorDirtyFlags.HasFlag(EditorDirtyFlags.Mesh))
                {
                    if (m_EditorDirtyFlags.HasFlag(EditorDirtyFlags.BeamGeomGAO))
                        DestroyBeam();

                    GenerateGeometry(); // regenerate everything
                }
                else if (m_EditorDirtyFlags.HasFlag(EditorDirtyFlags.Props))
                {
                    ValidateProperties();
                }
            }

            // If we modify the attached Spotlight properties, or if we animate the beam via Unity 2017's timeline,
            // we are not notified of properties changes. So we update the material anyway.
            UpdateAfterManualPropertyChange();

            m_EditorDirtyFlags = EditorDirtyFlags.Clean;
        }

        public override void CopyPropsFrom(VolumetricLightBeamAbstractBase beamSrc, BeamProps beamProps)
        {
            base.CopyPropsFrom(beamSrc, beamProps);

            if (beamSrc is VolumetricLightBeamSD)
            {
                var beamSD = beamSrc as VolumetricLightBeamSD;
                if (beamProps.HasFlag(BeamProps.Color))         { colorMode = beamSD.colorMode; color = beamSD.color; colorGradient = beamSD.colorGradient; }
                if (beamProps.HasFlag(BeamProps.BlendingMode))  { blendingMode = beamSD.blendingMode; }
                if (beamProps.HasFlag(BeamProps.Intensity))     { intensityModeAdvanced = beamSD.intensityModeAdvanced; intensityInside = beamSD.intensityInside; intensityOutside = beamSD.intensityOutside; intensityMultiplier = beamSD.intensityMultiplier; }
                if (beamProps.HasFlag(BeamProps.SideSoftness))  { glareFrontal = beamSD.glareFrontal; glareBehind = beamSD.glareBehind; }
                if (beamProps.HasFlag(BeamProps.FallOffAttenuation))
                {
                    attenuationEquation = beamSD.attenuationEquation; attenuationCustomBlending = beamSD.attenuationCustomBlending;
                    fallOffStart = beamSD.fallOffStart;
                }
                if (beamProps.HasFlag(BeamProps.SpotShape))
                {
                    spotAngle = beamSD.spotAngle; spotAngleMultiplier = beamSD.spotAngleMultiplier;
                    coneRadiusStart = beamSD.coneRadiusStart;
                    fallOffEnd = beamSD.fallOffEnd; fallOffEndMultiplier = beamSD.fallOffEndMultiplier;
                    geomCap = beamSD.geomCap;
                }
                if (beamProps.HasFlag(BeamProps.Noise3D))
                {
                    noiseMode = beamSD.noiseMode; noiseIntensity = beamSD.noiseIntensity;
                    noiseScaleUseGlobal = beamSD.noiseScaleUseGlobal; noiseScaleLocal = beamSD.noiseScaleLocal;
                    noiseVelocityUseGlobal = beamSD.noiseVelocityUseGlobal; noiseVelocityLocal = beamSD.noiseVelocityLocal;
                }
                if (beamProps.HasFlag(BeamProps.SDConeGeometry)) { geomMeshType = beamSD.geomMeshType; geomCustomSides = beamSD.geomCustomSides; geomCustomSegments = beamSD.geomCustomSegments; }
                if (beamProps.HasFlag(BeamProps.SDSoftIntersectBlendingDist)) { depthBlendDistance = beamSD.depthBlendDistance; cameraClippingDistance = beamSD.cameraClippingDistance; }
                if (beamProps.HasFlag(BeamProps.Props2D)) { dimensions = beamSD.dimensions; sortingLayerID = beamSD.sortingLayerID; sortingOrder = beamSD.sortingOrder; }
            }
            else if (beamSrc is VolumetricLightBeamHD)
            {
                var beamHD = beamSrc as VolumetricLightBeamHD;
                if (beamProps.HasFlag(BeamProps.Color))         { colorMode = beamHD.colorMode; color = beamHD.colorFlat; colorGradient = beamHD.colorGradient; }
                if (beamProps.HasFlag(BeamProps.BlendingMode))  { blendingMode = beamHD.blendingMode; }
                if (beamProps.HasFlag(BeamProps.Intensity))     { intensityGlobal = beamHD.intensity; intensityMultiplier = beamHD.intensityMultiplier; }
                if (beamProps.HasFlag(BeamProps.FallOffAttenuation))
                {
                    attenuationEquation = UtilsBeamProps.ConvertAttenuation(beamHD.attenuationEquation);
                    fallOffStart = beamHD.fallOffStart;
                }
                if (beamProps.HasFlag(BeamProps.SpotShape))
                {
                    spotAngle = beamHD.spotAngle; spotAngleMultiplier = beamHD.spotAngleMultiplier;
                    coneRadiusStart = beamHD.coneRadiusStart;
                    fallOffEnd = beamHD.fallOffEnd; fallOffEndMultiplier = beamHD.fallOffEndMultiplier;
                }
                if (beamProps.HasFlag(BeamProps.Noise3D))
                {
                    noiseMode = beamHD.noiseMode; noiseIntensity = beamHD.noiseIntensity;
                    noiseScaleUseGlobal = beamHD.noiseScaleUseGlobal; noiseScaleLocal = beamHD.noiseScaleLocal;
                    noiseVelocityUseGlobal = beamHD.noiseVelocityUseGlobal; noiseVelocityLocal = beamHD.noiseVelocityLocal;
                }
                if (beamProps.HasFlag(BeamProps.Props2D))
                {
                    if (beamSrc is VolumetricLightBeamHD2D)
                    {
                        var beamHD2D = beamSrc as VolumetricLightBeamHD2D;
                        dimensions = Dimensions.Dim2D; sortingLayerID = beamHD2D.sortingLayerID; sortingOrder = beamHD2D.sortingOrder;
                    }
                    else
                    {
                        dimensions = Dimensions.Dim3D;
                    }
                }

            }
            UpdateAfterManualPropertyChange();
        }

        public void Reset()
        {
            colorMode = Consts.Beam.ColorModeDefault;
            color = Consts.Beam.FlatColor;
            colorFromLight = true;

            intensityFromLight = true;
            intensityModeAdvanced = false;
            intensityInside = Consts.Beam.IntensityDefault;
            intensityOutside = Consts.Beam.IntensityDefault;
            intensityMultiplier = Consts.Beam.MultiplierDefault;
            hdrpExposureWeight = Consts.Beam.HDRPExposureWeightDefault;

            blendingMode = Consts.Beam.BlendingModeDefault;
            shaderAccuracy = Consts.Beam.ShaderAccuracyDefault;

            spotAngleFromLight = true;
            spotAngle = Consts.Beam.SpotAngleDefault;
            spotAngleMultiplier = Consts.Beam.MultiplierDefault;

            coneRadiusStart = Consts.Beam.ConeRadiusStart;
            geomMeshType = Consts.Beam.GeomMeshType;
            geomCustomSides = Consts.Beam.GeomSidesDefault;
            geomCustomSegments = Consts.Beam.GeomSegmentsDefault;
            geomCap = Consts.Beam.GeomCap;

            attenuationEquation = Consts.Beam.AttenuationEquationDefault;
            attenuationCustomBlending = Consts.Beam.AttenuationCustomBlendingDefault;

            fallOffEndFromLight = true;
            fallOffStart = Consts.Beam.FallOffStart;
            fallOffEnd = Consts.Beam.FallOffEnd;
            fallOffEndMultiplier = Consts.Beam.MultiplierDefault;

            depthBlendDistance = Consts.Beam.DepthBlendDistance;
            cameraClippingDistance = Consts.Beam.CameraClippingDistance;

            glareFrontal = Consts.Beam.SD.GlareFrontalDefault;
            glareBehind = Consts.Beam.SD.GlareBehindDefault;

            fresnelPow = Consts.Beam.SD.FresnelPow;

            noiseMode = Consts.Beam.NoiseModeDefault;
            noiseIntensity = Consts.Beam.NoiseIntensityDefault;
            noiseScaleUseGlobal = true;
            noiseScaleLocal = Consts.Beam.NoiseScaleDefault;
            noiseVelocityUseGlobal = true;
            noiseVelocityLocal = Consts.Beam.NoiseVelocityDefault;

            sortingLayerID = 0;
            sortingOrder = 0;

            fadeOutBegin = Consts.Beam.FadeOutBeginDefault;
            fadeOutEnd = Consts.Beam.FadeOutEndDefault;

            dimensions = Consts.Beam.DimensionsDefault;
            tiltFactor = Consts.Beam.SD.TiltDefault;
            skewingLocalForwardDirection = Consts.Beam.SD.SkewingLocalForwardDirectionDefault;
            clippingPlaneTransform = Consts.Beam.SD.ClippingPlaneTransformDefault;

            trackChangesDuringPlaytime = false;

            m_EditorDirtyFlags = EditorDirtyFlags.Everything;
        }
#endif

        void OnEnable()
        {
            if (m_BeamGeom)
                m_BeamGeom.OnMasterEnable();

            StartPlaytimeUpdateIfNeeded();

#if UNITY_EDITOR
            EditorLoadPrefs();
#endif
        }

        void OnDisable()
        {
            if (m_BeamGeom)
                m_BeamGeom.OnMasterDisable();

            m_CoPlaytimeUpdate = null;
        }

        void StartPlaytimeUpdateIfNeeded()
        {
            if (Application.isPlaying && trackChangesDuringPlaytime && m_CoPlaytimeUpdate == null)
            {
                m_CoPlaytimeUpdate = StartCoroutine(CoPlaytimeUpdate());
            }
        }

        IEnumerator CoPlaytimeUpdate()
        {
            while (trackChangesDuringPlaytime && enabled)
            {
                UpdateAfterManualPropertyChange();
                yield return null;
            }
            m_CoPlaytimeUpdate = null;
        }

        void AssignPropertiesFromAttachedSpotLight()
        {
            var lightSpot = lightSpotAttached;
            if (lightSpot)
            {
                Debug.AssertFormat(lightSpot.type == LightType.Spot, "Light attached to {0} '{1}' must be a Spot", ClassName, name);
                if (intensityFromLight)
                {
                    intensityModeAdvanced = false;
                    intensityGlobal = SpotLightHelper.GetIntensity(lightSpot) * intensityMultiplier;
                }

                if (fallOffEndFromLight) fallOffEnd = SpotLightHelper.GetFallOffEnd(lightSpot) * fallOffEndMultiplier;
                if (spotAngleFromLight) spotAngle = Mathf.Clamp(SpotLightHelper.GetSpotAngle(lightSpot) * spotAngleMultiplier, Consts.Beam.SpotAngleMin, Consts.Beam.SpotAngleMax);
                if (colorFromLight)
                {
                    colorMode = ColorMode.Flat;

#if VLB_LIGHT_TEMPERATURE_SUPPORT
                    if (useColorTemperatureFromAttachedLightSpot)
                    {
                        Color colorFromTemp = Mathf.CorrelatedColorTemperatureToRGB(lightSpot.colorTemperature);
                        var finalColor = lightSpot.color.linear * colorFromTemp;
                        color = finalColor.gamma;
                    }
                    else
#endif
                    {
                        color = lightSpot.color;
                    }
                }
            }
        }

        void ClampProperties()
        {
            intensityInside = Mathf.Max(intensityInside, Consts.Beam.IntensityMin);
            intensityOutside = Mathf.Max(intensityOutside, Consts.Beam.IntensityMin);
            intensityMultiplier = Mathf.Max(intensityMultiplier, Consts.Beam.MultiplierMin);

            attenuationCustomBlending = Mathf.Clamp(attenuationCustomBlending, Consts.Beam.AttenuationCustomBlendingMin, Consts.Beam.AttenuationCustomBlendingMax);

            fallOffEnd = Mathf.Max(Consts.Beam.FallOffDistancesMinThreshold, fallOffEnd);
            fallOffStart = Mathf.Clamp(fallOffStart, 0f, fallOffEnd - Consts.Beam.FallOffDistancesMinThreshold);
            fallOffEndMultiplier = Mathf.Max(fallOffEndMultiplier, Consts.Beam.MultiplierMin);

            spotAngle = Mathf.Clamp(spotAngle, Consts.Beam.SpotAngleMin, Consts.Beam.SpotAngleMax);
            spotAngleMultiplier = Mathf.Max(spotAngleMultiplier, Consts.Beam.MultiplierMin);
            coneRadiusStart = Mathf.Max(coneRadiusStart, 0f);

            depthBlendDistance = Mathf.Max(depthBlendDistance, 0f);
            cameraClippingDistance = Mathf.Max(cameraClippingDistance, 0f);

            geomCustomSides = Mathf.Clamp(geomCustomSides, Consts.Beam.GeomSidesMin, Consts.Beam.GeomSidesMax);
            geomCustomSegments = Mathf.Clamp(geomCustomSegments, Consts.Beam.GeomSegmentsMin, Consts.Beam.GeomSegmentsMax);

            fresnelPow = Mathf.Max(0f, fresnelPow);

            glareBehind = Mathf.Clamp(glareBehind, Consts.Beam.SD.GlareMin, Consts.Beam.SD.GlareMax);
            glareFrontal = Mathf.Clamp(glareFrontal, Consts.Beam.SD.GlareMin, Consts.Beam.SD.GlareMax);

            noiseIntensity = Mathf.Clamp(noiseIntensity, Consts.Beam.NoiseIntensityMin, Consts.Beam.NoiseIntensityMax);
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

            if (serializedVersion < 1301)
            {

                attenuationEquation = AttenuationEquation.Linear;
            }

            if (serializedVersion < 1501)
            {

                geomMeshType = MeshType.Custom;
                geomCustomSegments = 5;
            }

            if (serializedVersion < 1610)
            {

                intensityFromLight = false;
                intensityModeAdvanced = !Mathf.Approximately(intensityInside, intensityOutside);
            }

            if (serializedVersion < 1910)
            {

                if(!intensityModeAdvanced && !Mathf.Approximately(intensityInside, intensityOutside))
                {
                    intensityInside = intensityOutside;
                }
            }
        }

#if UNITY_EDITOR
        public static bool editorShowTiltFactor = false;
        public static bool editorShowClippingPlane = false;
        private static bool editorPrefsLoaded = false;

        static void EditorLoadPrefs()
        {
            if (!editorPrefsLoaded)
            {
                editorShowTiltFactor = UnityEditor.EditorPrefs.GetBool(EditorPrefsStrings.Beam.PrefShowTiltDir, false);
                editorPrefsLoaded = true;
            }
        }

        void OnDrawGizmos()
        {
#if DEBUG_SHOW_APEX
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(coneApexPositionLocal, 0.025f);
#endif

#if DEBUG_GLOBAL_VECTORS
            Debug.DrawLine(transform.position, transform.position + beamGlobalForward, Color.blue);
#endif

#if DEBUG_GLOBAL_RAYCAST_VECTORS
            Debug.DrawLine(transform.position, transform.position + raycastGlobalForward, Color.blue);
            Debug.DrawLine(transform.position, transform.position + raycastGlobalRight, Color.red);
            Debug.DrawLine(transform.position, transform.position + raycastGlobalUp, Color.green);
#endif

            if (editorShowTiltFactor)
            {
                Utils.GizmosDrawPlane(
                    new Vector3(tiltFactor.x, tiltFactor.y, 1.0f),
                    Vector3.zero,
                    Color.white,
                    transform.localToWorldMatrix,
                    0.25f,
                    0.5f);
            }

            if (editorShowClippingPlane && clippingPlaneTransform != null)
            {
                float kPlaneSize = 0.7f;
                Utils.GizmosDrawPlane(
                    Vector3.forward,
                    Vector3.zero,
                    Color.white,
                    clippingPlaneTransform.localToWorldMatrix,
                    kPlaneSize,
                    kPlaneSize * 0.5f);
            }
        }
#endif
        }
    }