let isrunning = false;
let examplesFile = 'example_links.txt';
let examples = {};
let weburl = '';

function GetExamplesUrl()
{
  return weburl + "/" + examplesFile;
}
function httpGetAsync(theUrl, callback)
{
  var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function() { 
        callback(xmlHttp);
    }
    xmlHttp.open("GET", theUrl, true); // true for asynchronous 
    xmlHttp.send(null);
}

function httpGet(theUrl)
{
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open( "GET", theUrl, false ); // false for synchronous request
    xmlHttp.send( null );
    return xmlHttp;
}

function GetFileName(str)
{
  return str.split('\\').pop().split('/').pop();
}



function escapeHtml(unsafe) {
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/"/g, "&quot;")
         .replace(/'/g, "&#039;");
 }

function LoadExamplesList()
{
  if(examplesFile != '')
  {
    var exampleDD = document.getElementById('examples');
    var list = httpGet(GetExamplesUrl()).responseText.match(/[^\r\n]+/g);;
    for (var i = 0; i < list.length; i++) 
    {
      if(list[i] == '')
      {
        continue;
      }
      var name = GetFileName(list[i]);
      examples[name] = list[i];
      exampleDD.innerHTML = exampleDD.innerHTML + '<option value="'+name+'">'+name+'</option>';
    }
  }
}

function LoadExample()
{
  var exampleDD = document.getElementById('examples');
  var key = exampleDD.options[exampleDD.selectedIndex].value;
  var text = httpGet(examples[key]).responseText;
  var new_file= {id: key, text: escapeHtml(text), syntax: 'openfl', title: key};
      
  editAreaLoader.openFile('fl-script-input', new_file);
}

function OnLoad()
{
  weburl = httpGet('web_url.txt').responseText;
  InitializeFL(weburl, httpGet('endpoint_url.txt').responseText);
  SearchInstructions();
  CreateHighlighting();

  LoadExamplesList();

  let target = document.getElementById('fl-build-script');
  target.innerHTML = '<img id="fl-output" src="imgs/OpenFL.png"></img>';

}

function initCodeArea()
{
  
    editAreaLoader.init({
      id: "fl-script-input" // id of the textarea to transform  
      ,start_highlight: true  // if start with highlight
      ,allow_resize: "no"
      ,allow_toggle: false
      ,word_wrap: true
      ,language: "en"
      ,syntax: "openfl"
    });
}

function GetParameter(name, value)
{
  return name.concat(encodeURIComponent(value));
}

function RunScript()
{
  let source = editAreaLoader.getValue('fl-script-input');
  let target = document.getElementById('fl-build-script');
  let width = 256;
  let height = 256;
  
  RunScriptApi(handleRunResponse, source, width, height);

  ToggleBuildButton(true);
  target.innerHTML='<img id="fl-output" src="imgs/loading.gif"></img>'
}

function ToggleBuildButton(state)
{
  let btn = document.getElementById('fl-build-script-btn');
  btn.disabled = state;
}

function SearchInstructions()
{
	let query = document.getElementById('instr_search').value;

  GetInstructions(query, handleInstructionResponse);
  
  let instrs = document.getElementById('instruction-view');
  instrs.innerHTML='<h3>Loading Instructions</h3>';
}

function handleRunResponse(result)
{
  
    let target = document.getElementById('fl-build-script');
  if(result.status==200)
  {
    let img = 'data:image/png;base64,'.concat(result.result);
    let htmlContent = '<img id="fl-output" src="'.concat(img, '"></img>')
    target.innerHTML=htmlContent;
  }
  else{
    let message = result.message;
    let errorType = result.type;
    let stack=result.stack;

    let htmlContent = '<div id="exception" style: "background: red;"><div id="exception-ex"><h3>';
    if(errorType!= null)
    {
      htmlContent = htmlContent + "Run Endpoint Query Failed with: " + escapeHtml(errorType) + '</h3>';
    }
    else
    {
      htmlContent = htmlContent + "Run Endpoint Query Failed</h3>";
    }

    if(message != null)
    {
      htmlContent = htmlContent + '<div id="exception-message">Message:<br>' + escapeHtml(message) + '<br></div>';
    }

    if(stack != null)
    {
      htmlContent = htmlContent + '<div id= "exception-stack">Stacktrace:<br>' + escapeHtml(stack) + '<br></div>';
    }

    htmlContent= htmlContent + '</div></div>';

    target.innerHTML=htmlContent;
  }
  ToggleBuildButton(false);
}

function handleInstructionResponse(result)
{

  	let instrs = document.getElementById('instruction-view');
  	let content = '<ol id="instruction-list"><li id=instruction-element><h3 id=instruction-name>"API ERROR"</h3></li></ol>';
	if(result.status != 200)
	{
  		instrs.innerHTML=content;
		return;
	}
  	content='<ol id="instruction-list">';

  	for (var i = 0; i < result.instructions.length; i++) {
    	let instritem = result.instructions[i];
    	content = content.concat('<li id="instruction-element"><h3 id="instruction-name">', instritem.name, '</h3><div>Argument Types: ', instritem.params.replaceAll('|', ' '),'</div><div id="instruction-desc">', instritem.desc.replace(/(?:\r\n|\r|\n)/g, '<br>'), '</div></li>');
  	}

  	content = content.concat('</ol>')

  	instrs.innerHTML=content;
}

let wUrl = '';
let aUrl = '';

function GetWebUrl(path)
{
  return wUrl.concat(path);
}

function GetApiUrl(path)
{
  return aUrl.concat(path);
}

function InitializeFL(webUrl, apiUrl)
{
  wUrl = webUrl;
  aUrl = apiUrl;
}

function RunScriptApi(callback, source, width, height)
{ 
  let queryUrl = GetApiUrl('/fl-online/run').concat(GetParameter('?source=', source), GetParameter('&width=', width), GetParameter('&height=', height));
    
  httpGetAsync(queryUrl, function (response) {
    parseAndCall(callback, response);
  });
}

function parseAndCall(callback, response)
{
  	if(response.readyState != 4)return;

  	if(response.responseText != "")
  	{
    	let result = JSON.parse(response.responseText);
      	result.status = response.status;
      	result.originalText = response.responseText;
    	callback(result);
    }else{
    	callback({
    		status: response.status,
      		originalText: response.responseText
    	});
    }
}

function GetInstructions(searchterm, callback)
{
  let queryUrl = GetApiUrl('/fl-online/instructions').concat(GetParameter('?filter=', searchterm));

    httpGetAsync(queryUrl, function (response) {
      parseAndCall(callback, response);
    });
}


function CreateHighlighting()
{
  GetInstructions("", handleInstructionCallback);
}

function handleInstructionCallback(result)
{
	if(result.status != 200) return;

let syntaxObject = {
    'DISPLAY_NAME' : 'openfl',
    'COMMENT_SINGLE' : {1 : '#'},
    'QUOTEMARKS' : ['"'],
    'KEYWORD_CASE_SENSITIVE' : false,
    'KEYWORDS' : {
      'types' : [ "wfc", "empty", "rnd", "urnd"],
      'keywords' : [
        "texture", "array", "script"
      ],
      'constants' : [ "in", "current" ],
      'statements' : [],
      'specials' : [
        "static", "dynamic", "readonly", "readwrite", "nojump", "nocall", "optimizecall", "once", "init"
      ]
    },
    'DELIMITERS' :[
      '[', ']'
    ],
    'REGEXPS' : {
      'precompiler' : {
        'search' : '()(~[^\r\n]*)()'
        ,'class' : 'precompiler'
        ,'modifiers' : 'g'
        ,'execute' : 'before'
      },
      'definestatements' : {
        'search' : '()(--[^ ]*)()'
        ,'class' : 'definestatements'
        ,'modifiers' : 'g'
        ,'execute' : 'before'
      },
      'functionname' : {
        'search' : '()([A-Za-z][A-Za-z0-9]*:)()'
        ,'class' : 'functionname'
        ,'modifiers' : 'g'
        ,'execute' : 'before'
      }
    },
    'STYLES' : {
      'COMMENTS': 'color: #969896;', 
      'QUOTESMARKS': 'color: #373b41;',
      'KEYWORDS' : {
        'constants' : 'color: #000cb8;',
        'types' : 'color: #167fc9;',
        'statements' : 'color: #167fc9;',
        'keywords' : 'color: #a10b7e;',
        'specials' : 'color: #ff0000;'
      }, 
      'DELIMITERS' : 'color: #c91616;',
      'REGEXPS' : {
        'precompiler' : 'color: #009900;',
        'definestatements' : 'color: #ff0000;',
        'functionname' : 'color: #fc4903;'
      }
    }
  };

  for (var i = 0; i < result.instructions.length; i++) 
  {
      let instritem = result.instructions[i];
      syntaxObject.KEYWORDS.statements.push(instritem.name);
  }

  editAreaLoader.load_syntax["openfl"] = syntaxObject;
  initCodeArea();
}