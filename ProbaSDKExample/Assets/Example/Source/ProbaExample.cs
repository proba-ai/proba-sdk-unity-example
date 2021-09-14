using System;
using ProbaSDK;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
public class ProbaExample : MonoBehaviour
{
	[SerializeField] private string _sdkToken = "Insert you SDK token here!";
	[SerializeField] private string _appId = "Insert you AppId here!";

	[Space, SerializeField] private Button _button;

	void Start()
	{
		Proba.Initialize(sdkToken: _sdkToken,
			appId: _appId,
			usingShake: true,
			debugLogs: true,
			defaults: new[]
			{
				("buttonColor", "blue")
			},
			deviceProperties: new[]
			{
				("installedAt", DateTime.Now.ToString()),
			}
			);

		if (Proba.HasValue("buttonColor"))
		{
			_button.GetComponentInChildren<Text>().text = Proba.GetValue("buttonColor");
		}

		FetchData();
	}

	private async void FetchData()
	{
		try
		{
			await Proba.FetchAsync();

			if (Proba.HasValue("buttonColor"))
			{
				_button.GetComponentInChildren<Text>().text = Proba.GetValue("buttonColor");
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"Error initializing Proba: {e}");
		}
	}
}