# 🚗 Boss NPC — 2D Top-Down Car Game (Unity)

Система Boss-ворога для 2D гри з видом зверху на Unity.  
Повністю автономний NPC з фазами, поведінками та налаштуваннями через Inspector.

---

## 📁 Файли

| Файл | Призначення |
|------|-------------|
| `BossPhase.cs` | Data-клас фази. Не чіпляти нікуди — просто лежить у папці |
| `BossController.cs` | Головний AI: State Machine, рух, Dash |
| `BossHealth.cs` | Здоров'я, зміна фаз, смерть |
| `BossShooting.cs` | Стрільба, серії пострілів, спред куль |
| `BossBullet.cs` | Поведінка кулі Boss-а: політ, влучення |

---

## ⚙️ Вимоги

- Unity **2021.3** або новіша (до Unity 6)
- Якщо Unity 6+ → замінити `rb.velocity` на `rb.linearVelocity` у всіх файлах
- Physics 2D увімкнено в проєкті

---

## 🔧 Налаштування сцени

### Крок 1 — Boss GameObject

1. Створи порожній GameObject → назви **`Boss`**
2. Додай компоненти в такому порядку:
   - `Rigidbody2D`
   - `BoxCollider2D` (або `PolygonCollider2D` по формі машини)
   - `BossHealth`
   - `BossShooting`
   - `BossController`
3. Налаштуй `Rigidbody2D`:
   - **Gravity Scale = 0** ← обов'язково, інакше впаде вниз
   - **Freeze Rotation → Z = ✓** ← щоб не крутився від зіткнень

---

### Крок 2 — FirePoint (точка пострілу)

1. Всередині `Boss` створи дочірній **Empty GameObject** → назви **`FirePoint`**
2. Постав його **на ніс машини** (по локальній осі Y вгору від центру)
3. Перетягни `FirePoint` у поле **`Fire Point`** компонента `BossShooting`

```
Boss (GameObject)
 └── FirePoint (Empty) ← ніс машини
```

---

### Крок 3 — Bullet Prefab (куля Boss-а)

1. Створи новий GameObject → назви **`BossBulletPrefab`**
2. Додай компоненти:
   - `Rigidbody2D` → **Gravity Scale = 0**
   - `CircleCollider2D` → **Is Trigger = ✓**
   - `BossBullet`
3. **Збережи як Prefab** у папку `Prefabs/`
4. Перетягни prefab у поле **`Bullet Prefab`** компонента `BossShooting`

---

### Крок 4 — Налаштування Inspector на Boss

#### BossController
| Поле | Що вказати |
|------|-----------|
| `Player Transform` | Transform гравця зі сцени |
| `Phases` (розмір 3) | Налаштування 3 фаз (дивись нижче) |
| `Dash Speed` | Швидкість Dash-атаки (рекомендовано: 12) |
| `Dash Duration` | Тривалість Dash у секундах (рекомендовано: 0.3) |
| `Dash Cooldown` | Перезарядка Dash (рекомендовано: 5) |
| `Arena Min / Max` | Межі арени — Vector2 лівого нижнього і правого верхнього кута |

#### Фази (Phases) — рекомендовані значення

| Параметр | Phase 1 | Phase 2 | Phase 3 |
|----------|---------|---------|---------|
| `Phase Name` | Patrol | Rage | Berserk |
| `Hp Threshold` | **1.0** | **0.6** | **0.3** |
| `Speed` | 3 | 4.5 | 6 |
| `Rotation Speed` | 2 | 3 | 4 |
| `Shoot Cooldown` | 2.5 | 1.5 | 0.8 |
| `Burst Count` | 1 | 2 | 3 |
| `Burst Interval` | 0.15 | 0.12 | 0.1 |
| `Spread Count` | 1 | 2 | 3 |
| `Spread Angle` | 0 | 20 | 30 |
| `Bullet Speed` | 7 | 9 | 11 |
| `Bullet Damage` | 10 | 15 | 20 |
| `Chase Range` | 10 | 12 | 15 |
| `Orbit Range` | 3 | 3.5 | 4 |
| `Reposition Range` | 14 | 16 | 18 |
| `Dash Trigger Range` | 0 | 7 | 9 |
| `Can Dash` | ✗ | ✓ | ✓ |

