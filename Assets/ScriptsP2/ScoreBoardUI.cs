using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreboardUI : NetworkBehaviour
{
    public static ScoreboardUI Instance;

    public TextMeshProUGUI myScoreText;
    public Transform listParent;
    public GameObject rowPrefab;

    void Awake() => Instance = this;

    void OnEnable()  => InvokeRepeating(nameof(RefreshNow), 0.1f, 0.5f);
    void OnDisable() => CancelInvoke(nameof(RefreshNow));

    public void RefreshNow()
    {
        // 1) Mi score (local)
        var local = NetworkClient.localPlayer;
        if (local)
        {
            var myStats = local.GetComponent<PlayerThirdPerson>();
            if (myStats && myScoreText)
                myScoreText.text = $"Mi score: {myStats.score}";
        }

        // 2) Lista de todos
        foreach (Transform c in listParent) Destroy(c.gameObject);

        var all = Object.FindObjectsByType<PlayerThirdPerson>(FindObjectsSortMode.InstanceID)
                 .OrderByDescending(p => p.score)
                 .ToList();

        foreach (var p in all)
        {
            var row = Instantiate(rowPrefab, listParent);
            var txt = row.GetComponentInChildren<Text>();
            if (txt)
                txt.text = $"Player {p.netId}: {p.score}";
        }
    }
}