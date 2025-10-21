Of course. Here is an extremely detailed, task-level roadmap for **Phase 2: Vertical Slice Production**. This phase covers the next 8 weeks of development (Months 3-4), building upon the validated prototype from Phase 1.

**Phase Goal:** To construct a "Vertical Slice"—a small, self-contained, and fully polished segment of the game that represents the final quality. This slice will contain every major feature working in unison: overworld exploration, narrative context, combat, RPG progression, and rewards.

---

### **Week 9 (Month 3, Week 1): The World Context**
**Objective:** Transition from a single-scene prototype to a multi-scene game world. Establish the player's existence outside of combat.

*   **9.1. Scene Management Architecture (Day 1)**
    *   Implement a persistent `GameManager.cs` using `DontDestroyOnLoad`. This object will manage game state, player data, and scene loading.
    *   Create a simple scene loader/fader script to handle asynchronous scene loading with a clean black fade transition.
*   **9.2. Overworld Creation V1 (Day 2-3)**
    *   Create a new scene: `Overworld_StartingVillage`.
    *   Use Unity's `Tilemap` system. Create a basic tileset (grass, path, dirt). Paint a small, enclosed village area.
    *   Add placeholder sprites for a few houses and a well. Use `Tilemap Collider 2D` for environmental collision.
*   **9.3. Overworld Player Controller (Day 4)**
    *   Create a new player prefab for the overworld (`Player_Overworld`).
    *   Implement a top-down movement script (`OverworldController.cs`) reading from the same Input Actions as the battle.
    *   Implement a simple 4-direction sprite-flipping animation (idle/walk for Up, Down, Left, Right).
*   **9.4. Scene Transition Trigger (Day 5)**
    *   Create an invisible "Encounter Zone" prefab with a `BoxCollider2D` set to be a trigger.
    *   When the `Player_Overworld` enters this trigger, call the scene loader to transition from the overworld to the `Battle` scene.
    *   *Deliverable:* Player can walk around a small village and entering a specific area loads the familiar battle prototype.

---

### **Week 10 (Month 3, Week 2): The Reward Loop - Loot & Inventory**
**Objective:** Give combat a purpose by implementing the systems for earning and equipping loot.

*   **10.1. Data Structure for Items (Day 1-2)**
    *   Use `ScriptableObject`s for all item data. Create a base class `ItemData.cs`.
    *   Create derived classes: `AegisData.cs` (contains stats like PWR, AGI, GRT bonuses, and a sprite reference), `CoreData.cs`, `CharmData.cs`.
    *   Create 3-4 test items as assets (e.g., "Worn Aegis," "Swiftsteel Aegis").
*   **10.2. Inventory System Backend (Day 3)**
    *   Create `InventoryManager.cs` (persists with the `GameManager`).
    *   It will hold a `List<ItemData>` for items the player owns.
    *   It will also have slots for `equippedAegis`, `equippedCore`, `equippedCharm`.
*   **10.3. Inventory UI Implementation (Day 4-5)**
    *   Create a new UI Canvas for the Inventory screen.
    *   Design UI "slots" (prefabs) that can display an item's icon and name.
    *   Write `InventoryUI.cs` to populate these slots based on the `InventoryManager`'s data.
    *   Implement a basic "click to equip" functionality. When an item in the inventory list is clicked, it's moved to the corresponding "equipped" slot in the `InventoryManager`.
    *   *Deliverable:* Player can open a menu, see items they theoretically own, and "equip" them.

---

### **Week 11 (Month 3, Week 3): The Growth Loop - XP & Leveling**
**Objective:** Implement character progression through experience points and stat allocation.

*   **11.1. XP & Leveling Backend (Day 1)**
    *   Expand `CharacterStats.cs` to include `currentXP`, `xpToNextLevel`, and `level`.
    *   Add a public method `AddXP(int amount)`. Inside this method, check if `currentXP >= xpToNextLevel` to trigger a level-up.
*   **11.2. Level Up Curve (Day 2)**
    *   Implement a formula for `xpToNextLevel` (e.g., `(level * 100) ^ 1.1`).
    *   *Pro Tip:* Use an `AnimationCurve` in the inspector to visually design the XP curve, giving you artistic control over progression pacing.
