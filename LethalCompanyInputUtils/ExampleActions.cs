using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Api.Composite;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils;

public class ExampleActions : LcInputActions
{
    public InputAction ExampleMovementAction { get; private set; }
    
    public override void CreateInputActions(in InputActionMapBuilder builder)
    {
        ExampleMovementAction = builder.NewActionBinding()
            .WithActionId("Movement")
            .WithActionType(InputActionType.Value)
            .AddVector2DCompositeBinding()
            .WithKeyboardAxisBinding(Vector2DAxis.Up, KeyboardControl.W)
            .WithKeyboardAxisBinding(Vector2DAxis.Down, KeyboardControl.S)
            .WithKeyboardAxisBinding(Vector2DAxis.Left, KeyboardControl.A)
            .WithKeyboardAxisBinding(Vector2DAxis.Right, KeyboardControl.D)
            .Finish();
    }
}