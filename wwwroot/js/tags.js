$( function() {
    var $grid = $('.grid').isotope({
        itemSelector: 'article'
    });

    // filter buttons
    $('.filters-button-group').on( 'click', 'button', function() {
        var filterValue = $( this ).attr('data-filter');
        $grid.isotope({ filter: filterValue });
    });
    $('.button-group').each( function( i, buttonGroup ) {
        var $buttonGroup = $( buttonGroup );
        $buttonGroup.on( 'click', 'button', function() {
            $buttonGroup.find('.is-checked').removeClass('is-checked');
            $( this ).addClass('is-checked');
        });
    });
});

// debounce so filtering doesn't happen every millisecond
function debounce( fn, threshold ) {
    var timeout;
    return function debounced() {
        if ( timeout ) {
            clearTimeout( timeout );
        }
        function delayed() {
            fn();
            timeout = null;
        }
        timeout = setTimeout( delayed, threshold || 100 );
    }
}

$(window).bind("load", function() {
    $('#all').click();
});



$(function (){
    $('#status').click(function (){
        $('.status').toggleClass('hidden');
        $('.priority').addClass('hidden');
        $('#priority').removeClass('hidden');
        $(this).toggleClass('hidden');
    });
    $('#priority').click(function (){
        $('.priority').toggleClass('hidden');
        $('.status').addClass('hidden');
        $('#status').removeClass('hidden');
        $(this).toggleClass('hidden')
    });
    $('#all').click(function (){
        $('#priority').removeClass('hidden');
        $('#status').removeClass('hidden');
        $('.priority').addClass('hidden');
        $('.status').addClass('hidden');
    });
});