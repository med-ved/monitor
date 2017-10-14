const Container = ({ children, style, type = "column"}) => {
        let containerStyle = {
            display: "flex",
            flexBasis: "100%",
            flexDirection: type, // ? type : "column",
            flexWrap: "nowrap",
            justifyContent: "flex-start",
            boxSizing: "border-box"
        };
        Object.assign(containerStyle, style);

        return (
            <div data-debug="flexContainer" style={containerStyle}>
                {children}
            </div>);
}
