using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Api.Composite;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils;

public class ExampleActions : LcInputActions
{
    [CompositeVector2DAction]
    public InputAction ExampleMovementAction { get; private set; }
    
    public override void CreateInputActions(in InputActionMapBuilder builder)
    {
        ExampleMovementAction = builder.NewActionBinding()
            .WithActionId("Movement")
            .WithActionType(InputActionType.Value)
            .AddVector2DCompositeBinding()
            .WithKbmAxisBinding(Vector2DAxis.Up, KeyboardControl.W)
            .WithKbmAxisBinding(Vector2DAxis.Down, KeyboardControl.S)
            .WithKbmAxisBinding(Vector2DAxis.Left, KeyboardControl.A)
            .WithKbmAxisBinding(Vector2DAxis.Right, KeyboardControl.D)
            .Finish();
    }
}