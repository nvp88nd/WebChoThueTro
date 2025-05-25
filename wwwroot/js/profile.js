// script edit profile
$(document).ready(function () {
    $('#edit-profile').click(function (e) {
        e.preventDefault();
        $('input').prop('disabled', false); // Bật các trường input
        $('#edit-profile').hide(); // Ẩn nút "Chỉnh sửa hồ sơ"
        $('#submit-profile').show(); // Hiển thị nút "Lưu hồ sơ"
    });

    $('#submit-profile').click(function (e) {
        e.preventDefault();

        let form = $("form")[0];

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        const data = {
            FullName: $('#username').val(),
            Phone: $('#phone').val(),
            Email: $('#email').val()
        };
        
        let csrfToken = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            url: '/Account/UpdateUser',
            type: 'POST',
            contentType: "application/json",
            headers: { "RequestVerificationToken": csrfToken },
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    $('#wrong').text('');
                    $('input').prop('disabled', true); // Tắt các trường input
                    $('#edit-profile').show(); // Hiển thị nút "Chỉnh sửa hồ sơ"
                    $('#submit-profile').hide(); // Ẩn nút "Lưu hồ sơ"
                    alert('Hồ sơ đã được cập nhật thành công!');
                } else {
                    $('#wrong').text(response.message); // Hiển thị lỗi
                }
            },
            error: function (error) {
                alert(error.message);
            }
        });
    });

    $('#submit-profile').hide();
});