// Đóng menu khi người dùng nhấn vào một mục trong navbar (mobile)
$(document).ready(function () {
    $('.navbar-nav>li>a').on('click', function () {
        if ($('.navbar-toggler').is(':visible')) {
            $('.navbar-collapse').collapse('hide');
        }
    });
});

// Tự động ẩn thông báo sau 3 giây (ví dụ cho alert thêm vào giỏ hàng thành công)
$(document).ready(function () {
    setTimeout(function () {
        $('#success-alert').fadeOut('slow');
    }, 3000); // 3000ms = 3 giây
});

// Thay đổi kích thước các phần tử khi thay đổi kích thước trình duyệt
$(window).resize(function () {
    if ($(window).width() < 768) {
        // Điều chỉnh cho thiết bị di động
        $('.product-card').css('margin', '10px 0');
        $('.config-container .row div').css('text-align', 'center');
    } else {
        // Điều chỉnh cho màn hình lớn hơn
        $('.product-card').css('margin', '20px 0');
    }
}).resize(); // Gọi resize() để chạy khi trang vừa tải

// Cuộn mượt khi nhấn vào liên kết trong trang
$(document).on('click', 'a[href^="#"]', function (event) {
    event.preventDefault();
    $('html, body').animate({
        scrollTop: $($.attr(this, 'href')).offset().top
    }, 500);
});

// Tắt hiệu ứng hover trên thiết bị cảm ứng
function isTouchDevice() {
    return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
}

if (isTouchDevice()) {
    $('*').css('hover', 'none');
}

// Hiệu ứng khi hover vào các card sản phẩm (điều chỉnh hiệu ứng khi hover)
$('.product-card').hover(
    function () {
        $(this).addClass('shadow-lg').css('cursor', 'pointer');
    }, function () {
        $(this).removeClass('shadow-lg');
    }
);

// Điều chỉnh độ trong suốt cho các ảnh sản phẩm khi hover (mang lại hiệu ứng hiện đại)
$('.product-card img').hover(
    function () {
        $(this).css('opacity', '0.9');
    }, function () {
        $(this).css('opacity', '1');
    }
);
