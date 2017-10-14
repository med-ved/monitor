class Common {
    static resizeChart(id, chart) {
        let parentEl = $("#" + id).parent();
        let height = parentEl.height();
        let width = parentEl.width();
        chart.setSize(width, height, doAnimation = false);
    }
}