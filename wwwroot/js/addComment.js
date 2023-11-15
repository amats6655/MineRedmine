$(document).ready(function(){
    $('#commentForm').on('submit', function (event){
        event.preventDefault();
        $('#loadingSpinner').show();
        
        var formData = new FormData(this);
        var issueId = $('.comment-btn').data('issue-id');
        var url = $('.comment-btn').data('url');
        
        formData.append('id', issueId);
        formData.append('comment', $('#commentText').val());
        formData.append('privateNotes', $('#privateComment').is(':checked'));
        formData.append('file', $('#commentFile')[0].files[0]);
        
        $.ajax({
            url:url,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                location.reload();
            },
            error: function () {
                console.error(this.error)
            }
        });
    });
});