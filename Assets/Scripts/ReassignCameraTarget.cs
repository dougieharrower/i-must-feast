using UnityEngine;
using Unity.Cinemachine;

public class ReassignCameraTarget : MonoBehaviour
{
    public string playerTag = "Player";

    void Start()
    {
        var vcam = GetComponent<CinemachineCamera>();
        if (vcam != null)
        {
            Transform player = GameObject.FindGameObjectWithTag(playerTag)?.transform;
            if (player != null)
            {
                vcam.Follow = player;
                vcam.LookAt = player;
                Debug.Log("🎥 Cinemachine reassigned to player.");
            }
            else
            {
                Debug.LogWarning("Player not found when assigning Cinemachine Follow.");
            }
        }
    }
}
