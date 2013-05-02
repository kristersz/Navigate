var map;
var directionsDisplay;
var directionsService;
var stepDisplay;
var markerArray = [];
var distanceService;
var geocodingService;
var warnings = document.getElementById("warnings_panel");

function initialize(location, zoom) {
    // Instantiate a directions service and get current location
    directionsService = new google.maps.DirectionsService();
    
    // Create a map and center it on current location
    var myLocation = new google.maps.LatLng(37.0625000, -95.6770680);
    var mapOptions = {
        zoom: zoom,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        center: location
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
    var request;
    var travelMode = getRadioValue();
    switch (travelMode) {
        case "DRIVING":
            request = {
                origin: start,
                destination: end,
                travelMode: google.maps.TravelMode.DRIVING
            };
            break;
        case "WALKING":
            request = {
                origin: start,
                destination: end,
                travelMode: google.maps.TravelMode.WALKING
            };
            break;
        case "TRANSIT":
            request = {
                origin: start,
                destination: end,
                travelMode: google.maps.TravelMode.TRANSIT
            };
            break;
        default:
            request = {
                origin: start,
                destination: end,
                travelMode: google.maps.TravelMode.DRIVING
            };
            break;
    }

    // Route the directions and pass the response to a
    // function to create markers for each step.
    directionsService.route(request, function (response, status) {
        if (status == google.maps.DirectionsStatus.OK) {

            // If the response returns a successful result, we set warnings, directions and step markers
            var warnings = document.getElementById("warnings_panel");
            warnings.innerHTML = "" + response.routes[0].warnings + "";
            directionsDisplay.setDirections(response);
            showSteps(response);
        }

        // Else we handle errors and show them to the user
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

function calcDistance() {
    // Instantiate a Distance Matrix service
    distanceService = new google.maps.DistanceMatrixService();
    distanceService.getDistanceMatrix(
      {
          origins: [document.getElementById("start").value],
          destinations: [document.getElementById("end").value],
          travelMode: google.maps.TravelMode.DRIVING,
          unitSystem: google.maps.UnitSystem.METRIC,
          avoidHighways: false,
          avoidTolls: false
      }, callback);
}

function callback(response, status) {
    if (status != google.maps.DistanceMatrixStatus.OK) {
        alert("Notika sekojoša kļūda: " + status);
    }
    else {
        var output = document.getElementById("distanceOutput"),
            origin = response.originAddresses[0],
            destination = response.destinationAddresses[0],
            result = response.rows[0].elements[0],
            distanceValue = result.distance.value,
            distanceText = result.distance.text,
            durationValue = result.duration.value,
            durationText = result.duration.text

        output.innerHTML = "Attālums no <b>" + origin + "</b> līdz <b>" + destination + "</b> ir " + distanceText +
            "<br> ceļā būs jāpavada aptuveni " + durationText;
    }
};

google.maps.event.addDomListener(window, 'load', initialize);

function getRadioValue() {
    var inputs = document.getElementsByTagName("input");
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].checked) {
            return inputs[i].value;
        }
    }
}

function geolocateUser() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(geolocationSuccess, geolocationError);
    }
    else { warnings.innerHTML = "Geolocation is not supported by this browser."; }
}

function geolocationSuccess(position) {
    var myLatLng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
    initialize(myLatLng, 13);
    var marker = new google.maps.Marker({
        position: myLatLng,
        map: map
    });
    var circle = new google.maps.Circle({
        center: myLatLng,
        radius: position.coords.accuracy,
        map: map,
        fillColor: '#0000FF',
        fillOpacity: 0.3,
        strokeColor: '#0000FF',
        strokeOpacity: 0.7,
        strokeWeight: 2
    });
    //map.fitBounds(circle.getBounds());
    attachInstructionText(marker, "Your are around here, courtesy of &copy; HTML5 geolocation service <br> The blue circle represents the accuracy of the geolocation");
    setAddress(myLatLng);
}

function geolocationError(error) {
    var latvia = new google.maps.LatLng(37.0625000, -95.6770680);
    initialize(latvia, 10);
    switch (error.code) {
        case error.PERMISSION_DENIED:
            warnings.innerHTML = "User denied the request for Geolocation."
            break;
        case error.POSITION_UNAVAILABLE:
            warnings.innerHTML = "Location information is unavailable."
            break;
        case error.TIMEOUT:
            warnings.innerHTML = "The request to get user location timed out."
            break;
        case error.UNKNOWN_ERROR:
            warnings.innerHTML = "An unknown error occurred."
            break;
    }
}

function setAddress(myLatLng) {
    geocodingService = new google.maps.Geocoder();
    var address;
    geocodingService.geocode({ 'latLng': myLatLng }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            if (results[0]) {
                address = results[0].formatted_address;
                document.getElementById("start").value = address;
            }
            else {
                warnings.innerHTML = "No results found";
            }
        }
        else {
            warnings.innerHTML = "Geocode was not successful for the following reason: " + status;
        }
    });
}

google.maps.event.addDomListener(window, 'load', geolocateUser);