$(document).ready(function (){
    $(".load-issue").click(function (){
        $("#loading").show();
    });
    
});
$(window).on("load",function (){
    $("#loading").hide();
});