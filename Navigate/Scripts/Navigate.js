function showMessage(message) {
    $("#dialogMessageText").text(message);
    $("#dialogMessage").dialog("open");
};

$(document).ready(function () {
    $("#dialogMessage").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        minWidth: 400,
        buttons: {
            "Labi": function () {
                $(this).dialog("close");
            }
        }
    });
});

$.validator.methods.number = function (value, element) {
    return this.optional(element) ||
        !isNaN(Globalize.parseFloat(value));
};

$(document).ready(function () {
    Globalize.culture('lv-LV');
});

$.validator.methods.date = function (value, element) {
    if (Globalize.parseDate(value)) {
        return true;
    }
    return false;
};

jQuery.extend(jQuery.validator.methods, {    
    range: function (value, element, param) {        
        //Use the Globalization plugin to parse the value        
        var val = $.global.parseFloat(value);
        return this.optional(element) || (
            val >= param[0] && val <= param[1]);
    }
});