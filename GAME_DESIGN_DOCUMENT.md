# Robot Tower Defense - Game Design Document

## 1. Overview

### 1.1 Concept
**Robot Tower Defense** is a premium mobile tower defense game featuring futuristic robotic warfare. Players defend their base by strategically placing intelligent combat robots to intercept waves of hostile machines.

### 1.2 Target Platform
- **Primary:** Android (Google Play Store)
- **Secondary:** iOS (App Store)
- **Engine:** Unity 2022.3 LTS

### 1.3 Target Audience
- Tower defense enthusiasts (ages 12+)
- Strategy game fans
- Sci-fi and robot theme lovers

### 1.4 Unique Selling Points
- Original robot IP with striking visual design
- Deep strategic gameplay with status effects and synergies
- Premium experience - no pay-to-win mechanics
- Offline play supported

---

## 2. Visual Style

### 2.1 Art Direction
The visual style draws from the **Kaptivio robot** design:
- **Color Palette:** Deep metallics (gunmetal, chrome), blue/purple energy accents, glowing cyan highlights
- **Energy Effects:** Blue and purple flames, plasma effects, electrical arcs
- **Character Design:** Industrial-futuristic robots with glowing eyes/cores
- **Environment:** Neo-industrial landscapes, circuit-board terrain patterns, holographic elements

### 2.2 UI Theme
- Sleek, holographic interface elements
- Glowing blue outlines and highlights
- Dark backgrounds with high contrast
- Animated energy flow effects
- Hexagonal and circular motifs

### 2.3 Shader Effects
- Metallic PBR surfaces with scratches/weathering
- Energy pulse effects on towers
- Heat distortion from Flamethrower/laser units
- Electric arc particles for Tesla/Shock towers
- Shield bubble shaders for defensive abilities

---

## 3. Core Gameplay

### 3.1 Game Loop
1. **Wave Preparation** - Place/upgrade towers, review upcoming enemies
2. **Wave Combat** - Enemies spawn, towers engage automatically
3. **Wave Completion** - Earn credits, unlock new options
4. **Repeat** until victory or defeat

### 3.2 Victory/Defeat
- **Victory:** Survive all waves
- **Defeat:** Lives reach zero (enemies reaching end point)

### 3.3 Economy
| Resource | Source | Use |
|----------|--------|-----|
| Credits | Starting pool, enemy kills, wave bonuses | Tower purchase/upgrade |
| Tech Points | Level completion, achievements | Permanent upgrades |
| Energy Cells | Boss kills, rare drops | Special abilities |

### 3.4 Difficulty Scaling
| Wave | HP Multiplier | Speed Multiplier | Enemies |
|------|--------------|-----------------|---------|
| 1-10 | 1.0x - 1.5x | 1.0x | Basic types |
| 11-20 | 1.5x - 2.5x | 1.0x - 1.1x | + Special types |
| 21-30 | 2.5x - 4.0x | 1.1x - 1.2x | + Boss encounters |

---

## 4. Towers

### 4.1 Tower Categories

#### Damage Towers
| Tower | Cost | DPS | Range | Special |
|-------|------|-----|-------|---------|
| Laser Turret | 100 | 25 | 6 | Instant hit, high accuracy |
| Plasma Cannon | 150 | 40 | 5 | Projectile, energy damage |
| Rocket Launcher | 200 | 60 | 7 | Splash damage (radius 2) |
| Sniper Bot | 250 | 100 | 12 | 20% crit chance |
| Flamethrower | 175 | 35 | 4 | Cone AoE, burn DOT |
| Tesla Coil | 300 | 45 | 5 | Chain lightning (3 targets) |

#### Support Towers
| Tower | Cost | Range | Effect |
|-------|------|-------|--------|
| Freeze Turret | 150 | 5 | 40% slow for 2s |
| Shock Tower | 200 | 6 | Stun 0.5s + chain |
| Buff Station | 250 | 4 | +25% damage to nearby towers |

### 4.2 Upgrade System
Each tower has 3 upgrade tiers:
- **Tier 1:** +20% stats, costs 50% of base
- **Tier 2:** +40% stats, costs 75% of base
- **Tier 3:** +60% stats + special ability, costs 100% of base

### 4.3 Targeting Priorities
- **First:** Target enemy closest to exit
- **Last:** Target enemy furthest from exit
- **Strongest:** Target highest HP enemy
- **Weakest:** Target lowest HP enemy
- **Closest:** Target nearest enemy to tower

---

## 5. Enemies

### 5.1 Enemy Categories

#### Standard Units
| Type | HP | Speed | Armor | Special |
|------|-----|-------|-------|---------|
| Scout Drone | 50 | 3.0 | 0% | 15% dodge |
| Soldier Bot | 100 | 2.0 | 10% | None |
| Tank Mech | 300 | 1.0 | 30% | Reduced slow |
| Elite Unit | 200 | 2.5 | 15% | Regenerating shield |

