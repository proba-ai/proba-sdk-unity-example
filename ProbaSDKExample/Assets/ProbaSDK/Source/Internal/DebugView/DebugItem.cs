using ProbaSDK.Types;
using UnityEngine;
using UnityEngine.UI;

namespace ProbaSDK.Internal.DebugView
{
	internal class DebugItem: MonoBehaviour
	{
		public Text title;
		public Text desc;
		public Toggle toggle;
		
		public CompositeExperiment Experiment { get; set; }
		public ExperimentOption Item { get; set; }
	}
}