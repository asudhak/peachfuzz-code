
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;

using Peach.Core;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Dom
{
	public delegate void StateModelStartingEventHandler(StateModel model);
	public delegate void StateModelFinishedEventHandler(StateModel model);

    //[Serializable]
	public class StateModel : INamed, IPitSerializable
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public string _name = null;
		public object parent;
		protected State _initialState = null;
		public List<Action> dataActions = new List<Action>();

		/// <summary>
		/// All states in state model.
		/// </summary>
		public Dictionary<string, State> states = new Dictionary<string, State>();

		public string name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// The initial state to run when state machine executes.
		/// </summary>
		public State initialState
		{
			get
			{
				return _initialState;
			}

			set
			{
				_initialState = value;
			}
		}

		/// <summary>
		/// StateModel is starting to execute.
		/// </summary>
		public static event StateModelStartingEventHandler Starting;
		/// <summary>
		/// StateModel has finished executing.
		/// </summary>
		public static event StateModelFinishedEventHandler Finished;

		protected virtual void OnStarting()
		{
			if (Starting != null)
				Starting(this);
		}

		protected virtual void OnFinished()
		{
			if (Finished != null)
				Finished(this);
		}

		/// <summary>
		/// Start running the State Machine
		/// </summary>
		/// <remarks>
		/// This will start the initial State.
		/// </remarks>
		/// <param name="context"></param>
		public void Run(RunContext context)
		{
			try
			{
				OnStarting();

				foreach (Publisher publisher in context.test.publishers.Values)
					publisher.Iteration = context.test.strategy.Iteration;

				dataActions.Clear();

				// Prior to starting our state model, on iteration #1 lets
				// locate all data sets and load our initial data.
				//
				// Additionally this is were we setup origionalDataModel.
				//
				if (context.needDataModel)
				{
					context.needDataModel = false;

					foreach (State state in states.Values)
					{
						foreach (Action action in state.actions)
						{
							if (action.dataModel != null && action.dataSet != null && action.dataSet.Datas.Count > 0)
							{
								Data data = action.dataSet.Datas[0];
								string fileName = null;

								if (data.DataType == DataType.File)
									fileName = data.FileName;
								else if (data.DataType == DataType.Files)
									fileName = data.Files[0];
								else if (data.fields.Count > 0)
								{
									data.ApplyFields(action.dataModel);
								}

								//Peach.Core.Cracker.DataCracker.ClearRelationsRecursively(action.dataModel);
								//Peach.Core.Analyzers.PitParser.displayDataModel(action.dataModel);

								if (fileName != null)
								{
									try
									{
										Cracker.DataCracker cracker = new Cracker.DataCracker();
										cracker.CrackData(action.dataModel,
											new BitStream(File.OpenRead(fileName)));
									}
									catch (Cracker.CrackingFailure ex)
									{
										throw new PeachException("Error, failed to crack \"" + fileName + "\" into \"" + action.dataModel.fullName + "\": " + ex.ToString());
									}
								}

								//Peach.Core.Cracker.DataCracker.ClearRelationsRecursively(action.dataModel);
								//Peach.Core.Analyzers.PitParser.displayDataModel(action.dataModel);

								var value = action.dataModel.Value;

								// Update our origional copy to have data!
								action.origionalDataModel = ObjectCopier.Clone<DataModel>(action.dataModel);
								Peach.Core.Cracker.DataCracker.ClearRelationsRecursively(action.origionalDataModel);
							}
							else if (action.dataModel != null)
							{
								var value = action.dataModel.Value;

								// Update our origional copy to have data!
								action.origionalDataModel = ObjectCopier.Clone<DataModel>(action.dataModel);
								Peach.Core.Cracker.DataCracker.ClearRelationsRecursively(action.origionalDataModel);
							}
							else if (action.parameters.Count > 0)
							{
								foreach (ActionParameter param in action.parameters)
								{
									if (param.dataModel != null && param.data != null)
									{
										Data data = param.data as Data;
										string fileName = null;

										if (data.DataType == DataType.File)
											fileName = data.FileName;
										else if (data.DataType == DataType.Files)
											fileName = data.Files[0];
										else
											data.ApplyFields(param.dataModel);

										if (fileName != null)
										{
											try
											{
												Cracker.DataCracker cracker = new Cracker.DataCracker();
												cracker.CrackData(param.dataModel,
													new BitStream(File.OpenRead(fileName)));
											}
											catch (Cracker.CrackingFailure ex)
											{
												throw new PeachException("Error, failed to crack \"" + fileName + "\" into \"" + action.dataModel.fullName + "\": " + ex.ToString());
											}
										}
									}

									// Invalidate model and produce value
									var value = param.dataModel.Value;

									// Update our origional copy to have data!
									param.origionalDataModel = ObjectCopier.Clone<DataModel>(param.dataModel);
									Peach.Core.Cracker.DataCracker.ClearRelationsRecursively(param.origionalDataModel);
								}
							}
						}
					}
				}

				// Update all data model to clones of origionalDataModel
				// before we start down the state path.
				foreach (State state in states.Values)
				{
					foreach (Action action in state.actions)
						action.UpdateToOrigionalDataModel();
				}

				State currentState = _initialState;

				while (true)
				{
					try
					{
						currentState.Run(context);
						break;
					}
					catch (ActionChangeStateException ase)
					{
						currentState = ase.changeToState;
					}
				}
			}
			catch (ActionException)
			{
				// Exit state model!
			}
			finally
			{
				foreach (Publisher publisher in context.test.publishers.Values)
					publisher.close(null);

				OnFinished();
			}
		}

    public XmlNode pitSerialize(XmlDocument doc, XmlNode parent)
    {
      XmlNode node = doc.CreateNode(XmlNodeType.Element, "StateModel", null);

      node.AppendAttribute("name", this.name);
      node.AppendAttribute("initialState", this.initialState.name);


      foreach (State state in states.Values)
      {
        node.AppendChild(state.pitSerialize(doc, node));
      }

      return node;
    }
  }
}

// END
