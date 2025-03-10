$(function () {
    skinChanger();
    activateNotificationAndTasksScroll();

    setSkinListHeightAndScroll(true);
    setSettingListHeightAndScroll(true);
    ScrollSidebarLeft();
    $(window).resize(function () {
        setSkinListHeightAndScroll(false);
        setSettingListHeightAndScroll(false);
        ScrollSidebarLeft();
    });
});

function openNav() {
    document.getElementById("leftsidebar").style.width = "300px";
    $('.showhideSidebarLeft').animate({ 'left': '10px' });
    $('section.content').attr({ 'style': 'margin:100px 15px 0 315px' });

}

function closeNav() {
    document.getElementById("leftsidebar").style.width = "0";
    $('.showhideSidebarLeft').animate({ 'left': '10px' }, 'fast');
    $('section.content').attr({ 'style': 'margin:100px 15px 0 15px' });
}

function ScrollSidebarLeft() {
    //alert($('body').hasClass('ls-closed'));
    if ($('body').hasClass('ls-closed')) {
        $('.showhideSidebarLeft').hide();
        $('section.content').attr({ 'style': 'margin-left:15px' });
        if (parseInt($('#leftsidebar').css('width')) <= 0) {
            document.getElementById("leftsidebar").style.width = "300px";
        }
    }
    else {
        $('.showhideSidebarLeft').show();
        $('.overlay').hide();
        $('section.content').attr({ 'style': 'margin:100px 15px 0 315px' });
    }

    $('.showhideSidebarLeft').on('click', function () {
        if (parseInt($('#leftsidebar').css('width')) > 0) {
            closeNav();
        }
        else {
            openNav();
        }
    });
}

//Skin changer
function skinChanger() {
    $('.right-sidebar .demo-choose-skin li').on('click', function () {
        var $body = $('body');
        var $this = $(this);

        var existTheme = $('.right-sidebar .demo-choose-skin li.active').data('theme');
        $('.right-sidebar .demo-choose-skin li').removeClass('active');
        $body.removeClass('theme-' + existTheme);
        $this.addClass('active');

        $body.addClass('theme-' + $this.data('theme'));
    });
}

//Skin tab content set height and show scroll
function setSkinListHeightAndScroll(isFirstTime) {
    var height = $(window).height() - ($('.navbar').innerHeight() + $('.right-sidebar .nav-tabs').outerHeight());
    var $el = $('.demo-choose-skin');

    if (!isFirstTime){
      $el.slimScroll({ destroy: true }).height('auto');
      $el.parent().find('.slimScrollBar, .slimScrollRail').remove();
    }

    $el.slimscroll({
        height: height + 'px',
        color: 'rgba(0,0,0,0.5)',
        size: '6px',
        alwaysVisible: false,
        borderRadius: '0',
        railBorderRadius: '0'
    });
}

//Setting tab content set height and show scroll
function setSettingListHeightAndScroll(isFirstTime) {
    var height = $(window).height() - ($('.navbar').innerHeight() + $('.right-sidebar .nav-tabs').outerHeight());
    var $el = $('.right-sidebar .demo-settings');

    if (!isFirstTime){
      $el.slimScroll({ destroy: true }).height('auto');
      $el.parent().find('.slimScrollBar, .slimScrollRail').remove();
    }

    $el.slimscroll({
        height: height + 'px',
        color: 'rgba(0,0,0,0.5)',
        size: '6px',
        alwaysVisible: false,
        borderRadius: '0',
        railBorderRadius: '0'
    });
}

//Activate notification and task dropdown on top right menu
function activateNotificationAndTasksScroll() {
    $('.navbar-right .dropdown-menu .body .menu').slimscroll({
        height: '254px',
        color: 'rgba(0,0,0,0.5)',
        size: '4px',
        alwaysVisible: false,
        borderRadius: '0',
        railBorderRadius: '0'
    });
}

//Google Analiytics ======================================================================================
addLoadEvent(loadTracking);
var trackingId = '';

function addLoadEvent(func) {
    var oldonload = window.onload;
    if (typeof window.onload != 'function') {
        window.onload = func;
    } else {
        window.onload = function () {
            oldonload();
            func();
        }
    }
}

function loadTracking() {
    (function (i, s, o, g, r, a, m) {
        i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
            (i[r].q = i[r].q || []).push(arguments)
        }, i[r].l = 1 * new Date(); a = s.createElement(o),
        m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
    })(window, document, 'script', 'https://www.google-analytics.com/analytics.js', 'ga');

    ga('create', trackingId, 'auto');
    ga('send', 'pageview');
}
//========================================================================================================
