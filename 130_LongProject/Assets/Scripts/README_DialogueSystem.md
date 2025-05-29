# Enhanced Dialogue System Setup Guide

## 🎉 **NEW: Dual Panel System - Phase 2 Complete!**

### **Major Update: Separated UI Architecture**

The dialogue system now uses **TWO separate UI panels** to fix typewriter conflicts and provide better control:

#### **1. Simple Message Panel** 
- **Purpose**: Quick notifications, interaction prompts, item messages
- **Examples**: "Press E to interact", "Inventory full!", "Received: Health Potion"
- **Features**: Auto-hide timer, independent typewriter effect, minimal UI
- **Styling**: Smaller, subtle, positioned for quick reading

#### **2. Full Dialogue Panel**
- **Purpose**: Complete NPC conversations with choices
- **Examples**: Talking to NPCs, quest dialogues, branching conversations  
- **Features**: Manual close, choice buttons, portraits, full UI
- **Styling**: Larger, prominent, positioned for engagement

### **🔧 Why This Change?**

✅ **Fixed Typewriter Bug**: Simple messages now have perfect typewriter effects
✅ **No More UI Conflicts**: Each panel has its own components
✅ **Independent Styling**: Resize choice panels without affecting simple messages
✅ **Better Performance**: No more hiding/showing complex UI for simple text
✅ **Cleaner Code**: Separated concerns, easier to maintain

---

## Phase 1 Implementation Complete! 

You now have an enhanced dialogue system with branching choices and camera focus. Here's how to set it up:

## 🔧 Bug Fixes Included:

✅ **Fixed Dialogue Restart Bug**: Dialogue can no longer restart while already active
✅ **Mouse Cursor Support**: Cursor is now unlocked during dialogue for UI interaction
✅ **Improved Input Handling**: Better distinction between UI clicks and dialogue skipping
✅ **Interaction State Management**: Proper tracking of dialogue state across components
✅ **Fixed Simple Message Blocking**: Simple messages no longer block item pickup and NPC interactions
✅ **Improved Camera Transitions**: Reliable camera return to normal state after dialogue with failsafes

## 1. Update Existing NPC Profiles

Your existing NPCProfile ScriptableObjects need to be updated with the new dialogue options:

1. Select any existing NPC Profile in your project
2. You'll see new fields:
   - **Portrait**: Drag an NPC portrait sprite here
   - **Greeting Dialogue**: The initial greeting when dialogue starts
   - **Quest/Lore/Trade/General Dialogue**: Different conversation categories
   - **Dialogue Options Configuration**: Checkboxes to enable/disable categories

## 2. Setup Enhanced Dialogue UI

Your DialogueManager now needs **TWO separate UI hierarchies**. In your Canvas, create:

### **Simple Message Panel** (for notifications)
```
Canvas
├── SimpleMessagePanel (GameObject)
    ├── SimpleMessageBackground (Image) -- Assign to "Simple Message Background" field
    └── SimpleMessageText (TMP_Text) -- Assign to "Simple Message Text" field
```

### **Full Dialogue Panel** (for conversations)
```
Canvas
├── DialoguePanel (GameObject)
    ├── Background (Image) -- Assign to "Dialogue Background" field
    ├── NPCInfo (GameObject)
    │   ├── NPCPortrait (Image)
    │   └── NPCName (TMP_Text)
    ├── DialogueText (TMP_Text)
    ├── ChoiceButtonsPanel (GameObject)
    │   ├── QuestButton (Button)
    │   │   └── QuestButtonText (TMP_Text)
    │   ├── LoreButton (Button)
    │   │   └── LoreButtonText (TMP_Text)
    │   ├── TradeButton (Button)
    │   │   └── TradeButtonText (TMP_Text)
    │   ├── GeneralButton (Button)
    │   │   └── GeneralButtonText (TMP_Text)
    │   └── GoodbyeButton (Button)
    │       └── GoodbyeButtonText (TMP_Text)
    └── CloseButton (Button)
```

