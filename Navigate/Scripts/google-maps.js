var map;
var directionsDisplay;
var directionsService;
var stepDisplay;
var markerArray = [];

function initialize() {
    // Instantiate a directions service.
    directionsService = new google.maps.DirectionsService();

    // Create a map and center it on Manhattan.
    var myLocation = new google.maps.LatLng(56.949468, 24.105377);
    var mapOptions = {
        zoom: 13,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        center: myLocation
    }
    map = new google.maps.Map(document.getElementById("map-canvas"), mapOptions);

    // Create a renderer for directions and bind it to the map.
    var rendererOptions = {
        map: map
    }
    directionsDisplay = new google.maps.DirectionsRenderer(rendererOptions)
    directionsDisplay.setPanel(document.getElementById("directions_panel"));

    // Instantiate an info window to hold step text.
    stepDisplay = new google.maps.InfoWindow();
}

function calcRoute() {

    // First, clear out any existing markerArray
    // from previous calculations.
    for (i = 0; i < markerArray.length; i++) {
        markerArray[i].setMap(null);
    }

    // Retrieve the start and end locations and create
    // a DirectionsRequest using DRIVING directions.
    var start = document.getElementById("start").value;
    var end = document.getElementById("end").value;
    var request = {
        origin: start,
        destination: end,
        travelMode: google.maps.TravelMode.DRIVING
    };

    // Route the directions and pass the response to a
    // function to create markers for each step.
    directionsService.route(request, function (response, status) {
        if (status == google.maps.DirectionsStatus.OK) {
            var warnings = document.getElementById("warnings_panel");
            warnings.innerHTML = "" + response.routes[0].warnings + "";
            directionsDisplay.setDirections(response);
            showSteps(response);
        }
        else if (status == google.maps.DirectionsStatus.NOT_FOUND) {
            showMessage("The specified location could not be found, please enter a more specific location", function () {
                document.location.href = '@Html.AttributeEncode(Url.Action("Navigate", "WorkItem"))';
            });
        }
        else if (status == google.maps.DirectionsStatus.ZERO_RESULTS) {
            showMessage("A route between these locations could not be found", function () {
                document.location.href = '@Html.AttributeEncode(Url.Action("Navigate", "WorkItem"))';
            });
        }
        else if (status == google.maps.DirectionsStatus.UNKNOWN_ERROR) {
            showMessage("An unexpected error occured", function () {
                document.location.href = '@Html.AttributeEncode(Url.Action("Navigate", "WorkItem"))';
            });
        }
    });
}

function showSteps(directionResult) {
    // For each step, place a marker, and add the text to the marker's
    // info window. Also attach the marker to an array so we
    // can keep track of it and remove it when calculating new
    // routes.
    var myRoute = directionResult.routes[0].legs[0];

    for (var i = 0; i < myRoute.steps.length; i++) {
        var marker = new google.maps.Marker({
            position: myRoute.steps[i].start_point,
            map: map
        });
        attachInstructionText(marker, myRoute.steps[i].instructions);
        markerArray[i] = marker;
    }
}

function attachInstructionText(marker, text) {
    google.maps.event.addListener(marker, 'click', function () {
        stepDisplay.setContent(text);
        stepDisplay.open(map, marker);
    });
}

google.maps.event.addDomListener(window, 'load', initialize);