﻿var startDateTextBox = $('#StartDate');
var endDateTextBox = $('#EndDate');

startDateTextBox.datetimepicker({
    onClose: function (dateText, inst) {
        if (endDateTextBox.val() != '') {
            var testStartDate = startDateTextBox.datetimepicker('getDate');
            var testEndDate = endDateTextBox.datetimepicker('getDate');
            if (testStartDate > testEndDate)
                endDateTextBox.datetimepicker('setDate', testStartDate);
        }
        else {
            endDateTextBox.val(dateText);
        }
    },
    onSelect: function (selectedDateTime) {
        endDateTextBox.datetimepicker('option', 'minDate', startDateTextBox.datetimepicker('getDate'));
    }
});
endDateTextBox.datetimepicker({
    onClose: function (dateText, inst) {
        if (startDateTextBox.val() != '') {
            var testStartDate = startDateTextBox.datetimepicker('getDate');
            var testEndDate = endDateTextBox.datetimepicker('getDate');
            if (testStartDate > testEndDate)
                startDateTextBox.datetimepicker('setDate', testEndDate);
        }
        else {
            startDateTextBox.val(dateText);
        }
    },
    onSelect: function (selectedDateTime) {
        startDateTextBox.datetimepicker('option', 'maxDate', endDateTextBox.datetimepicker('getDate'));
    }
});