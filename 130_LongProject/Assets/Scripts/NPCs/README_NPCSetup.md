# 🎭 Complete NPC Setup Guide

This guide shows you how to set up a new NPC from scratch using the simplified 3-state NPC system.

## 🚀 **Quick Setup (5 Minutes)**

### **Step 1: Create the NPC GameObject**
1. **Create GameObject**: Right-click in Hierarchy → Create Empty
2. **Name it**: `NPC_[Name]` (e.g., "NPC_Villager", "NPC_Merchant")
3. **Add 3D Model**: Drag your character model as child of the NPC GameObject
4. **Position**: Move the NPC to desired location in your scene

### **Step 2: Auto-Setup with NPCSetupHelper**
1. **Add Component**: Select NPC GameObject → Add Component → NPCSetupHelper
2. **Configure Settings**:
   ```
   ✅ Auto Setup On Start: true (optional)
   
   Animation Settings:
   🎭 Animator Controller: (leave empty for now)
   🚶 Use Root Motion: false
   
   Movement Settings:
   🏃 Walk Speed: 1.5
   📍 Walk Radius: 10
   ⏰ State Change Interval: 5
   🎲 Walk Chance: 0.3
   
   Debug:
   🔍 Show Debug Info: true
   ```

3. **Click "Setup NPC Components"** - This automatically adds:
   - NPCBehavior
   - NPCAnimationController  
   - Animator
   - NavMeshAgent
   - Collider (trigger for interactions)

### **Step 3: Create Animator Controller**
1. **Click "Create Basic Animator Controller"** in NPCSetupHelper
   - Creates controller at `Assets/Animators/[NPCName]_Controller.controller`
   - Sets up 3 states: Idle, Walking, Talking
   - Configures transitions and parameters

### **Step 4: Assign Animation Clips**
1. **Open Animator Window**: Window → Animation → Animator
2. **Select your NPC** to see the controller
3. **Assign clips to each state**:
   - **Idle State**: Click state → Inspector → Motion → Your idle animation
   - **Walking State**: Click state → Inspector → Motion → Your walk animation
   - **Talking State**: Click state → Inspector → Motion → Your talk animation

### **Step 5: Create NPC Profile**
1. **Create Profile Asset**:
   - Right-click in Project → Create → NPC → NPC Profile
   - Name it: `[NPCName]_Profile`

2. **Configure Profile**:
   ```
   Basic Info:
   📝 NPC Name: "Villager Smith"
   🖼️ Portrait: [Character portrait sprite]
   
   Dialogue Categories:
   💬 Greeting: "Hello there, traveler!"
   🎯 Quest Dialogue: "I have a task for you..."
   📚 Lore Dialogue: "This village has a rich history..."
   🛒 Trade Dialogue: "Looking to buy or sell?"
   💭 General Dialogue: "How can I help you?"
   
   Quest Link:
   🎯 Assigned Quest: [Optional quest data]
   
   Options:
   ✅ Has Quests: true
   ✅ Has Lore: true
   ✅ Has Trade: false
   ✅ Has General Chat: true
   ```

3. **Assign Profile**: Select NPC → NPCBehavior → Profile → Assign your profile

### **Step 6: Validation & Testing**
1. **Validate Setup**: Click "Validate Setup" in NPCSetupHelper
2. **Check Output**: All items should show ✅ in Console
3. **Test in Play Mode**:
   - NPC should idle and randomly walk around
   - Walk up to NPC to see interaction prompt
   - Press E to start dialogue

---

## 🎨 **Advanced Customization**

### **Custom Animations**
```csharp
// Access animation controller for custom animations
NPCAnimationController animController = npc.GetComponent<NPCAnimationController>();

// Play custom animations
animController.PlayCustomAnimation("CustomWave");
animController.SetCustomParameter("CustomBool", true);
animController.TriggerCustomParameter("CustomTrigger");

// Facial expressions (if using facial materials)
animController.SetHappyExpression();
animController.SetRandomExpression();
```

### **Item Exchange Setup**
```csharp
// In NPCBehavior component:
[Header("Item Exchange")]
public ItemData itemToGive;      // Item NPC gives to player
public ItemData itemToReceive;   // Item NPC wants from player
public string giveItemDialogue = "Here, take this.";
public string receivedItemDialogue = "Thank you!";
```

### **Quest Integration**
1. **Create QuestData**: Right-click → Create → Quest → Quest Data
2. **Assign to Profile**: NPCProfile → Assigned Quest
3. **Add QuestTrigger**: Add Component → QuestTrigger to NPC

---

## 🎬 **Animation Troubleshooting Guide**

### **🚨 Quick Fix for Animation Issues**

If your NPC animations aren't playing, follow these steps:

#### **Step 1: Add Animation Debugger**
1. **Add Component**: Select your NPC → Add Component → NPCAnimationDebugger
2. **Right-click** on NPCAnimationDebugger → Debug Animator Setup
3. **Check Console** for detailed debug info

#### **Step 2: Verify Animator Setup**
1. **Check Animator Component**:
   - Select NPC → Animator component should exist
   - Controller field should have your controller assigned

2. **Check Animator Controller**:
   - Open Window → Animation → Animator
   - Select your NPC to see the controller
   - Should show: Idle, Walking, Talking states

#### **Step 3: Assign Animation Clips**
**This is the most common issue!**
1. **In Animator window**, click each state:
   - **Idle State**: Inspector → Motion → Assign idle animation clip
   - **Walking State**: Inspector → Motion → Assign walk animation clip  
   - **Talking State**: Inspector → Motion → Assign talk animation clip

