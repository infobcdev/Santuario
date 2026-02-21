// jquery-click-scroll (fix completo)
// by syamsul'isul' Arifin (adaptado/robustecido)

(function ($) {
    'use strict';

    // Seções esperadas pelo tema (pode manter assim)
    var sectionArray = [1, 2, 3, 4, 5, 6];

    // Ajuste de topo (altura do menu fixo)
    var headerOffset = 90;

    // ==============================
    // Helper: pega o elemento da seção e valida
    // ==============================
    function getSectionEl(value) {
        var $section = $('#section_' + value);
        if ($section.length === 0) return null;
        return $section;
    }

    // ==============================
    // Scroll spy (1 handler só)
    // ==============================
    $(document).on('scroll', function () {
        var docScroll = $(document).scrollTop() + 1;

        $.each(sectionArray, function (index, value) {
            var $section = getSectionEl(value);
            if (!$section) return; // ✅ ignora se a seção não existe

            var offsetSection = $section.offset().top - headerOffset;

            if (docScroll >= offsetSection) {
                $('.navbar-nav .nav-item .nav-link').removeClass('active');
                $('.navbar-nav .nav-item .nav-link:link').addClass('inactive');

                // Mantém o comportamento original (por índice)
                $('.navbar-nav .nav-item .nav-link').eq(index).addClass('active');
                $('.navbar-nav .nav-item .nav-link').eq(index).removeClass('inactive');
            }
        });
    });

    // ==============================
    // Click scroll
    // ==============================
    $.each(sectionArray, function (index, value) {
        $('.click-scroll').eq(index).on('click', function (e) {
            var $section = getSectionEl(value);
            if (!$section) return; // ✅ seção não existe, não faz nada

            e.preventDefault();

            var offsetClick = $section.offset().top - headerOffset;

            $('html, body').animate({
                scrollTop: offsetClick
            }, 300);
        });
    });

    // ==============================
    // Inicialização
    // ==============================
    $(document).ready(function () {
        $('.navbar-nav .nav-item .nav-link:link').addClass('inactive');

        // ativa o primeiro item, como no original
        $('.navbar-nav .nav-item .nav-link').eq(0).addClass('active');
        $('.navbar-nav .nav-item .nav-link:link').eq(0).removeClass('inactive');

        // dispara um scroll para sincronizar estado ao carregar
        $(document).trigger('scroll');
    });

})(jQuery);