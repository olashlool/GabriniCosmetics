var SmartProductTab={producttabdetailsurl:"",containerselector:"",loaderselector:"",loadwait:!0,localized_data:!1,init:function(n,t,i,r){this.producttabdetailsurl=n;this.containerselector=t;this.loaderselector=i;this.localized_data=r;this.loadwait=!0;SmartProductTab.check_producttabs();$(window).scroll(function(){SmartProductTab.loadwait||SmartProductTab.check_producttabs()})},check_producttabs:function(){$(SmartProductTab.containerselector+'[data-loaded!="true"]').each(function(){var n=$(this),t;SmartProductTab.chek_element_on_screen(n)&&(n.data("loading")||(n.attr("data-loading",!0),t=n.data("producttabid"),SmartProductTab.load_producttab_details(t)))});SmartProductTab.loadwait=!1},chek_element_on_screen:function(n){var t=$(window).scrollTop(),r=t+$(window).height(),i=n.offset().top,u=i+n.height();return u<=r&&u>=t||i<=r&&i>=t},load_producttab_details:function(n){$.ajax({cache:!1,type:"POST",data:{productTabId:n},url:SmartProductTab.producttabdetailsurl,success:function(t){var i=$(SmartProductTab.containerselector+'[data-producttabid="'+n+'"]');t.result?i.html(t.html):(i.parent().remove(),i.remove(),i.html(SmartProductTab.localized_data.SmartProductTabFailure));i.attr("data-loaded",!0);i.removeClass("producttab-container");i.removeAttr("data-loading");$(".product-tab-item .product-item").each(function(){var n=$(this).find(".picture .buttons"),t=$(this).find(".add-info");n.appendTo(t)})},error:SmartProductTab.ajaxFailure})},ajaxFailure:function(){$(SmartProductTab.containerselector).html(SmartProductTab.localized_data.SmartProductTabFailure)}};
