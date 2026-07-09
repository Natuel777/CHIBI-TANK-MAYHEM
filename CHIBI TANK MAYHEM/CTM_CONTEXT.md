# CTM — Contexto del proyecto

Notas técnicas acumuladas durante el desarrollo de Chibi Tank Mayhem, para no perder contexto entre sesiones.

## Unity Editor: SerializedObjectNotCreatableException / MissingReferenceException tras guardar scripts

**Síntoma:**
```
SerializedObjectNotCreatableException: Object at index 0 is null
  UnityEditor.InputSystem.Editor.PlayerInputEditor.OnEnable ()

MissingReferenceException: The variable m_Targets of GameObjectInspector doesn't exist anymore.
  UnityEditor.GameObjectInspector.OnEnable ()

SerializedObjectNotCreatableException: Object at index 0 is null
  UnityEditor.TransformInspector.OnEnable ()
```

**Causa:** No es un bug de código de gameplay. Es una carrera del Editor entre el domain reload (recompilación de scripts) y el Inspector, que queda con una referencia cacheada a un objeto/componente que el reload ya destruyó y recreó. Pasa típicamente cuando el GameObject seleccionado en la Hierarchy (ej. `Tank`) queda con el Inspector abierto justo cuando se guarda un script y Unity recompila.

**Contexto en que apareció:** proyecto en Unity 6000.4.9f1, justo después de editar `Player.cs` / `PlayerMovement.cs` (renombrado de métodos `Tick`→`ArtificialUpdate`, `RotateWithoutMoving`→`Rotate`) mientras el prefab `Tank` estaba seleccionado en el Inspector.

**Solución superficial (mitiga pero no siempre alcanza):**
1. Deseleccionar el objeto afectado en la Hierarchy y volver a seleccionarlo (fuerza reconstrucción del `SerializedObject` del Inspector).
2. Si persiste: cerrar la pestaña del Inspector (click derecho → Close Tab) y reabrir con `Window > General > Inspector`.

**Causa raíz real encontrada (2026-07-09):** el problema no era solo una carrera de domain reload — había datos inconsistentes en el proyecto:
- `Assets/FlyWeight/PlayerSettingsSO.cs` definía la clase `PlayerSettings` (vacía, sin ninguna referencia en el código), mientras que `Assets/Scripts/PlayerSettings.cs` definía la clase `PlayerSettingsSO` (la que sí se usa). Nombres de archivo y de clase cruzados entre ambos scripts.
- `Assets/FlyWeight/PlayerSettingsSO.asset` (el asset real usado por `Player.cs`) tenía en su YAML un campo `inputReader` serializado (apuntando al `InputReader` del `Tank.prefab`) que ya no existía en la clase C#, porque se había eliminado ese campo en un fix previo (ver nota histórica más abajo) sin limpiar el `.asset`.

Esta combinación —campo huérfano en un `.asset` + nombres de clase/archivo cruzados en el mismo dominio de tipos— es lo que hacía fallar la reconstrucción del `SerializedObject` en `PlayerInputEditor`, `GameObjectInspector` y `TransformInspector` al reabrir el Inspector sobre el `Tank` (que referencia tanto `InputReader` como, indirectamente, `PlayerSettingsSO`).

**Fix aplicado:**
1. Se quitó el campo `inputReader` huérfano del YAML de `Assets/FlyWeight/PlayerSettingsSO.asset`.
2. Se verificó (grep de GUID `959c0c781c9caa644b497cf7db28b0da` en todo `Assets/`) que la clase `PlayerSettings` no estaba referenciada en ningún prefab/escena/asset, y se eliminó `Assets/FlyWeight/PlayerSettingsSO.cs` + su `.meta`.

No afecta el build ni corrompe el proyecto — pero si vuelve a aparecer, revisar primero si hay algún otro `.asset`/`.prefab` con campos serializados que ya no existen en su script (Unity los tolera casi siempre, pero combinado con otros problemas de tipos puede disparar esto), antes de asumir que es solo una carrera de reload.

## Arquitectura de input/movimiento (Player)

- `InputReader` (`Assets/Scripts/InputReader.cs`): MonoBehaviour puente entre `PlayerInput` (Unity Input System) y el resto del código. Expone eventos C# (`Move`, `ShootCannon`, etc.) invocados desde callbacks públicos (`OnMove`, `OnShootCannon`) que `PlayerInput` llama vía Unity Events, configurados en el prefab `Tank.prefab`.
- `PlayerMovement` (`Assets/Scripts/PlayerMovement.cs`): clase C# pura (no MonoBehaviour). Se suscribe a `InputReader.Move` para cachear el último `Vector2` de input (`SetMoveInput`), y aplica el movimiento en `ArtificialUpdate()`, llamado desde `Player.Update()` cada frame.
  - **Por qué el movimiento no se cachea directo en el evento:** el Input System solo dispara el evento `Move` cuando el *valor* cambia, no en cada frame que una tecla se mantiene apretada (para actions `Value` con composite `Dpad`/2DVector como WASD). Si se moviera el tanque directo en el callback del evento, el movimiento se cortaría a un solo frame por tecla. La solución es cachear el valor y aplicarlo en un loop de `Update()` continuo, desacoplando "cuándo llega el input" de "cuándo se aplica el movimiento".
- `Player` (`Assets/Scripts/Player.cs`): MonoBehaviour que vive en el GameObject `Tank`. Construye `PlayerMovement` en `Awake()`, lo inicializa con `GetComponent<InputReader>()` en `Start()` (referencia al componente hermano en el mismo GameObject — no vía `PlayerSettingsSO`, ver nota abajo), y llama `playerMovement.ArtificialUpdate()` en `Update()`.

**Nota histórica:** `PlayerSettingsSO.inputReader` (ScriptableObject) se eliminó porque era la causa de un bug — el ScriptableObject es un asset de proyecto, no puede referenciar de forma fiable un componente `InputReader` que solo existe en runtime dentro de una instancia de escena. Ahora `Player.Start()` resuelve `InputReader` con `GetComponent<InputReader>()` directamente.
