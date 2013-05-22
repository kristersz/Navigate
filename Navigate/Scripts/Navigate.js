//to invoke the modal dialog for various information and error prompts accross the application
function showMessage(message) {
    $("#myModalBody").text(message);
    $("#myModal").modal('show');
};

$(document).ready(function () {
    //Various settings for third party JavaScript libraries used in this application
    $.culture = Globalize.culture('lv-LV');
    $.pnotify.defaults.styling = "bootstrap";

    //to make main menu active elements inset
    var url = window.location;
    $('ul.nav a[href="' + url + '"]').parent().addClass('active');
    $('ul.nav a').filter(function () {
        return this.href == url;
    }).parent().addClass('active');

});

//Custom validators for form inputs
$.validator.methods.date = function (value, element) {
    if (Globalize.parseDate(value)) {
        return true;
    }
    return false;
};

$.validator.methods.number = function (value, element) {
    return this.optional(element) ||
        !isNaN(Globalize.parseFloat(value));
};

jQuery.extend(jQuery.validator.methods, {    
    range: function (value, element, param) {        
        //Use the Globalization plugin to parse the value        
        var val = $.global.parseFloat(value);
        return this.optional(element) || (
            val >= param[0] && val <= param[1]);
    }
});