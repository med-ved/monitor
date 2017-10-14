class BaseStore extends Reflux.Store {
    constructor() {
        super();
        this.cache = {};
        this.state = {
            loading: false
        };
    }

    getCacheKey(queryParams) {
        return queryParams.toString();
    }

    getData(params) {
        let key = this.getCacheKey(params.queryParams);
        if (key in this.cache) {
            this.setState({ loading: false });
            this.onDataReceived(this.cache[key], params);
        }
        else {
            this.requestData(params);
        }
    }

    dataReceived(data, params) {
        let key = this.getCacheKey(params.queryParams);
        this.cache[key] = data;

        this.setState({ loading: false });
        this.onDataReceived(data, params);
    }

    toJavascriptDate(dt) {
        dt = dt.replace(/[^0-9 +]/g, '');
        let date = new Date(parseInt(dt));
        return moment(date).valueOf();
    }

    prepareOneLineChartPart(data, field) {
        let result = [];
        for (let i = 0; i < data.length; i++) {
            let item = [this.toJavascriptDate(data[i].Date), +(data[i][field]).toFixed(0)];
            result.push(item);
        }

        return result;
    }

    prepareLineChartData(data) {
        return {
            revenue: this.prepareOneLineChartPart(data, "AvgEstimatedMonthlyRevenue"),
            occupacy: this.prepareOneLineChartPart(data, "AvgOccupacyPercent"),
            revenuePerDay: this.prepareOneLineChartPart(data, "AvgRevenuePerDay"),
        }
    }

    percentNameFunc(from, to) {
        return `&lt; ${to}%`;
    }

    revenueNameFunc(from, to) {
        return `&lt; ${to / 1000} ths. RUR`;
    }

    prepareDonutData(data, nameFunc) {
        return data.Data.map((item) => {
            return {
                name: nameFunc(item.From, item.To),
                y: item.Count
            }
        });
    }

    requestData(params) {
        this.setState({ loading: true });

        $.ajax({
            type: "POST",
            url: params.url,
            data: params.queryParams, 
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: (data) => this.dataReceived(data, params),
            error: (r) => {
                this.setState({ loading: false });
                alert(`Error: ${r.statusText}`);
            },
        });
    }

}