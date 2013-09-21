
/*
 * Peach WebSocket Client Script
 */

/* Global variables */

var output = null;
var peachFrame = null;
var timer = null;
var interval = null;
var debug = false;
var buffer = "";
var ready = false;
var logDump = false;
var logBuffer = new Array();
var isOpera = typeof window.opera !== 'undefined';
var isChrome = navigator.userAgent.toLowerCase().indexOf('chrome') > -1;

/* End of global variables */

if (typeof(dump) == "undefined")
{
	dump = function(msg) { log(msg); }
	// Avoid recursion
	logDump = false;
}

function init()
{
  output = document.getElementById("output");
  peachFrame = document.getElementById("peachFrame");
  
  document.getElementById("peachFrame").onload = loadedChild;
  document.getElementById("peachFrame").onerror = loadedChild;

  ws = new WebSocket("ws://127.0.0.1:8080/");
  log("Socket: Attempting connection...");
  ws.onopen = function(evt) { onOpen(evt) };
  ws.onclose = function(evt) { onClose(evt) };
  ws.onmessage = function(evt) { onMessage(evt) };
  ws.onerror = function(evt) { onError(evt) };
}

function onOpen(evt)
{
  log("Socket: Open");

  if (isChrome)
  {
    var body = document.getElementsByTagName('body')[0];
    var img = document.createElement('img');
    img.width=300;
    img.height=300;
    img.alt="Here be images";
    img.id="img";
    body.appendChild(img);
    peachFrameDoc = document;
  }
  else
  {
    // Initialize to some empty document
    peachFrame.src = 'data:text/html;charset=utf-8,'  
  	  + escape('<html><head></head><body></body></html>');
    peachFrameDoc = peachFrame.contentDocument;
  }
}

function onClose(evt)
{
  log("Socket: closed");
}

function onError(evt)
{
  log("Socket: error: " + evt.data);
}

function onMessage(evt)
{
  var data = evt.data;
  debugLog("Socked: received: " + data);

  var keepLastChunk = true;
  if (data.substr(-1) == "\n")
  {
    keepLastChunk = false;
  }

  var chunks = data.split("\n");
  var lastChunk;

  // If the last chunk is incomplete, don't process it
  if (keepLastChunk)
  {
    lastChunk = chunks.pop();
  }

  for (i in chunks)
  {
    chunk = chunks[i];

    // First chunk, prepend buffer and reset it
    if (i == 0)
	{
      chunk = buffer + chunk;
      buffer = "";
    }

    // Process only JSON encoded messages, ignore everything else
    if (chunk.substr(0,1) == '{')
	{
      processJSONLine(chunk);
    }
  }

  // If we have an incomplete chunk, keep it in our buffer
  if (keepLastChunk)
  {
    buffer += lastChunk;
  }
}

// when we receive a JSON-encoded testcase from the server, write it to a local
// temp file and notify our test-loading worker (via a custom event)
function processJSONLine(data)
{
  debugLog("JSON received: " + data);
  var resp = JSON.parse(data);
  
  switch (resp.type)
  {
  case "msg":
    var msg = resp.content;
    switch(msg)
	{
      case "ping":
        ws.send('{"msg": "pong"}\n');
        break;
		
//      default:
//        warnLog("Received unknown message from server: " + msg);
    }
    break;
	
  case "template":
    debugLog("Loading template content into iframe content");
	peachFrame.src = 'data:text/html;charset=utf-8,' + resp.content;
	timer = setTimeout("loadedChild()", 5000);

    break;
	
  default:
    warnLog("Malformed or incomplete JSON object received: " + data);
    break;
  }
}

function completedTest()
{
	if (!ready)
	{
	  // request the first testcase
	  debugLog("Ready");
	  ws.send('{"msg": "Client ready"}\n');
	  ready = true;
	}
	else
	{
	  debugLog("Evaluation complete");
	  ws.send('{"msg": "Evaluation complete"}\n');
	}
}

function loadedChild()
{
   clearTimeout(timer);
   debugLog("Child load complete");
   completedTest();
}

/* Logging functions */
function debugLog(message)
{
  if (debug) { log(message); }
}

function warnLog(message)
{
  log(message);
}

function log(message)
{
  var pre = document.createElement("p");
  pre.style.wordWrap = "break-word";
  pre.innerHTML = message;
  output.appendChild(pre);

  logBuffer.push(pre);
  if (logBuffer.length > 100)
  {
	  var oldPre = logBuffer.shift();
	  output.removeChild(oldPre);
  }
  
  if (logDump)
  {
  	dump(message + "\n");
  }
}

/* End of logging functions */

// Add event listener for starting up
window.addEventListener("load", init, false);

// end
