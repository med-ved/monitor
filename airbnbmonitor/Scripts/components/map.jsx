class Map extends Reflux.Component {
    constructor(props) {
        super(props);
        this.map = null;
        this.heatmap = null;

        this.store = DataStoreInst;
        this.state = { mapData: null };
    }

    marksClusterOnClick(cluster) {
        let ids = cluster.markers_.map((m) => m.id);
        Actions.getFlatsPopupData(ids);
    }

    onClickSingleMarker() {
        Actions.getOneFlatDetailsPopupData(this.id);
    }

    addMarks(marks) {
        if (this.markerCluster) {
            for (var i = 0; i < this.markers.length; i++) {
                this.markers[i].setMap(null);
            }

            this.markers = [];
            this.markerCluster.clearMarkers();
        }

        this.markers = marks.map((mark, i) => {
            return new google.maps.Marker({
                id: mark.Id,
                position: { lat: mark.Latitude, lng: mark.Longitude },
                label: "",
                revenue: mark.Revenue,
                occupancy: mark.Occupancy
            });
        });

        this.markers.forEach( m => m.addListener('click', this.onClickSingleMarker));

        let optionsMarkerClusterer = {
            zoomOnClick: false,
            styles: [{
                height: 53,
                textColor: colorPallette.textColor,
                url: 'https://github.com/googlemaps/v3-utility-library/blob/master/markerclusterer/images/m1.png?raw=true',
                width: 53
            }],
        };

        this.markerCluster = new MarkerClusterer(this.map, this.markers, optionsMarkerClusterer);
        google.maps.event.addListener(this.markerCluster, 'clusterclick', this.marksClusterOnClick);
    }

    setMapData(data) {
        this.addMarks(data.FlatsInfoList);
        this.addHeatMap(data.FlatsGrid);
    }

    createHeatMap() {
        return new HeatmapOverlay(this.map,
          {
              radius: 0.02,
              maxOpacity: 0.4,
              scaleRadius: true,
              useLocalExtrema: !true,
              latField: 'Latitude',
              lngField: 'Longitude',
              valueField: 'Revenue'
          });
    }

    addHeatMap(data) {
        if (!this.heatmap) {
            this.heatmap = this.createHeatMap();
        }
        
        let heatmapData = {
            min: 0,
            max: 250000,
            data: data
        };

        this.heatmap.setData(heatmapData);
    }

    componentDidUpdate(prevProps, prevState) {
        if (!this.map || prevState.mapData === this.state.mapData) {
            return;
        }

        this.setMapData(this.state.mapData);
    }

    getCustomMapStyle() {
        return [
          {
              "elementType": "geometry",
              "stylers": [
                {
                    "color": "#1d2c4d"
                }
              ]
          },
          {
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#8ec3b9"
                }
              ]
          },
          {
              "elementType": "labels.text.stroke",
              "stylers": [
                {
                    "color": "#1a3646"
                }
              ]
          },
          {
              "featureType": "administrative.country",
              "elementType": "geometry.stroke",
              "stylers": [
                {
                    "color": "#4b6878"
                }
              ]
          },
          {
              "featureType": "administrative.land_parcel",
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#64779e"
                }
              ]
          },
          {
              "featureType": "administrative.province",
              "elementType": "geometry.stroke",
              "stylers": [
                {
                    "color": "#4b6878"
                }
              ]
          },
          {
              "featureType": "landscape.man_made",
              "elementType": "geometry.stroke",
              "stylers": [
                {
                    "color": "#334e87"
                }
              ]
          },
          {
              "featureType": "landscape.natural",
              "elementType": "geometry",
              "stylers": [
                {
                    "color": "#023e58"
                }
              ]
          },
          {
              "featureType": "poi",
              "elementType": "geometry",
              "stylers": [
                {
                    "color": "#283d6a"
                }
              ]
          },
          {
              "featureType": "poi",
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#6f9ba5"
                }
              ]
          },
          {
              "featureType": "poi",
              "elementType": "labels.text.stroke",
              "stylers": [
                {
                    "color": "#1d2c4d"
                }
              ]
          },
          {
              "featureType": "poi.park",
              "elementType": "geometry.fill",
              "stylers": [
                {
                    "color": "#023e58"
                }
              ]
          },
          {
              "featureType": "poi.park",
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#3C7680"
                }
              ]
          },
          {
              "featureType": "road",
              "elementType": "geometry",
              "stylers": [
                {
                    "color": "#304a7d"
                }
              ]
          },
          {
              "featureType": "road",
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#98a5be"
                }
              ]
          },
          {
              "featureType": "road",
              "elementType": "labels.text.stroke",
              "stylers": [
                {
                    "color": "#1d2c4d"
                }
              ]
          },
          {
              "featureType": "road.highway",
              "elementType": "geometry",
              "stylers": [
                {
                    "color": "#2c6675"
                }
              ]
          },
          {
              "featureType": "road.highway",
              "elementType": "geometry.stroke",
              "stylers": [
                {
                    "color": "#255763"
                }
              ]
          },
          {
              "featureType": "road.highway",
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#b0d5ce"
                }
              ]
          },
          {
              "featureType": "road.highway",
              "elementType": "labels.text.stroke",
              "stylers": [
                {
                    "color": "#023e58"
                }
              ]
          },
          {
              "featureType": "transit",
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#98a5be"
                }
              ]
          },
          {
              "featureType": "transit",
              "elementType": "labels.text.stroke",
              "stylers": [
                {
                    "color": "#1d2c4d"
                }
              ]
          },
          {
              "featureType": "transit.line",
              "elementType": "geometry.fill",
              "stylers": [
                {
                    "color": "#283d6a"
                }
              ]
          },
          {
              "featureType": "transit.station",
              "elementType": "geometry",
              "stylers": [
                {
                    "color": "#3a4762"
                }
              ]
          },
          {
              "featureType": "water",
              "elementType": "geometry",
              "stylers": [
                {
                    "color": "#0e1626"
                }
              ]
          },
          {
              "featureType": "water",
              "elementType": "labels.text.fill",
              "stylers": [
                {
                    "color": "#4e6d70"
                }
              ]
          }
        ]
    }

    componentDidMount(mapId) {
        let mapOptions = {
            zoom: +this.props.zoom,
            center: new google.maps.LatLng(+this.props.lat, +this.props.lng),
            styles: this.getCustomMapStyle(),
            disableDefaultUI: true
        };

        this.map = new google.maps.Map(document.getElementById(this.props.id), mapOptions);
        Actions.updateMap(MonitoringDataType.All);
    }

    render() {
        let style = {
            height: "100%",
            width: "100%",
        }

        return (<div id={this.props.id} style={style}> </div> 
        );
    }
}

