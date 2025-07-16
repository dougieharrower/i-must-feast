using UnityEngine;

public class FollowPlayerPositionOnly : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 2.6f, -4f); // adjust as needed

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}
