// script login
$(document).ready(function () {
    $("#submit-login").click(function (event) {
        event.preventDefault();

        let form = $("form")[0];

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        let phoneInput = $("#phone").val().trim();
        let passwordInput = $("#password").val().trim();

        let csrfToken = $("input[name='__RequestVerificationToken']").val();

        let url = '/Account/Login';

        $.ajax({
            url: url,
            type: "POST",
            contentType: "application/json",
            headers: { "RequestVerificationToken": csrfToken },
            data: JSON.stringify({ Phone: phoneInput, Password: passwordInput }),
            success: function (data) {
                if (data.success) {
                    window.location.href = "/";
                } else {
                    $("#wrong").text(data.message);
                }
            },
            error: function (error) {
                console.error("Lỗi:", error);
            }
        });
    });
});


// script register
$(document).ready(function () {
    $("#submit-register").click(function (event) {
        event.preventDefault();

        let form = $("form")[0];

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        let fullName = $("#username").val().trim();
        let phoneInput = $("#phone").val().trim();
        let emailInput = $("#email").val().trim();
        let passwordInput = $("#password").val().trim();
        let confirmPasswordInput = $("#confirmPassword").val().trim();
        let userType = $("input[name='user_type']:checked").val();

        if (passwordInput != confirmPasswordInput) {
            wrongCP.textContent = "Mật khẩu xác nhận không khớp!";
            $("#wrong").text("");
            return;
        }
        let csrfToken = $("input[name='__RequestVerificationToken']").val();

        let url = '/Account/Register';

        $.ajax({
            url: url,
            type: "POST",
            contentType: "application/json",
            headers: { "RequestVerificationToken": csrfToken },
            data: JSON.stringify({ FullName: fullName, Phone: phoneInput, Email: emailInput, Password: passwordInput, Role: userType }),
            success: function (data) {
                if (data.success) {
                    window.location.href = "/Account/Login";
                } else {
                    $("#wrong").text(data.message);
                    wrongCP.textContent = "";
                }
            },
            error: function (error) {
                console.error("Lỗi:", error);
            }
        });
    });
});


// script get province
let locationData = [];

$.getJSON('/provinces/vietnam-provinces.json', function (data) {
    locationData = data;
    let $province = $('#province');

    $.each(data, function (i, province) {
        $province.append($('<option>', {
            value: province.name,
            text: province.name
        }));
    });
});

$('#province').on('change', function () {
    let selectedProvince = $(this).val();
    let $district = $('#district');
    let $ward = $('#ward');

    $district.empty().append('<option value="">-- Chọn Quận/Huyện --</option>').prop('disabled', true);
    $ward.empty().append('<option value="">-- Chọn Phường/Xã --</option>').prop('disabled', true);

    let province = locationData.find(p => p.name === selectedProvince);
    if (province && province.districts.length > 0) {
        $.each(province.districts, function (i, district) {
            $district.append($('<option>', {
                value: district.name,
                text: district.name
            }));
        });
        $district.prop('disabled', false);
    }
});

$('#district').on('change', function () {
    let selectedProvince = $('#province').val();
    let selectedDistrict = $(this).val();
    let $ward = $('#ward');
    let $address = $('#address');

    $ward.empty().append('<option value="">-- Chọn Phường/Xã --</option>').prop('disabled', true);
    $address.prop('disabled', true);

    let province = locationData.find(p => p.name === selectedProvince);
    let district = province?.districts.find(d => d.name === selectedDistrict);

    if (district && district.wards.length > 0) {
        $.each(district.wards, function (i, ward) {
            $ward.append($('<option>', {
                value: ward.name,
                text: ward.name
            }));
        });
        $ward.prop('disabled', false);
    }
});

$('#ward').on('change', function () {
    let wardSelected = $(this).val();
    if (wardSelected) {
        $('#address').prop('disabled', false);
    } else {
        $('#address').prop('disabled', true);
    }
});

// script new post
$(document).ready(function () {
    $("#submit-newpost").click(function (e) {
        e.preventDefault(); // Ngăn form submit truyền thống
        
        let form = $("form")[0];

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        const province = $('#province').val();
        const district = $('#district').val();
        const ward = $('#ward').val();
        const detailAddress = $('#address').val();
        const fullAddress = `${detailAddress}, ${ward}, ${district}, ${province}`;

        const price = parseFloat($('#price').val());
        const area = parseFloat($('#area').val());
        if (price <= 0) {
            alert('Giá không hợp lệ!');
            return;
        }
        if (area <= 0) {
            alert('Diện tích không hợp lệ!');
            return;
        }
        let formData = new FormData();
        formData.append("Title", $('#title').val());
        formData.append("Price", price);
        formData.append("Area", area);
        formData.append("Address", fullAddress);
        formData.append("Description", $('#description').val());
        formData.append("RoomType", $('#roomtype').val());
        formData.append("Amenities", $('#amenities').val());

        const files = $('#imageInput')[0].files;
        for (let i = 0; i < files.length; i++) {
            formData.append("Images", files[i]);
        }

        $.ajax({
            url: '/Manage/NewPost',
            type: 'POST',
            contentType: false,
            processData: false,
            data: formData,
            success: function (response) {
                alert('Đăng bài thành công!');
                window.location.href = '/Manage/ListPost';
            },
            error: function (xhr, status, error) {
                console.error('Lỗi:', error);
                alert('Đăng bài thất bại!');
            }
        });
    });
});

// upload images
const imageInput = document.getElementById('imageInput');
const fileList = document.getElementById('fileList');
let selectedFiles = [];

imageInput.addEventListener('change', function () {
    const files = Array.from(this.files);

    // Thêm file mới tránh trùng
    files.forEach(file => {
        if (!selectedFiles.some(f => f.name === file.name && f.lastModified === file.lastModified)) {
            selectedFiles.push(file);
        }
    });

    renderFileList();
});

function renderFileList() {
    fileList.innerHTML = '';

    selectedFiles.forEach((file, index) => {
        const li = document.createElement('li');
        li.textContent = file.name + ' ';

        const removeBtn = document.createElement('button');
        removeBtn.textContent = 'Xoá';
        removeBtn.type = 'button';
        removeBtn.style.marginLeft = '10px';
        removeBtn.style.color = 'red';
        removeBtn.addEventListener('click', () => {
            selectedFiles.splice(index, 1);
            renderFileList();
        });

        li.appendChild(removeBtn);
        fileList.appendChild(li);
    });

    updateInputFiles();
}

function updateInputFiles() {
    const dataTransfer = new DataTransfer();
    selectedFiles.forEach(file => dataTransfer.items.add(file));
    imageInput.files = dataTransfer.files;
}
