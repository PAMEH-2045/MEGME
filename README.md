# ME Gesture Manager Emulator

A port of BlackStartx's [Gesture Manager](https://github.com/BlackStartx/VRC-Gesture-Manager) for [Mate Engine](https://github.com/shinyflvre/Mate-Engine)

| Feature                                | Ported? | Implementation Details         |
|:-------------------------------------- |:-------:| ------------------------------ |
| Locomotion, Tracking, States and Extra | -       |                                |
| Edit Mode                              | -       |                                |
| Expressions menu                       | +       | Replaces the clothing menu     |
| Gestures control                       | -       |                                |
| Scene Camera                           | -       |                                |
| Clickable Contacts                     | +       | Active only in Big Screen mode |
| Pose Avatar                            | -       |                                |
| Avatar Background                      | -       |                                |
| Test Animation                         | -       |                                |
| Animator Performance                   | -       |                                |
| Avatar Debug                           | -       |                                |
| OSC Debug                              | -       |                                |

## Installation

1. Download archive and `.me` file from [release](https://github.com/PAMEH-2045/MEGME/releases/latest)

2. Install `.me`

3. Unpack archive

4. Copy and paste MateEngineX_Data folder to game root with overwrite

## Model Preparing

### 1.A Installing VRCSDK to Mate Engine project

1. Download VRCSDK Base and Avatars https://github.com/vrchat/packages/releases/latest
   
   > com.vrchat.avatars-X.X.X.zip and com.vrchat.base-X.X.X.zip archives

2. Import VRCSDK packages to your ME Unity project
   
   > Window > Package Managment > Package Manager, 
   > 
   > "+" in upper left corner > Install package from disk
   > locate `package.json` file in your extracted archive

3. Delete `Packages/VRChat SDK - Base/Runtime/VRCSDK/Plugins/SDKBase-Legacy.dll`

### 1.B Installing MESDK to VRChat Creator Companion project

To export model you actually need only `Assets\Editor\MEModelExporter.cs` script

So copy it from ME project and place in `Editor` directory ( If dont have one create somewhere under `Assets` ) of VRCCC avatar project

### 2. Configure Blendshapes

1. Add `VRM Blend Shape Proxy` to a model

2. Create `Blend Shape Avatar` asset in `Project`

3. Add and configure `BlendShapeClip` for every preset

### 3.A In case of Modular Avatar

- RMB to a avatar gameobject in Hierarchy, `Modular Avatar > Manual Bake Avatar`

### 3.B In case of VRCFury

- Select avatar gameobject, in menubar: `Tools > VRCFury > Build an Editor Test Copy`

### 4. Export

## How does it work?

- Replaces clothes button in ME's Radial menu if  `VRC Avatar Descriptor` present on model

- Binds ME controller to a Base layer of `VRC Avatar Descriptor`
