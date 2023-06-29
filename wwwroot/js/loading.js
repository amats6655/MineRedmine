$(document).ready(function (){
    console.log("is ready");
    $(".load-issue").click(function (){
        console.log("is clicked");
        $("#loading").show();
        setTimeout(function (){
            window.location.href = "Issues/Index";
        }, 3000);
    });
    $(".submit-key").submit(function (e){
        console.log("form is clicked");
        $("#loading").show();
        setTimeout(function (){
            window.location.href = "Issues/Index";
        });
    });
    
});
$(window).on("load",function (){
    console.log("Window is loaded");
    $("#loading").hide();
});