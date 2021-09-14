using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProbaSDK.Internal.DebugView
{
	internal class ProbaDebugView: MonoBehaviour
	{
		[SerializeField] private CanvasScaler _scaler;
		[SerializeField] private GameObject _pDebugRoot;
		[SerializeField] private ScrollRect _scroll;
		[SerializeField] private Transform _pContent;
		[SerializeField] private GameObject _headerTemplate;
		[SerializeField] private GameObject _itemTemplate;
		
		private List<DebugHeader> _headers = new List<DebugHeader>(8);
		private List<DebugItem> _items = new List<DebugItem>(8);

		private ProbaManager _manager;
			
		private void Awake()
		{
			Close();
			DontDestroyOnLoad(gameObject);

			Cleanup();
		}

		public void Show(ProbaManager manager)
		{
			if (_pDebugRoot.activeSelf)
			{
				return;
			}

			UpdateAspect();
			
			_manager = manager;
			
			_pDebugRoot.SetActive(true);

			RebuildView();
		}

		private void UpdateAspect()
		{
			if (Screen.width > Screen.height)
			{
				_scaler.matchWidthOrHeight = 1;
			}
			else
			{
				_scaler.matchWidthOrHeight = 0;
			}
		}

		public void Close()
		{
			_manager = null;
			_pDebugRoot.SetActive(false);
		}
		
		public void ResetData()
		{
			_manager.ResetExperiments();
			RebuildView();
		}
		
		public async void Reload()
		{
			try
			{
				await _manager.FetchDebugAsync();
				if (_manager == null)
				{
					return;
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
			}

			RebuildView();
		}

		private void Update()
		{
			if (!_pDebugRoot.activeSelf)
			{
				return;
			}

			if (Input.GetKeyUp(KeyCode.Escape))
			{
				Close();
			}

			UpdateAspect();
		}
		
		private void Cleanup()
		{
			_scroll.normalizedPosition = Vector2.zero;
			_headerTemplate.gameObject.SetActive(false);
			_itemTemplate.gameObject.SetActive(false);
			
			foreach (var debugHeader in _headers)
			{
				GameObject.Destroy(debugHeader.gameObject);
			}
			foreach (var item in _items)
			{
				GameObject.Destroy(item.gameObject);
			}
			
			_headers.Clear();
			_items.Clear();
		}

		private void RebuildView()
		{
			Cleanup();
			
			var experiments = _manager.ExperimentsDebug ?? new CompositeExperiment[0];
			
			foreach (var exp in experiments)
			{
				var header = GameObject.Instantiate(_headerTemplate, _pContent, false);
				var cHeader = header.GetComponent<DebugHeader>();
				cHeader.Experiment = exp;
				cHeader.title.text = string.IsNullOrEmpty(exp.name) ? exp.key : exp.name;
				cHeader.desc.text = exp.key;
				
				header.SetActive(true);
				
				_headers.Add(cHeader);

				if (exp.options == null)
				{
					continue;
				}

				foreach (var experimentOption in exp.options)
				{
					var option = GameObject.Instantiate(_itemTemplate, _pContent, false);
					var cOption = option.GetComponent<DebugItem>();
					cOption.Experiment = exp;
					cOption.Item = experimentOption;
					cOption.title.text = string.IsNullOrEmpty(experimentOption.description) ? experimentOption.value : experimentOption.description;
					cOption.desc.text = experimentOption.value;
					option.SetActive(true);

					cOption.toggle.onValueChanged.AddListener((x) => ClickOption(cOption.Experiment, cOption.Item));
					
					_items.Add(cOption);
				}
			}

			UpdateCheckState();
		}

		private bool _updating;
		
		private void UpdateCheckState()
		{
			var values = _manager.GetExperiments();

			_updating = true;
			
			foreach (var debugItem in _items)
			{
				debugItem.toggle.isOn = values.TryGetValue(debugItem.Experiment.key, out var value)
					&& debugItem.Item.value.Equals(value, StringComparison.InvariantCultureIgnoreCase);
			}

			_updating = false;
		}

		private void ClickOption(CompositeExperiment exp, ExperimentOption experimentOption)
		{
			if (_updating)
			{
				return;
			}
			
			_manager.SetExperiment(exp.key, experimentOption.value);
			
			UpdateCheckState();
		}
	}
}