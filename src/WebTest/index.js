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
  let target = document.getElementById('fl-output');
  target.src = GetWebUrl('/default.html');

  let instrs = document.getElementById('instrs-view');
  instrs.src = GetUrl('/fl-online/instructions');

  document.getElementById('fl-script').addEventListener('keydown', function(e) {
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

function SetDebugQuery(target, input)
{
	let a = document.getElementById(target);
  a.innerHTML=input;
}

function RunScript()
{
  let source = document.getElementById('fl-script').value;
  let target = document.getElementById('fl-output');
  let width = document.getElementById('xpx').value;
  let height = document.getElementById('ypx').value;
  
  let queryUrl = GetUrl('/fl-online/run').concat(GetParameter('?source=', source), GetParameter('&width=', width), GetParameter('&height=', height));
  SetDebugQuery('dbg', queryUrl);
  target.src = queryUrl;
}

function SearchInstructions()
{
	let target = document.getElementById('instrs-view');
	let query = document.getElementById('search_instr').value;
	let queryUrl = GetUrl('/fl-online/instructions').concat(GetParameter('?filter=', query));
	SetDebugQuery('dbg', queryUrl);
	target.src = queryUrl;
}