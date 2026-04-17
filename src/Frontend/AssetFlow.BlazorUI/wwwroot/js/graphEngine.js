// ============================================================
// AssetFlow – Mémoire Intelligente – Graph Engine
// Canvas-based force-directed graph (no external deps)
// ============================================================

window.GraphEngine = (function () {
    let canvas, ctx, dotnetRef;
    let nodes = [], links = [];
    let animFrame, width, height;
    let intelligence = false;
    let hoveredNode = null;
    let selectedNode = null;
    let dragging = null;
    let mouse = { x: 0, y: 0 };
    let particles = [];

    // ── Palette ──────────────────────────────────────────────
    const COLORS = {
        materiel:    { fill: '#3b82f6', glow: 'rgba(59,130,246,0.5)', text: '#93c5fd' },
        incident:    { fill: '#ef4444', glow: 'rgba(239,68,68,0.5)',  text: '#fca5a5' },
        utilisateur: { fill: '#8b5cf6', glow: 'rgba(139,92,246,0.5)', text: '#c4b5fd' },
        ia:          { fill: '#10b981', glow: 'rgba(16,185,129,0.6)', text: '#6ee7b7' },
    };

    // ── Init ─────────────────────────────────────────────────
    function init(canvasId, ref) {
        canvas    = document.getElementById(canvasId);
        dotnetRef = ref;
        if (!canvas) return;
        ctx = canvas.getContext('2d');
        resize();
        window.addEventListener('resize', resize);
        canvas.addEventListener('mousemove', onMouseMove);
        canvas.addEventListener('click',     onClick);
        canvas.addEventListener('mousedown', onMouseDown);
        canvas.addEventListener('mouseup',   () => dragging = null);
        canvas.addEventListener('mouseleave',() => { hoveredNode = null; dragging = null; });
        spawnParticles();
        loop();
    }

    function resize() {
        if (!canvas) return;
        width  = canvas.parentElement?.offsetWidth  || 800;
        height = canvas.parentElement?.offsetHeight || 600;
        canvas.width  = width;
        canvas.height = height;
        layoutNodes();
    }

    // ── Data ─────────────────────────────────────────────────
    function setData(graphNodes, graphLinks) {
        nodes = graphNodes.map((n, i) => ({
            ...n,
            x:  width  * (0.2 + Math.random() * 0.6),
            y:  height * (0.2 + Math.random() * 0.6),
            vx: 0, vy: 0,
            radius: baseRadius(n),
            pulse: Math.random() * Math.PI * 2
        }));
        links = graphLinks;
    }

    function baseRadius(n) {
        const base = { materiel: 18, incident: 16, utilisateur: 14, ia: 28 };
        return (base[n.type] || 16) + (n.weight || 1) * 2;
    }

    // ── Physics ──────────────────────────────────────────────
    function layoutNodes() {
        if (!nodes.length) return;
        nodes.forEach(n => {
            n.x = width  * (0.15 + Math.random() * 0.7);
            n.y = height * (0.15 + Math.random() * 0.7);
        });
    }

    function tick() {
        const repulsion  = intelligence ? 5500 : 4000;
        const attraction = intelligence ? 0.04  : 0.02;
        const damping    = 0.88;
        const center     = { x: width / 2, y: height / 2 };

        // Gravity toward center
        nodes.forEach(n => {
            n.vx += (center.x - n.x) * 0.003;
            n.vy += (center.y - n.y) * 0.003;
        });

        // Node-node repulsion
        for (let i = 0; i < nodes.length; i++) {
            for (let j = i + 1; j < nodes.length; j++) {
                const a = nodes[i], b = nodes[j];
                const dx = a.x - b.x, dy = a.y - b.y;
                const dist = Math.sqrt(dx * dx + dy * dy) || 1;
                const force = repulsion / (dist * dist);
                const fx = (dx / dist) * force, fy = (dy / dist) * force;
                if (!a.pinned) { a.vx += fx; a.vy += fy; }
                if (!b.pinned) { b.vx -= fx; b.vy -= fy; }
            }
        }

        // Link attraction
        links.forEach(l => {
            const a = nodes.find(n => n.id === l.source);
            const b = nodes.find(n => n.id === l.target);
            if (!a || !b) return;
            const dx = b.x - a.x, dy = b.y - a.y;
            const dist = Math.sqrt(dx * dx + dy * dy) || 1;
            const strength = (l.strength || 0.5) * attraction;
            const fx = dx * strength, fy = dy * strength;
            if (!a.pinned) { a.vx += fx; a.vy += fy; }
            if (!b.pinned) { b.vx -= fx; b.vy -= fy; }
        });

        // Integrate
        nodes.forEach(n => {
            if (n === dragging) return;
            n.vx *= damping;
            n.vy *= damping;
            n.x  = Math.max(n.radius + 10, Math.min(width  - n.radius - 10, n.x + n.vx));
            n.y  = Math.max(n.radius + 10, Math.min(height - n.radius - 10, n.y + n.vy));
            n.pulse += intelligence ? 0.04 : 0.02;
        });
    }

    // ── Particles ────────────────────────────────────────────
    function spawnParticles() {
        particles = Array.from({ length: 40 }, () => ({
            x: Math.random() * (window.innerWidth || 800),
            y: Math.random() * (window.innerHeight || 600),
            r: Math.random() * 1.5 + 0.3,
            a: Math.random(),
            vx: (Math.random() - 0.5) * 0.3,
            vy: (Math.random() - 0.5) * 0.3
        }));
    }

    function drawParticles() {
        particles.forEach(p => {
            p.x += p.vx; p.y += p.vy;
            if (p.x < 0 || p.x > width)  p.vx *= -1;
            if (p.y < 0 || p.y > height) p.vy *= -1;
            ctx.beginPath();
            ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(59,130,246,${p.a * 0.25})`;
            ctx.fill();
        });
    }

    // ── Draw ─────────────────────────────────────────────────
    function draw() {
        ctx.clearRect(0, 0, width, height);
        drawParticles();
        drawLinks();
        drawNodes();
        if (hoveredNode) drawTooltip(hoveredNode);
    }

    function drawLinks() {
        links.forEach(l => {
            const a = nodes.find(n => n.id === l.source);
            const b = nodes.find(n => n.id === l.target);
            if (!a || !b) return;

            const isHighlighted = selectedNode &&
                (selectedNode.id === a.id || selectedNode.id === b.id);

            ctx.beginPath();
            ctx.moveTo(a.x, a.y);

            // Curved lines for aesthetics
            const mx = (a.x + b.x) / 2 + (Math.random() * 0 - 0); // static curve
            const my = (a.y + b.y) / 2;
            ctx.lineTo(b.x, b.y);

            ctx.strokeStyle = isHighlighted
                ? 'rgba(139,92,246,0.8)'
                : 'rgba(148,163,184,0.12)';
            ctx.lineWidth   = isHighlighted ? 1.5 : 0.8;
            ctx.stroke();

            // Animated dot along link (intelligence mode)
            if (intelligence && isHighlighted) {
                const t = (Date.now() % 2000) / 2000;
                const px = a.x + (b.x - a.x) * t;
                const py = a.y + (b.y - a.y) * t;
                ctx.beginPath();
                ctx.arc(px, py, 2.5, 0, Math.PI * 2);
                ctx.fillStyle = 'rgba(139,92,246,0.9)';
                ctx.fill();
            }
        });
    }

    function drawNodes() {
        nodes.forEach(n => {
            const col    = COLORS[n.type] || COLORS.materiel;
            const isHov  = hoveredNode  === n;
            const isSel  = selectedNode === n;
            const pulse  = Math.sin(n.pulse) * (intelligence ? 5 : 3);
            const r      = n.radius + (isHov ? 5 : 0);
            const glowR  = r + 8 + pulse;

            // Outer glow
            const grd = ctx.createRadialGradient(n.x, n.y, r * 0.5, n.x, n.y, glowR);
            grd.addColorStop(0, col.glow);
            grd.addColorStop(1, 'transparent');
            ctx.beginPath();
            ctx.arc(n.x, n.y, glowR, 0, Math.PI * 2);
            ctx.fillStyle = grd;
            ctx.fill();

            // Selection ring
            if (isSel) {
                ctx.beginPath();
                ctx.arc(n.x, n.y, r + 7, 0, Math.PI * 2);
                ctx.strokeStyle = col.fill;
                ctx.lineWidth   = 1.5;
                ctx.setLineDash([4, 4]);
                ctx.stroke();
                ctx.setLineDash([]);
            }

            // Node body
            const bodyGrd = ctx.createRadialGradient(n.x - r * 0.3, n.y - r * 0.3, 1, n.x, n.y, r);
            bodyGrd.addColorStop(0, lighten(col.fill, 40));
            bodyGrd.addColorStop(1, col.fill);
            ctx.beginPath();
            ctx.arc(n.x, n.y, r, 0, Math.PI * 2);
            ctx.fillStyle = bodyGrd;
            ctx.shadowColor = col.fill;
            ctx.shadowBlur  = intelligence ? 20 : 10;
            ctx.fill();
            ctx.shadowBlur  = 0;

            // Icon / label
            ctx.fillStyle   = '#fff';
            ctx.font        = `bold ${Math.max(9, r * 0.45)}px 'Segoe UI', sans-serif`;
            ctx.textAlign   = 'center';
            ctx.textBaseline= 'middle';
            ctx.fillText(typeIcon(n.type), n.x, n.y);

            // Label below
            ctx.fillStyle   = col.text;
            ctx.font        = `500 ${Math.max(8, r * 0.38)}px 'Segoe UI', sans-serif`;
            ctx.fillText(
                n.label.length > 10 ? n.label.slice(0, 10) + '…' : n.label,
                n.x, n.y + r + 11
            );
        });
    }

    function typeIcon(type) {
        return { materiel: '■', incident: '!', utilisateur: '●', ia: '◆' }[type] || '●';
    }

    function drawTooltip(n) {
        const col   = COLORS[n.type] || COLORS.materiel;
        const pad   = 10;
        const lines = [n.label, n.detail || ''].filter(Boolean);
        const w     = 200, lineH = 18;
        const h     = lines.length * lineH + pad * 2;
        let   tx    = n.x + n.radius + 12;
        let   ty    = n.y - h / 2;
        if (tx + w > width)  tx = n.x - n.radius - w - 12;
        if (ty < 0)          ty = 4;
        if (ty + h > height) ty = height - h - 4;

        // Background
        ctx.fillStyle   = 'rgba(15,23,42,0.95)';
        ctx.strokeStyle = col.fill;
        ctx.lineWidth   = 1;
        roundRect(ctx, tx, ty, w, h, 8);
        ctx.fill();
        ctx.stroke();

        // Text
        lines.forEach((line, i) => {
            ctx.fillStyle   = i === 0 ? '#e2e8f0' : '#94a3b8';
            ctx.font        = i === 0 ? 'bold 12px Segoe UI' : '11px Segoe UI';
            ctx.textAlign   = 'left';
            ctx.textBaseline= 'top';
            ctx.fillText(line.length > 28 ? line.slice(0, 28) + '…' : line, tx + pad, ty + pad + i * lineH);
        });
    }

    function roundRect(ctx, x, y, w, h, r) {
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.lineTo(x + w - r, y);
        ctx.quadraticCurveTo(x + w, y, x + w, y + r);
        ctx.lineTo(x + w, y + h - r);
        ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        ctx.lineTo(x + r, y + h);
        ctx.quadraticCurveTo(x, y + h, x, y + h - r);
        ctx.lineTo(x, y + r);
        ctx.quadraticCurveTo(x, y, x + r, y);
        ctx.closePath();
    }

    function lighten(hex, amount) {
        const num = parseInt(hex.replace('#', ''), 16);
        const r   = Math.min(255, (num >> 16) + amount);
        const g   = Math.min(255, ((num >> 8) & 0xff) + amount);
        const b   = Math.min(255, (num & 0xff) + amount);
        return `rgb(${r},${g},${b})`;
    }

    // ── Events ───────────────────────────────────────────────
    function getNode(e) {
        const rect = canvas.getBoundingClientRect();
        const mx = e.clientX - rect.left, my = e.clientY - rect.top;
        return nodes.find(n => {
            const dx = n.x - mx, dy = n.y - my;
            return Math.sqrt(dx * dx + dy * dy) <= n.radius + 5;
        });
    }

    function onMouseMove(e) {
        const rect = canvas.getBoundingClientRect();
        mouse.x = e.clientX - rect.left;
        mouse.y = e.clientY - rect.top;
        hoveredNode = getNode(e);
        canvas.style.cursor = hoveredNode ? 'pointer' : 'default';
        if (dragging) {
            dragging.x = mouse.x;
            dragging.y = mouse.y;
        }
    }

    function onClick(e) {
        const n = getNode(e);
        selectedNode = n || null;
        if (n && dotnetRef) {
            dotnetRef.invokeMethodAsync('OnNodeClicked', n.id);
        }
    }

    function onMouseDown(e) {
        dragging = getNode(e) || null;
        if (dragging) { dragging.pinned = true; }
    }

    // ── Loop ─────────────────────────────────────────────────
    function loop() {
        tick();
        draw();
        animFrame = requestAnimationFrame(loop);
    }

    function setIntelligenceMode(enabled) {
        intelligence = enabled;
    }

    function highlight(nodeId) {
        selectedNode = nodes.find(n => n.id === nodeId) || null;
    }

    function destroy() {
        cancelAnimationFrame(animFrame);
        window.removeEventListener('resize', resize);
    }

    return { init, setData, setIntelligenceMode, highlight, destroy, resize };
})();