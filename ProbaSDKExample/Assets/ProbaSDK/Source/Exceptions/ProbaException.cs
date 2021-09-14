using System;

namespace ProbaSDK.Exceptions
{
	public class ProbaException: Exception
	{
		public ProbaException()
		{
		}

		public ProbaException(string message) : base(message)
		{
		}

		public ProbaException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}