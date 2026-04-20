using UnityEngine;
using UnityEngine.Playables;

public class CinematicCameraHandler : MonoBehaviour
{
    [Header("Referencias")]
    public PlayableDirector cinematicDirector;     // Tu PlayableDirector de la cinemática
    public Transform playerCamera;                 // La cámara del jugador (normalmente child del jugador)
    public MonoBehaviour playerController;         // El script de First Person (el que tiene el mouse look)

    private bool wasPlaying = false;

    private void Update()
    {
        // Detectar cuando la cinemática termina
        if (cinematicDirector != null)
        {
            bool isPlayingNow = cinematicDirector.state == PlayState.Playing;

            if (wasPlaying && !isPlayingNow)
            {
                // La cinemática acaba de terminar
                OnCinematicEnded();
            }

            wasPlaying = isPlayingNow;
        }
    }

    private void OnCinematicEnded()
    {
        Debug.Log("Cinemática terminada → sincronizando mirada del jugador");

        if (playerCamera == null) return;

        // === SOLUCIÓN PRINCIPAL ===
        // Forzar que el controlador tome la rotación actual de la cámara
        if (playerController != null)
        {
            // Desactivamos temporalmente el controlador para evitar conflictos
            playerController.enabled = false;

            // Sincronizamos la rotación del jugador y la cámara
            Transform playerBody = playerCamera.parent; // Normalmente el cuerpo del jugador

            if (playerBody != null)
            {
                // Copiamos la rotación Y del cuerpo (mirada horizontal)
                playerBody.rotation = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
            }

            // Copiamos la rotación X de la cámara (mirada vertical)
            playerCamera.localRotation = Quaternion.Euler(playerCamera.localEulerAngles.x, 0f, 0f);
        }

        // Reactivamos el controlador después de sincronizar
        if (playerController != null)
            playerController.enabled = true;

        // Si usas un sistema de MouseLook personalizado, llama a una función de reinicio aquí
        // Ejemplo: playerController.GetComponent<FirstPersonController>().ReInitMouseLook();
    }

    // Método público por si quieres llamarlo manualmente desde un Signal o Animation Event
    public void ForceSyncCamera()
    {
        OnCinematicEnded();
    }
}