using ProbaSDK;
using UnityEngine;

public class ProbaOpenDebugger : MonoBehaviour
{
    private float shakeDetectionThreshold = 2.0f;

    private float accelerometerUpdateInterval = 1.0f / 60.0f;
    private float lowPassKernelWidthInSeconds = 1.0f;
    private float lowPassFilterFactor;
    private Vector3 lowPassValue;

    private void Start()
    {
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
    }

    private void Update()
    {
        Vector3 acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp( lowPassValue, acceleration, lowPassFilterFactor );
        Vector3 deltaAcceleration = acceleration - lowPassValue;

        if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
            Proba.LaunchDebugMode();
    }
}
