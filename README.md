# CustomAA12 — 全自动速射霰弹枪 / Full-Auto Shotgun

[![LabAPI](https://img.shields.io/badge/LabAPI-1.1.4-blue)](https://github.com/northwood-studios/LabAPI)
[![SCP:SL](https://img.shields.io/badge/SCP%3ASL-14.x-green)](https://scpslgame.com)

一个基于 **LabAPI** 框架的 SCP: Secret Laboratory 插件，将 AK 改装为 AA12 全自动速射霰弹枪，在阵营刷新时掉落于出生点。

A **LabAPI** plugin for SCP: Secret Laboratory that converts the AK into an AA12 full-auto shotgun, dropping at spawn points during team respawn waves.

---

## 📦 安装 / Installation

1. 将 `CustomAA12.dll` 放入 `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`（Linux）或 `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\`（Windows）
2. 重启服务器或执行 `reload plugins` 指令
3. 插件自动生效，无需额外配置

1. Place `CustomAA12.dll` into `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` (Linux) or `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` (Windows)
2. Restart the server or use the `reload plugins` command
3. No configuration required — the plugin works out of the box

---

## 🎯 功能 / Features

### 🔫 武器特性 / Weapon Stats

| 属性 / Property | 值 / Value |
|----------------|------------|
| 基础枪械 / Base Weapon | AK (`ItemType.GunAK`) |
| 弹匣容量 / Magazine Size | **90 发 / rounds** |
| 每次射击 / Shots per Trigger | **7 发弹丸 / pellets** |
| 单发伤害 / Damage per Pellet | **~7 HP** |
| 射击模式 / Fire Mode | 全自动 / Full-Auto |

### 📍 生成机制 / Spawn System

| 波次 / Wave | 数量 / Count | 位置 / Location |
|-------------|-------------|----------------|
| NTF 大波 / Primary MTF | 3 | NTF 出生点 / NTF Spawn `(134.9, 297.6, -43.2)` |
| NTF 小波 / Mini MTF | 5 | NTF 出生点 / NTF Spawn `(134.9, 297.6, -43.2)` |
| Chaos 大波 / Primary Chaos | 3 | Chaos 出生点 / Chaos Spawn `(0.6, 302.5, -39.9)` |
| Chaos 小波 / Mini Chaos | 5 | Chaos 出生点 / Chaos Spawn `(0.6, 302.5, -39.9)` |

武器以可拾取形态（Pickup）直接掉落在地面，阵营双方均可抢夺。

Weapons spawn as world pickups at the spawn location — both teams can grab them.

### 🎯 伤害平衡 / Damage Balance

| 目标 / Target | 击杀所需射击次数 / Shots to Kill |
|--------------|--------------------------------|
| 无甲 (~85 HP) / Unarmored | **2 枪 / shots** |
| 轻甲 (~115 HP) / Light Armor | **3 枪 / shots** |
| 重甲 (~200 HP) / Heavy Armor | **5 枪 / shots** |

> 7 发弹丸×7 伤害 = 49 总伤害/次。散射使远距离命中减少，近战爆发力强。
> 7 pellets × 7 HP = 49 total damage per trigger pull. Spread reduces damage at range while maintaining close-range power.

### 🔄 拾取与掉落 / Pickup & Drop

- **拾取提示 / Pickup Broadcast**: 捡起时显示 "你捡起了AA12:一把全自动速射霰弹枪"
- **掉落重拾 / Drop & Re-pickup**: 丢在地上再捡起仍保留 AA12 属性 / Dropping and re-picking preserves AA12 status
- **不影响普通 AK**: 通过序列号追踪，不会误标记普通 AK / Serial-based tracking prevents interference with normal AKs

---

## ⚙️ 技术实现 / Technical Implementation

| 功能 / Feature | 实现方式 / Method |
|---------------|------------------|
| 7 发散射 / 7 Pellet Spread | Harmony Patch `AutomaticActionModule.get_ChamberSize()` → 返回 7 |
| 弹药消耗 / Ammo Consumption | 由游戏原生逻辑自动处理（ChamberSize=7 → 扣 7 发） |
| 伤害削减 / Damage Reduction | Harmony Patch `HitscanHitregModuleBase.DamageAtDistance()` × 0.22 |
| 90 发弹匣 / 90-Round Magazine | 监听 `PlayerEvents.ReloadedWeapon`，换弹后 `StoredAmmo = 90` |
| 波次生成 / Wave Spawning | 监听 `ServerEvents.WaveRespawned`，判断波次类型 |
| 拾取标记 / Pickup Tracking | 通过 `Pickup.Serial` + `FirearmItem.Serial` 双向追踪 |
| 掉落持久化 / Drop Persistence | `DroppingItem` + `DroppedItem` 事件联动，保留序列号标记 |

---

## 🛠️ 构建 / Building

```bash
# 克隆 / Clone
git clone <your-repo>
cd CustomAA12

# 构建 / Build
dotnet build

# 输出在 / Output at
# bin/CustomAA12.dll
```

### 依赖 / Dependencies

- [LabAPI](https://github.com/northwood-studios/LabAPI) v1.1.4+
- Assembly-CSharp.dll
- UnityEngine.CoreModule.dll
- 0Harmony.dll
- Mirror.dll
- CommandSystem.Core.dll

---

## 📜 开源协议 / License

MIT License — 随意使用、修改和分发。

MIT License — Feel free to use, modify, and distribute.

---

## 👤 作者 / Author

**Developer** — [GitHub](https://github.com/your-username)

---

> 💡 **提示 / Tip**: 如需调整伤害，修改 `CustomAA12Plugin.cs` 中 `DamagePatch.DamageMultiplier` 的值（当前 `0.22f`）。
> To adjust damage, modify `DamagePatch.DamageMultiplier` in `CustomAA12Plugin.cs` (currently `0.22f`).