(function($){

  $.fn.newsScroll = function(options) {  

    var settings = {
      'interval' : 60,
	  'direction' : 1
    };
	
	if (options) { 
        $.extend(settings, options);
    };
    
	return $(this).each(function() 
    {
        var scroller = $(this);
        var list = scroller.children('ul');

        var listH = list.height();
        var scrollerH = scroller.height();
        list.css({
            marginTop: scrollerH,
            marginBottom: scrollerH
        });
        scroller.css({
            overflow: 'hidden'
        });

        var isOver = false;
        scroller.bind('mouseenter mouseleave', function(e) {
            isOver = (e.type == 'mouseenter');
        });

        var scroll;
		var totalH = (listH + scrollerH);
		
		if(settings.direction === 1) {
			scroll = 0;
		} else {
			scroll = scrollerH;
		}
		
        setInterval(function() 
        {
            if (isOver) return;
            scroll++;
			var newTop;
			if(scroll > totalH){
				scroll = scroll % totalH;
			}
			if(settings.direction === 1) {
				newTop = scroll % totalH;
			} else {
				newTop = totalH - scroll % totalH;
			}
			if(newTop === totalH - parseInt(scrollerH/10)) {
			  newTop += parseInt(scrollerH/3);
			  scroll += parseInt(scrollerH/3);
			}
            
            scroller.scrollTop(newTop);
        }, settings.interval);

    });
  };
})(jQuery);