*   **11.3. Granting Rewards (Day 3)**
    *   Modify the `EnemyAI.cs` to have an `xpValue` field.
    *   When an enemy is defeated, its `OnDeath` event now calls `Player.CharacterStats.AddXP(xpValue)`.
    *   Modify the `OnDeath` event to also include the loot drop logic (e.g., 20% chance to add a specific `ItemData` to the player's `InventoryManager`).
*   **11.4. Level Up UI (Day 4-5)**
    *   Design a "Level Up!" screen. This screen should pause the game.
    *   Display the player's current stats and give them "Stat Points" to allocate (e.g., 3 points per level).
    *   Implement "+" and "-" buttons next to each stat (PWR, AGI, GRT, FCS) and a "Confirm" button.
    *   *Deliverable:* A complete gameplay loop: Defeat an enemy -> Gain XP and a chance at loot -> Level up -> Allocate stats to become stronger -> Equip new loot to become stronger.

---

### **Week 12 (Month 3, Week 4): Deepening Combat - Abilities & AI**
**Objective:** Add a layer of tactical depth to combat with player abilities and a more challenging AI.

*   **12.1. Spirit (Mana) System (Day 1)**
    *   Add `maxSpirit` and `currentSpirit` to `CharacterStats.cs`.
    *   Implement a slow, passive `RegenerateSpirit()` method called in `Update()`.
    *   Hook this up to a new UI bar in the Battle HUD.
*   **12.2. Ability System Architecture (Day 2)**
    *   Create a base `Ability.cs` ScriptableObject. It should contain `spiritCost`, `cooldownTime`, `abilityIcon`, and an abstract `Activate()` method.
    *   Create two concrete abilities: `BarrierAbility.cs` and `AegisSlamAbility.cs`.
*   **12.3. Ability Implementation (Day 3-4)**
    *   **Barrier:** The `Activate()` method instantiates a "Barrier" prefab (a simple sprite with a collider) behind the player. The Barrier has a script to destroy itself after one hit or after a few seconds.
    *   **Aegis Slam:** The `Activate()` method gives the player a temporary status effect. The `PlayerAegis.cs` script will check for this status effect on its next collision with the Orb and apply a massive speed boost.
*   **12.4. AI 2.0 (Day 5)**
    *   Upgrade the simple AI. Instead of perfectly tracking the ball, give it a "reaction time" delay.
    *   Add a "prediction" component: it moves towards where the ball *will be* after the next bounce.
    *   Introduce a deliberate "error chance" that increases as the ball's speed increases.
    *   *Deliverable:* Combat is no longer just reactive; the player has tactical choices (use Spirit now or save it?) and the AI feels more human and less robotic.

---

### **Weeks 13-16 (Month 4): Polish & Integration**
This month is dedicated to transforming the functional-but-ugly collection of systems into a cohesive, polished, and enjoyable experience.

### **Week 13: Bringing the World to Life - Audio**
*   **13.1. Audio Manager (Day 1-2):** Create a singleton `AudioManager.cs` that persists. It needs channels for Music, SFX, and UI sounds, with volume controls.
*   **13.2. Sound Effect Integration (Day 3-4):** Create/source and implement SFX for:
    *   Orb hitting Aegis (multiple variations).
    *   Orb hitting a wall.
    *   Taking damage.
    *   Casting Barrier / Aegis Slam.
    *   Level Up fanfare.
    *   UI button clicks.
*   **13.3. Music Implementation (Day 5):** Compose/source two music tracks: a calm "Village Theme" and an energetic "Battle Theme." The `AudioManager` should fade between them during scene transitions.

### **Week 14: Player Experience - UI & HUD**
*   **14.1. Final Battle HUD (Day 1-2):** Replace the debug sliders with final, stylized pixel art HUD elements for HP and Spirit. Add icon slots for the two abilities, complete with a radial "cooldown" timer animation.
*   **14.2. Styled Menus (Day 3-4):** Reskin the Inventory and Level Up screens to match the game's fantasy aesthetic. Use custom fonts (TextMeshPro) and 9-sliced sprite borders.
*   **14.3. NPC Interaction (Day 5):** Create a basic dialogue UI panel (character portrait, name, text box). Write a simple `DialogueTrigger.cs` to activate it when the player interacts with an NPC.

### **Week 15: The Full Slice - Quest & Narrative**
*   **15.1. Quest System (Day 1-2):** Create a simple `Quest.cs` ScriptableObject to hold objectives (e.g., `ObjectiveType.DEFEAT`, `targetID`, `amount`).
*   **15.2. NPC & Quest Implementation (Day 3-4):**
    *   Create one NPC in the village: "Master Elmsworth."
    *   Write and implement a short dialogue where he gives the player their first quest: "Clear the forest of 3 Slimes."
    *   The `QuestManager` tracks the player's progress. Defeating a "Slime" enemy type now updates the quest objective.
*   **15.3. Quest Completion (Day 5):** When the objective is met, the player can return to Elmsworth. He has different dialogue, and completing the quest grants them a specific reward (e.g., the "Swiftsteel Aegis").

### **Week 16: Juice & Final Polish**
*   **16.1. Particle Effects (Day 1-2):** Add particle systems (using `Sprite-Unlit` materials) for:
    *   Sparks on Orb impact.
    *   A shimmer effect for the Barrier.
    *   A "heal" effect when leveling up.
*   **16.2. Game Feel Polish (Day 3-4):**
    *   Revisit Hit-Stop and Screen Shake, tuning them to feel perfect.
    *   Animate UI elements to slide in and out of the screen instead of just appearing.
    *   Add a floating text system to show damage numbers and "LEVEL UP!" on screen.
*   **16.3. Build & Review (Day 5):**
    *   **Milestone Achieved:** The "Vertical Slice."
    *   Perform a final bug-fixing pass.
    *   Create a standalone PC build.
    *   Review the slice as a team. It should represent a complete, 10-15 minute experience that feels like the final game.
    *   *Deliverable:* A polished, standalone demo that can be shown to publishers, used for marketing, or serve as the definitive blueprint for all future content.