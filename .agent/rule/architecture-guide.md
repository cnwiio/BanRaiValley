
---

# **AI-Agent Version — Unity Architecture Rules (Condensed)**

**Target:** Unity 6.3 / OOP / Event-Driven / Optimized
**Goal:** Decoupled, testable, scalable systems. Zero leaks. No polling. No god-classes.

---

# **1. Event-Driven by Default**

* Never poll values that don’t change every frame.
* Only use `Update` for: continuous input, movement, real-time interpolation.
* All other changes → dispatch events.

**Examples**

| Need         | Wrong             | Right            |
| ------------ | ----------------- | ---------------- |
| HP           | Check in Update   | `OnHpChanged`    |
| Score        | Read UI in Update | `OnScoreChanged` |
| Range detect | Raycast Update    | `OnTriggerEnter` |
| GameState    | Check in Update   | `OnStateEntered` |

---

# **2. Global Event Bus (Strong-Typed Events)**

All cross-system communication must go through a static event bus.

```csharp
public static class GameEventBus
{
    public static event Action<float> OnPlayerHpChanged;
    public static event Action OnPlayerDied;
    public static event Action<int> OnScoreChanged;
    public static event Action<GameState, GameState> OnGameStateChanged;

    public static void EmitPlayerHpChanged(float hp)=>OnPlayerHpChanged?.Invoke(hp);
    public static void EmitPlayerDied()=>OnPlayerDied?.Invoke();
    public static void EmitScoreChanged(int s)=>OnScoreChanged?.Invoke(s);
    public static void EmitGameStateChanged(GameState f,GameState t)=>OnGameStateChanged?.Invoke(f,t);
}
```

**Lifecycle**

* Subscribe in `OnEnable`
* Unsubscribe in `OnDisable`
* Never subscribe in Awake/Start without unsubscribing.

---

# **3. Composition over Inheritance**

* Max inheritance depth: **2** (MonoBehaviour → Derived → Child).
* Avoid god-classes (input + state + combat + audio = ❌)
* Use:

  * Interfaces (`IDamageable`, etc.)
  * Components (HealthComponent)
  * ScriptableObjects (config only)
  * Pure services (static logic)

---

# **4. Interface-Driven Architecture**

* Never depend on concrete types across boundaries.
* Use `GetComponent<IInterface>()`.

**Common Interfaces**

```csharp
public interface IDamageable { float CurrentHp{get;} bool IsAlive{get;} void TakeDamage(float amt,GameObject src); }
public interface IPoolable { void OnSpawnedFromPool(); void OnReturnedToPool(); }
public interface IInteractable { string InteractionLabel{get;} bool CanInteract(GameObject i); void Interact(GameObject i); }
```

Rules:

* No `is`, `as` type checks.
* Each interface in separate file.

---

# **5. Manager Separation**

One responsibility per manager.

| Manager      | Owns        | Never Owns |
| ------------ | ----------- | ---------- |
| GameManager  | game states | UI, audio  |
| UIManager    | canvas      | gameplay   |
| AudioManager | SFX/BGM     | logic      |
| LevelManager | scenes      | player     |
| SpawnManager | spawns/pool | AI         |
| InputManager | raw input   | gameplay   |

No cross-manager references. Use EventBus.

---

# **6. Input Manager → Events Only**

Gameplay classes must never touch Unity’s input directly.

```csharp
public class InputManager:MonoBehaviour
{
    public static event Action OnJump;
    public static event Action<Vector2> OnMove;
    public static event Action OnAttack;
    PlayerInputActions _a;

    void Awake()=>_a=new PlayerInputActions();
    void OnEnable(){
        _a.Enable();
        _a.Player.Jump.performed +=_=>OnJump?.Invoke();
        _a.Player.Move.performed +=c=>OnMove?.Invoke(c.ReadValue<Vector2>());
        _a.Player.Attack.performed+=_=>OnAttack?.Invoke();
    }
    void OnDisable()=>_a.Disable();
}
```

---

# **7. ScriptableObject = Data Only**

* Holds only config.
* Never stores runtime state.
* Never references scene objects.
* Never runs coroutines or subscribes to events.

---

# **8. Object Pooling Required**

No runtime Instantiate/Destroy during gameplay.

Unity 6.3 ObjectPool example:

```csharp
public class ProjectilePool:MonoBehaviour
{
    [SerializeField] Projectile prefab;
    [SerializeField] int init=20, max=100;
    ObjectPool<Projectile> pool;

    void Awake(){
        pool=new ObjectPool<Projectile>(
            ()=>Instantiate(prefab),
            p=>p.OnSpawnedFromPool(),
            p=>p.OnReturnedToPool(),
            p=>Destroy(p.gameObject),
            false, init, max);
    }

    public Projectile Get()=>pool.Get();
    public void Return(Projectile p)=>pool.Release(p);
}
```

---

# **9. State Machines Must Emit Events**

Never poll the state. Emit on transition.

