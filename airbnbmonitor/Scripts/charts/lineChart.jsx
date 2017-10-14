class LineChart extends Reflux.Component {
    constructor(props) {
        super(props);
        this.store = this.props.store;
        this.state = { [this.props.dataField]: null };
    }

    onStatusChange(status) {
        this.setState({
            currentStatus: status
        });
    }

    componentDidUpdate(prevProps, prevState) {
        if (prevState[this.props.dataField] !== this.state[this.props.dataField]) {
            this.createChart(this.state[this.props.dataField]);
        }
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

    createChart(data) {
        this.chart = Highcharts.chart(this.props.id, {
            
            chart: {
                backgroundColor: 'rgba(255, 255, 255, 0.0)'
            },

            title: {
                text: this.props.caption,
            },
            tooltip: {
                formatter: function () {
                    var dt = new Date(this.x);
                    dt.setDate(dt.getDate() + 1);
                    var s = `<b>${Highcharts.dateFormat('%b %Y', +dt)}</b>`;

                    $.each(this.points, function (i, point) {
                        s += `<br /><span style="color:${point.color}">\u25CF</span><b> 
                            ${point.series.name}: ${point.y}${point.series.options.valueSuffix}</b>`;
                    });

                    return s;
                },
                shared: true
            },
            legend: {
                layout: 'horizontal',
                itemStyle: {
                    color: colorPallette.lineChartLegendColor,
                },
                itemHoverStyle: {
                    color: colorPallette.chartTextHoverColor,
                }
            },

            xAxis: {
                type: 'datetime',
                minTickInterval: +moment.duration(1, 'month'),
                labels: {
                    style: {
                        color: colorPallette.lineChartLegendColor
                    }
                },
            },

            yAxis: [{ // Revenue yAxis
                labels: {
                    format: '{value} RUR',
                    style: {
                        color: colorPallette.lineChartLegendColor
                    }
                },
                title: {
                    text: 'Roubles',
                    style: {
                        color: colorPallette.lineChartLegendColor
                    }
                },
                min: 0,
                gridLineColor: colorPallette.lineColor,
            }, { // Percent yAxis
                min: 0,
                max: 100,

                title: {
                    text: 'Percent',
                    style: {
                        color: colorPallette.lineChartLegendColor
                    }
                },
                labels: {
                    format: '{value}%',
                    
                    style: {
                        color: colorPallette.lineChartLegendColor
                    }
                },
                opposite: true,
                gridLineColor: colorPallette.lineColor,
            }],

            series: [{
                name: 'Revenue',
                color: colorPallette.revenueColor,
                yAxis: 0,
                data: data.revenue,
                valueSuffix: ' RUR',
            },
            {
                name: 'Occupacy',
                color: colorPallette.occupacyColor,
                yAxis: 1,
                data: data.occupacy,
                valueSuffix: '%'
            },
            {
                name: 'Revenue per day',
                color: colorPallette.revenuePerDayColor,
                yAxis: 0,
                data: data.revenuePerDay,
                valueSuffix: ' RUR',
            },
            ]
        })
    }

    render() {
        let style = {
            //flex: "1 0 100%",
            flexBasis: "100%",
        };
        Object.assign(style, this.props.style);

        return (
                <div id={this.props.id} data-debug="lineChart" style={style}> </div>
            );
    }
}