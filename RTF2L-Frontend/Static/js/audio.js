 window.addEventListener("load", setListeners());
    /* this line dinamically adds a method to the String object 
     * working on the prototype of the object
     */
    String.prototype.endsWith = function(suffix) {
    return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
    var audio = new Audio();
    function setListeners(){
      var index = 0;
        var buttons = document.getElementsByClassName("soundBt");
        for (index = 0; index < buttons.length; index++){
            var button = buttons[index];
            button.addEventListener("click", function(){
                var buttons = document.getElementsByClassName("soundBt");
                var index = 0;
                for (index = 0; index < buttons.length; index++){
                    buttons[index].style.background = "url(images/play.jpg) no-repeat";
                }
                if(audio.paused){
                    var fileToPlay = this.getAttribute("name");
                    audio.src = fileToPlay;
                    audio.play();
                    this.style.background = "url(images/pause.jpg) no-repeat";
                } 
                else{ 
                    audio.pause();
                    this.style.background = "url(images/play.jpg) no-repeat";
                }
            });
        }
    }
    audio.addEventListener("ended", function(){
        var buttons = document.getElementsByClassName("soundBt");
        var index = 0;
        for (index = 0; index < buttons.length; index++){
            var button = buttons[index];
            var buttonName = button.getAttribute("name");
            var audiosrc = audio.src;
            if (audio.src.endsWith(buttonName)){
                button.style.background = "url(images/play.jpg) no-repeat";
                return;
            }
        }
    });