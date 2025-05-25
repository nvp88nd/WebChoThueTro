$(document).on("click", ".favorite-btn", function () {
    const btn = $(this);
    const postId = btn.data("post-id");

    $.post('/Home/ToggleFavorite', { postId }, function (res) {
        if (res.success) {
            const icon = btn.find("i");
            const isFavorited = icon.hasClass("bi-heart-fill");

            if (isFavorited) {
                icon.removeClass("bi-heart-fill").addClass("bi-heart");
                btn.text(" Lưu").prepend(icon);
            } else {
                icon.removeClass("bi-heart").addClass("bi-heart-fill");
                btn.text(" Bỏ lưu").prepend(icon);
            }
        }
    });
});