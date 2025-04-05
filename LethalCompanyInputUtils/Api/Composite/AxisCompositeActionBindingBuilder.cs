namespace LethalCompanyInputUtils.Api.Composite;

public class AxisCompositeActionBindingBuilder : CompositeActionBindingBuilder<Vector2DAxis, AxisCompositeActionBindingBuilder>
{
    private readonly WhichSideWins _whichSideWins;
    private readonly float _minValue;
    private readonly float _maxValue;
    
    protected override string Composite => $"Axis(whichSideWins={(int)_whichSideWins},minValue={_minValue},maxValue={_maxValue})";

    public AxisCompositeActionBindingBuilder(InputActionMapBuilder mapBuilder, InputActionBindingBuilder actionBuilder, WhichSideWins whichSideWins = WhichSideWins.Neither, float minValue = -1, float maxValue = 1): base(mapBuilder, actionBuilder)
    {
        _whichSideWins = whichSideWins;
        _minValue = minValue;
        _maxValue = maxValue;
    }
}