---

### Крок 5 — Теги

Переконайся що на GameObject гравця стоїть тег **`Player`**  
(`Inspector → Tag → Player`)  
Це потрібно для `BossBullet`, щоб визначити влучення.

---

### Крок 6 — Підключення здоров'я гравця

Відкрий `BossBullet.cs` і знайди:

```csharp
// other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
```

Розкоментуй і заміни `PlayerHealth` на назву свого компонента здоров'я гравця.

Також, коли куля гравця влучає в Boss-а, виклич:

```csharp
bossGameObject.GetComponent<BossHealth>().TakeDamage(damage);
```

---

## 🧠 State Machine — стани Boss-а

```
         старт
           │
        PATROL ──── гравець в зоні ──→ CHASE
           │                              │
     час вийшов                    дуже близько
           │                              │
        STRAFE ◄──────────────────── ORBIT
           │                              │
     гравець далеко               час вийшов / відстань
           │
       REPOSITION
           │
      гравець поруч
           │
         CHASE

   HP < 30%:
      будь-який стан → BERSERK (не виходить)
```

| Стан | Поведінка |
|------|-----------|
| `Patrol` | Об'їзд випадкових точок арени |
| `Chase` | Пряме переслідування гравця |
| `Strafe` | Бічний рух, постійно дивиться на гравця |
| `Orbit` | Кружляння навколо гравця на близькій дистанції |
| `Reposition` | Відступ, якщо гравець відійшов занадто далеко |
| `Berserk` | Фаза 3: нон-стоп атака + частий Dash |

---

## 🔴 Фази Boss-а

| Фаза | HP% | Що змінюється |
|------|-----|--------------|
| **Phase 1** | 100–60% | Патруль, повільний, одиночні постріли |
| **Phase 2** | 60–30% | Агресивніший, спред з 2 куль, вмикається Dash |
| **Phase 3** | 30–0% | BERSERK стан, 3 кулі за постріл, частий Dash |

Перехід між фазами відбувається автоматично при зниженні HP.

---

## 📡 Зв'язки між скриптами

```
Куля гравця влучає в Boss
         │
         ▼
BossHealth.TakeDamage(damage)
         │
         ├─→ Перевіряє зміну фази
         │         │
         │         ▼
         │   BossController.OnHealthChanged()
         │         │
         │         ▼
         │   BossShooting.SetPhase()
         │
         └─→ HP <= 0 → Die()
                   │
                   ▼
           BossController.enabled = false
           UnityEvent onDeath → (твій код)
```

---

## 🎮 UnityEvents у BossHealth

`BossHealth` має три події які можна підписати прямо в Inspector:

| Подія | Коли | Параметр |
|-------|------|---------|
| `On Damaged` | при кожному влученні | float: поточний % HP (0..1) |
| `On Phase Changed` | при зміні фази | int: номер фази (0, 1, 2) |
| `On Death` | при смерті Boss-а | — |

Використай їх для: оновлення UI, програвання VFX, музики, відкриття дверей тощо.

---

## 🐛 Часті помилки

| Помилка | Рішення |
|---------|---------|
| `'linearVelocity' does not contain definition` | Замінити `linearVelocity` → `velocity` (Unity < 6) |
| Boss не рухається | Перевір `Gravity Scale = 0` на Rigidbody2D |
| Boss крутиться хаотично | Увімкни `Freeze Rotation Z` на Rigidbody2D |
| Кулі не завдають шкоди | Перевір тег `Player` на гравці і `Is Trigger = ✓` на колайдері кулі |
| Boss не бачить гравця | Перевір поле `Player Transform` в Inspector |
| `NullReferenceException` в BossShooting | Призначено `FirePoint` та `BulletPrefab` в Inspector? |
