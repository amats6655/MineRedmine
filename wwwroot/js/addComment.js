 document.getElementById('commentForm').addEventListener('submit', function(event) {
    event.preventDefault();

    var formData = new FormData(this);
    formData.append('id', document.querySelector('.comment-btn').getAttribute('data-issue-id'));
    formData.append('comment', document.getElementById('commentText').value);
    formData.append('privateNotes', document.getElementById('privateComment').checked);
    formData.append('file', document.getElementById('commentFile').files[0]);
    

    
    fetch('@Url.Action("AddComment", "Issues")', {
    method: 'POST',
    body: formData
}).then(response => {
    if (response.ok) {
    // Обработка успешной отправки
} else {
    // Обработка ошибки
}
});
});