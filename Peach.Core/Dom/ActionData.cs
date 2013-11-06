using System;
using System.Collections.Generic;
using System.Linq;

namespace Peach.Core.Dom
{
	[Serializable]
	public class ActionData : INamed
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ActionData()
		{
			dataSets = new NamedCollection<DataSet>();
		}

		/// <summary>
		/// The action that this belongs to
		/// </summary>
		public Action action { get; set; }

		/// <summary>
		/// A cached copy of the clean data model.  Has fields/data applied
		/// when applicable.
		/// </summary>
		public DataModel originalDataModel { get; private set; }

		/// <summary>
		/// The data model we use for input/output when running the state model.
		/// </summary>
		public DataModel dataModel { get; set; }

		/// <summary>
		/// All of the data sets that correspond to the data model
		/// </summary>
		public NamedCollection<DataSet> dataSets { get; private set; }

		/// <summary>
		/// Enumerable view of all Data that corresponds to the data model
		/// </summary>
		public IEnumerable<Data> allData { get { return dataSets.SelectMany(d => d.AsEnumerable()); } }

		/// <summary>
		/// The currently selected Data in use by the model
		/// </summary>
		public Data selectedData { get; private set; }

		/// <summary>
		/// A clean copy of the data model that has never had fields/data applied.
		/// Only set when using dataSets since applying data requires
		/// the clean model.
		/// </summary>
		private DataModel sourceDataModel { get; set; }

		/// <summary>
		/// The name of this record.  Non-null when actions have multiple data models
		/// (Action.Call) and null otherwise (Input/Output/SetProperty/GetProperty).
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Initialize dataModel to its original state.
		/// If this is the first time through and a dataSet exists,
		/// the data will be applied to the model.
		/// </summary>
		public void UpdateToOriginalDataModel()
		{
			System.Diagnostics.Debug.Assert(dataModel != null);
			dataModel.action = null;

			// If is the first time through we need to cache a clean data model
			if (originalDataModel == null)
			{
				// Apply data samples
				var option = allData.FirstOrDefault();
				if (option != null)
				{
					// Cache the model before any cracking has ever occured
					// since we can't crack into an model that has previously
					// been cracked (eg: Placement won't work).
					sourceDataModel = dataModel;
					Apply(option);
				}
				else
				{
					// Evaulate the full dataModel prior to saving as the original
					var val = dataModel.Value;
					System.Diagnostics.Debug.Assert(val != null);

					originalDataModel = dataModel.Clone() as DataModel;
				}
			}
			else
			{
				dataModel = originalDataModel.Clone() as DataModel;
			}

			// If dataOption == null and have 
			dataModel.action = action;
		}

		/// <summary>
		/// Apply data from the dataSet to the data model.
		/// </summary>
		/// <param name="option"></param>
		public void Apply(Data option)
		{
			System.Diagnostics.Debug.Assert(sourceDataModel != null);
			System.Diagnostics.Debug.Assert(allData.Contains(option));

			// Work in a clean copy of the original
			var copy = sourceDataModel.Clone() as DataModel;
			option.Apply(copy);

			// Evaulate the full dataModel prior to saving as the original
			var val = copy.Value;
			System.Diagnostics.Debug.Assert(val != null);

			originalDataModel = copy;

			UpdateToOriginalDataModel();
		}

		/// <summary>
		/// The unique instance name for this action data.
		/// Includes the run count to disambiguate multiple
		/// runs of the action via a re-enterant state.
		/// </summary>
		public string instanceName
		{
			get
			{
				return string.Format("Run_{0}.{1}", action.parent.runCount, modelName);
			}
		}

		/// <summary>
		/// The name of this action data.  Does not include the
		/// run count so the name will be the same across multiple
		/// runs of the action via a re-enterant state.
		/// </summary>
		public string modelName
		{
			get
			{
				if (string.IsNullOrEmpty(name))
					return string.Join(".", action.parent.name, action.name, dataModel.name);
				else
					return string.Join(".", action.parent.name, action.name, name, dataModel.name);
			}
		}
	}
}
