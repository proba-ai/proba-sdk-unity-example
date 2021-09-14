namespace ProbaSDK.Internal
{
	public class CredentialsData
	{
		public string AppId { get; }
		public string DeviceId { get; }
		public string AppFlyerId { get; }
		public string SdkToken { get; }
		public string AuthToken { get; }

		public CredentialsData(string appId, string deviceId, string appFlyerId, string sdkToken, string authToken)
		{
			AppId = appId;
			DeviceId = deviceId;
			AppFlyerId = appFlyerId;
			SdkToken = sdkToken;
			AuthToken = authToken;
		}
	}
}