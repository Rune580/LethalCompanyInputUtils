# LethalCompany InputUtils

Utilities for creating InputActions and having them be accessible in-game.
InputActions created through this mod are accessible in-game via the keybinds menu added in update v45.

### For feature requests or issues head over to my [repo](https://github.com/Rune580/LethalCompanyInputUtils)

## General Users
This mod is just a dependency for other mods, it doesn't add content, but it allows mods to add keybinds.
### Recommended Install
Use a Mod manager. I won't provide support if a mod manager wasn't used, a mod manager makes it far easier to debug issues since users can just share a modpack code.

## Developer Quick-Start
*This Api/Mod is still in beta, please keep in mind that stuff may change.*
Feedback is appreciated.

Download the latest release from either the [Thunderstore](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils) or the [Releases](https://github.com/Rune580/LethalCompanyInputUtils/releases).
Extract the zip and add a reference to the dll file of the mod in Visual Studio or Rider.

Create a new class, name it whatever you prefer, this class will contain all the InputActions your mod needs.
Make sure it extends `LcInputActions`.

```csharp
public class [CLASSNAME] : LcInputActions 
{

}
```

Next make properties for all of the InputActions you want/need
```csharp
public InputAction Explode { get; set; }
```

In order for the action to be registered to the API, you must use the attribute `[InputAction(...)]`.
This attribute has 3 required parameters: actionId, keyboard/mouse binding path, and gamepad binding path.
There are also 2 optional parameters: ActionType (default: InputActionType.Button), and Name. The Name parameter is what will be displayed in game.
```csharp
[InputAction("explode", "<Keyboard>/j", "<Gamepad>/Button North", Name = "Explode")]
public InputAction Explode { get; set; }
```

Finally to use the InputAction you need an instance of this class. Due to how registration works, only 1 instance of this class can exist.
The easiest (opinionated) way to do so would be to have a static instance in your plugin class.
```csharp
[BepInPlugin(...)]
public class [MODPLUGIN] : BaseUnityPlugin
{
    internal static [CLASSNAME] InputActionsInstance = new [CLASSNAME]();
}
```
You could also opt for having the instance in the InputActions class.
```csharp
public class [CLASSNAME] : LcInputActions 
{
    public static [CLASSNAME] Instance = new();

    [InputAction("explode", "<Keyboard>/j", "<Gamepad>/Button North", Name = "Explode")]
    public InputAction Explode { get; set; }
}
```

You could then simply reference the instance anywhere you need to have your actions at
```csharp
public class [SomeOtherClassOrMonoBehavior]
{
    public void DoSomething()
    {
        [MODPLUGIN].InputActionsInstance.Explode ...
    }
}
```
or
```csharp
public class [SomeOtherClassOrMonoBehavior]
{
    public void DoSomething()
    {
        [CLASSNAME].Instance.Explode ...
    }
}
```

### Next Steps
Check out Unity's documentation for their [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

# Contact
Discord: @rune

Github: Rune580

# Changelog
    0.3.0:
        The only required parameter for the InputActions attribute is now only just the kbmPath, the rest are now optional.
        Should help improve DX.

    0.2.0:
        Interactions for the kbm and gamepad bindings can now be set in the attribute.

    0.1.0:
        Initial Beta Release.