using System;
using System.Collections.Generic;
using System.Linq;
using Peach.Core.IO;
using Peach.Core.Cracker;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Peach.Core.Dom
{
	[Serializable]
	public class ActionData : INamed
	{
		/// <summary>
		/// Currently unused.  Exists for schema generation.
		/// </summary>
		[XmlElement]
		[DefaultValue(null)]
		public Peach.Core.Xsd.DataModel schemaModel { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ActionData()
		{
			dataSets = new NamedCollection<DataSet>("Data");
		}

		/// <summary>
		/// The action that this belongs to
		/// </summary>
		public Action action { get; set; }

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
		/// A cached copy of the clean data model.  Has fields/data applied
		/// when applicable.
		/// </summary>
		private DataModel originalDataModel { get; set; }

		/// <summary>
		/// The name of this record.
		/// </summary>
		/// <remarks>
		/// Non-null when actions have multiple data models
		/// (Action.Call) and null otherwise (Input/Output/SetProperty/GetProperty).
		/// </remarks>
		[XmlAttribute]
		[DefaultValue(null)]
		public string name { get; protected set; }

		/// <summary>
		/// Full name of this record when viewed as input data
		/// </summary>
		public virtual string inputName
		{
			get
			{
				if (name == null)
					return string.Format("{0}.{1}", action.parent.name, action.name);
				else
					return string.Format("{0}.{1}.{2}", action.parent.name, action.name, name);
			}
		}

		/// <summary>
		/// Full name of this record when viewed as output data
		/// </summary>
		public virtual string outputName
		{
			get
			{
				if (name == null)
					return string.Format("{0}.{1}", action.parent.name, action.name);
				else
					return string.Format("{0}.{1}.{2}", action.parent.name, action.name, name);
			}
		}

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
			selectedData = option;

			UpdateToOriginalDataModel();
		}

		/// <summary>
		/// Crack the BitStream into the data model.
		/// Will automatically update to the original model
		/// prior to cracking.  Used by InOut action parameters.
		/// </summary>
		/// <param name="bs"></param>
		public void Crack(BitStream bs)
		{
			DataModel copy;

			if (selectedData != null)
			{
				// If we have selected data, we need to have the un-cracked data model
				System.Diagnostics.Debug.Assert(sourceDataModel != null);
				copy = sourceDataModel.Clone() as DataModel;
			}
			else
			{
				// If we have never selected data, originalDataModel is fine
				System.Diagnostics.Debug.Assert(sourceDataModel == null);
				copy = originalDataModel.Clone() as DataModel;
			}

			var cracker = new DataCracker();
			cracker.CrackData(copy, bs);

			dataModel = copy;
			dataModel.action = action;
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
