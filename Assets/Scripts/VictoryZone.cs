using UnityEngine;

public class VictoryZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            player.Victory();
        }
    }
}