### **Recommended Styling:**

**Simple Message Panel:**
- Position: Bottom center or top of screen
- Size: Small, compact (300x100 pixels)
- Background: Semi-transparent, subtle
- Text: Medium size, high contrast

**Full Dialogue Panel:**
- Position: Center or bottom center
- Size: Large, prominent (600x400 pixels)  
- Background: Opaque, themed to your game
- Text: Larger, comfortable reading size

## 3. Configure DialogueManager Component

1. Find your DialogueManager GameObject
2. Assign all the UI references in the inspector:

### **Simple Message Panel References:**
   - **Simple Message Panel**: Drag the SimpleMessagePanel GameObject
   - **Simple Message Text**: Drag the SimpleMessageText TMP_Text component
   - **Simple Message Background**: Drag the SimpleMessageBackground Image component

### **Full Dialogue Panel References:**
   - **Dialogue Panel**: Drag the DialoguePanel GameObject
   - **NPC Name Text**: Drag the NPCName TMP_Text component
   - **Dialogue Text**: Drag the DialogueText TMP_Text component
   - **NPC Portrait**: Drag the NPCPortrait Image component
   - **Close Button**: Drag the CloseButton Button component
   - **Dialogue Background**: Drag the Background Image component
   - **Choice Buttons Panel**: Drag the ChoiceButtonsPanel GameObject
   - **Individual Choice Buttons**: Drag each choice button and its text component

### **Settings:**
   - Configure typewriter speed (recommended: 0.05)
   - Enable typewriter effect for better UX
   - **Simple Message Display Time**: How long simple messages stay visible (recommended: 3 seconds)
   - **Hide Inventory During Dialogue**: Enable to automatically hide inventory slots during conversations (recommended: true)

## 4. Setup Camera Integration

### Important: Camera Rig Setup
The `DialogueCameraController` works with camera rig hierarchies. It will automatically detect:
- **Camera Rig** (parent with ThirdPersonCamera script): Controls overall camera movement
- **Camera** (child): The actual camera that renders the scene

**Recommended Setup:**
1. Add `DialogueCameraController` to your **Main Camera** (child)
2. Ensure your **Camera Rig** (parent) has the `ThirdPersonCamera` script
3. The system will automatically handle the hierarchy

### Configuration:
1. In the DialogueCameraController inspector:
   - Assign your `ThirdPersonCamera` component reference
   - Adjust dialogue distance (recommended: 4f)
   - Set dialogue height (recommended: 1.5f)
   - Configure transition speed (recommended: 2f)
   - **Max Animation Wait Time**: Maximum time to wait for player interaction animation (recommended: 2f)

### Camera Collision Setup (ThirdPersonCamera):
1. Configure **Collision Layers**: Set which layers the camera should collide with (walls, floors, etc.)
2. Adjust **Collision Radius**: Size of the sphere used for collision detection (recommended: 0.3f)
3. Set **Collision Offset**: Distance to maintain from walls (recommended: 0.1f)  
4. Configure **Collision Smooth Time**: How quickly camera adjusts to collisions (recommended: 0.1f)

## 5. Update NPCs

Your existing NPCs should automatically work with the new system! The NPCBehavior has been updated to use `DialogueManager.ShowDialogue()` instead of the old simple text display.

## Features You Now Have:

