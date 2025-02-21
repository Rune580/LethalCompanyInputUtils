### [0.7.10]

Fixes:
- Actually load LethalCompany keybinds

### [0.7.9]

Fixes:
- Fixed issue where InputUtils failing to load LethalCompany keybinds caused other issues.

### [0.7.8]

Added:
- Lethal Company keybinds now support Local keybinds, you can now ship vanilla keybinds with modpacks/profiles.
- Added warnings for different scenarios in the rebind ui to better inform users of possible issues with their current configuration.

Fixes:
- Fixed issue when using an `-Only` priority and editing the opposite config resulting in the UI not visually updating newly rebound keys.

Locale:
- Chinese translation by @CoolLKKPS (I don't know if it's simplified or not)

### [0.7.7]

Fixes:
- I also forgot to include Locale with the new build system, this fixes things for real this time.

### [0.7.6]

Fixes:
- Forgot to include AssetBundles with new build system, sorry about that.

### [0.7.5]

Fixes:
- Delay searching for device layouts until *after* unity has loaded, fixes a harmless error at startup.

### [0.7.4]

Locale:
- Russian translation by @Singularia

Developer Experience Improvements:

Keyboard, Mouse, and Gamepad have generated Enums under the `LethalCompanyInputUtils.BindingPathEnums` namespace.
These enums are called `KeyboardControl`, `MouseControl`, and `GamepadControl` respectively.
These enums contain **all** the keys/buttons that are accepted by Unity's InputSystem.

The docs have been updated to reflect usage of the Enums, you may still use the string input paths if you'd like.

### [0.7.3]

Mod Integration:
- LethalConfig - Use TextDropDownConfigItem for the locale option instead of TextInputFieldConfigItem.

### [0.7.2]

Mod Integration:
- LobbyCompatibility - Register InputUtils as a client-side mod.
- LethalConfig - Change locale in-game.

### [0.7.1]

It wouldn't be an InputUtils update without a hotfix for bug I never noticed.

Fixes:
- Binds actually save instead of gaslighting you into thinking they saved.

### [0.7.0]

**Highlights:**
- Basic locale/lang loading system
  - Starting with only `en_US` for now.
  - Configurable
- Global controls
  - Controls can now be saved globally which allows for Controls to work across Modpacks/Profiles.
  - With the addition of Global Controls comes the ability to configure how InputUtils handles Control priority between Local (Modpack/Profile/Manual install) and Global Controls.
    - By default both Local and Global Controls are loaded, but with Global Controls having priority.

Internal Changes:
- Better handling of device specific overrides when serializing/deserializing.

### [0.6.3]

Fixes:
- Some Keyboard/Mouse binds were being incorrectly recognized as gamepad only binds,
  this was because I forgot about the existence of lower-case letters. I've studied up on the alphabet
  to make up for this.

Notes:
- It was brought to my attention that the wording of the previous description could potentially come across as an insult to the vanilla rebind UI,
  which was not my intention. As such I have updated the description to more accurately provide an overview of the features provided, while also using
  more neutral wording.

### [0.6.2]

Fixes:
- Gamepad only binds now only allow gamepads.

### [0.6.1]

Fixes:
- Empty bind paths for InputActions created using the builder now properly resolve.

### [0.6.0]

Features:
- Scroll Wheel rebinding support.
- Mouse button glyphs.

### [0.5.6]

Allow LcInputAction's to have NonPublic InputActions.

### [0.5.5]

Fixes unbinding vanilla keys to no longer cause the UI to be permanently broken.

### [0.5.4]

Hotfix for rebind ui breaking due to another oversight on my part.

### [0.5.3]

Fix issue with the runtime api causing errors when not defining a gamepad path.

### [0.5.2]

Fix vanilla key rebinding being broken when re-opening the rebind UI.

### [0.5.1]

Hotfix for broken installs with mod managers, sorry that was my bad.

### [0.5.0]

Highlights:

**Massive UI Overhaul**
- Cleaned up the base game controls to be more organized.
- Added Controller Glyphs for Xbox and DualSense controllers with automatic detection.
    - *Glyphs only show up in the rebind menu for now, a future update will have them be visible in game.*
    - *Api to request controller glyphs for a control path will be available in a future update.*
- Controls added by mods are seperated by mod.
- Controls manually injected into the vanilla ui by mods are available in "Legacy Controls".

Controls can now be unbound.

*Controller navigation of the new UI is still a ***WIP***, hoping to improve it in a future update.*

### [0.4.4]

Text in the remap ui now has auto-scaling size.
Remap ui no longer cuts-off the bottom of the controller section.
Changed method for determining the BepInPlugin of dependents.

### [0.4.3]

`LcInputActions.Asset` now has a public getter.
Fixed issue where binds that were unbound by default would not save/load their new binds after restarting your game.

### [0.4.2]

Hotfix for applying migrations when the new file already existed, no longer crashes the mod.

### [0.4.1]

Bind overrides have been moved from `BepInEx/controls` to `BepInEx/config/controls` allowing for bind overrides to be distributed with modpacks.

### [0.4.0]

InputActions can now be made at runtime by overriding the method `CreateInputActions(...)` and using the provided builder.

### [0.3.0]

The only required parameter for the InputActions attribute is now only just the kbmPath, the rest are now optional.
Should help improve DX.

### [0.2.0]

Interactions for the kbm and gamepad bindings can now be set in the attribute.

### [0.1.0]

Initial Beta Release.
