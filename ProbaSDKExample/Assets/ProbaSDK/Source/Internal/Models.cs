using System;
using System.Collections.Generic;

namespace ProbaSDK.Internal
{
	[Serializable]
	internal class Experiment
	{
		public string key;
		public string value;
		public string optionId;
	}
	
	[Serializable]
	internal class Meta
	{
		public bool debug;
	}
	
	[Serializable]
	internal class FetchResult
	{
		public Experiment[] experiments;

		public Meta meta;
	}
	
	[Serializable]
	internal class DebugFetchResult
	{
		public CompositeExperiment[] experiments;
	}
	
	[Serializable]
	internal class CompositeExperiment
	{
		public string name;
		public string key;
		public string status;
		public List<ExperimentOption> options;
	}
	
	[Serializable]
	internal class ExperimentOption
	{
		public string value;
		public string description;
	}
}