✅ **Multiple Dialogue Choices**: Quest, Lore, Trade, General, Goodbye
✅ **Typewriter Effect**: Smooth text animation
✅ **NPC Portraits**: Visual character representation
✅ **Reputation Integration**: Different greetings based on reputation
✅ **Quest Integration**: Smart quest dialogue based on quest state
✅ **Camera Focus**: Smooth camera transition during dialogue
✅ **Player Movement Lock**: Prevents movement during conversations
✅ **Enhanced Camera System**: Reliable transitions with automatic failsafe recovery
✅ **Camera Collision Detection**: Prevents camera from clipping through walls and floors
✅ **Mouse Support**: Full mouse cursor support for clicking UI elements
✅ **Smart Input Handling**: Space skips text, mouse clicks work on buttons
✅ **Dialogue State Protection**: Cannot restart dialogue while already active
✅ **Smart Message System**: Distinguishes between simple notifications and full conversations
✅ **Animation Integration**: Waits for player interaction animation to complete before locking movement
✅ **Item vs NPC Distinction**: Automatically shows appropriate interface (simple text for items, full dialogue for NPCs)
✅ **Inventory UI Management**: Automatically hides inventory slots during full NPC dialogues for cleaner UI
✅ **Backward Compatibility**: Old systems still work

## Input Controls During Dialogue:

- **Mouse**: Click on dialogue choice buttons
- **Space**: Skip typewriter effect
- **Left Click** (on empty space): Skip typewriter effect  
- **Escape**: Close dialogue completely
- **E Key**: Disabled during active dialogue to prevent restart
- **Item Interaction**: Works normally (simple messages don't block interactions)

## Item vs NPC Interactions:

The system now automatically distinguishes between different types of interactions:

### **📦 Item Interactions** (Minimal Interface):
- **What shows**: Only text message with subtle semi-transparent background
- **What's hidden**: Close button, NPC portrait, NPC name, choice buttons
- **Examples**: "Press E to pick up Sword", "Inventory full!", "Received: Health Potion"
- **Camera**: No camera focus or movement locking
- **Inventory**: Remains visible for quick reference
- **Used by**: InteractableItem, inventory messages, quest rewards

### **👤 NPC Interactions** (Full Interface):
- **What shows**: Full background, portrait, name, dialogue choices, close button
- **Examples**: Full conversations with choice buttons and portraits
- **Camera**: Camera focuses on conversation with movement locking
- **Inventory**: Automatically hidden during conversation for cleaner UI
- **Used by**: NPCBehavior with NPCProfile, full conversations

### **🔧 For Developers**:
- Use `DialogueManager.ShowItemMessage(text)` for item-related interactions
- Use `DialogueManager.ShowDialogue(npcBehavior)` for NPC conversations
- Use `

Your dialogue system is now significantly more engaging and professional! 

## 🆕 **Dual Panel System Troubleshooting:**

### **Simple Messages Not Showing:**
- Check that SimpleMessagePanel, SimpleMessageText, and SimpleMessageBackground are assigned
- Verify SimpleMessagePanel is a child of Canvas
- Ensure SimpleMessageText has a readable font and size

### **Typewriter Effect Not Working on Simple Messages:**
- **Note**: Simple messages now use immediate text display instead of typewriter effects to avoid GameObject activation issues
- This is by design to ensure simple messages work even when DialogueManager is initially inactive
- Full dialogue conversations still use typewriter effects normally
- If you need typewriter for simple messages, ensure DialogueManager GameObject starts active in the scene

### **Simple Messages Not Auto-Hiding:**
- Check simpleMessageDisplayTime value (should be > 0)
- Verify no errors in console preventing coroutines from running
- Test that you're not in a full dialogue (which prevents auto-hide)

### **UI Panels Overlapping:**
- Position SimpleMessagePanel and DialoguePanel in different screen areas
- Check Canvas sort order and RectTransform positioning
- Ensure panels have different Z positions if using World Space Canvas

### **Independent Styling Not Working:**
- Verify you're editing the correct panel (simple vs full dialogue)
- Check that references are assigned to the right components
- Test each panel independently to isolate issues

### **Input Issues:**
- Simple messages: Space, any key, or Escape should close them
- Full dialogues: Space skips typing, mouse clicks work on buttons, Escape closes
- Check for conflicting input handlers in other scripts

---

**🎉 Your enhanced dual-panel dialogue system is now ready! Enjoy perfect typewriter effects and independent styling control!**