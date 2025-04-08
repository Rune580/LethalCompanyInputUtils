using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils;

public class ExampleActions : LcInputActions
{
    [CompositeVector2DAction(KeyboardControl.W, KeyboardControl.S, KeyboardControl.A, KeyboardControl.D, ActionId = "Move")]
    public InputAction ExampleMovementAction { get; set; }
    
    // public override void CreateInputActions(in InputActionMapBuilder builder)
    // {
    //     ExampleMovementAction = builder.NewActionBinding()
    //         .WithActionId("Movement")
    //         .AddVector2DCompositeBinding()
    //         .WithKbmAxisBinding(Vector2DAxis.Up, KeyboardControl.W)
    //         .WithKbmAxisBinding(Vector2DAxis.Down, KeyboardControl.S)
    //         .WithKbmAxisBinding(Vector2DAxis.Left, KeyboardControl.A)
    //         .WithKbmAxisBinding(Vector2DAxis.Right, KeyboardControl.D)
    //         .Finish();
    //     
    //     ExampleMovementAction.Enable();
    // }
}