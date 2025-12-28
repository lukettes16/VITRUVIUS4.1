using UnityEngine;

namespace VLB
{
    public abstract class BeamGeometryAbstractBase : MonoBehaviour
    {
        public MeshRenderer meshRenderer { get; protected set; }

        public MeshFilter meshFilter { get; protected set; }
        public Mesh coneMesh { get; protected set; }

        protected Matrix4x4 m_ColorGradientMatrix;
        protected Material m_CustomMaterial = null;

        protected abstract VolumetricLightBeamAbstractBase GetMaster();

        void Start()
        {
            DestroyOrphanBeamGeom();
        }

        void OnDestroy()
        {
            if (m_CustomMaterial)
            {
                DestroyImmediate(m_CustomMaterial);
                m_CustomMaterial = null;
            }
        }

        void DestroyOrphanBeamGeom()
        {
            var master = GetMaster();
            if(master)
            {
                var beamGeom = master.GetBeamGeometry();
                if(beamGeom == this)
                {

                    return;
                }
            }

            DestroyBeamGeometryGameObject(this);
        }

        public static void DestroyBeamGeometryGameObject(BeamGeometryAbstractBase beamGeom)
        {
            if (beamGeom)
                DestroyImmediate(beamGeom.gameObject);
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
            {
                DestroyOrphanBeamGeom();
            }
        }

        public bool _EDITOR_IsUsingCustomMaterial { get { return m_CustomMaterial != null; } }
#endif
    }
}