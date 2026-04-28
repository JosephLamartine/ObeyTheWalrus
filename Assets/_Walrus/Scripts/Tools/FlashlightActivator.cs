using UnityEngine;

public class FlashlightActivator : MonoBehaviour
{
    [Tooltip("El GameObject de la flashlight que está desactivado al inicio")]
    public GameObject flashlightObject;

    private void Start()
    {
        flashlightObject.SetActive(false);
        InventoryManager.Instance.OnFlashlightPickedUp += Activate;
    }

    private void OnDestroy()
    {
        InventoryManager.Instance.OnFlashlightPickedUp -= Activate;
    }

    private void Activate()            => flashlightObject.SetActive(true);
}