2. **If you don't have animation clips**:
   - Create simple ones or download from Unity Asset Store
   - Or use Unity's default Humanoid animations

#### **Step 4: Check Parameters**
In Animator window, verify these parameters exist:
- **Speed** (Float)
- **IsWalking** (Bool)  
- **IsTalking** (Bool)
- **TriggerTalk** (Trigger)

#### **Step 5: Test Animation States**
1. **Add NPCAnimationDebugger** to your NPC
2. **In Play Mode**, use the checkboxes to test:
   - ✅ Test Idle State
   - ✅ Test Walk State  
   - ✅ Test Talk State

---

### **🔧 Common Animation Problems & Solutions**

#### **❌ Problem: No animations play at all**
**Solutions:**
- ✅ Ensure Animator Controller is assigned
- ✅ Check animation clips are assigned to states
- ✅ Verify NPCAnimationController component exists
- ✅ Check Console for error messages

#### **❌ Problem: Only some animations work**
**Solutions:**
- ✅ Check specific state has animation clip assigned
- ✅ Verify transitions between states are set up
- ✅ Check parameter names match exactly

#### **❌ Problem: Animations play but wrong ones**
**Solutions:**
- ✅ Check which clip is assigned to which state
- ✅ Verify state names match (Idle, Walking, Talking)
- ✅ Check animation controller transitions

#### **❌ Problem: Animations are choppy/glitchy**
**Solutions:**
- ✅ Set Transition Duration to 0.25s in Animator
- ✅ Disable "Has Exit Time" on transitions
- ✅ Check animation clip loop settings

---

### **🎯 Animation System Overview**

The NPC uses **Boolean Parameters** to control animations:

```
State Mappings:
- Idle     → All booleans false
- Walking  → IsWalking = true
- Talking  → IsTalking = true + TriggerTalk trigger
```

**Animation Flow:**
1. NPCBehavior changes state (Idle/Walking/Talking)
2. NPCAnimationController updates animator parameters
3. Animator transitions to appropriate state
4. Animation clip plays

---

### **🛠️ Manual Animation Testing**

#### **Test in Inspector (Play Mode)**
1. Add NPCAnimationDebugger component
2. Use checkboxes to force state changes:
   ```
   ✅ Test Idle State
   ✅ Test Walk State
   ✅ Test Talk State
   ```

#### **Test via Console Commands**
```csharp
// Get NPC and test states
NPCBehavior npc = GameObject.Find("YourNPC").GetComponent<NPCBehavior>();
npc.SetState(NPCState.Walking);
npc.SetState(NPCState.Talking);
npc.SetState(NPCState.Idle);
```

#### **Test Animator Directly**
```csharp
// Force animator parameters
Animator anim = npc.GetComponent<Animator>();
anim.SetBool("IsWalking", true);
anim.SetBool("IsTalking", true);
anim.SetTrigger("TriggerTalk");
```

---

## 🛠️ **Troubleshooting**

### **Common Issues**

**❌ NPC doesn't move**
- Check NavMesh is baked in scene
- Ensure NPC is positioned on NavMesh
- Verify NavMeshAgent is enabled

**❌ No interaction prompt**
- Check Collider is set as Trigger
- Ensure Player has PlayerController component
- Verify IInteractable system is working

**❌ Animations not playing**
- **MOST COMMON**: Animation clips not assigned to states
- Check Animator Controller is assigned
- Verify animation parameter names match
- Use NPCAnimationDebugger for detailed diagnosis

**❌ Dialogue not working**
- Ensure DialogueManager exists in scene
- Check NPCProfile is assigned and configured
- Verify dialogue UI panels are set up

### **Debug Commands**
```csharp
// Validation
npcSetupHelper.ValidateSetup();

// Animation debugging
npcAnimationDebugger.DebugAnimatorSetup();
npcAnimationDebugger.TestAllStates();

// Check components
bool hasAllComponents = npc.GetComponent<NPCBehavior>() != null &&
                       npc.GetComponent<NPCAnimationController>() != null &&
                       npc.GetComponent<Animator>() != null &&
                       npc.GetComponent<NavMeshAgent>() != null;

// Force state change (for testing)
npcBehavior.SetState(NPCState.Walking);
npcBehavior.SetState(NPCState.Talking);
```

---

## 📋 **Quick Checklist**

- [ ] GameObject created and positioned
- [ ] NPCSetupHelper added and configured
- [ ] "Setup NPC Components" clicked
- [ ] "Create Basic Animator Controller" clicked
- [ ] Animation clips assigned to states
- [ ] NPCProfile created and configured
- [ ] Profile assigned to NPCBehavior
- [ ] "Validate Setup" shows all ✅
- [ ] Tested in Play Mode

---

## 🎯 **3-State System Overview**

The simplified NPC system uses only 3 states:

1. **🧍 Idle**: NPC stands still, plays idle animation
2. **🚶 Walking**: NPC moves to random point within radius
3. **💬 Talking**: NPC is in dialogue with player

**Behavior Logic**:
- Every 5 seconds (configurable), NPC decides whether to walk or stay idle
- 30% chance (configurable) to start walking
- When walking, picks random point within walkRadius
- Automatically returns to Idle when reaching destination
- Talking state activates during dialogue interactions

This simple system is easy to understand, debug, and extend while providing natural-looking NPC behavior. 