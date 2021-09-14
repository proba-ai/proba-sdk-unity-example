using System;

namespace ProbaSDK.Exceptions
{
	public class ProbaNotInitializedException: Exception
	{
		public ProbaNotInitializedException(): base("You must call Proba.Initialize() first!")
		{
		}
	}
}