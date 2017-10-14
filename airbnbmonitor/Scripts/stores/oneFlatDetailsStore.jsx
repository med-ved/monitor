class OneFlatDetailsStore extends BaseStore {
    constructor() {
        super();

        this.state = {};
        this.listenTo(Actions.getOneFlatDetailsPopupData, this.getOneFlatDetailsPopupData);
    }

    onDataReceived(data, params) {
        this.setState({
            flat: data.Flat,
            avgEstimatedMonthlyRevenue: data.Summary.AvgEstimatedMonthlyRevenue,
            occupacyPercent: data.Summary.AvgOccupacyPercent,
            avgRevenuePerDay: data.Summary.AvgRevenuePerDay,
            lineChartData: this.prepareLineChartData(data.Summary.GraphData.LineChartData),
        });
    }

    openPopup() {
        this.setState({ popupVisible: true });
    }

    getOneFlatDetailsPopupData(id) {
        this.openPopup();
        let params = {
            url: "/Home/GetOneFlatData",
            queryParams: JSON.stringify({ 'flatId': id })
        };
        this.getData(params);
    }
}

const OneFlatDetailsStoreInst = new OneFlatDetailsStore();