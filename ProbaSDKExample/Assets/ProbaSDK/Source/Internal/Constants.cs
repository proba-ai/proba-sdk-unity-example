namespace ProbaSDK.Internal
{
	internal static class Constants
	{
		public const string ApiHost = "https://api.proba.ai";
		public const string MobileApiPath = "api/mobile";

		public const string HeaderContent = "ContentType";
		public const string HeaderContentValue = "application/json";
		
		public const string HeaderAppId = "SDK-App-ID";
		public const string HeaderAuth = "Authorization";

		public const string HeaderAppVersion = "AppVersion";
		public const string HeaderAppVersionValue = "application/json";
		
		public const string ExperimentsPath = "experiments";
		public const string OptionsPath = "experiments/options";

		public const int RequestTimeoutSec = 3;
		
		public const string DebugPrefabName = "[ProbaDebugView]";
	}
}