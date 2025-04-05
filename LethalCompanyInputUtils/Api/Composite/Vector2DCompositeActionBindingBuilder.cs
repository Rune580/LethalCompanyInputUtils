namespace LethalCompanyInputUtils.Api.Composite;

public class Vector2DCompositeActionBindingBuilder : CompositeActionBindingBuilder<Vector2DAxis, Vector2DCompositeActionBindingBuilder>
{
    private readonly CompositeVectorMode _mode;

    protected override string Composite => $"2DVector(mode={(int)_mode})";

    internal Vector2DCompositeActionBindingBuilder(InputActionMapBuilder mapBuilder, InputActionBindingBuilder actionBuilder, CompositeVectorMode mode) : base(mapBuilder, actionBuilder)
    {
        _mode = mode;
    }
}