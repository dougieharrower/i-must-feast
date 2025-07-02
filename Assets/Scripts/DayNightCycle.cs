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

    private float elapsed = 0f;

    void Update()
    {
        // 1. Update time
        elapsed += Time.deltaTime;
        timeOfDay = (elapsed % dayDuration) / dayDuration;

        // 2. Rotate the light
        float angle = timeOfDay * 360f;
        transform.rotation = Quaternion.Euler(angle, 50f, 0f); // Tilt angle

        // 3. Calculate light appearance
        if (directionalLight)
        {
            directionalLight.color = colorOverTime.Evaluate(timeOfDay);
            directionalLight.intensity = intensityOverTime.Evaluate(timeOfDay);
        }

        // 4. Determine night (sun below horizon)
        isNight = angle > 180f;
    }
}
