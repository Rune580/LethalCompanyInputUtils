using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils;

public class TestActions : LcInputActions
{
    [InputAction("OpenEmoteWheel", "<Keyboard>/c", "<Gamepad>/rightStickButton", Name = "Open Emote Wheel", ActionType = InputActionType.Value)]
    public InputAction EmoteWheel { get; set; }
}