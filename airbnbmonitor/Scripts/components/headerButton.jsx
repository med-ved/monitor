class HeaderButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = () => {
            if (this.props.selected) {
                return;
            }

            this.props.onClick();
        }
    }
   
    render() {
        let style = {
            color: colorPallette.textColor,
            textAlign: "center",
            borderBottom: "5px solid",
            fontWeight: "bold",
            marginLeft: "15px",
            marginRight: "15px",
        };

        if (this.props.selected) {
            style.cursor = "default";
            style.borderBottom = "5px solid" + colorPallette.selectedHeaderButton;
            style.color = colorPallette.brighterTextColor;
        }
        else {
            style.cursor = "pointer";
            style.borderBottom = "5px solid transparent";
        }

        return (
           <div style={style} onClick={this.onClick}>{this.props.caption}</div>
            )
    }
}