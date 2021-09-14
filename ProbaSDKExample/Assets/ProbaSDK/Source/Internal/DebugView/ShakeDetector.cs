using System;
using UnityEngine;

namespace ProbaSDK.Internal.DebugView
{
	internal class ShakeDetector: MonoBehaviour
	{
		private const float AccelerometerUpdateInterval = 1.0f / 60.0f;
		// The greater the value of LowPassKernelWidthInSeconds, the slower the
		// filtered value will converge towards current input sample (and vice versa).
		private const float LowPassKernelWidthInSeconds = 1.0f;
		// This next parameter is initialized to 2.0 per Apple's recommendation,
		// or at least according to Brady! ;)
		private const float ShakeDetectionThresholdSq = 2.0f * 2.0f;

		private float _lowPassFilterFactor;
		private Vector3 _lowPassValue;

		public Action ShakeDetected { get; set; }

		private void Start()
		{
			_lowPassFilterFactor = AccelerometerUpdateInterval / LowPassKernelWidthInSeconds;
			_lowPassValue = Input.acceleration;
		}

		private void Update()
		{
			var acceleration = Input.acceleration;
			_lowPassValue = Vector3.Lerp(_lowPassValue, acceleration, _lowPassFilterFactor);
			var deltaAcceleration = acceleration - _lowPassValue;

			if (deltaAcceleration.sqrMagnitude >= ShakeDetectionThresholdSq)
			{
				ShakeDetected?.Invoke();
			}
		}
	}
}