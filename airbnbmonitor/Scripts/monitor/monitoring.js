"use strict"

function init() {
    let myMap = new ymaps.Map("map", {
        center: [59.88999017, 30.46708698],
        zoom: 10
    });

    let marks = $("#data").data().monitoring;
    for (let i = 0; i < marks.length; i++) {
        let m = marks[i];
        let myPlacemark = new ymaps.Placemark([m.Latitude, m.Longitude], { hintContent: m.FillFactor, balloonContent: m.Revenue });
        myMap.geoObjects.add(myPlacemark);
    }
}

$(document).ready(function () {
    ymaps.ready(init);
});
