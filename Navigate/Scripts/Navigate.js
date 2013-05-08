function showMessage(message) {
    $("#myModalBody").text(message);
    $("#myModal").modal('show');
};

$(document).ready(function () {
    //$("#dialogMessage").dialog({
    //    autoOpen: false,
    //    modal: true,
    //    resizable: false,
    //    draggable: false,
    //    minWidth: 400,
    //    buttons: {
    //        "Labi": function () {
    //            $(this).dialog("close");
    //        }
    //    }
    //});

    $.culture = Globalize.culture('lv-LV');

    var url = window.location;
    $('ul.nav a[href="' + url + '"]').parent().addClass('active');
    $('ul.nav a').filter(function () {
        return this.href == url;
    }).parent().addClass('active');
});

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