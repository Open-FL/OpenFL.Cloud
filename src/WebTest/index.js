let url = '';
let weburl = '';

function GetWebUrl(path)
{
  return weburl.concat(path);
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

function GetUrl(path)
{
  return url.concat(path);
}

function OnLoad()
{

  url = httpGet('endpoint_url.txt').responseText;
  weburl = httpGet('web_url.txt').responseText;

  console.log("Web URL: ".concat(weburl));
  console.log("API URL: ".concat(url));

  let target = document.getElementById('fl-build-script');
  target.innerHTML = '<img id="fl-output" src="imgs/OpenFL.png"></img>';


  SearchInstructions();

  document.getElementById('fl-script-input').addEventListener('keydown', function(e) {
  if (e.key == 'Tab') {
    e.preventDefault();
    var start = this.selectionStart;
    var end = this.selectionEnd;

    // set textarea value to: text before caret + tab + text after caret
    this.value = this.value.substring(0, start) +
      "\t" + this.value.substring(end);

    // put caret at right position again
    this.selectionStart =
      this.selectionEnd = start + 1;
  }
});
}

function GetParameter(name, value)
{
  return name.concat(encodeURIComponent(value));
}

function RunScript()
{
  let source = document.getElementById('fl-script-input').value;
  let target = document.getElementById('fl-build-script');
  let width = 256;
  let height = 256;
  
  let queryUrl = GetUrl('/fl-online/run').concat(GetParameter('?source=', source), GetParameter('&width=', width), GetParameter('&height=', height));
  
  httpGetAsync(queryUrl, handleRunResponse);
  target.innerHTML='<img id="fl-output" src="imgs/loading.gif"></img>'
}

function SearchInstructions()
{
	let query = document.getElementById('instr_search').value;
	let queryUrl = GetUrl('/fl-online/instructions').concat(GetParameter('?filter=', query));
  let instrs = document.getElementById('instruction-view');

  httpGetAsync(queryUrl, handleInstructionResponse);
  
  instrs.innerHTML='<h3>Loading Instructions</h3>';
}

function handleRunResponse(response)
{
  if(response.readyState != 4)return;
  let target = document.getElementById('fl-build-script');
  if(response.status==200)
  {
    let result = JSON.parse(response.responseText);
    let img = 'data:image/png;base64,'.concat(result.result);
    let htmlContent = '<img id="fl-output" src="'.concat(img, '"></img>')
    target.innerHTML=htmlContent;
  }
  else{
    let result = JSON.parse(response.responseText);
    target.innerHTML=result.message;
  }
}

function handleInstructionResponse(response)
{
  if(response.readyState != 4)return;
  let instrs = document.getElementById('instruction-view');
  let content = '<ol id="instruction-list"><li id=instruction-element><h3 id=instruction-name>"API ERROR"</h3></li></ol>';
  if(response.status==200 && response.responseText != "")
  {
    content='<ol id="instruction-list">';
    let result = JSON.parse(response.responseText);
    for (var i = 0; i < result.instructions.length; i++) {
      let instritem = result.instructions[i];
      content = content.concat('<li id="instruction-element"><h3 id="instruction-name">', instritem.name, '</h3><div>Argument Types: ', instritem.params.replaceAll('|', ' '),'</div><div id="instruction-desc">', instritem.desc.replace(/(?:\r\n|\r|\n)/g, '<br>'), '</div></li>');
    }
    content = content.concat('</ol>')
  }
  else{
    console.log("Instruction Query Failed:");
    console.log(response.responseText);
  }

  instrs.innerHTML=content;
}