# Quest System and Inventory Updates

## Overview
This update significantly improves the quest system, inventory UI consistency, and NPC interactions by making everything more streamlined and quest-focused.

## Key Improvements

### 1. Enhanced Quest System

#### Multiple Required Items Support
- **QuestData** now supports arrays of required items (`ItemData[] requiredItems`) instead of single items
- **QuestData** supports multiple reward items (`ItemData[] rewardItems`)
- Added quest status messages for better player feedback:
  - `needItemsMessage`: Shown when player lacks required items
  - `hasItemsMessage`: Shown when player has all required items
  - `completionMessage`: Shown when quest is completed

#### Improved Quest Item Checking
- New `QuestItemCheckResult` class provides detailed feedback about quest item status
- `QuestManager.CheckQuestItemRequirements()` returns comprehensive information:
  - Whether player has all required items
  - Detailed message listing missing/found items
  - Arrays of missing and found items for further processing

#### Better Quest Completion Flow
- `QuestManager.ConsumeQuestItems()` safely removes required items only after verification
- `QuestManager.GiveQuestRewards()` handles multiple reward items
- Automatic reward distribution during quest completion

### 2. Inventory System Improvements

#### Consistent UI Updates
- Added `OnInventoryChanged` event that triggers whenever inventory contents change
- **InventoryUI** now uses cached slot images for better performance
- Improved event handling with proper cleanup in `OnDestroy()`
- Added `ForceUIUpdate()` method for manual refresh when needed

#### Enhanced Inventory Methods
- `GetQuestItems()`: Returns items matching specific quest requirements
- `HasAllQuestItems()`: Checks if inventory contains all required quest items
- `GetEmptySlotCount()`: Returns number of available slots
- `GetCurrentWeight()`: Returns current total weight

#### Better Error Handling
- Robust slot image finding with fallback mechanisms
- Improved null checking and error logging
- Consistent debug output for troubleshooting

### 3. Streamlined NPC Interactions

#### Quest-Focused Design
- **Removed independent item trading** - all item interactions are now quest-related
- NPCs no longer have `itemToGive` and `itemToReceive` fields
- All item exchanges happen through the quest system

#### Improved Quest Status Checking
- `CheckQuestItemStatus()`: Returns detailed quest item status without completing quest
- `CanCompleteQuestNow()`: Checks if quest can be completed with current inventory
- Better dialogue flow based on quest and item status

#### Enhanced Dialogue System
- **DialogueManager** now properly handles multiple required items
- Dynamic button text based on quest status:
  - "Accept Quest" when quest can be offered
  - "Complete Quest" when player has all required items
  - "Check Quest Status" when player is missing items
  - "Quest Progress" for general quest information

### 4. Better User Experience

#### Clear Quest Status Feedback
- Players can check quest status without completing the quest
- Detailed messages show exactly which items are needed/found
- Visual feedback through consistent inventory UI updates

#### Streamlined Interaction Flow
1. Player approaches NPC
2. Dialogue shows current quest status
3. Player can check what items they need
4. When ready, player can complete quest with proper item consumption
5. Rewards are automatically given

## Technical Implementation

### New Classes and Structures
```csharp
public class QuestItemCheckResult
{
    public bool hasAllItems;
    public string message;
    public ItemData[] missingItems;
    public ItemData[] foundItems;
}
```

### Key Method Updates
- `QuestManager.CheckQuestItemRequirements(QuestData, InventorySystem)`
- `QuestManager.ConsumeQuestItems(QuestData, InventorySystem)`
- `QuestManager.GiveQuestRewards(QuestData, InventorySystem)`
- `NPCBehavior.CheckQuestItemStatus(InventorySystem)`
- `NPCBehavior.CanCompleteQuestNow(InventorySystem)`

### Event System Improvements
- `InventorySystem.OnInventoryChanged` for comprehensive UI updates
- Proper event cleanup to prevent memory leaks
- Cached UI components for better performance

## Usage Examples

### Setting Up a Multi-Item Quest
1. Create a QuestData asset
2. Set `requiredItems` array with multiple ItemData assets
3. Set `rewardItems` array with reward ItemData assets
4. Configure quest messages for better player feedback
5. Assign quest to NPC's profile

