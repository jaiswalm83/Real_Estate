/*------------------------------------------------------------------------------*/
/*  Home_Page Slider
/*------------------------------------------------------------------------------*/

var revapi2,

  tpj;
  
  jQuery(function() {
    tpj = jQuery;
    if(tpj("#rev_slider_1_1").revolution == undefined){
      revslider_showDoubleJqueryError("#rev_slider_1_1");
    }else{
        revapi2 = tpj("#rev_slider_1_1").show().revolution({
        sliderType:"fullwidth",
        visibilityLevels:"1240,1240,778,480",
        gridwidth:"1240,1240,778,480",
        gridheight:"850,660,450,350",
        spinner:"spinner0",
        perspective:600,
        perspectiveType:"global",
        editorheight:"850,660,450,350",
        responsiveLevels:"1240,1240,778,480",
        progressBar:{disableProgressBar:true},
          navigation: {
            wheelCallDelay:1000,
            onHoverStop:false,
            arrows: {
              enable:true,
              style:"metis",
              hide_onmobile:true,
              hide_under:"1280px",
              left: {
                container:"layergrid",
                v_align:"bottom",
                h_offset:262
              },
              right: {
                container:"layergrid",
                h_align:"left",
                v_align:"bottom",
                h_offset:262,
                v_offset:60
              }
            },
            bullets: {
              enable:true,
              tmp:"",
              style:"inoterior",
              hide_onmobile:true,
              hide_under:"1280px",
              h_align:"left",
              v_offset:30,
              space:0,
              container:"layergrid"
            }
          },
        fallbacks: {
          allowHTML5AutoPlayOnAndroid:true
        },
      });
    }
    
  });



  /* (homepage -2 )*/


  jQuery(function() {
    tpj = jQuery;
    if(tpj("#rev_slider_2_1").revolution == undefined){
      revslider_showDoubleJqueryError("#rev_slider_2_1");
    }else{
        revapi2 = tpj("#rev_slider_2_1").show().revolution({
        sliderType:"standard",
        sliderLayout:"fullwidth",
        visibilityLevels:"1240,1024,778,480",
        gridwidth:"1240,1024,778,480",
        gridheight:"750,750,400,350",
        spinner:"spinner0",
        perspective:600,
        perspectiveType:"local",
        editorheight:"750,750,400,350",
        responsiveLevels:"1240,1024,778,480",
        disableProgressBar:true,
          navigation: {
            onHoverStop:false,
            arrows: {
              enable:true,
              style:"uranus",
              hide_onmobile:true,
              hide_under:778,
              left: {
                h_offset:30
              },
              right: {
                h_offset:30
              }
            }
          },
        fallbacks: {
          allowHTML5AutoPlayOnAndroid:true
        },
      });
    }
    
  });


  /* (homepage -3 )*/
  jQuery(function() {
    tpj = jQuery;

    if(tpj("#rev_slider_3_1").revolution == undefined){
        revslider_showDoubleJqueryError("#rev_slider_3_1");
        
      }else{

        revapi2 = tpj("#rev_slider_3_1").show().revolution({
        sliderType:"standard",
        visibilityLevels:"1240,1240,778,480",
        gridwidth:"1240,1240,778,480",
        gridheight:"750,750,450,350",
        spinner:"spinner0",
        perspective:600,
        perspectiveType:"local",
        editorheight:"750,750,450,350",
        responsiveLevels:"1240,1240,778,480",
        disableProgressBar:true,
          navigation: {
            onHoverStop:false,
            arrows: {
              enable:true,
              style:"uranus",
              hide_onmobile:true,
              hide_under:778,
              left: {
                h_offset:30
              },
              right: {
                h_offset:30
              }
            }
          },
        fallbacks: {
          allowHTML5AutoPlayOnAndroid:true
        },
      });
    }
  });


  /* (homepage -3 )*/
  jQuery(function() {
    tpj = jQuery;

    if(tpj("#rev_slider_4_1").revolution == undefined){
        revslider_showDoubleJqueryError("#rev_slider_4_1");
        
      }else{

        revapi2 = tpj("#rev_slider_4_1").show().revolution({
        sliderType:"standard",
        visibilityLevels:"1240,1200,778,480",
        gridwidth:"1240,1200,778,480",
        gridheight:"830,700,450,350",
        spinner:"spinner0",
        perspective:600,
        perspectiveType:"local",
        editorheight:"830,768,450,350",
        responsiveLevels:"1240,1200,778,480",
        disableProgressBar:true,
          navigation: {
            onHoverStop:false,
            arrows: {
              enable:true,
              style:"uranus",
              hide_onmobile:true,
              hide_under:778,
              left: {
                h_offset:30
              },
              right: {
                h_offset:30
              }
            }
          },
        fallbacks: {
          allowHTML5AutoPlayOnAndroid:true
        },
      });
    }
  });