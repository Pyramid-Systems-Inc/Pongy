# Pong Quest - Code Architecture

## Core Patterns

### Singleton Managers

- **GameManager**: Core game loop, global utilities
- **AudioManager**: Music and SFX playback
- **GameStateManager**: Game state machine (Menu, Battle, Overworld, etc.)

All managers:

- Use singleton pattern (`Instance`)
- Persist across scenes (`DontDestroyOnLoad`)
- Initialize in `_PersistentManagers` scene

### Scene Structure

- `_PersistentManagers`: Never unload, contains all managers
- Gameplay scenes load additively or standalone
- Each gameplay scene has `SceneBootstrap` to ensure managers exist

### Event Communication

- Managers broadcast events (e.g., `OnGameStateChanged`)
- UI and gameplay scripts subscribe to these events
- Decoupled: UI doesn't need direct reference to game logic

## Future Additions (Week 2+)

- Input handling system
- Combat manager
- Save/Load system
