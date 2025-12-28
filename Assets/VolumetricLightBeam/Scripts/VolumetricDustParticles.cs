using UnityEngine;

namespace VLB
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeamAbstractBase))]
    [HelpURL(Consts.Help.UrlDustParticles)]
    [AddComponentMenu(Consts.Help.AddComponentMenuDustParticles)]
    public class VolumetricDustParticles : MonoBehaviour
    {
        public const string ClassName = "VolumetricDustParticles";

        [Range(0f, 1f)]
        public float alpha = Consts.DustParticles.AlphaDefault;

        public float size = Consts.DustParticles.SizeDefault;

        public ParticlesDirection direction = Consts.DustParticles.DirectionDefault;

        public Vector3 velocity = Consts.DustParticles.VelocityDefault;

        [System.Obsolete("Use 'velocity' instead")]
        public float speed = 0.03f;

        public float density = Consts.DustParticles.DensityDefault;

        [MinMaxRange(0.0f, 1.0f)]
        public MinMaxRangeFloat spawnDistanceRange = Consts.DustParticles.SpawnDistanceRangeDefault;

        [System.Obsolete("Use 'spawnDistanceRange' instead")]
        public float spawnMinDistance = 0f;

        [System.Obsolete("Use 'spawnDistanceRange' instead")]
        public float spawnMaxDistance = 0.7f;

        public bool cullingEnabled = Consts.DustParticles.CullingEnabledDefault;

        public float cullingMaxDistance = Consts.DustParticles.CullingMaxDistanceDefault;

        public bool isCulled { get; private set; }

        [SerializeField] float m_AlphaAdditionalRuntime = 1.0f;
        public float alphaAdditionalRuntime
        {
            get { return m_AlphaAdditionalRuntime; }
            set { if (m_AlphaAdditionalRuntime != value) { m_AlphaAdditionalRuntime = value; m_RuntimePropertiesDirty = true; } }
        }

        public bool particlesAreInstantiated { get { return m_Particles; } }
        public int particlesCurrentCount { get { return m_Particles ? m_Particles.particleCount : 0; } }
        public int particlesMaxCount { get { return m_Particles ? m_Particles.main.maxParticles : 0; } }

        public ParticleSystemRenderer FindRenderer()
        {
            if (m_Renderer)
            {
                return m_Renderer;
            }
            else
            {
                return m_Particles.GetComponent<ParticleSystemRenderer>();
            }
        }

        ParticleSystem m_Particles = null;
        ParticleSystemRenderer m_Renderer = null;
        Material m_Material = null;
        Gradient m_GradientCached = new Gradient();
        bool m_RuntimePropertiesDirty = true;

#if UNITY_EDITOR
        void OnValidate()
        {
            density = Mathf.Clamp(density, Consts.DustParticles.DensityMin, Consts.DustParticles.DensityMax);
            cullingMaxDistance = Mathf.Max(cullingMaxDistance, Consts.DustParticles.CullingMaxDistanceMin);
            Play();
        }
#endif

        VolumetricLightBeamAbstractBase m_Master = null;

        void Start()
        {
            isCulled = false;

            m_Master = GetComponent<VolumetricLightBeamAbstractBase>();
            Debug.Assert(m_Master);
            HandleBackwardCompatibility(m_Master._INTERNAL_pluginVersion, Version.Current);

            InstantiateParticleSystem();

            SetActiveAndPlay();
        }

        void InstantiateParticleSystem()
        {

            gameObject.ForeachComponentsInDirectChildrenOnly<ParticleSystem>(ps => DestroyImmediate(ps.gameObject), true);

            m_Particles = Config.Instance.NewVolumetricDustParticles();

            if (m_Particles)
            {
#if UNITY_EDITOR
                if (m_Master)
                {
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(m_Particles.gameObject, m_Master.GetStaticEditorFlagsForSubObjects());
                    m_Particles.gameObject.SetSameSceneVisibilityStatesThan(m_Master.gameObject);
                }
#endif
                m_Particles.transform.SetParent(transform, false);

                m_Renderer = m_Particles.GetComponent<ParticleSystemRenderer>();
                Debug.Assert(m_Renderer);

                m_Material = new Material(m_Renderer.sharedMaterial);
                Debug.Assert(m_Material);
                m_Renderer.material = m_Material;
            }
        }

        void OnEnable()
        {
            SetActiveAndPlay();
        }

        void SetActive(bool active)
        {
            if (m_Particles) m_Particles.gameObject.SetActive(active);
        }

        void SetActiveAndPlay()
        {
            SetActive(true);
            Play();
        }

        void Play()
        {
            if (m_Particles)
            {
                SetParticleProperties();
                m_Particles.Simulate(0f);
                m_Particles.Play(true);
            }
        }

        void OnDisable()
        {
            SetActive(false);
        }

        void OnDestroy()
        {
            if (m_Particles)
            {
                DestroyImmediate(m_Particles.gameObject);
                m_Particles = null;
            }

            if (m_Material)
            {
                DestroyImmediate(m_Material);
                m_Material = null;
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                if (m_Particles == null)
                    InstantiateParticleSystem();

                Play();
            }
            else
#endif
            {
                UpdateCulling();

                if (UtilsBeamProps.CanChangeDuringPlaytime(m_Master))
                    SetParticleProperties();
            }

            if (m_RuntimePropertiesDirty && m_Material != null)
            {
                m_Material.SetColor(ShaderProperties.ParticlesTintColor, new Color(1.0f, 1.0f, 1.0f, alphaAdditionalRuntime));
                m_RuntimePropertiesDirty = false;
            }
        }

        void SetParticleProperties()
        {
            if (m_Particles && m_Particles.gameObject.activeSelf)
            {

                m_Particles.transform.localRotation = UtilsBeamProps.GetInternalLocalRotation(m_Master);
                m_Particles.transform.localScale = m_Master.IsScalable() ? Vector3.one : Vector3.one.Divide(m_Master.GetLossyScale());

                var coneLength = UtilsBeamProps.GetFallOffEnd(m_Master) * (spawnDistanceRange.maxValue - spawnDistanceRange.minValue);
                var ratePerSec = coneLength * density;
                int maxParticles = (int)(ratePerSec * 4);

                var main = m_Particles.main;

                var startLifetime = main.startLifetime;
                startLifetime.mode = ParticleSystemCurveMode.TwoConstants;
                startLifetime.constantMin = 4f;
                startLifetime.constantMax = 6f;
                main.startLifetime = startLifetime;

                var startSize = main.startSize;
                startSize.mode = ParticleSystemCurveMode.TwoConstants;
                startSize.constantMin = size * 0.9f;
                startSize.constantMax = size * 1.1f;
                main.startSize = startSize;

                var startColor = main.startColor;

                if (UtilsBeamProps.GetColorMode(m_Master) == ColorMode.Flat)
                {
                    startColor.mode = ParticleSystemGradientMode.Color;
                    var colorMax = UtilsBeamProps.GetColorFlat(m_Master);
                    colorMax.a *= alpha;
                    startColor.color = colorMax;
                }
                else
                {
                    startColor.mode = ParticleSystemGradientMode.Gradient;

                    var gradientRef = UtilsBeamProps.GetColorGradient(m_Master);
                    Debug.Assert(gradientRef != null);
                    var colorKeys = gradientRef.colorKeys;
                    var alphaKeys = gradientRef.alphaKeys;

                    for(int i=0; i< alphaKeys.Length; ++i)
                        alphaKeys[i].alpha *= alpha;

                    Debug.Assert(m_GradientCached != null);
                    m_GradientCached.SetKeys(colorKeys, alphaKeys);
                    startColor.gradient = m_GradientCached;
                }
                main.startColor = startColor;

                {
                    var startSpeed = main.startSpeed;
                    startSpeed.constant = (direction == ParticlesDirection.Random) ? Mathf.Abs(velocity.z) : 0.0f;
                    main.startSpeed = startSpeed;
                }

                {
                    var velocityOverLifetime = m_Particles.velocityOverLifetime;
                    velocityOverLifetime.enabled = (direction != ParticlesDirection.Random);
                    velocityOverLifetime.space = (direction == ParticlesDirection.LocalSpace) ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
                    velocityOverLifetime.xMultiplier = velocity.x;
                    velocityOverLifetime.yMultiplier = velocity.y;
                    velocityOverLifetime.zMultiplier = velocity.z;
                }

                main.maxParticles = maxParticles;

                {
                    float thickness = UtilsBeamProps.GetThickness(m_Master);
                    float fallOffEnd = UtilsBeamProps.GetFallOffEnd(m_Master);

                    var shape = m_Particles.shape;
                    shape.shapeType = ParticleSystemShapeType.ConeVolume;

                    float coneAngle = UtilsBeamProps.GetConeAngle(m_Master) * Mathf.Lerp(0.7f, 1f, thickness);
                    shape.angle = coneAngle * 0.5f;

                    float radiusStart = UtilsBeamProps.GetConeRadiusStart(m_Master) * Mathf.Lerp(0.3f, 1.0f, thickness);
                    float radiusEnd = Utils.ComputeConeRadiusEnd(fallOffEnd, coneAngle);
                    shape.radius = Mathf.Lerp(radiusStart, radiusEnd, spawnDistanceRange.minValue);

                    shape.length = coneLength;

                    var localOffset = fallOffEnd * spawnDistanceRange.minValue;
#if UNITY_2017_1_OR_NEWER
                    shape.position = new Vector3(0f, 0f, localOffset);
#else
                    m_Particles.transform.localPosition = m_Master.beamLocalForward * localOffset;
#endif
                    shape.arc = 360f;
                    shape.randomDirectionAmount = (direction == ParticlesDirection.Random) ? 1f : 0f;
                }

                var emission = m_Particles.emission;
                var rate = emission.rateOverTime;
                rate.constant = ratePerSec;
                emission.rateOverTime = rate;

                if(m_Renderer)
                {
                    m_Renderer.sortingLayerID = UtilsBeamProps.GetSortingLayerID(m_Master);
                    m_Renderer.sortingOrder = UtilsBeamProps.GetSortingOrder(m_Master);
                }
            }
        }

        void HandleBackwardCompatibility(int serializedVersion, int newVersion)
        {
            if (serializedVersion == -1) return;
            if (serializedVersion == newVersion) return;

#pragma warning disable 0618
            if (serializedVersion < 1880)
            {

                if ((int)direction == 0)    direction = (ParticlesDirection)1;
                else                        direction = (ParticlesDirection)0;

                velocity = new Vector3(0.0f, 0.0f, speed);
            }

            if (serializedVersion < 1940)
            {
                spawnDistanceRange = new MinMaxRangeFloat(spawnMinDistance, spawnMaxDistance);
            }
#pragma warning restore 0618
        }

        #region Culling
        void UpdateCulling()
        {
            if (m_Particles)
            {
                bool visible = true;
                bool isFadeOutEnabled = UtilsBeamProps.GetFadeOutEnabled(m_Master);
                if ((cullingEnabled || isFadeOutEnabled) && m_Master.hasGeometry)
                {
                    if (Config.Instance.fadeOutCameraTransform)
                    {
                        var maxDist = cullingMaxDistance;
                        if (isFadeOutEnabled) maxDist = Mathf.Min(maxDist, UtilsBeamProps.GetFadeOutEnd(m_Master));
                        var maxDistSqr = maxDist * maxDist;
                        var distSqr = m_Master.bounds.SqrDistance(Config.Instance.fadeOutCameraTransform.position);
                        visible = distSqr <= maxDistSqr;
                    }
                    else
                    {
                        Debug.LogErrorFormat(gameObject
                            , "Fail to retrieve the camera with tag '{0}' (specified in VLB Config's 'fadeOutCameraTag') for the {1} Culling feature."
                            , Config.Instance.fadeOutCameraTag
                            , VolumetricDustParticles.ClassName
                            );
                    }
                }

                if (m_Particles.gameObject.activeSelf != visible)
                {
                    SetActive(visible);
                    isCulled = !visible;
                }

                if (visible && !m_Particles.isPlaying)
                    m_Particles.Play();
            }
        }
        #endregion
    }
}