const Component = ({ id, width, height, children, style}) => {
    let componentStyle = {
        width: width,
        height: height
    };
    Object.assign(componentStyle, style);

    return (
        <div id={id} style={componentStyle}>{children}</div>
        );
}
