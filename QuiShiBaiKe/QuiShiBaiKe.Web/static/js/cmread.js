// cmRead links

$(function(){

	// url
	var url1 = '<img src="http://wap.cmread.com/iread/wml/p/kswtszq.jsp" alt=" " width="1" height="1"/>';
	var url2 = '<img src="http://wap.cmread.com/iread/wml/p/bjddsy.jsp" alt=" " width="1" height="1"/>';
	var url3 = '<img src="http://wap.cmread.com/iread/wml/p/bjddsy.jsp" alt=" " width="1" height="1"/>';
	
	// read date
	var now = new Date();
	var today = now.toDateString();
	
	// read cookie
	var cmstat = Number($.readCookie('_cmstat'));
	var cmdate = $.readCookie('_cmdate');
	
	// cookie state
	if ( cmdate != today ) {
		$("div.ext-link").html(url1);
		cmstat = 1;
	}
	else{
		switch(cmstat){
		   case 2:
				$("div.ext-link").html(url2);
				break;
		   case 3:
				$("div.ext-link").html(url3);
				break;
		   default:
				$("div.ext-link").html(url1);
		   }	 
	}

	// write cookie
	cmstat = cmstat + 1;
	$.setCookie("_cmstat", cmstat, { duration: 1, path:'/' });
	$.setCookie("_cmdate", today, { duration: 1, path: '/' });

});