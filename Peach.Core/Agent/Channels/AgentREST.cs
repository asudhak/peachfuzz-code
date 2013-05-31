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
//   Adam Cecchetti (adam@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft; 
using Newtonsoft.Json; 
using NLog;
using Newtonsoft.Json.Linq;

namespace Peach.Core.Agent.Channels
{
    [Serializable]
    public class AgentMessageREST
    {
        public string Method = null;
        public object[] Arguments = null;
        public SerializableDictionary<string, Variant> Parameters = null;
    }

		[Serializable]
		public class JSONResponse
		{
      public string Status { get; set;  }
      public string Data { get; set; }
      public Dictionary<string,object> Results { get; set; }
		}

    [Agent("http", true)]
    public class AgentClientREST: AgentClient
    {
        static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static string _url; 		
        public AgentClientREST(string name, string uri, string password)
        {
            _url = uri + "/Agent"; 
						if(string.IsNullOrEmpty(uri))
                            throw new PeachException("Uri for rest agent cannot be empty");
        }

        public override bool SupportedProtocol(string protocol)
        {
            logger.Trace("SupportedProtocol");
            OnSupportedProtocolEvent(protocol);

            protocol = protocol.ToLower();
            if (protocol == "http")
                return true;

            return false;
        }

				private JSONResponse ParseResponse(string json)
				{
						if(String.IsNullOrEmpty(json))
								throw new PeachException("Agent Response Empty");

            try
            {
                return JsonConvert.DeserializeObject<JSONResponse>(json);
            }
            catch(Exception e)
            {
               throw new PeachException("Failed to deserialize JSON response from Agent", e); 
            }
				}

        public override void AgentConnect(string name, string url, string password)
        {
            logger.Trace("AgentConnect");
            OnAgentConnectEvent(name, url, password);

            Send("AgentConnect");
        }

        public override void AgentDisconnect()
        {
            logger.Trace("AgentDisconnect");
            OnAgentDisconnectEvent();

            Send("AgentDisconnect");
        }

        public override Publisher CreatePublisher(string cls, SerializableDictionary<string, Variant> args)
        {
            logger.Trace("CreatePublisher: {0}", cls);
            OnCreatePublisherEvent(cls, args);
            throw new NotImplementedException();
        }

        public override void StartMonitor(string name, string cls, SerializableDictionary<string, Variant> args)
        {
            logger.Trace("StartMonitor: {0}, {1}", name, cls);
            OnStartMonitorEvent(name, cls, args);
            Send("StartMonitor?name=" + name + "&cls=" + cls, args); 
        }

        public override void StopMonitor(string name)
        {
            logger.Trace("AgentConnect: {0}", name);
            OnStopMonitorEvent(name);
            Send("StopMonitor?name=" +name);
        }

        public override void StopAllMonitors()
        {
            logger.Trace("StopAllMonitors");
            OnStopAllMonitorsEvent();
            Send("StopAllMonitors");
        }

        public override void SessionStarting()
        {
            logger.Trace("SessionStarting");
            OnSessionStartingEvent();
            Send("SessionStarting");
        }

        public override void SessionFinished()
        {
            logger.Trace("SessionFinished");
            OnSessionFinishedEvent();
            Send("SessionFinished");
        }

        public override void IterationStarting(uint iterationCount, bool isReproduction)
        {
            logger.Trace("IterationStarting: {0}, {1}", iterationCount, isReproduction);
            OnIterationStartingEvent(iterationCount, isReproduction);
            Send("IterationStarting?iterationCount=" + iterationCount.ToString() +"&" + "isReproduction=" + isReproduction.ToString());
        }

        public override bool IterationFinished()
        {
            logger.Trace("IterationFinished");
            OnIterationFinishedEvent();
						string json = Send("IterationFinished");
            JSONResponse response = ParseResponse(json); 
            return Convert.ToBoolean(response.Status); 
        }

        public override bool DetectedFault()
        {
            logger.Trace("DetectedFault");
            OnDetectedFaultEvent();
						string json = Send("DetectedFault");
						JSONResponse response = ParseResponse(json); 
            return Convert.ToBoolean(response.Status); 
        }

        public override Fault[] GetMonitorData()
        {
            logger.Trace("GetMonitorData");
            OnGetMonitorDataEvent();
            Fault fault = new Fault();
            try
            {
							string json = Send("GetMonitorData");
							JSONResponse response = ParseResponse(json); 
              fault = JsonConvert.DeserializeObject<Fault>(response.Results["Fault"].ToString());  
            }
						catch(Exception e)
						{
						   throw new PeachException("Failed to get Monitor Data", e);
						}

            return new Fault[]{fault}; 
        }

        public override bool MustStop()
        {
            logger.Trace("MustStop");
            OnMustStopEvent();
						string json = Send("MustStop");
						JSONResponse response = ParseResponse(json); 
            return Convert.ToBoolean(response.Status); 
        }

        public override Variant Message(string name, Variant data)
        {
            logger.Trace("Message: {0}", name);
            OnMessageEvent(name, data);
            throw new NotImplementedException(); 
        }

				public string Send(string query)
				{
				    return Send(query, ""); 
				}

        public string Send(string query, SerializableDictionary<string, Variant> args) 
        {
						StringBuilder stringBuilder = new StringBuilder();
						StringWriter stringWriter = new StringWriter(stringBuilder);


						Dictionary<string,string> newArg = new Dictionary<string, string>();

						foreach(string key in args.Keys)
						{
						    newArg[key] = args[key].ToString(); 
						}

						using(JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
						{
                jsonWriter.WriteStartObject();
						    jsonWriter.WritePropertyName("args");
								jsonWriter.WriteValue(JsonConvert.SerializeObject(newArg));
								jsonWriter.WriteEndObject();
                return Send(query, stringBuilder.ToString() ); 
						}
        }

				public string Send(string query, string json )
				{
				    try
				    {
					    var httpWebRequest = (HttpWebRequest) WebRequest.Create(_url + "/" + query); 
							httpWebRequest.ContentType = "text/json";
              if (String.IsNullOrEmpty(json))
              {
                   httpWebRequest.Method = "GET";
              }
              else
              { 
                  httpWebRequest.Method = "POST";
                  using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                  {
                      streamWriter.Write(json);
                  }
              }
				        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

							if(httpResponse.GetResponseStream() != null)
							{
							    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
							    {
							        return streamReader.ReadToEnd();
							    }
							}
							else
							{
							    return ""; 
							}
				    }
				    catch (Exception e)
				    {
				        throw new PeachException("Failure communicating with REST Agent", e);
				    }
				}
    }
}
// end
