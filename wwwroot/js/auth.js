$(document).ready(function() {
    $("#apiKeyModal").modal("show");

    // Переключение между вводом API-ключа и учетными данными
    $('#useApiKey').change(function() {
        if($(this).is(':checked')) {
            $('#apiKeyInput').show();
            $('#usernameInput').hide();
            $('#passwordInput').hide();
        } else {
            $('#apiKeyInput').hide();
            $('#usernameInput').show();
            $('#passwordInput').show();
        }
    });
});

$("#submitButton").click(function() {
    if($('#useApiKey').is(':checked')) {
        $.post("/Login/ValidateApiKey", { apiKey: $("#apiKeyInput").val() }, function(data) {
            if (data.isValid) {
                // Если ключ API действителен, обновляем страницу
                location.reload();
            } else {
                // Если ключ API недействителен, отображаем сообщение об ошибке
                $("#error").show();
            }
        });
    } else {
        $.post("/Login/IsValidUserCredentials",
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
    }
});
