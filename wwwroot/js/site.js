$(document).ready(function() {
    // Открытие модального окна
    $("#openFilterPopup").click(function() {
        $("#filterPopup").fadeIn();
    });

    // Закрытие модального окна
    $("#closeFilterPopup").click(function() {
        $("#filterPopup").fadeOut();
    });
});

