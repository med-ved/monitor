const ChartsPanel = ({style, idSuffix, store}) => {

    let chartsContainerStyle = {
        height: "100%",
        flexBasis: "auto"
    };
    Object.assign(chartsContainerStyle, style);

    /*let lineChartComponent = {
        height: "50%",
        width: "100%"
    }*/

    let donutChartContainerStyle = {
        /*display: "flex",
        flexDirection: "row",
        flexWrap: "nowrap",
        justifyContent: "flex-start",*/
        flexBasis: "auto",
        height: "50%",
        width: "100%",

        /*display: "flex",
        flexBasis: "100%",
    flexDirection: type ? type : "column",
    flexWrap: "nowrap",
    justifyContent: "flex-start",*/
    }

    /*let donutChartComponent = {
        height: "100%",
        width: "50%"
    }*/

    return (
        <Container data-debug="chartsContainer" style={chartsContainerStyle} >
            <Component height="50%" width="100%">
                <Widget caption="Summary">
                    <LineChart id={'lineChart' + idSuffix} store={store} caption="" dataField="lineChartData" />
                </Widget>
            </Component>
            <Container type="row" style={donutChartContainerStyle}>
                <Component height="100%" width="50%">
                    <Widget caption="Revenue distribution">
                        <DonutChart id={'revenueChart' + idSuffix} store={store}
                                    caption="" datafield="revenueChartData" mainColor={colorPallette.revenueColor} />
                    </Widget>
                </Component>
                <Component height="100%" width="50%">
                    <Widget caption="Occupacy distribution">
                        <DonutChart id={'occupacyChart' + idSuffix} store={store}
                                    caption="" datafield="occupacyChartData" mainColor={colorPallette.occupacyColor} />
                    </Widget>
                </Component>
            </Container>
        </Container>
        );
}