### Testing the System
Use the updated **DebugHelper** to:
- Add/remove test items from inventory
- Test quest item checking with `TestQuestItemCheck()`
- Verify inventory UI updates consistently
- Use keyboard shortcuts (F1-F4) for quick testing

## Migration Notes

### Breaking Changes
- `QuestData.requiredItem` → `QuestData.requiredItems[]`
- `QuestData.rewardItem` → `QuestData.rewardItems[]`
- `QuestData.consumeItem` → `QuestData.consumeItems`
- Removed `NPCBehavior.itemToGive` and `NPCBehavior.itemToReceive`

### Recommended Updates
1. Update existing QuestData assets to use arrays
2. Remove any independent item trading logic
3. Update NPC profiles to use quest-based item interactions
4. Test inventory UI updates after any inventory changes

## Benefits

1. **Consistency**: Inventory UI always reflects current state
2. **Flexibility**: Quests can require multiple items and give multiple rewards
3. **Clarity**: Players get clear feedback about quest requirements
4. **Performance**: Cached UI components and efficient event handling
5. **Maintainability**: Centralized quest logic, easier to debug and extend

This update creates a more robust, user-friendly quest system that provides clear feedback and consistent behavior across all interactions.

## Recent Fixes

### Quest Button State Management (Latest)
- **Fixed**: Quest button now properly transitions from "Accept Quest" → "Check Quest Status" after accepting a quest
- **Fixed**: "Check Quest Status" button automatically updates to "Complete Quest" after checking status if player has all required items
- **Added**: Real-time quest button updates when inventory changes during dialogue
- **Improved**: Consistent button state management with dedicated `RefreshQuestButtonState()` method
- **Enhanced**: Better event handling to prevent memory leaks during dialogue

### Button State Flow
1. **"Quests"** - Initial state or when no quest available
2. **"Accept Quest"** - When NPC can offer a quest  
3. **"Check Quest Status"** - After accepting quest, shows missing/found items
4. **"Complete Quest"** - When player has all required items
5. **"Quests"** - After quest completion (back to initial state)

### Quest Dialogue Flow (Fixed)
**Correct Sequence for New Quests:**
1. **Open Dialogue**: Button shows "Quests" (player hasn't heard quest details yet)
2. **Click "Quests"**: NPC explains quest details + button changes to "Accept Quest"
3. **Click "Accept Quest"**: Quest is accepted + button changes to "Check Quest Status"
4. **Ongoing**: Button shows "Check Quest Status" or "Complete Quest" based on inventory

**Previous Issues (Fixed):**
- Button would immediately show "Accept Quest" when dialogue opened
- Players would miss quest explanation dialogue  
- Quest text wasn't displayed when clicking "Quests" button
- This happened particularly with second/subsequent NPCs after completing first quest

**Current Behavior:**
- All NPCs follow the same dialogue flow
- Players always hear quest details before accepting
- Quest text is properly displayed in dialogue area
- Quest button state properly reflects current quest status

### Multi-NPC Interaction Safety (Latest)
- **Fixed**: Properly handles switching between NPCs during dialogue without bugs
- **Added**: Automatic cleanup of event subscriptions when switching NPCs
- **Improved**: Safety checks prevent quest actions on wrong NPCs
- **Enhanced**: Comprehensive state validation before quest operations
- **Protected**: Against double event subscriptions and memory leaks
- **Fixed**: Quest button state now properly initializes when opening dialogue
- **Improved**: Quest buttons show correct state (Accept/Check Status/Complete) immediately
- **Fixed**: Quest dialogue flow - players must hear quest details before accepting
- **Enhanced**: Proper quest offering sequence for all NPCs

### Safety Features
- **Event Management**: Proper cleanup when switching between NPCs
- **State Validation**: Multiple safety checks before quest operations
- **Error Prevention**: Robust handling of null references and invalid states
- **Debug Logging**: Comprehensive logging for troubleshooting NPC interactions
- **Quest State Tracking**: Real-time quest button updates and proper initialization 