class Header extends Reflux.Component {
    constructor(props) {
        super(props);
        this.store = this.props.store;
        this.state = { 
            selectedHeaderButton: MonitoringDataType.All
        };

        this.showAll = () => Actions.updateMap(MonitoringDataType.All);
        this.showTopOccupacy = () => Actions.updateMap(MonitoringDataType.TopOccupacy);
        this.showTopRevenue = () => Actions.updateMap(MonitoringDataType.TopRevenue);
    }

    render() {

        let containerStyle = {
            /*display: "flex",
            flexDirection: "row",
            flexWrap: "nowrap",
            justifyContent: "flex-start",*/
            flexBasis: "content",
            fontSize: "25px",
            marginLeft: "5px",
            marginRight: "5px",
            borderBottom: "1px solid " + colorPallette.headerLineColor
        };

        return (
            <Container type="row" style={containerStyle}>
                <HeaderButton selected={this.state.selectedHeaderButton === MonitoringDataType.All}
                         caption="All" onClick={this.showAll} />
                <HeaderButton selected={this.state.selectedHeaderButton === MonitoringDataType.TopOccupacy}
                              caption="Top 10% Occupancy" onClick={this.showTopOccupacy} />
                <HeaderButton selected={this.state.selectedHeaderButton === MonitoringDataType.TopRevenue}
                              caption="Top 10% Revenue" onClick={this.showTopRevenue} />
            </Container>
            );
    }
}