class Popup extends Reflux.Component {
    constructor(props) {
        super(props);
        this.store = this.props.store;
        this.state = {
             popupVisible: false
        };

        this.closePopup = () => this.setState({ popupVisible: false });
    }

    render() { 

        let containerStyle = {
            display: this.state.popupVisible ? "flex" : "none",
            position: "absolute",
            //height: "100vh",
            //width: "100vw",
            top: "0"
        };

        let overlayStyle = {
           /* height: "100vh",
            position: "absolute",
            width: "100vw",
            top: "0",
            backgroundColor: "#050404",
            opacity: "0.8",*/
            //zIndex: 2
        };

        let popupStyle = {
            //width: "90%",
            //height: "90%",
            position: "relative",
            opacity: "1",
            margin: "auto"
        };

        let loadingStyle = {
            display: this.state.loading ? "block" : "none",
        };

        let contentStyle = {
            display: this.state.loading ? "none" : "block",
            height: "100%",
            width: "100%"
        };

        let widgetStyle = {
            backgroundColor: colorPallette.backGroundColor,
            display: "flex"
        };

        return (
            <Component id={this.props.id} height="100vh" width="100vw" style={containerStyle} data-debug="popup">
                 <Overlay style={overlayStyle} onClick={this.closePopup}></Overlay>
                 <Component width="90%" height="90%" style={popupStyle}>
                    <Widget caption={this.props.caption} style={widgetStyle} close={this.closePopup.bind(this)}>
                        <LoadingAnimation style={loadingStyle} />
                        <div style={contentStyle}>{this.props.children}</div>
                    </Widget>
                </Component>
            </Component>
           );
    }
}
