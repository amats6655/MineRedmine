$(document).ready(function() {
    $("#apiKeyModal").modal("show");
});

$(".authInput").keyup(function(event) {
    if (event.keyCode === 13) {

        $(".load-issue").click();
        $("#submitButton").click();
    }
});

$("#submitButton").click(function() {

        $.post("/Issues/IsValidUserCredentials",
            {
                username: $("#usernameInput").val(),
                password: $("#passwordInput").val()
            },
            function(data) {
                if (data.isValid) {
                    // Если учетные данные пользователя действительны, обновляем страницу
                    location.reload();
                } else {
                    // Если учетные данные пользователя недействительны, отображаем сообщение об ошибке
                    $("#error").show();
                }
            }
        );
});
