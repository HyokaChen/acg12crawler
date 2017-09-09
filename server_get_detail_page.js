"use strict";
var port = 8088;
var server = require('webserver').create();

//服务端监听
server.listen(8088, function (request, response) {
	//传入的参数有待更改，目前为
	//{"Key":"https://acg12.com/200340/", "Value":"Tappable-inactive animated fadeIn"}的json字符窜
	//第一个参数为详情页，第二个为下载按钮的Dom
	var data = JSON.parse(request.postRaw);
	var url = data.Key.toString();
	var dom = data.Value.toString();
	var code = 0;
	var page = require('webpage').create();
	//初始化headers
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
		'Keep-Alive': 'timeout=20, max=100'
	};
	//根据Phantomjs的官网，这个回调在打开新标签页会触发
	page.onPageCreated = function(newPage) {
		//console.log('A new child page was created! Its requested URL is not yet available, though.');
		newPage.onLoadFinished = function(status) {
			console.log('A child page is Loaded: ' + newPage.url);
			//newPage.render('newPage.png');
			response.write(newPage.url);
			response.statusCode = code;
			response.close(); //写入返回给.net端的响应内容。
		};
	};
	//让Phantomjs帮助我们去请求页面
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
				//执行JavaScript代码，类似于在浏览器Console中执行JavaScript
				page.evaluate(function(dom) {
					console.log(dom);
					var btnList = document.getElementsByClassName(dom);
					if(btnList.length > 0){
						var btn = document.getElementsByClassName(dom)[1]; // 获取下载按钮
						btn.click(); //点击下载按钮，打开新标签页，触发page.onPageCreated回调函数。
					}
				}, dom);		
			}, 7000);
        }
    });
	//根据Phantomjs的官网，这个回调主要应对执行evaluate函数内部的console.log输出，因为两个环境是隔离的。
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
 page1.open('http://localhost:8089/', 'POST', 'https://acg12.com/199901', function (status) {
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
