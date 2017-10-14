class FlatsPopupDataStore extends BaseStore {
    constructor() {
        super();

        this.state = {};
        this.listenTo(Actions.getFlatsPopupData, this.getDataForPopup);
    }

    onDataReceived(data, params) {
        this.setState({
            flatsCount: data.FlatsCount,
            avgEstimatedMonthlyRevenue: data.AvgEstimatedMonthlyRevenue,
            occupacyPercent: data.AvgOccupacyPercent,
            avgRevenuePerDay: data.AvgRevenuePerDay,
            lineChartData: this.prepareLineChartData(data.GraphData.LineChartData),
            occupacyChartData: this.prepareDonutData(data.GraphData.DistributionByOccupacy, this.percentNameFunc.bind(this)),
            revenueChartData: this.prepareDonutData(data.GraphData.DistributionByRevenue, this.revenueNameFunc.bind(this))
        });
    }

    openPopup() {
        this.setState({ popupVisible: true });
    }

    getDataForPopup(ids) {
        this.openPopup();
        let params = {
            url: "/Home/GetFlatsPopupData",
            queryParams: JSON.stringify({ 'flatsIds': ids }),
        };
        this.getData(params);
    }
}

const FlatsPopupStoreInst = new FlatsPopupDataStore();