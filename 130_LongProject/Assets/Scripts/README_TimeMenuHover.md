# Time Menu Hover Controller

## Overview
The `TimeMenuHoverController` script allows you to automatically activate/deactivate a "time menu" game object when the mouse hovers over a Canvas UI panel. This provides an intuitive way for players to access time-related functionality when they need it.

## Features
- Automatic activation/deactivation of time menu on mouse hover
- Configurable activation and deactivation delays
- Automatic game object finding by name
- Support for manual game object assignment
- Proper coroutine management to prevent conflicts
- Debug logging for troubleshooting

## How to Set Up

### Step 1: Prepare Your Scene
1. Make sure you have a Canvas in your scene with a UI panel (e.g., Image, Panel, Button, etc.)
2. Create or ensure you have a game object named "time menu" (or any name you prefer)
3. Make sure your Canvas has a GraphicRaycaster component (automatically added to Canvas)
4. Ensure there's an EventSystem in your scene (automatically created with first UI element)

### Step 2: Add the Script
1. Select the UI panel that should detect mouse hover
2. Add the `TimeMenuHoverController` component to it
3. The UI panel must have a Graphic component (Image, Text, etc.) to receive mouse events

### Step 3: Configure the Script
In the Inspector, you'll see these options:

#### Time Menu Settings:
- **Time Menu Game Object**: Drag your time menu game object here, or leave empty to use auto-finding
- **Time Menu Name**: If you don't assign the game object manually, enter the name here (default: "time menu")

#### Optional Settings:
- **Activation Delay**: Time in seconds before the menu appears after hovering (default: 0)
- **Deactivation Delay**: Time in seconds before the menu disappears after stop hovering (default: 0)

## Example Setup Scenarios

### Scenario 1: Time Control Panel
```
1. Create a UI Image called "TimeControlPanel" 
2. Add TimeMenuHoverController to "TimeControlPanel"
3. Create a time menu UI with buttons for time acceleration, pause, etc.
4. Assign the time menu to the script or name it "time menu"
```

### Scenario 2: Status Bar Integration
```
1. Have a status bar with time display
2. Add TimeMenuHoverController to the time display element
3. Create a dropdown menu with time-related options
4. Configure a small delay to prevent accidental activation
```

## Important Notes

### UI Panel Requirements
- The UI panel MUST have a Graphic component (Image, Text, RawImage, etc.)
- The Graphic component must have "Raycast Target" enabled (default)
- The UI panel should be on a Canvas with a GraphicRaycaster

### EventSystem Requirements
- Your scene must have an EventSystem game object
- EventSystem is automatically created when you add UI elements
- If you delete it manually, create a new one: GameObject → UI → Event System

### Common Issues and Solutions

**Time menu doesn't activate:**
- Check that the UI panel has a Graphic component with "Raycast Target" enabled
- Verify EventSystem exists in the scene
- Check Console for warning messages
- Ensure the time menu game object exists and is properly assigned

**Time menu appears/disappears too quickly:**
- Adjust the activation/deactivation delays in the inspector
- Consider the size of your hover area

**Can't find time menu game object:**
- Check the exact name matches (case-sensitive)
- Use the manual assignment instead of auto-finding
- Verify the game object exists in the scene

## Script API

### Public Methods
```csharp
// Manually set the time menu game object
public void SetTimeMenuGameObject(GameObject timeMenu)

// Get the current time menu game object reference
public GameObject GetTimeMenuGameObject()
```

### Events
The script implements Unity's EventSystem interfaces:
- `IPointerEnterHandler` - Handles mouse enter events
- `IPointerExitHandler` - Handles mouse exit events

## Integration with Existing Time Systems

This script works well with the existing `DebugTimeUI` and `DayNightCycleManager` systems in your project. You can:

1. Use it to show/hide the existing DebugTimeUI panel
2. Create a new time menu that interfaces with DayNightCycleManager
3. Combine it with other UI elements for a comprehensive time control system

## Customization

Feel free to modify the script to add:
- Fade in/out animations
- Sound effects on hover
- Different activation methods (click, double-click, etc.)
- Multiple time menu variations
- Context-sensitive menus based on game state 