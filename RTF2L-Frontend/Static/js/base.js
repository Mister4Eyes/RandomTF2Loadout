function reload() {
	location.reload();
}
function playSound() {
	  var sound = document.getElementById("audio");
	  sound.play()
	  sound.volume = 0.2;
}
function playAnthem() {
	  var sound = document.getElementById("anthem");
	  sound.play()
	  sound.volume = 0.2;
}
function playDog() {
	  var sound = document.getElementById("dog");
	  sound.play()
	  sound.volume = 0.2;
}
function pictureChange()
{
	document.getElementById('theImage').src="https://s-media-cache-ak0.pinimg.com/originals/66/24/e1/6624e12a74dbc8c5cc07f7bcfad2f1f3.jpg";
	var sound = document.getElementById("secret");
	sound.play()
	sound.volume = 0.2;
	
}
function pauseSound(){
	var sound = document.getElementById("audio"); 
	sound.pause()
	sound.currentTime = 0
}
function pauseSecret(){
	var sound = document.getElementById("secret"); 
	sound.pause()
	sound.currentTime = 0
}
function pauseAnthem(){
	var sound = document.getElementById("anthem"); 
	sound.pause()
	sound.currentTime = 0
}
function pauseDog(){
	var sound = document.getElementById("dog"); 
	sound.pause()
	sound.currentTime = 0
}
function fail(){
	alert("Due to one of the editors being a lazy sacks of shit, you just get this. \n");
	var x;
	if (confirm("Want to go back?") == true){
		x = "They either forgot to add the feature in or just lazy to even think of adding it in.";
	}
	else{
		x="Oh...";
	}
	document.getElementById("fail").innerHTML = x;
}