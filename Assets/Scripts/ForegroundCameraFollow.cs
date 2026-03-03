using UnityEngine;

public class ForegroundCameraFollow : MonoBehaviour
{
    [SerializeField] private PlayerController2D player;
    [SerializeField] private float cameraMinX = -0.1f;
    [SerializeField] private float cameraMaxX = 0.1f;

    private void LateUpdate()
    {
        if (player == null) return;

        float t = Mathf.InverseLerp(player.minX, player.maxX, player.transform.position.x);
        Vector3 pos = transform.position;
        pos.x = -Mathf.Lerp(cameraMinX, cameraMaxX, t);
        transform.position = pos;
    }
}
