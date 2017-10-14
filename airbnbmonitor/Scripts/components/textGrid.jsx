const TextGrid = ({style, type, items}) => {
    let containerStyle = {
        flexBasis: "auto",
        paddingLeft: "5px",
        paddingTop: "5px",
    };
    Object.assign(containerStyle, style);

    return (
            <Container data-debug="textGrid" type={type} style={containerStyle} >
                { items.map((item) =>
                     <TextGridItem header={item[0]} text={item[1]} headerStyle={item[2]} valueStyle={item[3]} />) }
            </Container>
        );
}