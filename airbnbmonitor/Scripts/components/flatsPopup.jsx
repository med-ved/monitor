class FlatsPopup extends Reflux.Component {
    constructor(props) {
        super(props);
        this.store = FlatsPopupStoreInst;
        this.state = {
            flatsCount: 0,
            avgEstimatedMonthlyRevenue: null,
            occupacyPercent: null,
            avgRevenuePerDay: null
        };
    }

    render() {

        let containerStyle = {
            /*display: "flex",
            flexDirection: "column",
            flexWrap: "nowrap",
            justifyContent: "flex-start",
            flexBasis: "100%",*/
            height: "100%"
        };

        let textItems = [
            ["Flats count:", this.state.flatsCount],
            ["Average monthly revenue:", (+this.state.avgEstimatedMonthlyRevenue).toFixed(0)],
            ["Occupacy:", (+this.state.occupacyPercent).toFixed(0)],
            ["Revenue per day:", (+this.state.avgRevenuePerDay).toFixed(0)],
        ];

        return (
            <Popup id="flatsPopup" store={this.store} caption="Flats group details" >
                <Container data-debug="flatsPopupContainer" style={containerStyle} >
                   <TextGrid items={textItems} type="row"></TextGrid>
                   <ChartsPanel store={FlatsPopupStoreInst} idSuffix="FlatsPopup" />
                </Container>
            </Popup>
            
           );
    }
}
