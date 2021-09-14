using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProbaSDK.Exceptions;
using ProbaSDK.Internal;
using ProbaSDK.Types;

namespace ProbaSDK
{
	public static class Proba
	{
		private static ProbaManager _manager; 
		
		public static void Initialize(string sdkToken, string appId, string deviceId = null, string appsFlyerId = null, bool usingShake = true,
			string amplitudeUserId = null, bool debugLogs = false, (string key, string value)[] defaults = null, (string key, string value)[] deviceProperties = null)
		{
			_manager = new ProbaManager(sdkToken, appId, deviceId, appsFlyerId, amplitudeUserId, usingShake, debugLogs, 
				defaults?.Select(x => new ExperimentValue(x.key, x.value)).ToArray() ?? new ExperimentValue[0],
				deviceProperties?.Select(x => new ExperimentValue(x.key, x.value)).ToArray() ?? new ExperimentValue[0]);
		}

		public static Task FetchAsync(CancellationToken ct = default)
		{
			if (_manager == null)
			{
				throw new ProbaNotInitializedException();
			}
			
			return _manager.FetchAsync(ct);
		}

		public static void LaunchDebugMode()
		{
			if (_manager == null)
			{
				throw new ProbaNotInitializedException();
			}

			_manager.OpenDebugView();
		}

		public static bool HasValue(string key)
		{
			if (_manager == null)
			{
				throw new ProbaNotInitializedException();
			}

			return _manager.HasValue(key);
		}

		public static string GetValue(string key)
		{
			if (_manager == null)
			{
				throw new ProbaNotInitializedException();
			}

			return _manager.GetValue(key);
		}
		
		public static IReadOnlyDictionary<string, string> GetExperiments()
		{
			if (_manager == null)
			{
				throw new ProbaNotInitializedException();
			}

			return _manager.GetExperiments();
		}
		
		public static IReadOnlyDictionary<string, string> GetExperimentsWithDetails()
		{
			if (_manager == null)
			{
				throw new ProbaNotInitializedException();
			}

			return _manager.GetExperimentsWithDetails();
		}
	}
}