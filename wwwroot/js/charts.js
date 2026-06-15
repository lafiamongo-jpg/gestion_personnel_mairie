const chartInstances = {};

window.destroyChart = (id) => {
    if (chartInstances[id]) {
        chartInstances[id].destroy();
        delete chartInstances[id];
    }
};

const palette = {
    primary: '#3b82f6',
    success: '#10b981',
    warning: '#f59e0b',
    violet: '#8b5cf6',
    rose: '#f43f5e',
    slate: '#94a3b8',
    bars: ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#f43f5e', '#0ea5e9', '#c9a227']
};

window.renderBarChart = (id, labels, data) => {
    const ctx = document.getElementById(id);
    if (!ctx) return;
    destroyChart(id);

    const hasData = data.some(v => v > 0);
    if (!hasData) return;

    chartInstances[id] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Agents',
                data: data,
                backgroundColor: palette.bars.slice(0, labels.length),
                borderRadius: 6,
                maxBarThickness: 52
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#0f2137',
                    padding: 10,
                    cornerRadius: 8
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { color: '#64748b', font: { size: 11 } }
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        color: '#64748b',
                        font: { size: 11 },
                        precision: 0
                    },
                    grid: { color: 'rgba(148, 163, 184, 0.2)' },
                    suggestedMax: Math.max(...data, 1) + 0.5
                }
            }
        }
    });
};

window.renderDoughnutChart = (id, labels, data) => {
    const ctx = document.getElementById(id);
    if (!ctx) return;
    destroyChart(id);

    const filtered = labels
        .map((label, i) => ({ label, value: data[i] }))
        .filter(item => item.value > 0);

    if (filtered.length === 0) return;

    const colors = [palette.primary, palette.warning, palette.success, palette.violet, palette.rose, palette.slate];

    chartInstances[id] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: filtered.map(f => f.label),
            datasets: [{
                data: filtered.map(f => f.value),
                backgroundColor: colors.slice(0, filtered.length),
                borderWidth: 2,
                borderColor: '#f7f9fc',
                hoverOffset: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '62%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        color: '#64748b',
                        padding: 14,
                        usePointStyle: true,
                        pointStyle: 'circle',
                        font: { size: 11 }
                    }
                },
                tooltip: {
                    backgroundColor: '#0f2137',
                    padding: 10,
                    cornerRadius: 8
                }
            }
        }
    });
};