```csharp
public class GameStateMachine
{
    public GameState Current=GameState.Boot;

    public void TransitionTo(GameState next){
        if(Current==next) return;
        var prev=Current;
        Current=next;
        GameEventBus.EmitGameStateChanged(prev,next);
    }
}
```

---

# **10. AI Must Never Use Update for Logic**

AI is event/timer driven.

| Need             | Wrong          | Right             |
| ---------------- | -------------- | ----------------- |
| Player detection | Update raycast | Trigger zone      |
| Path update      | Every frame    | Timer or callback |
| Damage reaction  | Poll HP        | `OnHpChanged`     |
| Decision         | Update         | State enter       |

Separate AI into: Perception / Decision / Action components.

---

# **11. Dependency Injection Required**

Avoid:

* `FindObjectOfType`
* Hidden `Instance` singletons
* Cross-scene lookups

Use:

1. Constructor injection (services)
2. Serialized fields (scene)
3. `Initialize()` for runtime dependencies
4. Service Locator (rare, last resort)

Allowed singletons:

* EventBus
* AudioManager
* InputManager

---

# **12. Memory Rules — Zero Leak Tolerance**

### **12.1 Event Leaks**

* Every `+=` must have a matching `-=`.
* Use correct lifecycle pairs: OnEnable→OnDisable, etc.

---

### **12.2 Coroutine & Async Cleanup**

**Coroutines must be stopped:**

```csharp
Coroutine r;
void StartSpawning(){
    if(r!=null) StopCoroutine(r);
    r=StartCoroutine(Rtn());
}
void OnDisable(){
    if(r!=null) StopCoroutine(r);
}
```

**Async must use CancellationToken**

```csharp
CancellationTokenSource cts;
void Awake()=>cts=new CancellationTokenSource();
void OnDestroy(){ cts.Cancel(); cts.Dispose(); }

async Awaitable LoadAsync(){
    await SceneManager.LoadSceneAsync("A").ToAwaitable(cts.Token);
}
```

---

### **12.3 Native Collections Must Dispose**

```csharp
NativeArray<float> a;
void Awake()=>a=new NativeArray<float>(100,Allocator.Persistent);
void OnDestroy(){ if(a.IsCreated) a.Dispose(); }
```

Allocator rules:

* Temp → 1 frame
* TempJob → up to 4 frames
* Persistent → fields

---

### **12.4 Mesh/Texture Must Destroy**

```csharp
RenderTexture rt;
Texture2D tex;

void OnDestroy(){
    if(rt){ rt.Release(); Destroy(rt); }
    if(tex) Destroy(tex);
}
```

---

# **13. Object Pool Lifecycle Rules**

Every pooled object must have a guaranteed return path.

```csharp
public class Projectile:MonoBehaviour,IPoolable
{
    [SerializeField] float life=5f;
    ProjectilePool pool; 
    Coroutine auto;

    public void Initialize(ProjectilePool p)=>pool=p;

    public void OnSpawnedFromPool(){
        gameObject.SetActive(true);
        auto=StartCoroutine(ReturnAfter(life));
    }
    public void OnReturnedToPool(){
        if(auto!=null) StopCoroutine(auto);
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider c)=>Return();
    void Return(){ if(pool!=null) pool.Return(this); }

    IEnumerator ReturnAfter(float t){ yield return new WaitForSeconds(t); Return(); }
}
```

Rules:

* No abandoned objects.
* Auto-return required if lifetime is finite.
* Never store references to returned objects.

---

# **14. Scene Transition Cleanup**

Before loading a new scene, dispose all runtime objects.

```csharp
public class GameSession:IDisposable
{
    CancellationTokenSource cts;
    NativeArray<int> buf;
    bool disposed;

    public GameSession(){
        cts=new CancellationTokenSource();
        buf=new NativeArray<int>(64,Allocator.Persistent);
    }

    public void Dispose(){
        if(disposed) return;
        disposed=true;
        cts.Cancel(); cts.Dispose();
        if(buf.IsCreated) buf.Dispose();
        GameEventBus.OnPlayerDied -= OnDead;
        GameEventBus.OnScoreChanged -= OnScore;
    }
}
```

`LevelManager` must call `Dispose()` before `LoadSceneAsync`.

---

# **15. Leak Detection (Dev Builds Only)**

Use checks in editor/dev builds:

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
void OnApplicationQuit(){
    if(arr.IsCreated) Debug.LogError("[Leak] NativeArray not disposed");
}
#endif
```

Enable Unity leak detection:

```csharp
#if UNITY_EDITOR
[InitializeOnLoadMethod]
static void Enable() => NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
#endif
```

Must run:

* Memory Profiler before/after scene load
* Profiler → Memory (watch for growth)
* Check ObjectPool warnings

Zero tolerance: any leak log is a blocking bug.

16. Adding Readme file
each folder of script or each category or what it is must have readme file that contain overview and user manual. 
overview atlest must tell what it does
user manual must explain every essential step

---

