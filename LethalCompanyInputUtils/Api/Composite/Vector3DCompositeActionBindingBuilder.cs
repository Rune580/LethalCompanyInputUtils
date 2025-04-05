namespace LethalCompanyInputUtils.Api.Composite;

public class Vector3DCompositeActionBindingBuilder : CompositeActionBindingBuilder<Vector3DAxis, Vector3DCompositeActionBindingBuilder>
{
    private readonly CompositeVectorMode _mode;
    
    protected override string Composite => $"3DVector(mode={(int)_mode})";

    public Vector3DCompositeActionBindingBuilder(InputActionMapBuilder mapBuilder, InputActionBindingBuilder actionBuilder, CompositeVectorMode mode) : base(mapBuilder, actionBuilder)
    {
        _mode = mode;
    }
}