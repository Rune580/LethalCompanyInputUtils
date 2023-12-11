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
public class MyExampleInputClass : LcInputActions 
{

}
```

Next make properties for all of the InputActions you want/need
```csharp
public InputAction ExplodeKey { get; set; }
```

In order for the action to be registered to the API, you must use the attribute `[InputAction(...)]`.
This attribute has 3 required parameters: actionId, keyboard/mouse binding path, and gamepad binding path.
There are also 2 optional parameters: Name and ActionType (default: InputActionType.Button). The Name parameter is what will be displayed in game.


```csharp
// [InputAction(string actionId, string keyBinding, string gamepadBinding, Name = "Shown Name", ActionType = InputActionType...)]

[InputAction("explode", "<Keyboard>/minus", "<Gamepad>/Button North", Name = "Explode")]
public InputAction ExplodeKey { get; set; }
```

Finally to use the InputAction you need an instance of this class. Due to how registration works, only 1 instance of this class can exist.
The easiest (opinionated) way to do so would be to have a static instance in your plugin class.
```csharp
[BepInPlugin(...)]
public class MyExamplePlugin : BaseUnityPlugin
{
    internal static MyExampleInputClass InputActionsInstance = new MyExampleInputClass();
}
```
You could also opt for having the instance in the InputActions class.
```csharp
public class MyExamplePlugin : LcInputActions 
{
    public static MyExampleInputClass Instance = new();

    [InputAction("explodekey", "<Keyboard>/j", "<Gamepad>/Button North", Name = "Explode")]
    public InputAction ExplodeKey { get; set; }
}
```

You could then simply reference the instance anywhere you need to have your actions at
```csharp
public class MyOtherClassOrMonoBehavior
{
    public void DoSomething()
    {
        MyExamplePlugin.InputActionsInstance.ExplodeKey ...
    }
}
```
or
```csharp
public class MyOtherClassOrMonoBehavior
{
    public void DoSomething()
    {
        MyExampleInputClass.Instance.ExplodeKey ...
    }
}
```

A good implimentation of this is getting the boolean value from when it is triggered
```csharp
public class MyOtherClassOrMonoBehavior
{
    public void DoSomething()
    {
        if (MyExamplePlugin.InputActionsInstance.ExplodeKey.triggered)
        {
            //Your executing code here
        }
    }
}
```

### Next Steps
Check out Unity's documentation for their [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

# Contact
Discord: @rune

Github: Rune580

# Contributers
Thanks to @Boxofbiscuits97 for helping make the ReadME easier for Devs

# Changelog
    0.1.0:
        Initial Beta Release.
