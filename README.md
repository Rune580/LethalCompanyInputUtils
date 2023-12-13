# LethalCompany InputUtils

Utilities for creating InputActions and having them be accessible in-game.
InputActions created through this mod are accessible in-game via the keybinds menu added in update v45.

#### For feature requests or issues head over to my [repo](https://github.com/Rune580/LethalCompanyInputUtils)

## General Users
This mod is just a dependency for other mods, it doesn't add content, but it allows mods to add keybinds.
### Recommended Install
Use a Mod manager. I won't provide support if a mod manager wasn't used, a mod manager makes it far easier to debug issues since users can just share a modpack code.

## Developer Quick-Start
*This Api/Mod is still in beta, please keep in mind that stuff may change.*
Feedback is appreciated.

Download the latest release from either the [Thunderstore](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils) or the [Releases](https://github.com/Rune580/LethalCompanyInputUtils/releases).
Extract the zip and add a reference to the dll file of the mod in Visual Studio or Rider.

### Initializing Your Binds
- Create a **subclass of `LcInputActions`**
  - An instance of this class will contain all `InputAction`s your mod wishes to bind inputs for
  - Name the class appropriately
- Create InputActions [using Attributes](#using-attributes) and/or [at Runtime](#at-runtime)

```csharp
public class MyExampleInputClass : LcInputActions 
{
    [InputAction("<Keyboard>/g", Name = "Explode")]
    public InputAction ExplodeKey { get; set; }
    [InputAction("<Keyboard>/h", Name = "Another")]
    public InputAction AnotherKey { get; set; }
}
```

### Using Attributes
- **Create instance properties** for all desired `InputActions`
- **Annotate** the instance properties with the `[InputAction(...)]` annotation

> [!IMPORTANT]  
> For actions to be registered to the API, **Properties MUST be annotated with `[InputAction(...)]`**
>```csharp
>[InputAction("YourkbmPath", Name = "", GamepadPath = "", KbmInteractions = "", GamepadInteractions = "", ActionID = "", ActionType = InputActionType...)]
>```

#### Required Parameters
* `kbmPath`: The default bind for Keyboard and Mouse devices
  
#### Optional Parameters
* `Name`: The Displayed text in the game keybinds menu
* `GamepadPath`: The default bind for Gamepad devices
  
* `KbmInteractions`: Sets the interactions of the kbm binding. See [Interactions Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.Interactions.html)
* `GamepadInteractions`: Sets the interactions of the gamepad binding. See [Interactions Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.Interactions.html)

* `ActionID`: Overrides the generated actionId (Generally you don't want to change this)
* `ActionType`: Determines the behavior with which the action triggers. See [ActionType Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputActionType.html)

So your Attribute could be written like this:
```csharp
[InputAction("<Keyboard>/minus", Name = "Explode")]
public InputAction ExplodeKey { get; set; }
```
Or with any combination of optional parameters:
```csharp
[InputAction("<Keyboard>/minus", Name = "Explode", GamepadPath = "<Gamepad>/Button North", KbmInteractions = "hold(duration = 5)")]
public InputAction ExplodeKey { get; set; }
```
> [!NOTE]
> In this case above the Hold Interaction is being used. This keybind triggers after being held for *5* seconds. See [Interactions Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.Interactions.html)

### At Runtime
- **Override Method** `void CreateInputActions(in InputActionMapBuilder builder)`
- Use the builder to create InputActions
- **Reference InputAction** by calling `Asset["actionId"]` in your class

> [!IMPORTANT]
> Make sure you call `Finish()` after you're done creating each InputAction.

Here's an example usage of the runtime api
```csharp
public class MyExampleInputClass : LcInputActions
{
    public static readonly MyExampleInputClass Instance = new();
    
    public InputAction ExplodeKey => Asset["explodekey"];

    public override void CreateInputActions(in InputActionMapBuilder builder)
    {
        builder.NewActionBinding()
            .WithActionId("explodekey")
            .WithActionType(InputActionType.Button)
            .WithKbmPath("<Keyboard>/j")
            .WithBindingName("Explode")
            .Finish();
    }
}
```

### Referencing Your Binds
To use your InputActions class, you need to instantiate it.

> [!IMPORTANT]
> Do **not** create more than one instance of your InputActions class. 
> If your class is instantiated more than once, your InputActions are unlikely to work as intended.

The easiest (opinionated) way to do so would be to have a static instance in your plugin class.
```csharp
[BepInPlugin(...)]
public class MyExamplePlugin : BaseUnityPlugin
{
    internal static MyExampleInputClass InputActionsInstance = new MyExampleInputClass();
}
```
You could also opt for instantiating the instance in the InputActions class (Singleton-style).
```csharp
public class MyExamplePlugin : LcInputActions 
{
    public static MyExampleInputClass Instance = new();

    [InputAction("explodekey", "<Keyboard>/j", "<Gamepad>/Button North", Name = "Explode")]
    public InputAction ExplodeKey { get; set; }
}
```
> [!IMPORTANT]
> #### But How Do I Get My Binds String?
> You may have noticed that `<keyboard>/yourKey` can be a little confusing for the special buttons. So try this:
> 1. First, arbitrarily set the value to some regular value or just an empty string
> 2. Then, load up your mod and change the keybind to the desired key
> 3. After, look in your `.../BepInEx/controls/YOURMODID.json` file
> 4. Find the `{"action":"myaction","origPath":"","path":"<Keyboard>/f8"}]}`
> 5. Last, copy that `path:""` from the far right i.e. `"<Keyboard>/f8"`

### Using Your Binds
You could then simply reference the instance anywhere you need to have your actions at.
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

#### Best Practises
It is common to see tutorials call `InputAction.ReadValue<>()` or `InputAction.triggered` from mono-behaviour `Update()` functions.
```csharp
public class MyOtherClassOrMonoBehavior
{
    public void Update()
    {
        DoSomething();
    }
    
    public void DoSomething()
    {
        if (!MyExamplePlugin.InputActionsInstance.ExplodeKey.triggered) return;
        
        //Your executing code here
    }
}
```
This approach is sufficient for 'continuous' actions, e.g. movement. 

For 'discrete' actions, it's more appropriate to create event listeners that accept an `InputAction.CallbackContext` 
and subscribe to `InputAction.performed`.
```csharp
public class MyOtherClassOrMonoBehavior
{
    public void Awake()
    {
        SetupKeybindCallbacks();
    }    
    
    // Name this whatever you like. It needs to be called exactly once, so 
    public void SetupKeybindCallbacks()
    {
        MyExamplePlugin.InputActionsInstance.ExplodeKey.performed += OnExplodeKeyPressed;
    }

    public void OnExplodeKeyPressed(InputAction.CallbackContext explodeConext)
    {
        if (!explodeConext.performed) return; 
        // Add more context checks if desired
 
        // Your executing code here
    }
}
```

### Next Steps
Check out Unity's documentation for their [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

## Contact
Discord: @rune

Github: Rune580

## Contributors
Thanks to the following contributers:
- @Boxofbiscuits97 for reworking most of the documentation.
- @Lordfirespeed for housekeeping and additional documentation cleanup.

## Changelog
All notable changes to this project will be documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### [Unreleased]

### [0.4.0]

InputActions can now be made at runtime by overriding the method `CreateInputActions(...)` and using the provided builder.

### [0.3.0]

The only required parameter for the InputActions attribute is now only just the kbmPath, the rest are now optional.
Should help improve DX.

### [0.2.0]

Interactions for the kbm and gamepad bindings can now be set in the attribute.

### [0.1.0]

Initial Beta Release.
