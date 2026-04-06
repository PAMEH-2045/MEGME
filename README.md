# ME Gesture Manager Emulator

A port of BlackStartx's [Gesture Manager](https://github.com/BlackStartx/VRC-Gesture-Manager) for [Mate Engine](https://github.com/shinyflvre/Mate-Engine)

| Feature                                | Ported? | Inplementation Details         |
|:-------------------------------------- |:-------:| ------------------------------ |
| Locomotion, Tracking, States and Extra | -       |                                |
| Edit Mode                              | -       |                                |
| Expressions menu                       | +       | As is                          |
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

1. Install [MEPhysBone](https://github.com/PAMEH-2045/MEPhysBone)

2. Download release archive and unpack

3. Copy and paste MateEngineX_Data folder to game root with overwrite



## Model Preparing

1. Either install VRCSDK to ME project or bring MESDK to VRCCC project

2. Add `VRM Blend Shape Proxy` to a model

3. Remove `Pipeline Manager` component that is located on model root next to a `VRC Avatar Descriptor`

4. Export 



## How does it work?

- Replaces clothes button in ME's Radial menu if  `VRC Avatar Descriptor` present on model

- Binds ME controller to a Base layer of `VRC Avatar Descriptor`