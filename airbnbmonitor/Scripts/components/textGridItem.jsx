const TextGridItem = ({valueStyle, headerStyle, header, text}) => {

    let itemStyle = {
        borderLeft: "2px solid " + colorPallette.selectedHeaderButton,
        paddingLeft: "5px",
        paddingRight: "10px",
        marginBottom: "2px",
        color: colorPallette.textColor
    };

    let finalValueStyle = {
        paddingLeft: "5px",
        whiteSpace: "nowrap"
    };
    Object.assign(finalValueStyle, valueStyle);

    return (
            <div style={itemStyle}>
                <span style={headerStyle}>{header}</span>
                <span style={finalValueStyle}>{text}</span>
            </div>
        );
}