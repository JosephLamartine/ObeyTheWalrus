using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════
//  RoomManager  —  Optimización de visibilidad por rooms
//
//  SETUP:
//  1. Crea un GameObject vacío "HouseManager" en la escena y agrega este script.
//  2. En el Inspector define cada RoomData: asigna el padre de geometría,
//     el padre de luces, y los rooms vecinos.
//  3. En cada Room coloca un trigger (BoxCollider con Is Trigger = true)
//     y agrega el script RoomTrigger, asignando el roomId correspondiente.
//  4. El jugador debe estar en el LayerMask "Player" (o el que definas).
//
//  SOBRE LUCES:
//  - Si tus luces son REALTIME: activa lightHandlingMode = GameObjects
//    (desactiva/activa el GameObject de la luz directamente)
//  - Si son BAKED o MIXED: activa lightHandlingMode = Intensity
//    (pone intensidad a 0 en vez de desactivar, evita artefactos)
// ═══════════════════════════════════════════════════════════════════════

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    public enum LightHandlingMode
    {
        GameObjects,   // Activa/desactiva el GameObject de la luz (realtime)
        Intensity      // Pone intensidad 0/original (baked/mixed)
    }

    [Header("─── Configuración ──────────────────────────────────")]
    public LightHandlingMode lightHandlingMode = LightHandlingMode.GameObjects;

    [Tooltip("Delay en segundos antes de desactivar un room al salir. " +
             "Da tiempo a que el jugador entre al siguiente trigger.")]
    public float deactivationDelay = 0.5f;

    [Tooltip("Room activo al inicio de la escena (índice en el array rooms)")]
    public int initialRoomIndex = 0;

    [Header("─── Rooms ───────────────────────────────────────────")]
    public RoomData[] rooms;

    // ── Privados ─────────────────────────────────────────────────────
    private Dictionary<string, RoomData> roomMap     = new Dictionary<string, RoomData>();
    private HashSet<string>              activeRooms  = new HashSet<string>();
    private Coroutine                    deactivateCoroutine;

    // ═══════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        BuildRoomMap();
        InitializeLightCache();
    }

    private void Start()
    {
        // Desactivar todo al inicio
        foreach (var room in rooms)
            SetRoomActive(room, false, instant: true);

        // Activar room inicial y sus vecinos
        if (rooms != null && rooms.Length > 0 && initialRoomIndex < rooms.Length)
            ActivateRoom(rooms[initialRoomIndex].roomId);
    }

    // ═══════════════════════════════════════════════════════════════
    // API PÚBLICA — llamada por RoomTrigger
    // ═══════════════════════════════════════════════════════════════

    /// <summary>El jugador entró al trigger de un room.</summary>
    public void OnPlayerEnterRoom(string roomId)
    {
        if (!roomMap.TryGetValue(roomId, out RoomData entered)) return;

        // Cancelar desactivación pendiente si el jugador volvió rápido
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }

        // Activar este room y sus vecinos inmediatamente
        ActivateRoom(roomId);

        // Desactivar los rooms que ya no son relevantes con delay
        deactivateCoroutine = StartCoroutine(DeactivateIrrelevantRooms(entered));
    }

    /// <summary>El jugador salió del trigger de un room.</summary>
    public void OnPlayerExitRoom(string roomId)
    {
        // No hacemos nada aquí directamente — esperamos a que entre
        // al siguiente room antes de desactivar. El deactivationDelay
        // en OnPlayerEnterRoom maneja esto.
        // Si necesitas lógica extra al salir, agrégala aquí.
    }

    // ═══════════════════════════════════════════════════════════════
    // ACTIVACIÓN / DESACTIVACIÓN
    // ═══════════════════════════════════════════════════════════════
    private void ActivateRoom(string roomId)
    {
        if (!roomMap.TryGetValue(roomId, out RoomData room)) return;

        SetRoomActive(room, true);

        foreach (string neighborId in room.neighborRoomIds)
        {
            if (roomMap.TryGetValue(neighborId, out RoomData neighbor))
                SetRoomActive(neighbor, true);
        }
    }

    private IEnumerator DeactivateIrrelevantRooms(RoomData currentRoom)
    {
        yield return new WaitForSeconds(deactivationDelay);

        // Construir set de rooms que deben seguir activos
        HashSet<string> shouldBeActive = new HashSet<string>();
        shouldBeActive.Add(currentRoom.roomId);
        foreach (string nId in currentRoom.neighborRoomIds)
            shouldBeActive.Add(nId);

        // Desactivar los que no están en el set
        foreach (var room in rooms)
        {
            if (!shouldBeActive.Contains(room.roomId) && activeRooms.Contains(room.roomId))
                SetRoomActive(room, false);
        }

        deactivateCoroutine = null;
    }

    private void SetRoomActive(RoomData room, bool active, bool instant = false)
    {
        if (room == null) return;

        // Geometría
        if (room.geometryParent != null)
            room.geometryParent.SetActive(active);

        // Luces
        if (lightHandlingMode == LightHandlingMode.GameObjects)
        {
            if (room.lightsParent != null)
                room.lightsParent.SetActive(active);
        }
        else
        {
            SetLightIntensities(room, active);
        }

        // Tracking
        if (active)
            activeRooms.Add(room.roomId);
        else
            activeRooms.Remove(room.roomId);
    }

    // ═══════════════════════════════════════════════════════════════
    // MANEJO DE INTENSIDAD DE LUCES (modo Baked/Mixed)
    // ═══════════════════════════════════════════════════════════════
    private void InitializeLightCache()
    {
        if (lightHandlingMode != LightHandlingMode.Intensity) return;

        foreach (var room in rooms)
        {
            if (room.lightsParent == null) continue;

            Light[] lights = room.lightsParent.GetComponentsInChildren<Light>(true);
            room.cachedLights = lights;
            room.originalIntensities = new float[lights.Length];

            for (int i = 0; i < lights.Length; i++)
                room.originalIntensities[i] = lights[i].intensity;
        }
    }

    private void SetLightIntensities(RoomData room, bool active)
    {
        if (room.cachedLights == null) return;

        for (int i = 0; i < room.cachedLights.Length; i++)
        {
            if (room.cachedLights[i] == null) continue;
            room.cachedLights[i].intensity = active ? room.originalIntensities[i] : 0f;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private void BuildRoomMap()
    {
        roomMap.Clear();
        if (rooms == null) return;

        foreach (var room in rooms)
        {
            if (string.IsNullOrEmpty(room.roomId))
            {
                Debug.LogWarning("[HouseManager] Room sin roomId — asigna un ID único en el Inspector.");
                continue;
            }
            if (roomMap.ContainsKey(room.roomId))
            {
                Debug.LogWarning($"[HouseManager] roomId duplicado: '{room.roomId}'. Solo se registra el primero.");
                continue;
            }
            roomMap[room.roomId] = room;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // DEBUG GIZMOS — muestra en el editor qué rooms están activos
    // ═══════════════════════════════════════════════════════════════
    private void OnDrawGizmos()
    {
        if (rooms == null) return;

        foreach (var room in rooms)
        {
            if (room.geometryParent == null) continue;

            bool isActive = activeRooms.Contains(room.roomId);
            Gizmos.color  = isActive ? new Color(0f, 1f, 0f, 0.15f) : new Color(1f, 0f, 0f, 0.08f);

            Bounds bounds = GetRoomBounds(room.geometryParent);
            Gizmos.DrawCube(bounds.center, bounds.size);

            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    private Bounds GetRoomBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.one * 2f);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);
        return b;
    }
}

// ═══════════════════════════════════════════════════════════════════════
//  RoomData  —  Datos de un room (definidos en el Inspector de HouseManager)
// ═══════════════════════════════════════════════════════════════════════
[System.Serializable]
public class RoomData
{
    [Tooltip("ID único del room (ej: 'living_room', 'kitchen', 'hallway')")]
    public string roomId;

    [Tooltip("GameObject padre que contiene toda la geometría del room")]
    public GameObject geometryParent;

    [Tooltip("GameObject padre que contiene todas las luces del room")]
    public GameObject lightsParent;

    [Tooltip("IDs de los rooms adyacentes que deben estar visibles cuando estás aquí")]
    public string[] neighborRoomIds;

    // Cache interno — no lo toques en el Inspector
    [HideInInspector] public Light[] cachedLights;
    [HideInInspector] public float[] originalIntensities;
}