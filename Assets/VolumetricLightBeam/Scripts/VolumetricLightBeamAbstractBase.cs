using UnityEngine;

namespace VLB
{
    public abstract class VolumetricLightBeamAbstractBase : MonoBehaviour
    {
        public const string ClassName = "VolumetricLightBeamAbstractBase";

        public abstract BeamGeometryAbstractBase GetBeamGeometry();
        protected abstract void SetBeamGeometryNull();

        public bool hasGeometry { get { return GetBeamGeometry() != null; } }

        public Bounds bounds { get { return GetBeamGeometry() != null ? GetBeamGeometry().meshRenderer.bounds : new Bounds(Vector3.zero, Vector3.zero); } }

        public delegate void BeamGeometryGeneratedHandler(VolumetricLightBeamAbstractBase beam);

        private event BeamGeometryGeneratedHandler BeamGeometryGeneratedEvent;

        public void RegisterBeamGeometryGeneratedCallback(BeamGeometryGeneratedHandler callback)
        {
            if (hasGeometry)
            {
                callback(this);
            }
            else
            {
                BeamGeometryGeneratedEvent += callback;
            }
        }

        public virtual void GenerateGeometry()
        {
            if (BeamGeometryGeneratedEvent != null)
            {
                BeamGeometryGeneratedEvent.Invoke(this);
                BeamGeometryGeneratedEvent = null;
            }
        }

        public abstract bool IsScalable();
        public abstract Vector3 GetLossyScale();

        public virtual void CopyPropsFrom(VolumetricLightBeamAbstractBase beamSrc, BeamProps beamProps)
        {
            if (beamProps.HasFlag(BeamProps.Transform))
            {
                transform.position = beamSrc.transform.position;
                transform.rotation = beamSrc.transform.rotation;
                transform.localScale = beamSrc.transform.localScale;
            }
            if (beamProps.HasFlag(BeamProps.SideSoftness)) { UtilsBeamProps.SetThickness(this, UtilsBeamProps.GetThickness(beamSrc)); }
        }

#pragma warning disable 0414
        [SerializeField] protected int pluginVersion = -1;
        public int _INTERNAL_pluginVersion => pluginVersion;
#pragma warning restore 0414

        public enum AttachedLightType { NoLight, OtherLight, SpotLight }
        public Light GetLightSpotAttachedSlow(out AttachedLightType lightType)
        {
            var light = GetComponent<Light>();
            if (light)
            {
                if (light.type == LightType.Spot)
                {
                    lightType = AttachedLightType.SpotLight;
                    return light;
                }
                else
                {
                    lightType = AttachedLightType.OtherLight;
                    return null;
                }
            }

            lightType = AttachedLightType.NoLight;
            return null;
        }

        protected Light m_CachedLightSpot = null;
        public Light lightSpotAttached
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) { AttachedLightType lightType; return GetLightSpotAttachedSlow(out lightType); }
#endif
                return m_CachedLightSpot;
            }
        }

        protected void InitLightSpotAttachedCached()
        {
            Debug.Assert(Application.isPlaying);
            AttachedLightType lightType;
            m_CachedLightSpot = GetLightSpotAttachedSlow(out lightType);
        }

        void OnDestroy()
        {
            DestroyBeam();
        }

        protected void DestroyBeam()
        {

            if (Application.isPlaying)
                BeamGeometryAbstractBase.DestroyBeamGeometryGameObject(GetBeamGeometry());
            SetBeamGeometryNull();
        }

#if UNITY_EDITOR
        public abstract Color ComputeColorAtDepth(float depthRatio);

        public abstract int _EDITOR_GetInstancedMaterialID();

        [System.Flags]
        protected enum EditorDirtyFlags
        {
            Clean = 0,
            Props = 1 << 1,
            Mesh = 1 << 2,
            BeamGeomGAO = 1 << 3,
            FullBeamGeomGAO = Mesh | BeamGeomGAO,
            Everything = Props | Mesh | BeamGeomGAO,
        }
        protected EditorDirtyFlags m_EditorDirtyFlags;
        protected CachedLightProperties m_PrevCachedLightProperties;

        protected void EditorHandleLightPropertiesUpdate()
        {

            if (!Application.isPlaying)
            {
                var newProps = new CachedLightProperties(lightSpotAttached);
                if (!newProps.Equals(m_PrevCachedLightProperties))
                    m_EditorDirtyFlags |= EditorDirtyFlags.Props;
                m_PrevCachedLightProperties = newProps;
            }
        }

        public UnityEditor.StaticEditorFlags GetStaticEditorFlagsForSubObjects()
        {

            var flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(gameObject);
            flags &= ~(

#if UNITY_2019_2_OR_NEWER
                UnityEditor.StaticEditorFlags.ContributeGI
#else
                UnityEditor.StaticEditorFlags.LightmapStatic
#endif
                | UnityEditor.StaticEditorFlags.OccluderStatic
#if !UNITY_2022_2_OR_NEWER
                | UnityEditor.StaticEditorFlags.NavigationStatic
                | UnityEditor.StaticEditorFlags.OffMeshLinkGeneration
#endif
                );
            return flags;
        }

        public bool _EditorIsDirty() { return m_EditorDirtyFlags != EditorDirtyFlags.Clean; }
        public void _EditorSetMeshDirty() { m_EditorDirtyFlags |= EditorDirtyFlags.Mesh; }
        public void _EditorSetBeamGeomDirty() { m_EditorDirtyFlags |= EditorDirtyFlags.FullBeamGeomGAO; }
#endif
    }
}