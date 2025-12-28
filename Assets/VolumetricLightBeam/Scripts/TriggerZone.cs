using UnityEngine;

namespace VLB
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeamAbstractBase))]
    [HelpURL(Consts.Help.UrlTriggerZone)]
    [AddComponentMenu(Consts.Help.AddComponentMenuTriggerZone)]
    public class TriggerZone : MonoBehaviour
    {
        public const string ClassName = "TriggerZone";

        public bool setIsTrigger = true;

        public float rangeMultiplier = 1.0f;

        enum TriggerZoneUpdateRate
        {

            OnEnable,

            OnOcclusionChange,
        }

        TriggerZoneUpdateRate updateRate
        {
            get
            {
                Debug.Assert(m_Beam != null);
                if(UtilsBeamProps.GetDimensions(m_Beam) == Dimensions.Dim3D) return TriggerZoneUpdateRate.OnEnable;
                return m_DynamicOcclusionRaycasting != null ? TriggerZoneUpdateRate.OnOcclusionChange : TriggerZoneUpdateRate.OnEnable;
            }
        }
        const int kMeshColliderNumSides = 8;

        VolumetricLightBeamAbstractBase m_Beam = null;
        DynamicOcclusionRaycasting m_DynamicOcclusionRaycasting = null;
        PolygonCollider2D m_PolygonCollider2D = null;

        void OnEnable()
        {
            m_Beam = GetComponent<VolumetricLightBeamAbstractBase>();
            Debug.Assert(m_Beam != null);

            m_DynamicOcclusionRaycasting = GetComponent<DynamicOcclusionRaycasting>();

            switch(updateRate)
            {
                case TriggerZoneUpdateRate.OnEnable:
                {
                    ComputeZone();
                    enabled = false;
                    break;
                }
                case TriggerZoneUpdateRate.OnOcclusionChange:
                {
                    if(m_DynamicOcclusionRaycasting)
                        m_DynamicOcclusionRaycasting.onOcclusionProcessed += OnOcclusionProcessed;
                    break;
                }
            }
        }

        void OnOcclusionProcessed()
        {
            ComputeZone();
        }

        void ComputeZone()
        {
            if (m_Beam)
            {
                var coneRadiusStart = UtilsBeamProps.GetConeRadiusStart(m_Beam);
                var rangeEnd = UtilsBeamProps.GetFallOffEnd(m_Beam) * rangeMultiplier;
                var lerpedRadiusEnd = Mathf.LerpUnclamped(coneRadiusStart, UtilsBeamProps.GetConeRadiusEnd(m_Beam), rangeMultiplier);

                if (UtilsBeamProps.GetDimensions(m_Beam) == Dimensions.Dim3D)
                {
                    var meshCollider = gameObject.GetOrAddComponent<MeshCollider>();
                    Debug.Assert(meshCollider);

                    int sides = Mathf.Min(UtilsBeamProps.GetGeomSides(m_Beam), kMeshColliderNumSides);
                    var mesh = MeshGenerator.GenerateConeZ_Radii_DoubleCaps(
                        lengthZ: rangeEnd,
                        radiusStart: coneRadiusStart,
                        radiusEnd: lerpedRadiusEnd,
                        numSides: kMeshColliderNumSides,
                        inverted: false);
                    mesh.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;

                    meshCollider.sharedMesh = mesh;
                    meshCollider.convex = setIsTrigger;
                    meshCollider.isTrigger = setIsTrigger;
                }
                else
                {
                    if (m_PolygonCollider2D == null)
                    {
                        m_PolygonCollider2D = gameObject.GetOrAddComponent<PolygonCollider2D>();
                        Debug.Assert(m_PolygonCollider2D);
                    }

                    var polyCoordsLS = new Vector2[]
                    {
                        new Vector2(0.0f,      -coneRadiusStart),
                        new Vector2(rangeEnd,  -lerpedRadiusEnd),
                        new Vector2(rangeEnd,  lerpedRadiusEnd),
                        new Vector2(0.0f,      coneRadiusStart)
                    };

                    if (m_DynamicOcclusionRaycasting && m_DynamicOcclusionRaycasting.planeEquationWS.IsValid())
                    {
                        var plane3dWS = m_DynamicOcclusionRaycasting.planeEquationWS;

                        if (Utils.IsAlmostZero(plane3dWS.normal.z))
                        {

                            var ptOnPlane1 = plane3dWS.ClosestPointOnPlaneCustom(Vector3.zero);
                            var ptOnPlane2 = plane3dWS.ClosestPointOnPlaneCustom(Vector3.up);

                            if(Utils.IsAlmostZero(Vector3.SqrMagnitude(ptOnPlane1 - ptOnPlane2)))
                                ptOnPlane1 = plane3dWS.ClosestPointOnPlaneCustom(Vector3.right);

                            ptOnPlane1 = transform.InverseTransformPoint(ptOnPlane1);
                            ptOnPlane2 = transform.InverseTransformPoint(ptOnPlane2);

                            var plane2dLS = PolygonHelper.Plane2D.FromPoints(ptOnPlane1, ptOnPlane2);
                            if (plane2dLS.normal.x > 0.0f) plane2dLS.Flip();

                            polyCoordsLS = plane2dLS.CutConvex(polyCoordsLS);
                        }
                    }

                    m_PolygonCollider2D.points = polyCoordsLS;
                    m_PolygonCollider2D.isTrigger = setIsTrigger;
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            rangeMultiplier = Mathf.Max(rangeMultiplier, 0.001f);
        }
#endif
    }
}