using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProbaSDK.Exceptions;
using ProbaSDK.Internal.DebugView;
using ProbaSDK.Types;
using UnityEngine;

namespace ProbaSDK.Internal
{
	internal class ProbaManager
	{
		private readonly bool _debugLogs;

		private readonly AsyncWebHelper _client;
		
		private readonly ExperimentValue[] _defaults;
		private Experiment[] _experiments;
		private readonly List<ExperimentValue> _debugOverride;
		private readonly Dictionary<string, string> _experimentMap = new Dictionary<string, string>(16, StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string, string> _optionIdMap = new Dictionary<string, string>(16, StringComparer.InvariantCultureIgnoreCase);

		private readonly ProbaDebugView _debugView;

		private bool _isDebug;

		public CompositeExperiment[] ExperimentsDebug { get; private set; }

		public ProbaManager(string sdkToken, string appId, string deviceId, string appsFlyerId, string amplitudeUserId, 
			bool usingShake, bool debugLogs, ExperimentValue[] defaults, ExperimentValue[] deviceProperties)
		{
			if (string.IsNullOrEmpty(sdkToken))
			{
				throw new ArgumentException("Must be non empty string", nameof(sdkToken));
			}

			if (string.IsNullOrEmpty(appId))
			{
				throw new ArgumentException("Must be non empty string", nameof(appId));
			}

			if (string.IsNullOrEmpty(deviceId))
			{
				deviceId = LocalStorage.GetString("deviceId");
				if (string.IsNullOrEmpty(deviceId))
				{
					deviceId = Guid.NewGuid().ToString();
					LocalStorage.SetString("deviceId", deviceId);
				}
			}

			_isDebug = LocalStorage.GetBool("debug", false);

			_debugLogs = debugLogs;
			_defaults = defaults;
			_experiments = LocalStorage.GetObjects<Experiment>("experiments");
			ExperimentsDebug = LocalStorage.GetObjects<CompositeExperiment>("experimentsDebug");
			_debugOverride = LocalStorage.GetObjects<ExperimentValue>("experimentsDebugOverride").ToList();
			
			UpdateMap();
			
			var authPayload = $"{{\"deviceId\": \"{deviceId}\",\"appsFlyerId\":\"{appsFlyerId}\",\"amplitudeId\":\"{amplitudeUserId}\"";

			if (deviceProperties.Length > 0)
            {
				authPayload += $",\"deviceProperties\": {{";
				for (int i = 0; i < deviceProperties.Length; i++)
                {
					var props = deviceProperties[i];
					authPayload += $"\"{props.key}\": \"{props.value}\"";

					if ( i != deviceProperties.Length - 1 )
						authPayload += ",";
                }
				authPayload += "}";
			}
			authPayload += "}";

			var authToken = JsonWebToken.Encode( authPayload, sdkToken, JwtHashAlgorithm.HS256 );

			var credentials = new CredentialsData(appId, deviceId, appsFlyerId, sdkToken, authToken);
			
			_client = new AsyncWebHelper(credentials, _debugLogs);

			var prefab = Resources.Load<ProbaDebugView>(Constants.DebugPrefabName);
			if (prefab == null)
			{
				throw new ProbaException($"Error loading debug prefab. Try reinstalling package");
			}

			_debugView = GameObject.Instantiate(prefab);

			if (usingShake)
			{
				var detector = _debugView.gameObject.AddComponent<ShakeDetector>();
				detector.ShakeDetected = OpenDebugView;
			}
		}

		private void UpdateMap()
		{
			_experimentMap.Clear();
			_optionIdMap.Clear();

			foreach (var experiment in _defaults)
			{
				_experimentMap[experiment.key] = experiment.value;
			}

			foreach (var experiment in _experiments)
			{
				_experimentMap[experiment.key] = experiment.value;
				_optionIdMap[experiment.key] = experiment.optionId;
			}

			foreach (var experiment in _debugOverride)
			{
				_experimentMap[experiment.key] = experiment.value;
			}
		}

		public async Task FetchAsync(CancellationToken ct = default)
		{
			var knownKeys = string.Join("&", _defaults.Select(x => $"knownKeys[]={x.key}"));

			var result = await _client.MakeApiRequestAsync(Constants.ExperimentsPath, knownKeys, ct);

			var data = JsonUtility.FromJson<FetchResult>(result);

			_isDebug = data.meta?.debug ?? false;
			LocalStorage.SetBool("debug", _isDebug);

			_experiments = data.experiments ?? new Experiment[0];
			LocalStorage.SetObjects("experiments", _experiments);
			UpdateMap();

			if (_isDebug)
				await FetchDebugAsync(ct);
		}

		public async Task FetchDebugAsync(CancellationToken ct = default)
		{
			var knownKeys = string.Join("&", _defaults.Select(x => $"knownKeys[]={x.key}"));

			var result = await _client.MakeApiRequestAsync(Constants.OptionsPath, knownKeys, ct);
		
			var data = JsonUtility.FromJson<DebugFetchResult>(result);

			ExperimentsDebug = data.experiments ?? new CompositeExperiment[0];
			LocalStorage.SetObjects("experimentsDebug", ExperimentsDebug);
		}

		public void OpenDebugView()
		{
			if (!_isDebug)
			{
				return;
			}
			
			_debugView.Show(this);
		}

		public bool HasValue(string key)
		{
			return _experimentMap.ContainsKey(key);
		}

		public string GetValue(string key)
		{
			return _experimentMap.TryGetValue(key, out var result) ? result : null;
		}

		public IReadOnlyDictionary<string, string> GetExperiments()
		{
			return _experimentMap;
		}

		public IReadOnlyDictionary<string, string> GetExperimentsWithDetails()
		{
			IEnumerable<(string, string)> Expand(string key, string value)
			{
				yield return ($"[Proba] {key}", value);

				var optionId = _optionIdMap.TryGetValue(key, out var result) ? result : string.Empty;  
				
				yield return ($"[Proba] [internal] {key}", optionId);
			}
			
			return _experimentMap.SelectMany(x =>Expand(x.Key, x.Value))
				.ToDictionary(x => x.Item1, x => x.Item2);
		}

		public void SetExperiment(string expKey, string experimentOptionValue)
		{
			var exp =_debugOverride.FirstOrDefault(x => x.key.Equals(expKey, StringComparison.InvariantCultureIgnoreCase));
			if (exp == null)
			{
				exp = new ExperimentValue(expKey, experimentOptionValue);
				_debugOverride.Add(exp);
			}

			exp.value = experimentOptionValue;

			LocalStorage.SetObjects("experimentsDebugOverride", _debugOverride);
			UpdateMap();

			if (_debugLogs)
			{
				Debug.Log($"[Proba] SetExperiment {expKey}={experimentOptionValue}");
			}
		}

		public void ResetExperiments()
		{
			_debugOverride.Clear();
			LocalStorage.SetObjects("experimentsDebugOverride", _debugOverride);
			UpdateMap();

			if (_debugLogs)
			{
				Debug.Log($"[Proba] ResetExperiments");
			}
		}
		
	}
}