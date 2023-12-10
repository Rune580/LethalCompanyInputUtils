# LethalCompany InputUtils

Utilities for creating InputActions and having them be accessible in-game.
InputActions created through this mod are accessible in-game via the keybinds menu added in update v45.

## General Users
This mod is just a dependency for other mods, install it manually or with a mod manager.

## Developer Quick-Start
*This Api/Mod is still in beta, please keep in mind that stuff may change.*
Feedback is appreciated.

Download the latest release from either the [Thunder Store Page TODO] or the [Releases on the sidebar TODO].
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
[InputAction("explode", "<Keyboard>/j", "<Gamepad>/Button North", Name: "Explode")]
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

# Contact
Discord: @rune
Github: Rune580

# Changelog
    0.1.0:
        Initial Beta Release.