# NPC Animation & State System Setup Guide

## 🎭 **Enhanced NPC System Overview**

Your NPCs now have a complete animation and state management system that integrates seamlessly with your existing dialogue system! This system provides:

✅ **6 Different NPC States**: Idle, Walking, Talking, Working, Sitting, Sleeping  
✅ **Smart State Transitions**: NPCs automatically switch between behaviors  
✅ **Time-Based Behavior**: NPCs behave differently during day/night cycles  
✅ **Dialogue Integration**: NPCs automatically enter talking state during conversations  
✅ **NavMesh Movement**: Intelligent pathfinding and wandering  
✅ **Animation Controller**: Clean interface for managing animations  
✅ **Facial Expressions**: Optional material-based expression system  
✅ **Patrol Routes**: Optional predefined paths for NPCs  
✅ **Work Stations**: NPCs can have jobs and work areas  

---

## 🚀 **Quick Setup (5 Steps)**

### **Step 1: Add Components to Your NPCs**

For each NPC GameObject, add these components:
1. **NPCStateMachine** (Main behavior system)
2. **NPCAnimationController** (Animation management)
3. **Animator** (Unity's animation component)
4. **NavMeshAgent** (For movement)
5. **NPCBehavior** (Your existing dialogue system - keep this!)

```
Your NPC GameObject:
├── NPCBehavior (existing)
├── NPCProfile (existing) 
├── NPCStateMachine (NEW)
├── NPCAnimationController (NEW)
├── Animator (Unity built-in)
├── NavMeshAgent (Unity built-in)
├── Collider (existing - for interactions)
└── Your 3D Model/Meshes
```

### **Step 2: Setup NavMesh in Your Scene**

1. Select all static geometry (floors, walls, etc.)
2. Mark them as **Navigation Static** in the Inspector
3. Go to **Window → AI → Navigation**
4. Click **Bake** to generate NavMesh
5. Make sure your NPCs are positioned on the NavMesh (blue areas)

### **Step 3: Create Animator Controller**

1. **Right-click** in Project → **Create → Animator Controller**
2. Name it `NPCAnimatorController`
3. **Double-click** to open the Animator window
4. Create states for your animations:

**Required States:**
- **Idle** (default state)
- **Walk** 
- **Talk**
- **Work** (optional)
- **Sit** (optional)  
- **Sleep** (optional)

**Required Parameters:**
- `Speed` (Float) - For movement speed
- `IsWalking` (Bool) - Walking state
- `IsTalking` (Bool) - Talking state
- `IsWorking` (Bool) - Working state
- `IsSitting` (Bool) - Sitting state
- `IsSleeping` (Bool) - Sleeping state
- `TriggerTalk` (Trigger) - Start talking animation
- `TriggerWork` (Trigger) - Start working animation

### **Step 4: Configure Components**

**NPCStateMachine Settings:**
- **Walk Radius**: How far NPCs wander (recommended: 10)
- **Walk Speed**: Movement speed (recommended: 1.5)
- **State Change Interval**: How often to change behaviors (recommended: 5 seconds)
- **Enable Random Behavior**: Check this for autonomous behavior

**NPCAnimationController Settings:**
- Assign your **Animator Controller** to the Animator component
- Configure animation parameter names to match your controller
- Set **Use Root Motion** if your animations have root motion

### **Step 5: Test Your Setup**

1. **Play** your scene
2. Watch NPCs automatically switch between idle and walking
3. **Approach an NPC** and press **E** to talk
4. **Observe**: NPC should face you and play talking animation
5. **Exit dialogue**: NPC returns to previous behavior

---

## 🎨 **Animation Controller Setup Details**

### **Basic Animator Controller Structure**

```
Entry → Idle (Default)
├── Idle ↔ Walk (Transition: IsWalking = true/false)
├── Any State → Talk (Transition: IsTalking = true)
├── Talk → Idle (Transition: IsTalking = false)
├── Any State → Work (Transition: IsWorking = true)
├── Work → Idle (Transition: IsWorking = false)
├── Any State → Sit (Transition: IsSitting = true)
├── Sit → Idle (Transition: IsSitting = false)
├── Any State → Sleep (Transition: IsSleeping = true)
└── Sleep → Idle (Transition: IsSleeping = false)
```

### **Transition Conditions Examples**

**Idle ↔ Walk:**
- **Idle → Walk**: `IsWalking = true`
- **Walk → Idle**: `IsWalking = false`

**Any State → Talk:**
- **Condition**: `IsTalking = true`
- **Duration**: 0.15 seconds
- **Can Transition To Self**: false

**Talk → Idle:**
- **Condition**: `IsTalking = false`
- **Duration**: 0.15 seconds

### **Using Blend Trees (Advanced)**

For more realistic movement, create a **Blend Tree** for the Walk state:
1. **Right-click Walk state** → **Create new BlendTree in State**
2. Add motion fields: **Idle, Walk, Run**
3. Set **Blend Parameter**: `Speed`
4. Configure thresholds: **0 (Idle), 1.5 (Walk), 3.0 (Run)**

---

## ⚙️ **Advanced Configuration**

### **Time-Based Behavior**

NPCs can behave differently based on the time of day:

```csharp
// In NPCStateMachine inspector:
✅ Use Time Based Behavior = true

// Configure curves:
Walk Chance By Hour: More walking during day (6-18), less at night
Sleep Chance By Hour: High chance (0.8) during night (22-6), low during day
```

### **Patrol Routes**

Make NPCs follow specific paths:

1. **Create empty GameObjects** for patrol points
2. **Position them** where you want NPCs to walk
3. **Assign patrol points** to NPCStateMachine
4. **Enable Use Patrol Points**

```csharp
// NPCStateMachine settings:
✅ Use Patrol Points = true
Patrol Points = [PatrolPoint1, PatrolPoint2, PatrolPoint3]
```

### **Work Stations**

Give NPCs jobs and work areas:

1. **Create a work station** (empty GameObject or model)
2. **Position it** where the NPC should work
3. **Assign to NPCStateMachine**

```csharp
// NPCStateMachine settings:
Work Station = WorkStationTransform
Work Duration = 10 (seconds)
Work Weight = 20 (probability)
```

### **Sitting Spots**

Create areas where NPCs can sit:

1. **Create sitting spot** (empty GameObject)
2. **Position and rotate** it properly
3. **Assign to NPCStateMachine**

```csharp
// NPCStateMachine settings:
Sitting Spot = ChairTransform
Sit Weight = 10 (probability)
```

---

## 🎭 **Facial Expressions (Optional)**

Add dynamic facial expressions to your NPCs:

### **Setup:**
1. **Create different materials** for expressions (Happy, Sad, Angry, Neutral)
2. **Assign them** to NPCAnimationController
3. **Set Face Mesh Renderer** to your character's face mesh

### **Usage:**
```csharp
// Automatic during dialogue (can be customized)
npcAnimationController.SetHappyExpression();   // During positive interactions
npcAnimationController.SetSadExpression();    // During negative interactions
npcAnimationController.SetNeutralExpression(); // Default state
```

---

## 🔧 **Integration with Existing Systems**

### **Dialogue System Integration**

The new system automatically integrates with your existing dialogue system:

- **When dialogue starts**: NPC enters `Talking` state
- **During dialogue**: NPC faces player and plays talking animation
- **When dialogue ends**: NPC returns to previous state

### **Quest System Integration**

NPCs maintain all existing quest functionality:
- Item exchanges still work
- Quest triggers still work
- Reputation system still works

### **Day/Night Cycle Integration**

NPCs automatically respond to your existing DayNightCycleManager:
- More active during day
- Sleep more during night
- Customizable behavior curves

---

## 🎮 **Runtime Control**

### **Force Specific States**

```csharp
// Get the state machine
NPCStateMachine stateMachine = npc.GetComponent<NPCStateMachine>();

// Force specific behavior
stateMachine.ForceState(NPCState.Working);  // Make NPC work
stateMachine.EnableRandomBehavior();        // Return to autonomous behavior
```

### **Custom Animation Control**

```csharp
// Get animation controller
NPCAnimationController animController = npc.GetComponent<NPCAnimationController>();

// Play custom animations
animController.PlayCustomAnimation("CustomAnimation");
animController.TriggerCustomParameter("CustomTrigger");
animController.SetCustomParameter("CustomFloat", 1.5f);
```

### **Dynamic Patrol Routes**

```csharp
// Change patrol route at runtime
Transform[] newRoute = {point1, point2, point3};
stateMachine.SetPatrolRoute(newRoute);
```

---

## 🐛 **Troubleshooting**

### **NPCs Not Moving**
- ✅ **Check NavMesh**: Is it baked? Are NPCs on NavMesh?
- ✅ **Check NavMeshAgent**: Is it enabled? Correct settings?
- ✅ **Check Walk Radius**: Is it > 0?
- ✅ **Check State**: Is random behavior enabled?

### **Animations Not Playing**
- ✅ **Check Animator Controller**: Is it assigned to Animator?
- ✅ **Check Animation Parameters**: Do names match exactly?
- ✅ **Check Transitions**: Are conditions set correctly?
- ✅ **Check Animation Clips**: Are they assigned to states?

### **Dialogue Issues**
- ✅ **Keep NPCBehavior**: Don't remove your existing dialogue system
- ✅ **Check Integration**: NPCBehavior should reference NPCStateMachine
- ✅ **Check Colliders**: Are they set as triggers?

### **Performance Issues**
- ✅ **Reduce State Change Frequency**: Increase stateChangeInterval
- ✅ **Limit Walk Radius**: Smaller radius = less pathfinding
- ✅ **Disable Debug Logs**: Turn off debug options in production

---

## 📝 **Example Complete Setup**

Here's a complete example of a well-configured NPC:

```csharp
// NPCStateMachine settings:
Current State = Idle
State Change Interval = 5
Enable Random Behavior = ✅
Walk Radius = 8
Walk Speed = 1.5
Use Time Based Behavior = ✅

// Behavior weights:
Idle Weight = 40
Walk Weight = 30  
Work Weight = 20
Sit Weight = 10

// Optional assignments:
Work Station = BarCounter (for bartender)
Sitting Spot = ChairByFire (for relaxing)
Patrol Points = [Waypoint1, Waypoint2] (for guards)
```

```csharp
// NPCAnimationController settings:
Speed Parameter = "Speed"
Is Walking Parameter = "IsWalking"
Is Talking Parameter = "IsTalking"
Transition Speed = 0.15
Use Root Motion = false (usually)
Debug Animations = ✅ (during testing)
```

---

## 🚀 **What's Next?**

Your NPCs are now fully animated and alive! They will:

1. **Wander around** autonomously
2. **Switch between** idle and walking naturally
3. **Face and talk** to players during dialogue  
4. **Work at stations** if assigned
5. **Sit and rest** occasionally
6. **Sleep during** appropriate hours
7. **Integrate seamlessly** with your existing systems

Enjoy your living, breathing NPCs! 🎭✨ 