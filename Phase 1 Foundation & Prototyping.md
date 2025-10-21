This is an extremely detailed, task-level roadmap for **Phase 1: Foundation & Prototyping**. This phase represents the first 8 weeks of development.

**Phase Goal:** To move from an empty Unity project to a "Proven Prototype"—a playable build that demonstrates the unique "RPG Pong" combat loop is fun, functional, and visually promising.

**Prerequisites:** Unity 6 (latest stable beta or LTS if available) installed with Universal Render Pipeline (URP) template.

---

### **Week 1: Project Genesis & Architecture**
**Objective:** Establish a clean, scalable project structure and secure version control.

*   **1.1. Version Control Setup (Day 1)**
    *   Initialize Git repository (GitHub/GitLab/Bitbucket).
    *   Configure `.gitignore` specifically for Unity (ignoring /Library, /Temp, /Logs, /Builds).
    *   *Deliverable:* Empty repo connectable by all team members.
*   **1.2. Unity 6 Project Initialization (Day 1-2)**
    *   Create new project using the **2D (URP)** template in Unity Hub.
    *   Verify URP settings: Ensure `Universal Render Pipeline Asset_2D` is active in Graphics settings.
    *   Import standard packages:
        *   `Input System` (New) - set project to use *only* New Input System to avoid conflicts.
        *   `TextMeshPro` - for all future UI.
        *   `Cinemachine` (optional but recommended for future screen shake/camera work).
*   **1.3. Folder Structure Standardization (Day 3)**
    *   Create standard root folders: `_Project` (to separate your assets from third-party assets), `ThirdParty`.
    *   Inside `_Project`: `Scripts`, `Art` (subfolders: `Sprites`, `Materials`, `Animations`), `Prefabs`, `Scenes`, `Audio`, `ScriptableObjects`.
    *   *Deliverable:* A clean project hierarchy that won't get messy later.
*   **1.4. Core Architecture Planning (Day 4-5)**
    *   Draft basic code architecture diagram. Decide on patterns (e.g., Singleton for `GameManager`, Event Bus for decoupled communication between UI and Combat).
    *   Create placeholder Master Scene that holds persistent managers (`AudioManager`, `GameStateManager`).

---

### **Week 2: The "Playable Paddle" (Basic Input & Movement)**
**Objective:** Get a paddle moving on screen that feels responsive.

*   **2.1. Input Setup (Day 1)**
    *   Create an Input Actions asset (`Controls.inputactions`).
    *   Define Action Map `Player`:
        *   `Move` (Vector2) - bind to WASD, Arrow Keys, Left Stick (Gamepad).
    *   Generate C# class from the Input Actions asset for strong typing.
*   **2.2. Player Controller V1 (Day 2-3)**
    *   Create `PlayerAegis.cs`.
    *   Implement physics-based movement using `Rigidbody2D` (set to Kinematic to avoid unwanted physics interactions, or Dynamic with high mass/drag if you want pushback later. Start with **Kinematic** for tight control).
    *   Implement `Move()` function reading from Input System.
    *   Add customizable `[SerializeField] float moveSpeed` for easy tweaking in Inspector.
*   **2.3. Arena Boundaries (Day 4-5)**
    *   Create the "Arena" scene.
    *   Place 4 sprite objects to act as walls (Top, Bottom, Left, Right).
    *   Add `BoxCollider2D` to walls.
    *   *Deliverable:* Player can move the paddle but cannot leave the defined arena box.

---

### **Week 3: The "Kinetic Orb" (Ball Physics & Core Loop)**
**Objective:** A ball that bounces reliably and a basic win/loss state.

*   **3.1. The Orb V1 (Day 1-2)**
    *   Create `EnergyOrb.cs` and attach to a circle sprite.
    *   Add `Rigidbody2D` (Dynamic, Mass = 0, Gravity Scale = 0) and `CircleCollider2D`.
    *   Create a `PhysicsMaterial2D` with Friction = 0 and Bounciness = 1. Assign to Orb collider.
    *   *Critical Task:* Implement an `InitialPush()` method to launch the ball at a random angle between -45 and 45 degrees towards the player on start.
*   **3.2. Deflection Logic (Day 3-4)**
    *   *Problem:* Standard physics bounce can get boring (always 90 degrees).
    *   *Solution:* In `EnergyOrb.cs`, use `OnCollisionEnter2D` to detect hitting the Paddle.
    *   Calculate "relative intersect Y"—where did the ball hit the paddle relative to the paddle's center?
    *   Use that value to modify the bounce angle (hitting the edge of the paddle creates a sharper angle).
*   **3.3. The Loop (Day 5)**
    *   Create `BattleManager.cs`.
    *   State Machine: `WaitingToStart` -> `RoundActive` -> `RoundEnd`.
    *   Detect when Ball hits the wall *behind* the player (fail state) or *behind* the dummy enemy wall (win state for now).
    *   Reset ball to center on state change.

---

### **Week 4: From Arcade to Action (HP & Damage)**
**Objective:** Replace "scoring points" with "dealing damage."

*   **4.1. Health System (Day 1-2)**
    *   Create generalized `Health.cs` script (Monobehaviour).
    *   Properties: `MaxHP`, `CurrentHP`.
    *   Events: `OnTakeDamage(int amount)`, `OnDeath`.
    *   Attach `Health.cs` to the Player object and a new dummy Enemy object.
