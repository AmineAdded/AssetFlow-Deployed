// ============================================================
// wwwroot/scripts/apexCharts.interop.js — v3
// FIX : ApexCharts reçoit une hauteur px lue depuis le DOM,
//       jamais '100%'. C'est la seule façon fiable.
// ============================================================

window.ApexInterop = (function () {

    const _charts = {};

    function _destroy(id) {
        if (_charts[id]) {
            try { _charts[id].destroy(); } catch (_) {}
            delete _charts[id];
        }
    }

    /** Lit la hauteur réelle (px) du conteneur DOM, avec fallback. */
    function _getHeight(id, fallback) {
        const el = document.getElementById(id);
        if (!el) return fallback;
        const h = el.clientHeight || el.offsetHeight;
        return h > 40 ? h : fallback;
    }

    function _p(dark) {
        return {
            bg:    dark ? '#1a232e' : '#ffffff',
            grid:  dark ? '#2d3f55' : '#e2e8f0',
            label: dark ? '#94a3b8' : '#64748b',
            title: dark ? '#e2e8f0' : '#0f172a',
        };
    }

    const C = {
        blue:    '#136dec',
        indigo:  '#6366f1',
        emerald: '#10b981',
        amber:   '#f59e0b',
        rose:    '#f43f5e',
        purple:  '#a855f7',
        cyan:    '#06b6d4',
        orange:  '#f97316',
        teal:    '#14b8a6',
        lime:    '#84cc16',
    };

    /** Options de base communes à tous les graphes */
    function _base(id, dark, fallbackH) {
        const p = _p(dark);
        const h = _getHeight(id, fallbackH || 260);
        return {
            chart: {
                height:     h,           // ← px explicites lus depuis le DOM
                background: p.bg,
                foreColor:  p.label,
                fontFamily: "'Segoe UI', system-ui, sans-serif",
                toolbar: {
                    show:  true,
                    tools: { download: true, selection: false, zoom: false,
                             zoomin: false, zoomout: false, pan: false, reset: false },
                },
                animations:          { enabled: true, easing: 'easeinout', speed: 500 },
                parentHeightOffset:  0,
                redrawOnParentResize:true,
                redrawOnWindowResize:true,
                sparkline:           { enabled: false },
            },
            grid: {
                borderColor:    p.grid,
                strokeDashArray:3,
                padding:        { top: 0, right: 8, bottom: 0, left: 8 },
            },
            tooltip:    { theme: dark ? 'dark' : 'light' },
            legend:     { labels: { colors: p.label }, fontSize: '11px' },
            dataLabels: { enabled: false },
            xaxis: {
                labels:     { style: { colors: p.label, fontSize: '11px' } },
                axisBorder: { color: p.grid },
                axisTicks:  { color: p.grid },
            },
            yaxis: {
                labels: { style: { colors: p.label, fontSize: '11px' } },
            },
        };
    }

    function _render(id, options) {
        _destroy(id);
        const el = document.getElementById(id);
        if (!el) return;
        const chart = new ApexCharts(el, options);
        chart.render();
        _charts[id] = chart;
    }


    // ================================================================
    // 1. Articles par CATÉGORIE — Bar distribué (une couleur par barre)
    // ================================================================
    function renderArticlesParCategorie(containerId, data, dark, filtre) {
        if (!data || !data.length) return;
        const p    = _p(dark);
        const base = _base(containerId, dark, 260);

        let values, seriesName;
        if      (filtre === 'disponible') { values = data.map(d => d.disponibles); seriesName = 'Disponibles'; }
        else if (filtre === 'affecte')    { values = data.map(d => d.affectes);    seriesName = 'Affectés';    }
        else {
            values     = data.map(d => d.disponibles + d.affectes + d.horsService + d.enReparation);
            seriesName = 'Total articles';
        }

        const palette = [C.blue,C.emerald,C.indigo,C.amber,C.rose,C.cyan,C.purple,C.teal,C.orange,C.lime];
        const labels  = data.map(d => d.categorie);

        _render(containerId, {
            ...base,
            series: [{ name: seriesName, data: values }],
            chart: { ...base.chart, type: 'bar' },
            colors: palette.slice(0, labels.length),
            plotOptions: {
                bar: {
                    horizontal:   false,
                    borderRadius: 6,
                    columnWidth:  '38%',
                    distributed:  true,
                }
            },
            xaxis: {
                categories: labels,
                labels: {
                    style:    { colors: p.label, fontSize: '11px' },
                    rotate:   -20,
                    formatter: v => v && v.length > 14 ? v.slice(0,13)+'…' : v,
                },
                axisBorder: { color: p.grid },
                axisTicks:  { color: p.grid },
            },
            yaxis: { labels: { style: { colors: p.label } }, min: 0 },
            legend: { show: false },
            dataLabels: {
                enabled: true,
                style:   { fontSize: '10px', colors: ['#fff'] },
                formatter: v => v > 0 ? v : '',
                dropShadow: { enabled: false },
            },
            tooltip: {
                theme: dark ? 'dark' : 'light',
                y: { formatter: v => `${v} article(s)` },
            },
        });
    }


    // ================================================================
    // 2. État des demandes — Donut
    // ================================================================
    function renderEtatDemandes(containerId, data, dark) {
        if (!data) return;
        const p    = _p(dark);
        const base = _base(containerId, dark, 260);
        const series = [data.enAttente, data.commande, data.traite, data.refuse];
        const total  = series.reduce((a, b) => a + b, 0);

        _render(containerId, {
            ...base,
            series,
            chart: { ...base.chart, type: 'donut' },
            colors: [C.amber, C.indigo, C.emerald, C.rose],
            labels: ['En attente', 'Commandée', 'Traitée', 'Refusée'],
            plotOptions: {
                pie: {
                    donut: {
                        size: '62%',
                        labels: {
                            show:  true,
                            total: {
                                show:       true,
                                label:      'Total',
                                color:      p.label,
                                fontSize:   '12px',
                                fontWeight: 700,
                                formatter:  () => total,
                            },
                            value: { color: p.title, fontSize: '20px', fontWeight: 800 },
                        }
                    }
                }
            },
            legend:     { position: 'bottom', labels: { colors: p.label }, fontSize: '11px' },
            stroke:     { width: 2, colors: [p.bg] },
            dataLabels: { enabled: false },
        });
    }


    // ================================================================
    // 3. Demandes par semaine — Area stacked
    // ================================================================
    function renderDemandesParSemaine(containerId, data, dark) {
        if (!data || !data.length) return;
        const p    = _p(dark);
        const base = _base(containerId, dark, 240);

        _render(containerId, {
            ...base,
            series: [
                { name: 'En attente', data: data.map(d => d.enAttente) },
                { name: 'Commandée',  data: data.map(d => d.commande)  },
                { name: 'Traitée',    data: data.map(d => d.traite)    },
            ],
            chart: { ...base.chart, type: 'area', stacked: true },
            colors: [C.amber, C.indigo, C.emerald],
            fill: {
                type:     'gradient',
                gradient: { opacityFrom: 0.5, opacityTo: 0.05, shadeIntensity: 0.3 },
            },
            stroke:  { curve: 'smooth', width: 2 },
            xaxis: {
                categories: data.map(d => d.label),
                labels:     { style: { colors: p.label, fontSize: '11px' } },
                axisBorder: { color: p.grid },
                axisTicks:  { color: p.grid },
            },
            yaxis:   { labels: { style: { colors: p.label } }, min: 0 },
            legend:  { position: 'top', labels: { colors: p.label }, fontSize: '11px' },
            markers: { size: 3 },
        });
    }


    // ================================================================
    // 4. Affectation matériel — Pie
    // ================================================================
    function renderAffectationMateriel(containerId, data, dark) {
        if (!data) return;
        const p    = _p(dark);
        const base = _base(containerId, dark, 260);

        _render(containerId, {
            ...base,
            series: [data.affecte, data.nonAffecte],
            chart:  { ...base.chart, type: 'pie' },
            colors: [C.blue, C.cyan],
            labels: ['Affecté', 'Non affecté'],
            plotOptions: { pie: { expandOnClick: true } },
            legend:     { position: 'bottom', labels: { colors: p.label }, fontSize: '11px' },
            stroke:     { width: 2, colors: [p.bg] },
            dataLabels: {
                enabled:   true,
                formatter: (val, opts) => {
                    const n = opts.w.globals.series[opts.seriesIndex];
                    return `${n} (${Math.round(val)}%)`;
                },
                style:      { fontSize: '11px', colors: ['#fff'] },
                dropShadow: { enabled: false },
            },
        });
    }


    // ================================================================
    // 5. Articles par matériel — Bar stacked horizontal
    // ================================================================
    function renderArticlesParMateriel(containerId, data, dark, filtre) {
        if (!data || !data.length) return;
        const p    = _p(dark);
        const base = _base(containerId, dark, 260);

        const f      = filtre || 'all';
        let   series = [];
        if (f === 'all' || f === 'disponible') series.push({ name: 'Disponibles',  data: data.map(d => d.disponibles)  });
        if (f === 'all' || f === 'affecte')    series.push({ name: 'Affectés',      data: data.map(d => d.affectes)    });
        if (f === 'all') {
            series.push({ name: 'Hors service',  data: data.map(d => d.horsService)  });
            series.push({ name: 'En réparation', data: data.map(d => d.enReparation) });
        }

        const colorMap = {
            'Disponibles':  C.emerald, 'Affectés':      C.blue,
            'Hors service': C.rose,    'En réparation': C.orange,
        };

        _render(containerId, {
            ...base,
            series,
            chart: { ...base.chart, type: 'bar', stacked: true },
            colors: series.map(s => colorMap[s.name]),
            plotOptions: {
                bar: {
                    horizontal:  true,
                    borderRadius:4,
                    barHeight:   '60%',
                    borderRadiusWhenStacked: 'last',
                }
            },
            xaxis: {
                categories: data.map(d => d.designation),
                labels: {
                    style:    { colors: p.label, fontSize: '10px' },
                    maxWidth: 140,
                    formatter: v => v && v.length > 18 ? v.slice(0,17)+'…' : v,
                },
                axisBorder: { color: p.grid },
                axisTicks:  { color: p.grid },
            },
            yaxis:  { labels: { style: { colors: p.label, fontSize: '10px' } } },
            legend: { position: 'top', labels: { colors: p.label }, fontSize: '11px' },
            fill:   { opacity: 1 },
        });
    }


    // ================================================================
    // 6. Demandes 4 semaines du mois — Column grouped
    // ================================================================
    function renderDemandesSemaineDuMois(containerId, data, dark) {
        if (!data || !data.length) return;
        const p    = _p(dark);
        const base = _base(containerId, dark, 240);

        _render(containerId, {
            ...base,
            series: [
                { name: 'En attente', data: data.map(d => d.enAttente) },
                { name: 'Commandée',  data: data.map(d => d.commande)  },
                { name: 'Traitée',    data: data.map(d => d.traite)    },
            ],
            chart: { ...base.chart, type: 'bar' },
            colors: [C.amber, C.indigo, C.emerald],
            plotOptions: {
                bar: { borderRadius: 5, columnWidth: '65%' }
            },
            fill: {
                type:     'gradient',
                gradient: { type: 'vertical', shadeIntensity: 0.2, opacityFrom: 1, opacityTo: 0.75, stops: [0, 100] }
            },
            xaxis: {
                categories: data.map(d => d.label),
                labels: { style: { colors: p.label, fontSize: '12px' } },
                axisBorder: { color: p.grid },
                axisTicks:  { color: p.grid },
            },
            yaxis:  { labels: { style: { colors: p.label } }, min: 0 },
            legend: { position: 'top', labels: { colors: p.label }, fontSize: '11px' },
            dataLabels: {
                enabled:   true,
                style:     { fontSize: '10px', colors: [p.title] },
                formatter: v => v > 0 ? v : '',
                offsetY:   -4,
            },
            tooltip: { theme: dark ? 'dark' : 'light', shared: true, intersect: false },
        });
    }


    // ── API publique ──────────────────────────────────────────
    return {
        renderArticlesParCategorie,
        renderEtatDemandes,
        renderDemandesParSemaine,
        renderAffectationMateriel,
        renderArticlesParMateriel,
        renderDemandesSemaineDuMois,
        destroyChart: _destroy,
        destroyAll:   () => Object.keys(_charts).forEach(_destroy),
    };

})();
