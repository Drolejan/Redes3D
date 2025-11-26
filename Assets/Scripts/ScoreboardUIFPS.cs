using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreboardUIFPS : MonoBehaviour
{
    public static ScoreboardUIFPS Instance;

    [Header("UI Refs")]
    public Transform listParent;   // objeto con VerticalLayoutGroup
    public GameObject rowPrefab;   // prefab con un Text

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        // Refresco periódico por si algo se nos va
        InvokeRepeating(nameof(Refresh), 0.2f, 0.5f);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Refresh));
        if (Instance == this)
            Instance = null;
    }

    public void Refresh()
    {
        if (listParent == null || rowPrefab == null) return;

        // Limpiar filas anteriores
        foreach (Transform child in listParent)
            Destroy(child.gameObject);

        // Buscar todos los jugadores (NetworkHealth) y ordenarlos por kills
        var players = Object.FindObjectsByType<NetworkHealth>(FindObjectsSortMode.InstanceID)
                     .OrderByDescending(p => p.kills)
                     .ToList();

        foreach (var p in players)
        {
            GameObject row = Instantiate(rowPrefab, listParent);
            TextMeshProUGUI txt = row.GetComponentInChildren<TextMeshProUGUI>(); // o TextMeshProUGUI si usas TMP

            string name = string.IsNullOrEmpty(p.displayName)
                ? $"Player {p.netId}"
                : p.displayName;

            if (txt != null)
                txt.text = $"{name}  -  Kills: {p.kills}";
        }
    }
}