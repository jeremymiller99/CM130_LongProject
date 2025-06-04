# Level Start Cutscene System

The `LevelStartCutscene` script provides a seamless opening cutscene for your level scene that takes control of the camera and player, moves them forward while dialogue plays, then smoothly returns control to the player.

## Features

- **Automatic Control Takeover**: Disables player and camera controls at scene start
- **Smooth Player Movement**: Moves the player forward with walking animation
- **Camera Control**: Can either keep the existing camera or use custom camera transitions
- **Dialogue Integration**: Uses your existing DialogueManager system
- **Fade Effects**: Optional fade in/out transitions
- **Skip Functionality**: Players can skip the cutscene if needed
- **Visual Debugging**: Gizmos in the Scene view to visualize paths and positions

## Setup Instructions

### 1. Add the Script to Your Level Scene

1. Create an empty GameObject in your level scene
2. Name it something like "LevelStartCutscene"
3. Add the `LevelStartCutscene` component to it

### 2. Configure Basic Settings

#### Cutscene Settings
- **Cutscene Duration**: How long the cutscene lasts (default: 3 seconds)
- **Walk Speed**: Speed at which the player moves (default: 2 units/second)
- **Auto Start**: Whether to start automatically when the scene loads (recommended: true)

#### Player Movement
- **Player Start Position**: Where to place the player at cutscene start (leave as 0,0,0 to use current position)
- **Player End Position**: Where the player should end up (leave as 0,0,0 to auto-calculate based on walk speed)
- **Rotate Player To Move Direction**: Whether to face the player toward their movement direction

#### Dialogue
- **Dialogue Lines**: Array of strings to display during the cutscene
- **Dialogue Display Time**: How long each line is shown (default: 2 seconds)
- **Show Dialogue Sequentially**: Whether to show lines one after another or as one continuous message

### 3. Optional Camera Control

If you want custom camera movement during the cutscene:

1. Enable **Use Camera Transition**
2. Set **Camera Start Position** and **Camera Start Rotation**
3. Set **Camera End Position** and **Camera End Rotation**
4. Adjust the **Camera Move Curve** for smooth transitions

If disabled, the script will simply disable your ThirdPersonCamera component and leave the camera in its current position.

### 4. Optional Fade Effects

- **Start With Fade In**: Fades from black at the beginning
- **End With Fade Out**: Fades to black at the end
- **Fade Duration**: How long the fade takes
- **Fade Color**: Color to fade to/from (default: black)

## Example Setup

Here's a typical configuration for a level opening:

```
Cutscene Duration: 4.0
Walk Speed: 1.5
Auto Start: ✓

Player Start Position: (0, 0, 0)  // Uses current position
Player End Position: (0, 0, 5)   // Walk 5 units forward
Rotate Player To Move Direction: ✓

Dialogue Lines:
- "You've arrived at the mysterious forest..."
- "Strange sounds echo from the depths ahead."
- "Your adventure begins now."

Dialogue Display Time: 2.5
Show Dialogue Sequentially: ✓

Start With Fade In: ✓
Fade Duration: 1.5
```

## Visual Debugging

When you select the GameObject with the LevelStartCutscene component in the Scene view, you'll see:

- **Green spheres and line**: Player start and end positions with movement path
- **Blue cubes and line**: Camera start and end positions with movement path (if camera transition is enabled)

This helps you visualize exactly where the cutscene will move the player and camera.

## Script Integration

The script automatically finds and works with:
- Your existing `PlayerController`
- Your existing `ThirdPersonCamera`
- Your existing `DialogueManager`

No additional setup is required for these systems.

## Public Methods

You can control the cutscene from other scripts:

```csharp
LevelStartCutscene cutscene = FindObjectOfType<LevelStartCutscene>();

// Check if cutscene is active
if (cutscene.IsCutsceneActive()) { /* ... */ }

// Check if cutscene has completed
if (cutscene.IsCutsceneCompleted()) { /* ... */ }

// Allow player to skip cutscene
if (Input.GetKeyDown(KeyCode.Escape))
{
    cutscene.SkipCutscene();
}

// Manually start cutscene (if AutoStart is disabled)
cutscene.StartCutscene();
```

## Tips

1. **Test Positioning**: Use the Scene view gizmos to position your start/end points accurately
2. **Dialogue Timing**: Match your dialogue display time with the natural speech pace
3. **Camera Transitions**: If not using custom camera positions, the existing camera will stay put while the player moves
4. **Player Animation**: The script automatically triggers walking animations during movement
5. **Scene Transitions**: This works great after loading from your intro cutscene or main menu

## Troubleshooting

**Player doesn't move**: Check that Player Start Position and Player End Position are set correctly, or that Player End Position is (0,0,0) for auto-calculation.

**No dialogue appears**: Ensure your DialogueManager is properly set up in the scene.

**Camera doesn't move**: If you want camera movement, make sure "Use Camera Transition" is enabled and camera positions are set.

**Cutscene doesn't start**: Check that "Auto Start" is enabled and the PlayerController exists in the scene.

**Control doesn't return**: This usually indicates an error in the coroutines. Check the console for error messages. 