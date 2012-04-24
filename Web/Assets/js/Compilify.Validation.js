;(function($) {

    function markControlGroup(input) {
        var el = $(input);
        if (el.hasClass('input-validation-error')) {
            el.parents('div.control-group').addClass('error');
        } else {
            el.parents('div.control-group').removeClass('error');
        }
    }

    $(function() {
        $('form').addTriggersToJqueryValidate().triggerElementValidationsOnFormValidation();

        $('form')
            .on('keyup', 'input', function() {
                markControlGroup(this);
            })
            .formValidation(function(element, result) {
                $(element).find('input').each(function(index, value) {
                    markControlGroup(value);
                });
            });
    });
}).call(window, jQuery);


 
