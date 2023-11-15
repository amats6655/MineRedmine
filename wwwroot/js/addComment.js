$(document).ready(function() {
    $('#commentForm').on('submit', function(event) {
        event.preventDefault();

        var isValid = true;
        var commentText = $('#commentText');
        var commentFile = $('#commentFile')[0];
        var file = commentFile.files[0];

        // Проверка комментария
        if (!commentText.val().trim()) {
            commentText.addClass('is-invalid');
            isValid = false;
        } else {
            commentText.removeClass('is-invalid');
        }

        // Проверка размера файла (если файл выбран)
        if (file && file.size > 5 * 1024 * 1024) { // 5 MB
            $(commentFile).addClass('is-invalid');
            isValid = false;
        } else {
            $(commentFile).removeClass('is-invalid');
        }

        // Если валидация успешна, отправляем форму
        if (isValid) {
            $('#loadingSpinner').show();

            var formData = new FormData(this);
            var issueId = $('.comment-btn').data('issue-id');
            var url = $('.comment-btn').data('url');

            formData.append('id', issueId);
            formData.append('comment', commentText.val());
            formData.append('privateNotes', $('#privateComment').is(':checked'));
            if (file) {
                formData.append('file', file);
            }

            $.ajax({
                url: url,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function(response) {
                    $('#loadingSpinner').hide();
                    $('#commentPopup').modal('hide');
                    location.reload(); // или другая логика по завершении
                },
                error: function(xhr, status, error) {
                    $('#loadingSpinner').hide();
                    console.error(error);
                    // Отображение сообщения об ошибке
                }
            });
        }
    });
});