*   **4.2. Damage Implementation (Day 3)**
    *   Modify `EnergyOrb.cs` to hold a `currentDamage` integer value (default e.g., 10).
    *   When Orb triggers the "goal" zone behind the Player, find the Player's `Health` component and call `TakeDamage(currentDamage)`.
    *   *Crucial:* Differentiate between whose "turn" it is. If Player last hit the ball, it should damage Enemy if it passes them.
*   **4.3. Basic UI (Day 4-5)**
    *   Create a World Space Canvas (or Screen Space Overlay for now).
    *   Add two Slider UI elements: Player HP, Enemy HP.
    *   Hook up `Health.cs` events to update these sliders instantly when damage is taken.
    *   *Deliverable:* A playable loop where the game ends when one HP bar empties.

---

### **Week 5: The "RPG" Layer (Stats Data Structure)**
**Objective:** Implement the underlying data structures for RPG stats.

*   **5.1. Stat System Architecture (Day 1-3)**
    *   Create `Stat.cs` class (not Monobehaviour).
    *   Features: `BaseValue`, list of `StatModifiers` (for future buffs/gear). Method `GetValue()` that returns (Base + Modifiers).
    *   Create `CharacterStats.cs` (Monobehaviour or ScriptableObject).
    *   Define instances of `Stat`: `Power`, `Agility`, `Grit`, `Focus`.
*   **5.2. Integration - Agility (Day 4)**
    *   Connect `PlayerAegis.cs` to `CharacterStats.cs`.
    *   Update movement: `actualMoveSpeed = baseMoveSpeed * (Agility.GetValue() / 10f)` (Example formula).
*   **5.3. Integration - Power (Day 5)**
    *   When Player Paddle hits the Orb, update Orb's speed.
    *   `Orb.velocity = baseVelocity + (PlayerStats.Power.GetValue() * 0.5f)`.
    *   *Deliverable:* Hard-coding different stat values in the Inspector noticeably changes how the paddle feels and how fast the ball flies.

---

### **Week 6: Defense & Tuning (Grit & Testing)**
**Objective:** Finalize basic stat integration and create tools to test them easily.

*   **6.1. Integration - Grit (Day 1-2)**
    *   Update `Health.cs`'s `TakeDamage` method.
    *   Implement formula: `finalDamage = incomingDamage - (DefenderStats.Grit.GetValue() * 0.25f)`.
    *   Ensure damage minimum is at least 1 (don't let high GRT make player invincible).
*   **6.2. The Debug Testing Bench (Day 3-4)**
    *   Create a temporary UI panel with sliders/input fields for PWR, AGI, GRT.
    *   Allow real-time adjustment of these values while the ball is bouncing.
    *   *Goal:* Find the "fun zone." How fast is too fast for AGI? How much PWR breaks the physics engine?
*   **6.3. "Game Feel" Pass V1 (Day 5)**
    *   Add "Hitstop" (very brief time scale pause, e.g., 0.05s) when the ball hits a paddle.
    *   Add slight screen shake (using Cinemachine Impulse source if installed, or a simple coroutine rattling the camera transform) on damage taken.

---

### **Week 7: The "Aegis" Look (URP 2D Visuals)**
**Objective:** Stop using white squares and circles. Implement the first pass of the intended art style.

*   **7.1. URP 2D Lighting Setup (Day 1)**
    *   Upgrade scene to use 2D Renderer Data if not already active.
    *   Add a `Global Light 2D` (dim, bluish tint for atmosphere).
    *   Replace white sprites with established placeholder pixel art (a mystic wooden paddle, a glowing orb).
    *   Ensure sprites use the `Sprite-Lit-Default` material (or custom URP equivalent) to react to light.
*   **7.2. The Glowing Orb (Day 2-3)**
    *   Add a `Point Light 2D` as a child of the Energy Orb. Set color to bright cyan or gold.
    *   Add a `Trail Renderer` component to the Orb. Configure a material with an additive shader for a "light streak" effect.
*   **7.3. Post-Processing Magic (Day 4-5)**
    *   Add a Global Volume to the scene.
    *   **Bloom:** Crank it up. The Orb and its trail should glow intensely.
    *   **Vignette:** Slight darkening of screen edges to focus on the center arena.
    *   *Deliverable:* The game now looks 50% closer to a final product just through lighting.

---

### **Week 8: The "Vertical Slice" Prototype & Review**
**Objective:** Package everything into a standalone demo scene that proves the concept.

*   **8.1. The Forest Sprite Enemy (Day 1-2)**
    *   Create a basic AI script `EnemyAI.cs`.
    *   Simple logic: `Aegis.y = Mathf.Lerp(Aegis.y, Ball.y, enemyAgility * Time.deltaTime)`.
    *   Give it different stats than the player (e.g., very high Agility, very low Power).
*   **8.2. Prototype Polish (Day 3-4)**
    *   Add a simple Main Menu scene (just a "Start Game" button).
    *   Add a "Game Over / Victory" text pop-up when a match ends.
    *   Ensure the build can be exported to standalone PC .exe without errors.
*   **8.3. Internal Review Milestone (Day 5)**
    *   **Milestone Achieved:** The "Foundation Prototype."
    *   Team plays the build. Key questions to answer:
        *   Is controlling the Aegis fun?
        *   Does upgrading Power feel satisfying?
        *   Does the URP lighting sell the "fantasy" theme?

---