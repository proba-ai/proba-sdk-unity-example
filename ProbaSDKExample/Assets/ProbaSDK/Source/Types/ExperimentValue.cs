using System;

namespace ProbaSDK.Types
{
	[Serializable]
	public class ExperimentValue
	{
		public string key;
		public string value;

		public ExperimentValue(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}
}