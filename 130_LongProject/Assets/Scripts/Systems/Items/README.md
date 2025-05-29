# Item System Guide

This document explains how to use the new Item System in your game.

## Core Components

### 1. ItemData

`ItemData` is a ScriptableObject that defines an item's properties:

- **itemName**: The display name of the item
- **icon**: The sprite used in the UI
- **description**: A text description of the item
- **weight**: How heavy the item is (affects player movement)
- **associatedQuestName**: Optional link to a quest
- **pickupEffect**: Optional VFX when picked up
- **pickupSound**: Optional SFX when picked up

#### How to create a new ItemData:

1. Right-click in the Project window
2. Select `Create > Inventory > Item`
3. Fill in the properties

### 2. InventorySystem

`InventorySystem` manages the player's inventory:

- Fixed 5-slot hotbar
- Weight system that affects player movement
- Events for item added/removed
- Methods to add, remove, and query items

#### Setup:

1. Add the `InventorySystem` component to your player
2. Configure the weight parameters:
   - `maxWeightBeforePenalty`: Maximum weight before speed reduction
   - `maxMovementPenalty`: Maximum speed reduction (0.5 = 50%)

### 3. InteractableItem

`InteractableItem` is a component for pickupable items in the world:

- Attach to any GameObject with a Collider
- Reference an ItemData
- Configure pickup options

#### Setup:

1. Create a GameObject with a Collider
2. Add the `InteractableItem` component
3. Assign an ItemData
4. Set `destroyOnPickup` to true/false

### 4. InventoryUI

`InventoryUI` provides a simple UI for the inventory:

- Displays item slots
- Shows current weight and capacity
- Tooltips for items

#### Setup:

1. Create a UI Canvas
2. Add the `InventoryUI` component
3. Create a slot prefab with:
   - Background image
   - ItemIcon image (named "ItemIcon")
   - Optional slot number text
4. Set the references in the Inspector

## Integration with Other Systems

### Player Controller

The PlayerController has been updated to:

- Select inventory slots using number keys 1-5
- Use items with the F key
- Handle interactions with items and NPCs
- Apply movement speed penalties based on inventory weight

### NPCs

NPCs can now:

- Give items to players
- Receive items from players
- Have specific dialogue for item exchanges
- Complete quests based on item exchanges

### Quests

Quests can now:

- Require specific items
- Provide item rewards on completion
- Track item-based objectives

## Examples

### Creating a Simple Quest Item:

1. Create a new ItemData (e.g., "Important Letter")
2. Set weight, icon and description
3. Set the associatedQuestName to match your quest
4. Create an InteractableItem in the world
5. Assign the ItemData to it

### Setting Up an NPC to Receive a Quest Item:

1. Select your NPC
2. In the NPCBehavior component:
   - Set itemToReceive to your quest item
   - Set the dialogue options
3. Link the NPC to a QuestTrigger

### Creating an Item Reward for Quest Completion:

1. Create a new ItemData for the reward
2. In the QuestTrigger component:
   - Enable giveItemOnCompletion
   - Set rewardItem to your reward item

## Tips and Best Practices

1. Keep item weights balanced for gameplay
2. Use clear icons and descriptions
3. Test quest item exchanges thoroughly
4. Use the ItemFactory for testing and debugging
5. Consider the impact of weight on player movement 