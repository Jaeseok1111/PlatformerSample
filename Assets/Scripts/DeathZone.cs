using System.Collections;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            var virtualCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;

            player.Die();

            StartCoroutine(RespawnPlayer(player));
        }
    }

    private IEnumerator RespawnPlayer(PlayerController player)
    {
        yield return new WaitForSeconds(2f);

        player.Spawn();
    }
}
