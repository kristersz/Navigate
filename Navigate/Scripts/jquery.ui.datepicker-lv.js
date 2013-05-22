$(function () {
    //translate the datepicker
	$.datepicker.regional['lv'] = {
		closeText: 'Aizvērt',
		prevText: 'Iepr',
		nextText: 'Nāka',
		currentText: 'Šodien',
		monthNames: ['Janvāris', 'Februāris', 'Marts', 'Aprīlis', 'Maijs', 'Jūnijs',
		'Jūlijs', 'Augusts', 'Septembris', 'Oktobris', 'Novembris', 'Decembris'],
		monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'Mai', 'Jūn',
		'Jūl', 'Aug', 'Sep', 'Okt', 'Nov', 'Dec'],
		dayNames: ['svētdiena', 'pirmdiena', 'otrdiena', 'trešdiena', 'ceturtdiena', 'piektdiena', 'sestdiena'],
		dayNamesShort: ['svt', 'prm', 'otr', 'tre', 'ctr', 'pkt', 'sst'],
		dayNamesMin: ['Sv', 'Pr', 'Ot', 'Tr', 'Ct', 'Pk', 'Se'],
		weekHeader: 'Nav',
		dateFormat: 'dd.mm.yy',
		firstDay: 1,
		isRTL: false,
		showMonthAfterYear: false,
		yearSuffix: ''
	};
	
    //translate the timepicker
	$.timepicker.regional['lv'] = {
	    timeOnlyTitle: 'Izvēlieties laiku',
	    timeText: 'Laiks',
	    hourText: 'Stundas',
	    minuteText: 'Minūtes',
	    secondText: 'Sekundes',
	    millisecText: 'Milisekundes',
	    timezoneText: 'laika zona',
	    currentText: 'Tagad',
	    closeText: 'Aizvērt',
	    timeFormat: 'HH:mm',
	    amNames: ['AM', 'A'],
	    pmNames: ['PM', 'P'],
	    isRTL: false
	};

    //set defaults and make the datepicker UI draggable for when it covers other inputs or controls
	$.timepicker.setDefaults($.timepicker.regional['lv']);
	$.datepicker.setDefaults($.datepicker.regional['lv']);
	$('#ui-datepicker-div').draggable();

    //slight modification for start and end date fields so that the user could not select a greater value in the StartDate field compared to EndDate value
	var startDateTextBox = $('#StartDate');
	var endDateTextBox = $('#EndDate');

	startDateTextBox.datetimepicker({
	    controlType: 'select',
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
	    controlType: 'select',
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

    //instantiate the controls
	$('.datetimefield').datetimepicker({
	    controlType: 'select',
	});

	$(".timefield").timepicker({
	    controlType: 'select',
	});

	$(".datefield").datepicker({
        yearRange: "1970:2030",
	});

});