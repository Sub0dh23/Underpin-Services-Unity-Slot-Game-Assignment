# Underpin Services - Unity Slot Game Assignment

## 📌 Project Overview
- **Position**: Unity Developer Intern (Underpin Technology / Underpin Services)
- **Evaluation Platform**: Internshala Assignment
- **Original Brief**: [Notion Assignment Document](https://www.notion.so/Unity-Slot-Game-Assignment-246d48d8867b804198a1f4d3365cf67e)
- **Target Deadline**: 9th September 2026, 11:59 PM (Target Completion: Within 24 hours)
- **Unity Version**: Unity 6 (6000.3.13f1) - URP
- **Submission Requirement**: Public GitHub Repository with full project, WebGL build in `/Build/WebGL`, clean commit history, and comprehensive `README.md`.

---

## 🎯 Evaluation Criteria & Weights
| Criteria | Weight | Focus Areas |
| :--- | :--- | :--- |
| **Core Functionality Implementation** | ★★★★★ | Accurate win logic, bet/balance flow, symbol matching, responsive controls |
| **Code Cleanliness & Structure** | ★★★★★ | Strict OOP, modular architecture, ScriptableObjects, SOLID, thorough XML/C# comments |
| **Reel Animation & Game Feel** | ★★★★☆ | Realistic reel spinning physics (anticipation, blur, deceleration, bounce-back easing), juice/VFX/SFX |
| **Git Commit History & Repo Quality** | ★★★★☆ | Granular, semantic commit history reflecting iterative development milestones |
| **Bonus Features & Creativity** | ★★★☆☆ | Wilds, Free Spins bonus round, Scatter payouts, Gamble/Multiplier mini-feature |
| **UI/UX Clarity** | ★★★☆☆ | Clean symbol alignment, responsive UI layout, clear payout table, bet/win visual feedback |

---

## 📋 Feature Requirements Breakdown

### 1. Core Mechanics & Logic
- [ ] **RNG Engine**: Cryptographically sound or seedable weighted RNG system to determine symbol outcomes fairly and unpredictably.
- [ ] **Winning Logic**: Dynamic evaluation across paylines (Horizontal, Diagonal, Multi-line support) when matching symbols align.
- [ ] **Payout Calculation**: Configurable payout table based on symbol tier / rarity multiplied by current bet.
- [ ] **Betting & Economy System**: Balance tracking, adjustable bet values (Min/Max/Step), Max Bet button, and win tallying animation.
- [ ] **Game State Machine**: Clear states (`Idle`, `Spinning`, `Evaluating`, `WinCelebration`, `BonusRound`, `OutOfFunds`).

### 2. Reel Animation & Visual Feel (Game Juice)
- [ ] **Dynamic Reel Spin**:
  - Spin initiation anticipation / slight pull-up.
  - Fast continuous looping with symbol recycling (infinite reel illusion).
  - Staggered stop (Reel 1 stops first, then Reel 2, Reel 3, etc. for suspense).
  - Bounce / overshoot deceleration on landing.
- [ ] **Visual Clarity & Layout**:
  - Consistent symbol dimensions and crisp UI rendering.
  - Frame clipping / Masking using 2D Mask / RectMask2D.
  - Winning line highlighting / glow / particle celebration effects.

### 3. Bonus Features (Creative Additions)
- [ ] **Wild Symbol**: Substitutes for any regular paying symbol to complete winning lines.
- [ ] **Scatter / Free Spins**: Hitting 3+ Scatters triggers a Free Spin bonus round with special visual theme & multiplier.
- [ ] **Gamble / Multiplier Mini-game**: Double-or-nothing card / coin flip feature on win.

### 4. Audio & Sound FX
- [ ] Sound manager with SFX for Reel Spin, Reel Stop click, Win Jingle, Big Win fanfare, Button clicks, and ambient casino background music.

---

## 🏗️ Architecture & Project Structure

```
Assets/
├── Animations/           # UI and Reel animation controllers / clips
├── Audio/                # Background music and sound effects (SFX)
├── Materials/            # Particle & UI custom materials
├── Prefabs/              # Symbol prefabs, Reel prefabs, UI dialogs, FX prefabs
├── Scenes/               # SlotGameScene, WebGLBootstrap
├── Scripts/
│   ├── Audio/            # AudioManager, SoundEffects
│   ├── Core/             # GameManager, SlotMachineController, GameState
│   ├── Data/             # SymbolData (ScriptableObject), PaytableConfig, GameSettings
│   ├── Reel/             # ReelController, ReelStrip, SymbolView
│   ├── Logic/            # RNGManager, WinEvaluator, PayoutCalculator
│   ├── UI/               # UIManager, BalanceDisplay, WinPopup, PaytableUI, BonusUI
│   └── Utils/            # Easing, Extensions, ObjectPooling
├── Sprites/              # Imported slot art, machine frame, symbols, buttons, icons
└── Settings/             # URP and Project Settings
Build/
└── WebGL/                # Playable WebGL build distribution
```

---

## 🚀 Development Milestones & Progress Tracker

- [ ] **Phase 1: Project Setup & Asset Ingestion**
  - [ ] Organize raw assets from `Slot Machine/` into `Assets/Sprites/` and `Assets/UI/`.
  - [ ] Configure sprite slice settings, pivots, and pixels-per-unit.
  - [ ] Initialize Git repository with proper `.gitignore` for Unity 6 and create initial baseline commit.

- [ ] **Phase 2: Core Data Architecture & ScriptableObjects**
  - [ ] Create `SymbolData` ScriptableObjects (ID, Name, Sprite, Payout Multipliers, Symbol Type: Regular/Wild/Scatter/Bonus).
  - [ ] Create `PaytableConfig` for configurable payout rules and combinations.
  - [ ] Create `RNGManager` with weighted random generation and audit logging.

- [ ] **Phase 3: Reel System & Smooth Animation**
  - [ ] Build Reel View / Strip with reusable pooled symbol elements.
  - [ ] Implement smooth easing motion (anticipation, high-speed blur/loop, staggered stop, overshoot bounce).
  - [ ] Ensure pixel-perfect symbol alignment and boundary masking.

- [ ] **Phase 4: Game Loop, State Machine & Win Evaluation**
  - [ ] Build `SlotMachineController` & `GameManager` with state machine.
  - [ ] Implement `WinEvaluator` to scan paylines, detect matches, calculate payouts.
  - [ ] Connect Betting & Balance system (Bet +, Bet -, Max Bet, Auto Spin).

- [ ] **Phase 5: UI/UX, Audio & Polish (Bonus Features)**
  - [ ] Wire up polished UI (Balance, Bet, Win Display, Spin Button states).
  - [ ] Add Paytable info modal / popup.
  - [ ] Add Wild & Scatter / Free Spins feature.
  - [ ] Add Win popup animations, particle confetti, and Audio Manager with SFX.

- [ ] **Phase 6: WebGL Build, Git Commits & Documentation**
  - [ ] Test in Editor and verify all edge cases.
  - [ ] Build WebGL player to `/Build/WebGL/`.
  - [ ] Commit progress incrementally with clear conventional commit messages.
  - [ ] Write detailed `README.md` including game overview, WebGL instructions, bonus features, and design decisions.
