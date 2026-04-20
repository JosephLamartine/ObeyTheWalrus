using UnityEngine;
using UnityEngine.UI;

public class AlwaysOnTopUI : MonoBehaviour
{
    [Tooltip("Always = se muestra siempre encima de todo")]
    public UnityEngine.Rendering.CompareFunction zTest = UnityEngine.Rendering.CompareFunction.Always;

    void Start()
    {
        Apply();
    }

    void OnValidate()   // Para que funcione también en el editor
    {
        Apply();
    }

    void Apply()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic.material == null) continue;

            Material mat = new Material(graphic.material);           // Creamos copia
            mat.SetInt("unity_GUIZTestMode", (int)zTest);            // ← Aquí está el truco
            graphic.material = mat;
        }
    }
}