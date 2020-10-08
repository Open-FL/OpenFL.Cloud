let url = 'http://localhost:8080';
let weburl = 'http://localhost';

function GetWebUrl(path)
{
  return weburl.concat(path);
}

function GetUrl(path)
{
  return url.concat(path);
}

function OnLoad()
{
  let target = document.getElementById('fl-build-script-iframe');
  target.src = GetWebUrl('/default.html');

  let instrs = document.getElementById('instruction-view');
  instrs.src = GetUrl('/fl-online/instructions');

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

function SetDebugQuery(input)
{
	console.log(input)
}

function RunScript()
{
  let source = document.getElementById('fl-script-input').value;
  let target = document.getElementById('fl-build-script-iframe');
  let width = 256;
  let height = 256;
  
  let queryUrl = GetUrl('/fl-online/run').concat(GetParameter('?source=', source), GetParameter('&width=', width), GetParameter('&height=', height));
  SetDebugQuery(queryUrl);
  target.src = queryUrl;
}

function SearchInstructions()
{
	let target = document.getElementById('instruction-view');
	let query = document.getElementById('instr_search').value;
	let queryUrl = GetUrl('/fl-online/instructions').concat(GetParameter('?filter=', query));
	SetDebugQuery(queryUrl);
	target.src = queryUrl;
}