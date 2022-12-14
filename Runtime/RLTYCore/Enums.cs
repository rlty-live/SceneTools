namespace RLTY
{
    public enum CustomisableType
    {
        Texture,
        Sprite,
        Video,
        Audio,
        Model,
        Color,
        Text,
        Font,
        ExternalPage,
        DonationBox,
        Web3Transaction,
        TypeForm
    }

    public enum SpaceType
    {
        Booth,
        Gallery,
        Billboard
    }

    public enum FullScreenCameraStates
    {
        Initialized,
        PerspectiveMode,
        PseudoOrthographicMode,
        Destroyed
    }

    public enum TextureProportions
    {
        Portrait,
        Landscape,
        Square
    }
    
    public enum HorizontalAlignment
    {
        Center,
        Left,
        Right
    }

    public enum VerticalAlignment
    {
        Center,
        Up,
        Down
    }
}
