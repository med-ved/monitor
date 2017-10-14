//no es6 modle support in ReactJS.NET (
const MonitoringDataType = {
    All: 0,
    TopOccupacy: 1,
    TopRevenue: 2
};

const colorPallette = {
    widgetHeaderColor: "#1f2f40",
    widgetBodyColor: "#1a2b39",
    backGroundColor: "#1A232D",
    textColor: "#D3DEE1",
    brighterTextColor: "#e8eeef",
    highlightedTextColor: "#0E98D1",
    selectedHeaderButton: "#0E98D1",

    lineColor: "#133644",
    headerLineColor: "#1e4555",

    revenueColor: "#eb3778",
    occupacyColor: "#c3d10e",
    revenuePerDayColor: "#1fef61",

    lineChartLegendColor: "#D3DEE1",
    chartTextHoverColor: "#0E98D1",

    overlayColor: "#050404"
};

class App extends Reflux.Component {
    constructor(props) {
        super(props);
        this.store = DataStoreInst;
    }

    render() {
        let appContainerStyle = {
            backgroundColor: colorPallette.backGroundColor,
            height: "100vh"
        };

        let overlayStyle = {
            display: this.state.loading ? "flex" : "none",
            zIndex: 2
        };

        let chartsContainer = {
            height: "100%",
            width: "55%",
        };

        return (       
          <div>
              <Container style={appContainerStyle}>
                  <Overlay style={overlayStyle}>
                    <LoadingAnimation />
                  </Overlay>
                  <Header store={DataStoreInst}/>
                  <Container type="row">
                     <ChartsPanel store={DataStoreInst} idSuffix="MainPanel" style={chartsContainer} />
                     <Component height="100%" width="45%">
                         <Widget caption="Average monthly revenue">
                             <Map id="map" zoom="10" lat="59.88999017" lng="30.46708698" />
                         </Widget>
                     </Component>
                  </Container>
              </Container>

              <FlatsPopup id="flatsPopup" />
              <OneFlatDetailsPopup />
        </div>
           ); 
    }
}

ReactDOM.render(
       <App />
  ,
  document.getElementsByTagName("body")[0]
  );


