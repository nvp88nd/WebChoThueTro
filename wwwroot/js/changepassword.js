$(document).ready(function () {
    $('#submit-change-password').click(function (e) {
        e.preventDefault();

        let form = $("form")[0];

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        if ($('#NewPassword').val() !== $('#ConfirmPassword').val()) {
            $('#error-message').text('Mật khẩu mới và xác nhận mật khẩu không khớp!');
            return;
        }


        const data = {
            OldPassword: $('#CurrentPassword').val(),
            NewPassword: $('#NewPassword').val(),
            ConfirmPassword: $('#ConfirmPassword').val()
        };
        
        let csrfToken = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            url: '/Account/UpdatePassword',
            type: 'POST',
            contentType: "application/json",
            headers: { "RequestVerificationToken": csrfToken },
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    alert('Mật khẩu đã được thay đổi thành công!');
                    $('#error-message').text('');
                    $('#change-password-form')[0].reset(); // Reset form
                } else {
                    $('#error-message').text(response.message); // Hiển thị lỗi
                }
            },
            error: function (error) {
                alert('Đã xảy ra lỗi. Vui lòng thử lại sau.');
            }
        });
    });
});
