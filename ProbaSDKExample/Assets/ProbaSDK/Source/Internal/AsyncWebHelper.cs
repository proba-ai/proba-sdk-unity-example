using System.Threading;
using System.Threading.Tasks;
using ProbaSDK.Exceptions;
using UnityEngine;
using UnityEngine.Networking;

namespace ProbaSDK.Internal
{
	internal class AsyncWebHelper
	{
		private readonly CredentialsData _credentials;
		private readonly bool _debugLogs;

		public AsyncWebHelper(CredentialsData credentials, bool debugLogs)
		{
			_credentials = credentials;
			_debugLogs = debugLogs;
		}
		
		public async Task<string> MakeApiRequestAsync(string apiUrl, string queryArgs, CancellationToken ct)
		{
			var url = $"{Constants.ApiHost}/{Constants.MobileApiPath}/{apiUrl}";

			if (!string.IsNullOrEmpty(queryArgs))
			{
				url += $"?{queryArgs}";
			}

			using (var request = new UnityWebRequest(url, "GET", new DownloadHandlerBuffer(), null))
			{
				request.SetRequestHeader(Constants.HeaderContent, Constants.HeaderContentValue);
				request.SetRequestHeader(Constants.HeaderAppVersion, Constants.HeaderAppVersionValue);
				request.SetRequestHeader(Constants.HeaderAppId, _credentials.AppId);
				request.SetRequestHeader(Constants.HeaderAuth, $"Bearer {_credentials.AuthToken}");
				request.timeout = Constants.RequestTimeoutSec;

				var tStart = Time.time;
				
				var op = request.SendWebRequest();
				while (!op.isDone)
				{
					await Task.Delay(100, ct);
				}

				if (_debugLogs)
				{
					Debug.Log($"[Proba] WebRequest {apiUrl} in {(Time.time - tStart):0.000} sec");
				}

				if (request.isNetworkError)
				{
					throw new ProbaException($"Network error while sending request '{url}': {request.error}");
				}

				if (request.isHttpError)
				{
					throw new ProbaException($"Http error while sending request '{url}': {request.error}");
				}

				return request.downloadHandler.text;
			}
		}
	}
}