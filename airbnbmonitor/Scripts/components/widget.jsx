class Widget extends Reflux.Component {
    constructor() {
        super();

        this.state = {
            hover: false
        };

        this.toggleHover = () => {
            this.setState(prevState => ({
                hover: !prevState.hover
            }));
        }
    }

    render() {

        let widgetContainer = {
            //display: "flex",
            //flexDirection: "column",
            //flexWrap: "nowrap",
            //justifyContent: "flex-start",
            height: "100%",
            width: "100%",
            padding: "5px",
        };

        let headerComponent = {
            flex: "0 1 auto",
            fontSize: "18px",
            backgroundColor: colorPallette.widgetHeaderColor,
            color: colorPallette.textColor,
            width: "100%",
            borderRadius: "2px 2px 0 0",
            textAlign: "center",
            fontWeight: "bold",
            paddingTop: "5px",
            paddingBottom: "5px",
        };

        let bodyContainerStyle = {
            backgroundColor: colorPallette.widgetBodyColor,
            borderRadius: "0 0 2px 2px",

            //flexBasis: "100%",
            //flexDirection: "column",
            //display: "flex"
        };
        Object.assign(bodyContainerStyle, this.props.style);

        let close = null;
        if (this.props.close) { 
            let closeStyle = {
                float: "right",
                cursor: "pointer",
                paddingRight: "10px",
                color: this.state.hover ? colorPallette.highlightedTextColor : colorPallette.textColor
            }
            
            close = <span onClick={this.props.close} style={closeStyle} 
                  onMouseEnter={this.toggleHover} onMouseLeave={this.toggleHover}>X</span>;
        }
        else {
            close = "";
        }

        return (
            <Container data-debug="widgetContainer" style={widgetContainer} >
                <div style={headerComponent}>
                    {this.props.caption}
                    {close}
                </div>
                <Container style={bodyContainerStyle} data-debug="popupBodyContainer">
                    {this.props.children}
                </Container>
            </Container>
            );
    }
}