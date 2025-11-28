using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class barra : MonoBehaviour
{
    public TextMeshProUGUI salud;
    Slider labarra;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        labarra = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        labarra.value = int.Parse(salud.text);
    }
}
