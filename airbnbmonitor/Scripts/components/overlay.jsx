const Overlay = ({children, style, onClick}) => {
    
    let overlayStyle = {
        //display: this.state.loading ? "flex" : "none",
        height: "100vh",
        position: "absolute",
        width: "100vw",
        top: "0",
        backgroundColor: colorPallette.overlayColor,
        opacity: "0.8",
        //zIndex: 2
    };
    Object.assign(overlayStyle, style);

    return (
        <div style={overlayStyle} onClick={onClick}>{children}</div>
        );
}
