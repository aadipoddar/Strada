// Minimal Leaflet live-fleet map for the WheelsEye demo. One map per element id,
// kept in a registry so the component can dispose its own.
const fleetMaps = new Map();

const STATUS_COLORS = { Moving: "#16a34a", Idle: "#f59e0b", Stopped: "#6b7280", Offline: "#b91c1c" };

function esc(s) {
	return (s ?? "").toString().replace(/[&<>]/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;" }[c]));
}

// A small status-coloured dot marker.
function pinIcon(color) {
	return L.divIcon({
		className: "fleet-pin",
		html: `<span style="display:block;width:18px;height:18px;border-radius:50%;background:${color};border:2px solid #fff;box-shadow:0 1px 4px rgba(0,0,0,.4)"></span>`,
		iconSize: [18, 18],
		iconAnchor: [9, 9],
	});
}

function popupHtml(v) {
	const color = STATUS_COLORS[v.status] ?? "#6b7280";
	const row = (label, value) =>
		value ? `<div style="color:#6b7280">${label}: <span style="color:#111;font-weight:600">${esc(value)}</span></div>` : "";
	return `<div style="font:13px/1.5 system-ui,sans-serif;min-width:170px">
		<div style="display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:4px">
			<span style="font-weight:700;font-size:14px">${esc(v.number)}</span>
			<span style="background:${color};color:#fff;font-size:11px;font-weight:600;padding:2px 8px;border-radius:999px">${esc(v.status)}</span>
		</div>
		${row("Speed", v.speed + " km/h")}
		${row("Ignition", v.ignition ? "On" : "Off")}
		${row("Type", v.type)}
		${row("Updated", v.updated)}
		${row("Location", v.address)}
	</div>`;
}

// Plot (or re-plot) the whole fleet. Creates the map on first call for an id,
// then just refreshes the markers on subsequent calls.
window.showFleet = function (elementId, vehicles) {
	const el = document.getElementById(elementId);
	if (!el || typeof L === "undefined") return;

	let inst = fleetMaps.get(elementId);
	if (!inst || inst.map.getContainer() !== el) {
		const map = L.map(el).setView([22.9734, 78.6569], 5); // centre of India
		L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
			maxZoom: 19,
			attribution: "&copy; OpenStreetMap contributors",
		}).addTo(map);
		inst = { map, layer: L.layerGroup().addTo(map) };
		fleetMaps.set(elementId, inst);
	}

	inst.layer.clearLayers();

	const points = [];
	for (const v of vehicles) {
		if (!v.lat && !v.lng) continue;
		const color = STATUS_COLORS[v.status] ?? "#6b7280";
		L.marker([v.lat, v.lng], { icon: pinIcon(color), title: v.number })
			.bindPopup(popupHtml(v))
			.addTo(inst.layer);
		points.push([v.lat, v.lng]);
	}

	if (points.length) inst.map.fitBounds(points, { padding: [40, 40], maxZoom: 12 });

	// Leaflet miscalculates size when its container was hidden during init
	// (the loading screen). Nudge it once the layout has settled.
	setTimeout(() => inst.map.invalidateSize(), 150);
};

window.disposeFleet = function (elementId) {
	const inst = fleetMaps.get(elementId);
	if (!inst) return;
	inst.map.remove();
	fleetMaps.delete(elementId);
};
