namespace VLB
{
    public enum FeatureEnabledColorGradient
    {
        Off,
        HighOnly,
        HighAndLow
    };

    public enum ColorMode
    {
        Flat,
        Gradient
    }

    public enum AttenuationEquation
    {
        Linear = 0,
        Quadratic = 1,
        Blend = 2
    }

    public enum AttenuationEquationHD
    {
        Linear = 0,
        Quadratic = 1,
    }

    public enum BlendingMode
    {
        Additive,
        SoftAdditive,
        TraditionalTransparency,
    }

    public enum ShaderAccuracy
    {

        Fast,

        High,
    }

    public enum NoiseMode
    {

        Disabled,

        WorldSpace,

        LocalSpace,
    }

    public enum MeshType
    {
        Shared,
        Custom,
    }

    public enum RenderPipeline
    {

        BuiltIn,

        URP,

        HDRP,
    }

    public enum ShaderMode { SD, HD }

    public enum RenderingMode
    {

        MultiPass,

        Default,

        GPUInstancing,

        SRPBatcher,
    }

    public enum RenderQueue
    {

        Custom = 0,

        Background = 1000,

        Geometry = 2000,

        AlphaTest = 2450,

        GeometryLast = 2500,

        Transparent = 3000,

        Overlay = 4000,
    }

    public enum Dimensions
    {

        Dim3D,

        Dim2D
    }

    public enum PlaneAlignment
    {

        Surface,

        Beam
    }

    [System.Flags]
    public enum DynamicOcclusionUpdateRate
    {
        Never = 1 << 0,
        OnEnable = 1 << 1,
        OnBeamMove = 1 << 2,
        EveryXFrames = 1 << 3,
        OnBeamMoveAndEveryXFrames = OnBeamMove | EveryXFrames,
    }

    public enum ParticlesDirection
    {

        Random,

        LocalSpace,

        WorldSpace
    }

    [System.Flags]
    public enum ShadowUpdateRate
    {
        Never = 1 << 0,
        OnEnable = 1 << 1,
        OnBeamMove = 1 << 2,
        EveryXFrames = 1 << 3,
        OnBeamMoveAndEveryXFrames = OnBeamMove | EveryXFrames,
    }

    public enum CookieChannel
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Alpha = 3,
        RGBA = 4
    }

    [System.Flags]
    public enum DirtyProps
    {
        None = 0,
        Intensity = 1 << 1,
        HDRPExposureWeight = 1 << 2,
        ColorMode = 1 << 3,
        Color = 1 << 4,
        BlendingMode = 1 << 5,
        Cone = 1 << 6,
        SideSoftness = 1 << 7,
        Attenuation = 1 << 8,
        Dimensions = 1 << 9,
        RaymarchingQuality = 1 << 10,
        Jittering = 1 << 11,
        NoiseMode = 1 << 12,
        NoiseIntensity = 1 << 13,
        NoiseVelocityAndScale = 1 << 14,
        CookieProps = 1 << 15,
        ShadowProps = 1 << 16,
        AllWithoutMaterialChange = Intensity | HDRPExposureWeight | Color | Cone | SideSoftness | Jittering | NoiseIntensity | NoiseVelocityAndScale | CookieProps | ShadowProps,
        OnlyMaterialChangeOnly = Attenuation | ColorMode | BlendingMode | Dimensions | RaymarchingQuality | NoiseMode,
        All = AllWithoutMaterialChange | OnlyMaterialChangeOnly,
    }

    [System.Flags]
    public enum BeamProps
    {
        Transform = 1 << 0,
        Color = 1 << 1,
        BlendingMode = 1 << 2,
        Intensity = 1 << 3,
        SideSoftness = 1 << 4,
        SpotShape = 1 << 5,
        FallOffAttenuation = 1 << 6,
        Noise3D = 1 << 7,
        SDConeGeometry = 1 << 8,
        SDSoftIntersectBlendingDist = 1 << 9,
        Props2D = 1 << 10,
    }
}