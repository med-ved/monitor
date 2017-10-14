class DataStore extends BaseStore {
    constructor() {
        super();

        this.state = {
            loading: false
        };
        this.listenTo(Actions.setMapData, this.setMapData);
        this.listenTo(Actions.updateMap, this.updateMap);
    }

    onDataReceived(data, params) {
        this.data = data;
        this.setState({
            loading: false,
            selectedHeaderButton: params.additonalParams,
            mapData: data,
            lineChartData: this.prepareLineChartData(data.Summary.GraphData.LineChartData),
            occupacyChartData: this.prepareDonutData(data.Summary.GraphData.DistributionByOccupacy, this.percentNameFunc.bind(this)),
            revenueChartData: this.prepareDonutData(data.Summary.GraphData.DistributionByRevenue, this.revenueNameFunc.bind(this))
        });
    }

    updateMap(updateType) {
        let params = {
            url: "/Home/GetData",
            queryParams: "{ 'type': '" + updateType + "' }",
            additonalParams: updateType
        };
        this.getData(params);
    }

};

const DataStoreInst = new DataStore();