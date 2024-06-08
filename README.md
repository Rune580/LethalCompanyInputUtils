# LethalCompany InputUtils

[![NuGet Version](https://img.shields.io/nuget/v/Rune580.Mods.LethalCompany.InputUtils?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Rune580.Mods.LethalCompany.InputUtils)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Rune580/LethalCompany_InputUtils?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/Rune580/LethalCompany_InputUtils?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Rune580/LethalCompanyInputUtils/build.yml?branch=master&style=for-the-badge&logo=github)](https://github.com/Rune580/LethalCompanyInputUtils/actions/workflows/build.yml)

Utilities for creating InputActions and having them be accessible in-game.
InputActions created through this mod are accessible in-game via the keybinds menu added in update v45.

#### For feature requests or issues head over to my [repo](https://github.com/Rune580/LethalCompanyInputUtils)

## General Users
This mod is just a dependency for other mods, it doesn't add content, but it allows mods to add keybinds.

### Where are my bind overrides stored?
Depends on the version of InputUtils:
- **>= 0.7.0** Global: `AppData/LocalLow/ZeekerssRBLX/Lethal Company/InputUtils/controls` Local: `BepInEx/config/controls`
- **>= 0.4.1** `BepInEx/config/controls`
- **<= 0.4.0** `BepInEx/controls`

### Lethal Company Version Support
InputUtils only _officially_ supports the latest version of Lethal Company, except when there's a public beta, in which case both the non-beta and beta version are included.

InputUtils is only guaranteed to work on these versions, older versions of Lethal Company and InputUtils are not supported by me, and issues found when using older versions of Lethal Company will not be provided support for.

If you choose to use an unsupported version of Lethal Company, you do so at your own risk.

### Recommended Install
Use a Mod manager. I won't provide support if a mod manager wasn't used, a mod manager makes it far easier to debug issues since users can just share a modpack code.

## Developer Quick-Start
*This Api/Mod is still in beta, please keep in mind that stuff may change.*
Feedback is appreciated.

Add the nuget package to your project, if you want a copy and paste solution:

add this to your project `.csproj`
```xml
<ItemGroup>
  <!-- Make sure the 'Version="..."' is set to the latest version -->
  <PackageReference Include="Rune580.Mods.LethalCompany.InputUtils" Version="0.7.3" />
</ItemGroup>
```
That should be all you need to get started.

Otherwise if you don't want to use nuget, you can download the latest release from either the [Thunderstore](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils) or the [Releases](https://github.com/Rune580/LethalCompanyInputUtils/releases).
Extract the zip and add a reference to the dll file of the mod in Visual Studio or Rider.

### Initializing Your Binds
- Create a **subclass of `LcInputActions`**
  - An instance of this class will contain all `InputAction`s your mod wishes to bind inputs for
  - Name the class appropriately
- Create InputActions [using Attributes](#using-attributes) and/or [at Runtime](#at-runtime)

```csharp
public class MyExampleInputClass : LcInputActions 
{
    [InputAction(KeyboardControl.G, Name = "Explode")]
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
>[InputAction("YourkbmPath" /* You can also use a KeyboardControl or MouseControl */, Name = "", GamepadPath = "", GamepadControl = GamepadControl.None, KbmInteractions = "", GamepadInteractions = "", ActionID = "", ActionType = InputActionType...)]
>```

#### Required Parameters
You only need to use **one** of the following overloads:
* `kbmPath`: The default bind for Keyboard and Mouse devices
* `keyboardControl`: The default bind for Keyboard devices, uses the KeyboardControl Enum
* `mouseControl`: The default bind for Mouse devices, uses the MouseControl Enum
  
#### Optional Parameters
* `Name`: The Displayed text in the game keybinds menu
* `GamepadPath` **_or_** `GamepadControl`: The default bind for Gamepad devices. When both are set, `GamepadPath` will take priority.
  
* `KbmInteractions`: Sets the interactions of the kbm binding. See [Interactions Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.Interactions.html)
* `GamepadInteractions`: Sets the interactions of the gamepad binding. See [Interactions Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.Interactions.html)

* `ActionID`: Overrides the generated actionId (Generally you don't want to change this)
* `ActionType`: Determines the behavior with which the action triggers. See [ActionType Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputActionType.html)

So your Attribute could be written like this:
```csharp
[InputAction(KeyboardControl.Minus, Name = "Explode")]
public InputAction ExplodeKey { get; set; }
```
Or with any combination of optional parameters:
```csharp
[InputAction(KeyboardControl.Minus, Name = "Explode", GamepadControl = GamepadControl.ButtonNorth, KbmInteractions = "hold(duration = 5)")]
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
            .WithKeyboardControl(KeyboardControl.J) // or .WithKbmPath("<Keyboard>/j")
            .WithGamepadControl(GamepadControl.ButtonNorth) // or .WithGamepadPath("<Gamepad>/buttonNorth")
            .WithBindingName("Explode")
            .Finish();
    }
}
```

> [!IMPORTANT]
> Omitting `WithGamepadControl`, `WithGamepadPath`, or `WithGamepadUnbound`, will disable binding the `InputAction` to a Gamepad device.
> Similarly, omitting `WithKeyboardControl`, `WithMouseControl`, `WithKbmPath`, or `WithKbmUnbound`, will disable binding the `InputAction` to a Keyboard or Mouse device.

### Referencing Your Binds
To use your InputActions class, you need to instantiate it.

> [!IMPORTANT]
> Do **not** create more than one instance of your InputActions class. 
> If your class is instantiated more than once, your InputActions are unlikely to work as intended.

The easiest (opinionated) way to do so would be to have a static instance in your plugin class.
```csharp
[BepInPlugin(...)]
[BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
public class MyExamplePlugin : BaseUnityPlugin
{
    internal static MyExampleInputClass InputActionsInstance;

    public void Awake()
    {
        InputActionsInstance = new MyExampleInputClass();
    }
}
```

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

#### Best Practices
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

### Using InputUtils as an Optional or Soft Dependency
First make sure to add the `[BepInDependency(...)]` attribute to your mods Plugin class, mark it as a `SoftDependency`.
If you already have the attribute set as a `HardDependency` make sure to replace that.
```csharp
[BepInPlugin(...)]
[BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.SoftDependency)]
public class MyExamplePlugin : BaseUnityPlugin
```

Create your InputActions class as you would following the guide above.
Make a class specifically for when the mod is loaded
```csharp
internal static class InputUtilsCompat
{
    public static bool Enabled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.LethalCompanyInputUtils");

    public static InputAction ExplodeKey =>
        MyExampleInputClass.Instance.ExplodeKey;
}
```

Finally whenever you reference stuff from `InputUtilsCompat`, make sure to check its `Enabled` Property first.
```csharp
if (InputUtilsCompat.Enabled)
    InputUtilsCompat.ExplodeKey...
```
Reference [Best Practices](#best-practices) for details on how to best use the InputAction

> [!IMPORTANT]
> #### If your mod uses [NetcodePatcher](https://github.com/EvaisaDev/UnityNetcodeWeaver) you may need to do additional steps.
> **This only applies to mods that use InputUtils as a soft-dependency**
> 
> Please check their Readme for more info.
> However for a possible fix, replace
> ```csharp
> var types = Assembly.GetExecutingAssembly().GetTypes();
> ```
> with
> ```csharp
> IEnumerable<Type> types;
> try
> {
>     types = Assembly.GetExecutingAssembly().GetTypes();
> }
> catch (ReflectionTypeLoadException e)
> {
>     types = e.Types.Where(t => t != null);
> }
> ```

### Next Steps
Check out Unity's documentation for their [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

## Build Instructions

Clone and enter the repo.
```shell
git clone https://github.com/Rune580/LethalCompanyInputUtils && cd LethalCompanyInputUtils
```
(Optional) Copy and rename `LethalCompanyInputUtils.csproj.user.example` to `LethalCompanyInputUtils.csproj.user`.
Edit `LethalCompanyInputUtils/LethalCompanyInputUtils.csproj.user` to fit your needs.
```shell
cp LethalCompanyInputUtils/LethalCompanyInputUtils.csproj.user.example LethalCompanyInputUtils/LethalCompanyInputUtils.csproj.user
```
Build the project using your IDE of choice or with the following command
```shell
dotnet build
```

## Contact
Discord: @rune

Github: Rune580

## Contributing
When making a PR make sure your changes are on a different branch. Do not make a PR with your changes on the `master` branch.

## Contributors
Thanks to the following contributers:
- @Boxofbiscuits97 for reworking most of the documentation.
- @Lordfirespeed for housekeeping and additional documentation cleanup.
- @Singularia - Russian Translation.

## Credits
- Reset to default icon from: @AinaVT
- PS5, Xbox Series X, Switch Glyph Source files from: https://thoseawesomeguys.com/prompts/
- Mouse Glyph Source files from: https://www.kenney.nl/assets/input-prompts
