using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    public float dayDuration = 30f; // Full cycle in seconds
    public Vector3 rotationAxis = Vector3.right;

    [Header("Lighting Control")]
    public Light directionalLight;
    public Gradient colorOverTime;   // Set in Inspector: warm -> white -> blueish
    public AnimationCurve intensityOverTime; // 0 (night) to 1 (day) to 0 again

    [Header("Debug")]
    [Range(0f, 1f)] public float timeOfDay;
    public bool isNight;
    public bool overrideStaticSun = false;

    private float elapsed = 0f;

    void Update()
    {
        if (overrideStaticSun)
        {
            // Static sun: directly overhead, no animation
            transform.rotation = Quaternion.Euler(90f, 50f, 0f); // Directly overhead (adjust Y as needed)
            if (directionalLight)
            {
                directionalLight.color = Color.white;
                directionalLight.intensity = 1f;
            }

            isNight = false;  // It's always "day" in this mode
            return;  // Skip the normal cycle
        }

        // Normal animated day/night cycle
        elapsed += Time.deltaTime;
        timeOfDay = (elapsed % dayDuration) / dayDuration;

        float angle = timeOfDay * 360f;
        transform.rotation = Quaternion.Euler(angle, 50f, 0f); // Tilt angle

        if (directionalLight)
        {
            directionalLight.color = colorOverTime.Evaluate(timeOfDay);
            directionalLight.intensity = intensityOverTime.Evaluate(timeOfDay);
        }

        isNight = angle > 180f;
    }
}