#### Special Units
| Type | HP | Speed | Special Ability |
|------|-----|-------|-----------------|
| Flying Drone | 75 | 2.5 | Ignores ground obstacles |
| Healer Bot | 80 | 2.0 | Heals nearby allies |
| Splitter | 150 | 2.0 | Splits into 2 smaller units |
| Teleporter | 100 | 2.0 | Jumps forward on path |

#### Bosses
| Type | HP | Speed | Abilities |
|------|-----|-------|-----------|
| Heavy Assault | 2000 | 0.8 | Regen, enrage at 30% HP |
| Swarm Mother | 1500 | 1.0 | Spawns drones continuously |
| Shield Commander | 1800 | 1.2 | Shields nearby units |

### 5.2 Damage Types & Resistances
| Damage Type | Strong Against | Weak Against |
|-------------|---------------|--------------|
| Physical | Flying | Tank |
| Energy | Tank | Elite (shields) |
| Fire | Standard | - |
| Electric | Flying, Groups | Tank |
| Plasma | Shielded | - |

---

## 6. Status Effects

### 6.1 Debuffs
| Effect | Source | Duration | Stacks |
|--------|--------|----------|--------|
| Slow | Freeze Turret | 2s | No (refresh) |
| Burn | Flamethrower | 3s | Yes (3x max) |
| Stun | Shock Tower | 0.5s | No |
| EMP | EMP Tower | 2s | No (disables abilities) |

### 6.2 Buffs (Tower)
| Effect | Source | Duration |
|--------|--------|----------|
| Damage Boost | Buff Station | Continuous |
| Attack Speed | Upgrade | Permanent |

---

## 7. Maps

### 7.1 Map Structure
- Grid-based placement (1-unit cells)
- Path cannot be blocked
- Varied terrain (open, chokepoints, multiple paths)

### 7.2 Launch Maps
| Map | Difficulty | Waves | Theme |
|-----|-----------|-------|-------|
| Training Grounds | ★☆☆☆☆ | 15 | Tutorial arena |
| Factory Floor | ★★☆☆☆ | 25 | Industrial plant |
| Circuit City | ★★★☆☆ | 30 | Urban environment |
| Nuclear Core | ★★★★☆ | 35 | Power plant |
| Command Center | ★★★★★ | 40 | Military base |

---

## 8. Progression System

### 8.1 Player Level
- XP earned from completing maps
- Level unlocks: new towers, maps, features

### 8.2 Tech Tree
Permanent upgrades purchased with Tech Points:
- **Firepower:** Global damage bonuses
- **Efficiency:** Reduced tower costs
- **Resilience:** More starting lives
- **Tactics:** Better kill rewards

### 8.3 Achievements
- Level-based (Beat X waves, Kill Y enemies)
- Challenge-based (No damage taken, Speed run)
- Collection-based (Use all towers, Kill all enemy types)

---

## 9. Monetization

### 9.1 Model: Premium with Optional IAP
- **Base Game:** $2.99-$4.99 (one-time purchase)
- **Optional:** Cosmetic tower skins, map packs

### 9.2 No Pay-to-Win
- All gameplay content earnable through play
- IAP strictly cosmetic or convenience (skip grind)
- No energy systems or wait timers

---

## 10. Technical Specs

### 10.1 Performance Targets
- 60 FPS on mid-range devices (2020+)
- 30 FPS on low-end devices
- < 150MB initial download
- < 50MB RAM usage

### 10.2 Mobile Optimizations
- Object pooling for all spawned objects
- LOD system for distant objects
- Texture atlasing
- Efficient particle systems
- Adaptive quality settings

### 10.3 Controls
- **Tap:** Select tower/placement cell
- **Long Press:** Tower info popup
- **Drag:** Camera pan
- **Pinch:** Camera zoom
- **Double Tap:** Quick upgrade

---

## 11. Audio Design

### 11.1 Sound Effects
- Robotic, mechanical sounds
- Energy weapon zaps and hums
- Metal impacts and explosions
- UI blips and confirmation tones

### 11.2 Music
- Electronic/synthwave soundtrack
- Dynamic intensity based on wave progress
- Distinct themes per map environment

---

## 12. Future Content

### 12.1 Update Roadmap
- **v1.1:** Endless Mode, Leaderboards
- **v1.2:** Daily Challenges, New Map Pack
- **v1.3:** Tower Customization System
- **v2.0:** PvP Mode (competitive placement)

---

## 13. Development Milestones

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Prototype | 2 weeks | Core loop playable |
| Alpha | 4 weeks | All systems integrated |
| Beta | 4 weeks | Content complete, polish |
| Release Candidate | 2 weeks | Bug fixes, optimization |
| Launch | - | Play Store release |

---

*Document Version: 1.0*
*Last Updated: Robot Tower Defense Development*
