var distanceService;

function calcDistance() {
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
            "<br> braucot ar mašīnu ceļā būs jāpavada aptuveni " + durationText;
    }
}