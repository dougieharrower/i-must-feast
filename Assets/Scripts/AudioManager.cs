using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource ambientLoop;
    public AudioSource heartbeatLoop;
    public AudioSource shadeSound;
    public AudioSource feastViolin;

void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        // Optional: remove this
        // DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}


public void PlayShadeTone()
{
    if (shadeSound != null && shadeSound.gameObject.activeInHierarchy)
    {
        shadeSound.Play();
    }
}


public void PlayFeastViolin()
{
    if (feastViolin != null && feastViolin.gameObject.activeInHierarchy)
    {
        if (!feastViolin.isPlaying)
            feastViolin.Play();
    }
}


    public void SetHeartbeatIntensity(float volume, float pitch)
    {
        heartbeatLoop.volume = volume;
        heartbeatLoop.pitch = pitch;
    }

    public void EaseToFeastState()
    {
        heartbeatLoop.volume = 1f;
        heartbeatLoop.pitch = 0.5f;
    }

    public void ResetHeartbeat()
    {
        heartbeatLoop.volume = 0f;
        heartbeatLoop.pitch = 1f;
    }

private void Start()
{
    // 🔄 Reassign if needed
    if (ambientLoop == null)
        ambientLoop = GameObject.Find("AmbientLoopSource")?.GetComponent<AudioSource>();
    if (heartbeatLoop == null)
        heartbeatLoop = GameObject.Find("HeartbeatSource")?.GetComponent<AudioSource>();
    // ...and so on for shadeSound, feastViolin

    if (ambientLoop != null)
    {
        ambientLoop.loop = true;
        ambientLoop.Play();
    }
    else
    {
        Debug.LogWarning("Ambient loop AudioSource not assigned in AudioManager.");
    }
}

    
    private Coroutine heartbeatRoutine;

public void StartHeartbeat(float initialDelay = 1f)
{
    if (heartbeatRoutine != null) StopCoroutine(heartbeatRoutine);
    heartbeatRoutine = StartCoroutine(HeartbeatLoop(initialDelay));
}

public void StopHeartbeat()
{
    if (heartbeatRoutine != null) StopCoroutine(heartbeatRoutine);
    heartbeatLoop.Stop();
}

private IEnumerator HeartbeatLoop(float delay)
{
    while (true)
    {
        heartbeatLoop.Play();
        yield return new WaitForSeconds(delay);
    }
}

}
