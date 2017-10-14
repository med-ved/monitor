class OneFlatDetailsPopup extends Reflux.Component {
    constructor(props) {
        super(props);
        this.store = OneFlatDetailsStoreInst;
        this.state = {
            flat: {},
            avgEstimatedMonthlyRevenue: null,
            occupacyPercent: null,
            avgRevenuePerDay: null
        };
    }

    show(v) {
        //null or undefined
        return v == null ? "-" : v;
    }

    getDescription(flat) {
        if (!flat || !flat.Description) {
            return "-";
        }

        let description = flat.Description.Summary;
        if (!description) {
            description = flat.Description.Description;
        }
        if (!description) {
            description = flat.Description.Name;
        }

        return description;
    }

    render() {

        let containerStyle = {
            height: "100%",
        };

        let descriptionContainerStyle = {
            flexBasis: "auto"
        };

        let lineChartStyle = {
            width: "100%"
        };

        let textItems = [
            ["More:", <a href={"https://www.airbnb.com/rooms/" + this.state.flat.Id} target="_blank">Flat's AirBnb page</a>],
            ["BathroomsCount:", this.show(this.state.flat.BathroomsCount)],
            ["BedroomsCount:", this.show(this.state.flat.BedroomsCount)],
            ["BedsCount:", this.show(this.state.flat.BedsCount)],
            ["MaxGuests:", this.show(this.state.flat.MaxGuests)],
            ["MatchDescription:", this.show(this.state.flat.MatchDescription) + "/10"],
            ["Communication:", this.show(this.state.flat.Communication) + "/10"],
            ["Cleanly:", this.show(this.state.flat.Cleanly) + "/10"],
            ["Location:", this.show(this.state.flat.Location) + "/10"],
            ["Settlement:", this.show(this.state.flat.Settlement) + "/10"],
            ["Monthly revenue:", this.show((+this.state.avgEstimatedMonthlyRevenue).toFixed(0))],
            ["Occupacy:", this.show((+this.state.occupacyPercent))],
            ["Revenue per day:", this.show((+this.state.avgRevenuePerDay).toFixed(0))],
            ];

        let detailsTextStyle = {
            whiteSpace: "initial"
        };
        let descriptionItem = [["Description:", this.getDescription(this.state.flat), {}, detailsTextStyle]];
        return (
            <Popup id="oneFlatDetailsPopup" store={this.store} caption="Flat details">
                <Container type="column" style={containerStyle}>
                    <Container style={descriptionContainerStyle}>
                        <TextGrid items={descriptionItem} type="row"></TextGrid>
                    </Container>
                    <Container type="row" style={containerStyle}>
                        <TextGrid items={textItems} type="column"></TextGrid>
                        <LineChart id='lineChartOneFlatDetails' store={this.store} caption="" style={lineChartStyle} dataField="lineChartData" />
                   </Container>
                </Container>
            </Popup>

           );
        }
}