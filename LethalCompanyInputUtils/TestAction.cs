using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils;

public class TestAction : LcInputActions
{
    public static TestAction Instance = new();
    
    [InputAction("explode", "<Keyboard>/j", "<Gamepad>/Button North", Name = "Explode")]
    public InputAction Explode { get; set; }
}