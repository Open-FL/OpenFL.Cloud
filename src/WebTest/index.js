let url = '';
let weburl = '';

function GetWebUrl(path)
{
  return weburl.concat(path);
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

  let target = document.getElementById('fl-build-script-iframe');
  target.src = GetWebUrl('/default.html');


  let instrs = document.getElementById('instruction-view');
  instrs.innerHTML=httpGet(GetUrl('/fl-online/instructions')).responseText;

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
  let target = document.getElementById('fl-build-script-iframe');
  let width = 256;
  let height = 256;
  
  let queryUrl = GetUrl('/fl-online/run').concat(GetParameter('?source=', source), GetParameter('&width=', width), GetParameter('&height=', height));
  target.src = queryUrl;
}

function SearchInstructions()
{
	let query = document.getElementById('instr_search').value;
	let queryUrl = GetUrl('/fl-online/instructions').concat(GetParameter('?filter=', query));
  let instrs = document.getElementById('instruction-view');
  instrs.innerHTML=httpGet(queryUrl).responseText;
}