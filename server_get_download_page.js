"use strict";
var port = 8089;
var server = require('webserver').create();
 
server.listen(8089, function (request, response) {
	var data = JSON.parse(request.postRaw);
	var url = data.Key.toString();
	console.log(url);
	var dom = data.Value.toString();
	console.log(dom);
	var code = 200;
	var pwdArray = new Array();
	var result = new Array();
	var page = require('webpage').create();
	page.onInitialized = function() {
	  page.customHeaders = {};
	};
	page.settings.loadImages = false;
	page.customHeaders = {
		"User-Agent": "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.120 Safari/537.36",
		"Referer": url
	};
	response.headers = {
		'Cache': 'no-cache',
		'Content-Type': 'text/plain',
		'Connection': 'Keep-Alive',
		'Keep-Alive': 'timeout=40, max=100'
	};
	page.onPageCreated = function(newPage) {
		//console.log('A new child page was created! Its requested URL is not yet available, though.');
		page.onInitialized = function() {
		  newPage.customHeaders = {};
		};
		newPage.settings.loadImages = false;
		newPage.customHeaders = {
			"User-Agent": "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.120 Safari/537.36"
		};
		//newPage.viewportSize = { width: 1920, height: 1080 };
		newPage.onLoadFinished = function(status) {
			//console.log('A child page is Loaded: ' + newPage.url);
			//newPage.render('newPage.png', {format: 'png', quality: '100'});
			//console.log(pwdArray.length);
			if(pwdArray.length > 0){
				//console.log("enter");
				var temp = {"url": newPage.url.toString(), "password": pwdArray.pop().toString()};
				console.log(JSON.stringify(temp));
				result.push(temp);
			}
		};
	};
	page.open(url, function (status) {
		console.log("----" + status);
        if (status !== 'success') {
            code = 400;
            response.write('4XX');
			response.statusCode = code;
			response.close();
        } else {
            code = 200;
			window.setTimeout(function (){
				//var dom = dom;
				pwdArray = page.evaluate(function(dom) {
					console.log(dom);
					var pwdArray = new Array();
					var btnList = document.getElementsByClassName(dom); // 百度云链接
					for(var i = 0; i < btnList.length;i ++ ){
						//猜测所有节点都有密码
						var temp = document.getElementById("downloadPwd-" + i);
						if(temp != undefined){
							//console.log("****" + temp.value);
							pwdArray.push(temp.value);
						}else{
							//console.log("****null");
							pwdArray.push("null");
						}
					}
					for(var i = 0; i < btnList.length;i ++ ){
						//console.log("click");
						btnList[i].click();
					}
					return pwdArray;
				}, dom);
			}, 6000);
        }
    });
	window.setTimeout(function(){
		var rs = JSON.stringify(result)
		console.log(rs);
		response.write(rs);
		response.statusCode = code;
		response.close();
	}, 20000);
	/*response.write('are you ok?');
	response.statusCode = code;
	response.close();*/
	page.onConsoleMessage = function(msg, lineNum, sourceId) {
	  console.log("$$$$$" + msg);
	};
	page.onError = function(msg, trace) {
	   var msgStack = ['PHANTOM ERROR: ' + msg];
	   if (trace && trace.length) {
		 msgStack.push('TRACE:');
		 trace.forEach(function(t) {
		   msgStack.push(' -> ' + (t.file || t.sourceURL) + ': ' + t.line + (t.function ? ' (in function ' + t.function +')' : ''));
		 });
	   }
	   console.log(msgStack.join('\n'));
	   phantom.exit(1);
	 };
});
phantom.onError = function(msg, trace) {
   var msgStack = ['PHANTOM ERROR: ' + msg];
   if (trace && trace.length) {
     msgStack.push('TRACE:');
     trace.forEach(function(t) {
       msgStack.push(' -> ' + (t.file || t.sourceURL) + ': ' + t.line + (t.function ? ' (in function ' + t.function +')' : ''));
     });
   }
   console.log(msgStack.join('\n'));
   phantom.exit(1);
 };
 /*var page1 = require('webpage').create();
 page1.open('http://localhost:8089/', 'POST', '{"Key": "https://acg12.com/199901", "Value": "Tappable-inactive btn btn-success btn-block"}', function (status) {
    if (status !== 'success') {
        console.log('Unable to post!');
    } else {
        console.log(page.content);
    }
	page1.close();
    phantom.exit();
});*/

/* page.open('https://acg12.com/199901', function (status) {
    if (status !== 'success') {
        console.log('Unable to post!');
    } else {
        console.log(page.content);
    }
	page.close();
    phantom.exit();
});*/
