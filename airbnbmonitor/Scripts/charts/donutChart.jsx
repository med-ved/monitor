class DonutChart extends Reflux.Component {
    constructor(props) {
        super(props);
        this.store = this.props.store;
        this.state = { [this.props.datafield]: null };
    }

    onStatusChange(status) {
        this.setState({
            currentStatus: status
        });
    }

    componentDidUpdate(prevProps, prevState) {
        if (prevState[this.props.datafield] !== this.state[this.props.datafield]) {
            let data = this.state[this.props.datafield];
            this.createChart(data);
        }
    }

    generateColors() {
        var colors = [],
        base = this.props.mainColor;

        for (let i = 0; i < 10; i ++) {
            colors.push(Highcharts.Color(base).brighten((i - 3) / 15).get());
        }

        return colors;
    }

    createChart(data) {
        this.chart = Highcharts.chart(this.props.id, {
            colors: this.generateColors(),
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                type: 'pie',
                backgroundColor: 'rgba(255, 255, 255, 0.0)'
            },
            title: {
                text: this.props.caption
            },
            tooltip: {
                pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    borderColor: colorPallette.widgetBodyColor,
                    dataLabels: {
                        enabled: true,
                        format: '<b>{point.name}</b>',
                        style: {
                            color: colorPallette.textColor
                        }
                    }
                }
            },
            series: [{
                size: '80%',
                innerSize: '60%',
                name: 'Value',
                data: data
            }]
        })
    }

    reflow() {
        if (this.chart) {
            Common.resizeChart(this.props.id, this.chart);
        }
    }

    componentDidMount() {
        window.addEventListener("resize", this.reflow.bind(this));
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.reflow.bind(this));
    }

    render() {
        let style = {
            flex: "1 0 100%",
            flexBasis: "100%",
        };
        Object.assign(style, this.props.style);

        return (<div data-debug="donutChart" id={this.props.id} style={style}> </div>);
    }
}