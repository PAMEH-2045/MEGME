# ME Gesture Manager Emulator

A port of BlackStartx's [Gesture Manager](https://github.com/BlackStartx/VRC-Gesture-Manager) for [Mate Engine](https://github.com/shinyflvre/Mate-Engine)

| Feature                                                                                    | Ported? | Implementation Details         |
|:------------------------------------------------------------------------------------------ |:-------:| ------------------------------ |
| Options - Locomotion, States, Edit Mode, Tracking<br> and Extra                              | -       |                                |
| Expressions - Menu                                                                         | +       | Replaces the clothing menu     |
| Expressions - Quick Actions                                                                | -       |                                |
| Looks                                                                                      | +       |                                |
| Clones                                                                                     | -       |                                |
| Gestures control                                                                           | -       |                                |
| Tools - Clickable Contacts                                                                 | +       | Active only in Big Screen mode |
| Tools - Scene Camera, Pose Avatar, Avatar Background,<br> Test Animation and Animator Performance | -       |                                |
| Debug - Avatar, OSC                                                                        | -       |                                |

## Installation

1. Download archive and `.me` file from [release](https://github.com/PAMEH-2045/MEGME/releases/latest)

2. Install `.me`

3. Unpack archive

4. Copy MateEngineX_Data folder to game root with overwrite

## Avatar Preparing

Assuming a working VRC avatar with an Expressions Menu already configured, the remaining steps are to set up VRM blendshapes and export avatar to the `.me` format

### 1.A Install VRCSDK to Mate Engine project

1. Download VRCSDK Base and Avatars packages https://github.com/vrchat/packages/releases/latest
   
   > com.vrchat.avatars-X.X.X.zip and com.vrchat.base-X.X.X.zip archives

2. Import packages to your ME Unity project
   
   > Window > Package Managment > Package Manager, 
   > 
   > "+" sign > Install package from disk
   > locate `package.json` file in your extracted archive

3. Delete `Packages/VRChat SDK - Base/Runtime/VRCSDK/Plugins/SDKBase-Legacy.dll`

### 1.B Install MESDK to VRChat Creator Companion project

To export an avatar you need `Assets\Editor\MEModelExporter.cs` script

Copy it from ME project to `Editor` directory of VCC avatar project ( If dont have one create anywhere under `Assets` )

### 2. Configure Blendshapes

1. Add `VRM Blend Shape Proxy` component to the avatar

2. Create `Blend Shape Avatar` asset in `Project`

3. Add and configure `BlendShapeClip` for every preset

### 3.A In case of Modular Avatar

Right click the avatar gameobject in Hierarchy, `Modular Avatar > Manual Bake Avatar`

### 3.B In case of VRCFury

Select avatar gameobject, in menubar: `Tools > VRCFury > Build an Editor Test Copy`

### 4. Export

## How does it work?

- Replaces clothes button in ME's Radial menu if  `VRC Avatar Descriptor` present on model

- Creates two Playables from original ME controller

- Connects them to Descriptor's mixer, resulting in the following graph:
  
  ⠀⠀⠀⠀⠀⠀⠀⠀┌─ First ME Playable
  
  Output ─ Mixer ─ Base - FX descriptor layers
  
  ⠀⠀⠀⠀⠀⠀⠀⠀└─ Second ME Playable
