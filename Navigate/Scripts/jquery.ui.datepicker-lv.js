$(function () {
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
	$.datepicker.setDefaults($.datepicker.regional['lv']);
	$(".datefield").datepicker({
		yearRange: '1930:2030',
	});

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
	$.timepicker.setDefaults($.timepicker.regional['lv']);
});