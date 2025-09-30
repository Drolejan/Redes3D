using UnityEngine;

public class coin : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerThirdPerson>().sumarPuntos();
            Destroy(gameObject);
        }
    }
    /*
    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        var player = collision.gameObject.GetComponent<PlayerThirdPerson>();
        if (player == null) return;

        // En vez de Destroy local, pedimos al player local que mande Command con la referencia
        var myId = GetComponent<NetworkIdentity>();
        if (myId != null)
            player.OnCoinPickedWithRef(myId, 1);
        else
            player.sumarPuntos(); // fallback si no tiene NetworkIdentity
    }
    */
}
