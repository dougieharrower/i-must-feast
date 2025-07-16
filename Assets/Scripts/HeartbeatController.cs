using UnityEngine;
using System.Collections;

public class HeartbeatController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform prey;

    [Header("Distance Settings")]
    public float minDistance = 2f;
    public float maxDistance = 25f;

    [Header("Heartbeat Timing")]
    public float minDelay = 0.3f;
    public float maxDelay = 1.5f;

    private AudioSource heartbeatSource;
    private Coroutine heartbeatCoroutine;
    private float currentDelay;
    private bool feastModeActive = false;

    void Start()
    {
        heartbeatSource = AudioManager.Instance.heartbeatLoop;
        currentDelay = maxDelay;
        heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
    }

    void Update()
    {
        if (feastModeActive) return;
        UpdateHeartbeatLogic();
    }

    void UpdateHeartbeatLogic()
    {
        float distance = Vector3.Distance(player.position, prey.position);
        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);

        currentDelay = Mathf.Lerp(maxDelay, minDelay, t);
        float volume = Mathf.Lerp(0.2f, 1f, t);

        AudioManager.Instance.SetHeartbeatIntensity(volume, 1f);
    }

    IEnumerator HeartbeatLoop()
    {
        while (true)
        {
            heartbeatSource.Play();
            yield return new WaitForSeconds(currentDelay);
        }
    }

    public void EnterFeastMode()
    {
        feastModeActive = true;
        AudioManager.Instance.EaseToFeastState();
        currentDelay = 2f;
    }

    public void ExitFeastMode()
    {
        feastModeActive = false;
        AudioManager.Instance.ResetHeartbeat();
        currentDelay = maxDelay;
    }

    public void ResetHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }

        if (heartbeatSource != null)
        {
            heartbeatSource.Stop();
        }

        currentDelay = maxDelay;
        heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
    }
}
