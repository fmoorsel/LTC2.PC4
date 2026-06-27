using System.Collections.Generic;
using System.Linq;

namespace LTC2.Desktopclients.AvaloniaClient.Utils
{
    public static class ScriptProvider
    {
        public static string GetStravaRouteBuilderScript()
        {
            var script = @"
function Init() {
    console.log('looking for mapbox element');

    window.checkedPlaces = ['noplaces'];
    window.checkedNewPlaces = ['nonewplaces'];

    var stravaMap = window.strava.maps.getMap();
    if (stravaMap == null) {
        console.log('mapbox element not found');
    } else {
        window.routeMap = stravaMap;

        if (window.routeMap != null) {
            console.log('route map found');
            console.log('try adding vector layer');

            const style = new URLSearchParams(window.location.search).get('style');
            console.log('current map style: ' + style);

            window.currentStyle = style;

            const origReplace = history.replaceState.bind(history);
            history.replaceState = function(...args) {
                const result = origReplace(...args);

                AdaptToStyle();

                return result;
            };

            window.routeMap.on('idle', () => {
                AdaptToStyle();
            });

            AddTileLayer();

            let _pendingResizeSync = false;

            const stravaCanvas = document.getElementById('canvas');
            if (stravaCanvas) {
                new ResizeObserver(() => {
                    if (window.routeMap) window.routeMap.resize();
                    _pendingResizeSync = true;
                }).observe(stravaCanvas);
            }

            const wasmRef = findWasmRef();
            if (wasmRef && typeof wasmRef.addViewUpdateListener === 'function') {
                wasmRef.addViewUpdateListener({
                    onViewUpdated: () => {
                        if (!_pendingResizeSync || !window.routeMap) return;
                        _pendingResizeSync = false;
                        try {
                            const scale = wasmRef.getCamera().getScaleMetersPerPixel();
                            const lat = window.routeMap.getCenter().lat;
                            const zoom = Math.log2(2 * Math.PI * 6378137 * Math.cos(lat * Math.PI / 180) / (512 * scale));
                            if (Math.abs(zoom - window.routeMap.getZoom()) > 0.005) {
                                window.routeMap.setZoom(zoom);
                            }
                        } catch(e) {}
                    }
                });
            }

            window.layervisible = true;
            window.ltc2RouteVisible = false;

            window.addEventListener('message', (event) => {
                console.log('message!');

                if (event.data == 'toggleLayer') {
                    console.log('toggleLayer message!');

                    window.layervisible = !window.layervisible;

                    setVisibility();
                } else if (event.data.command == 'toggleLayer') {
                    console.log('toggleLayer message!');

                    window.layervisible = event.data.visible;

                    setVisibility();
                }
            });

            AddCanvasListener();

            window.addEventListener('strava:route:change', () => {
                if (window.ltc2RouteVisible) {
                    window.ltc2DrawRoute();
                }
            });

            return '1';

        } else {
            console.log('route map is null try again');
        }
    }

    return '0';
}

function findWasmRef() {
    try {
        const coreMap = document.querySelector('[class*=""CoreMap_mapContainer""]');
        if (!coreMap) return null;
        const rk = Object.keys(coreMap).find(k => k.startsWith('__reactFiber$'));
        if (!rk) return null;
        let fiber = coreMap[rk];
        for (let i = 0; i < 3; i++) fiber = fiber?.child;
        let state = fiber?.memoizedState;
        while (state) {
            const ref = state.queue?.lastRenderedState;
            if (ref?._djinni_native_ref) return ref;
            state = state.next;
        }
    } catch(e) {}
    return null;
}

function AdaptToStyle() {
    const style = new URLSearchParams(window.location.search).get('style');
    console.log('current map style: ' + style);

    if (window.currentStyle !== style) {
        console.log('changed layer style detected, re-adding layer to: ' + style);

        const hadRoute = window.ltc2RouteVisible;
        window.ltc2ClearRoute();

        window.routeMap.removeLayer('ltc2tiles');
        window.routeMap.removeLayer('fltc2tiles');
        window.routeMap.removeSource('ltc2tiles');

        AddTileLayer();

        if (hadRoute) {
            window.ltc2DrawRoute();
        }

        window.currentStyle = style;
    }

    setVisibility();
}

function AddCanvasListener() {
    const stravaCanvas = document.getElementById('canvas');

    if (stravaCanvas) {
        stravaCanvas.addEventListener('mousemove', (e) => {
            const map = window.routeMap;

            if (map) {
                const rect = stravaCanvas.getBoundingClientRect();
                const point = [e.clientX - rect.left, e.clientY - rect.top];

                const features = map.queryRenderedFeatures(point, { layers: ['fltc2tiles'] });

                if (features.length > 0) {
                    console.log(features[0].properties.popupContent);
                    console.log(features[0].properties.featurePointer);

                    if (window.chrome && window.chrome.webview) {
                        console.log(""posting message to webview"");

                        const id = features[0].properties.featurePointer.split(':')[0];

                        const isChecked = GetVisitedAlltime().includes(id);
                        const isCheckedYear = GetVisitedYear().includes(id);

                        console.log('isChecked: ' + isChecked);
                        console.log('isCheckedYear: ' + isCheckedYear);

                        chrome.webview.postMessage(features[0].properties.popupContent + ',' + features[0].properties.featurePointer);
                    }
                } else {
                    chrome.webview.postMessage(""----,----"");
                }
            }
      });
    }
}

window.ltc2DrawRoute = function() {
    try {
        const route = window.strava.maps.getCurrentRoute();
        if (!route || !route.features || route.features.length === 0) {
            return '0';
        }

        const features = route.features.map(f => ({
            type: 'Feature',
            geometry: f.geometry,
            properties: {}
        }));

        if (window.routeMap.getLayer('ltc2-route-line')) {
            window.routeMap.removeLayer('ltc2-route-line');
        }
        if (window.routeMap.getSource('ltc2-route')) {
            window.routeMap.removeSource('ltc2-route');
        }

        window.routeMap.addSource('ltc2-route', {
            type: 'geojson',
            data: { type: 'FeatureCollection', features: features }
        });

        window.routeMap.addLayer({
            id: 'ltc2-route-line',
            type: 'line',
            source: 'ltc2-route',
            layout: {
                'line-cap': 'round',
                'line-join': 'round',
                'visibility': window.layervisible ? 'visible' : 'none'
            },
            paint: {
                'line-color': 'rgb(250, 80, 0)',
                'line-width': 2,
                'line-opacity': 1.0
            }
        });

        window.ltc2RouteVisible = true;
        return '1';
    } catch(e) {
        console.error('ltc2DrawRoute error:', e);
        return '0';
    }
};

window.ltc2ClearRoute = function() {
    try {
        if (window.routeMap) {
            if (window.routeMap.getLayer('ltc2-route-line')) {
                window.routeMap.removeLayer('ltc2-route-line');
            }
            if (window.routeMap.getSource('ltc2-route')) {
                window.routeMap.removeSource('ltc2-route');
            }
        }
        window.ltc2RouteVisible = false;
    } catch(e) {
        console.error('ltc2ClearRoute error:', e);
    }
};

window.createCheckExprOpacity = function() {
    var expr = [
        'match',
            ['slice', ['get', 'featurePointer'], 0, ['index-of', ':', ['get', 'featurePointer']]],
            GetVisitedAlltime(), 0.25,
            GetVisitedYear(), 0.45,
            GetCheckedPlaces(), 0.45,
            GetCheckedNewPlaces(), 0.45,
            0.0
    ];

    return expr;
}

window.createCheckExprColor = function() {
    var expr = [
        'match',
            ['slice', ['get', 'featurePointer'], 0, ['index-of', ':', ['get', 'featurePointer']]],
            GetVisitedAlltime(), GetFillColor(),
            GetVisitedYear(), GetFillColor(),
            GetCheckedPlaces(), GetFillColorOnTrack(),
            GetCheckedNewPlaces(), GetFillColorOnTrackNew(),
            GetFillColor()
    ];

    return expr;
}

function setVisibility() {
    if (window.routeMap != null) {
        const is3d = new URLSearchParams(window.location.search).get('3d') === 'true';
        const effectiveVisible = window.layervisible && !is3d;
        const visibilty = effectiveVisible ? 'visible' : 'none';

        window.routeMap.setLayoutProperty('ltc2tiles', 'visibility', visibilty);
        window.routeMap.setLayoutProperty('fltc2tiles', 'visibility', visibilty);

        if (window.routeMap.getLayer('ltc2-route-line')) {
            const routeVisibility = (effectiveVisible && window.ltc2RouteVisible) ? 'visible' : 'none';
            window.routeMap.setLayoutProperty('ltc2-route-line', 'visibility', routeVisibility);
        }
    }
}


function AddTileLayer() {
    console.log('in adding vector layer');
    if (window.routeMap != null) {
        console.log('adding vector layer');
        window.routeMap.addSource('ltc2tiles', {
            'type': 'vector',
            'tiles': ['http://localhost:50000/api/Tiles/tile/{z}/{x}/{y}.pbf'],
            'minzoom': 3,
            'maxzoom': 17
        });

        window.routeMap.addLayer({
            'id': 'fltc2tiles',
            'type': 'fill',
            'source': 'ltc2tiles',
            'source-layer': 'geojsonLayer',
            'paint': {
                'fill-opacity': 0.0,
                'fill-color': GetFillColor()
            },
            slot: 'middle'
        });

        window.routeMap.addLayer({
            'id': 'ltc2tiles',
            'type': 'line',
            'source': 'ltc2tiles',
            'source-layer': 'geojsonLayer',
            'layout': {
                'line-cap': 'round',
                'line-join': 'round'
            },
            'paint': {
                'line-opacity': 0.5,
                'line-color': GetColor(),
                'line-width': 2
            },
            slot: 'middle'
        });

        const checkExprOpacity = window.createCheckExprOpacity();
        const checkExprColor = window.createCheckExprColor();

        window.routeMap.setPaintProperty('fltc2tiles', 'fill-opacity', checkExprOpacity);
        window.routeMap.setPaintProperty('fltc2tiles', 'fill-color', checkExprColor);

    } else {
        console.log('route map is null cannot add tile layer');
    }
}

function GetColor() {
    const style = new URLSearchParams(window.location.search).get('style');

    if (style.includes('satellite')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('hybrid')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('dark')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('dark-standard')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('winter')) {
        return 'rgb(0,0,0)';
    } else if (style.includes('light')) {
        return 'rgb(0,0,0)';
    } else if (style.includes('standard')) {
        return 'rgb(0,0,0)';
    } else {
        return 'rgb(0,0,0)';
    }
}

function GetFillColor() {
    const style = new URLSearchParams(window.location.search).get('style');

    if (style.includes('satellite')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('hybrid')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('dark-standard')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('dark')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('winter')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('light')) {
        return 'rgb(255,165,0)';
    } else if (style.includes('standard')) {
        return 'rgb(255,165,0)';
    } else {
        return 'rgb(255,165,0)';
    }
}

function GetFillColorOnTrack() {
    const style = new URLSearchParams(window.location.search).get('style');

    if (style.includes('satellite')) {
        return 'rgb(0, 0, 255)';
    } else if (style.includes('hybrid')) {
        return 'rgb(0, 0, 255)';
    } else if (style.includes('dark-standard')) {
        return 'rgb(0, 0, 255)';
    } else if (style.includes('winter')) {
        return 'rgb(0, 100, 0)';
    } else if (style.includes('light')) {
        return 'rgb(0, 100, 0)';
    } else if (style.includes('standard')) {
        return 'rgb(0, 100, 0)';
    }
}


function GetFillColorOnTrackNew() {
    const style = new URLSearchParams(window.location.search).get('style');

    if (style.includes('satellite')) {
        return 'rgb(0, 255, 255)';
    } else if (style.includes('hybrid')) {
        return 'rgb(0, 255, 255)';
    } else if (style.includes('dark-standard')) {
        return 'rgb(0, 255, 255)';
    } else if (style.includes('winter')) {
        return 'rgb(0, 255, 0)';
    } else if (style.includes('light')) {
        return 'rgb(0, 255, 0)';
    } else if (style.includes('standard')) {
        return 'rgb(0, 255, 0)';
    }
}

function GetCheckedPlaces() {
    console.log('Getting checked places');

    const filteredPlaces = window.checkedPlaces.filter(p => !window.checkedNewPlaces.includes(p));

    return filteredPlaces;
}

function GetCheckedNewPlaces() {
    return window.checkedNewPlaces;
}

function GetVisitedAlltimeUnfiltered() {
    return [""GetVisitedAlltime""];
}

function GetVisitedAlltime() {
    const alltime = GetVisitedAlltimeUnfiltered();

    const filteredAlltime = alltime.filter(at =>
        !window.checkedPlaces.includes(at) && !window.checkedNewPlaces.includes(at)
    );

    return filteredAlltime;
}

function GetVisitedYear() {
    const year = [""GetVisitedYear""];

    const filteredYear = year.filter(y =>
        !window.checkedPlaces.includes(y) && !window.checkedNewPlaces.includes(y)
    );

    return filteredYear;
}

try {
    Init();
} catch (error) {
    console.error(error);
}

            ";

            return script;
        }

        public static string GetRwGpsRouteBuilderScript()
        {
            var script = @"
console.log('Start Initializing RWGPS');

function addScript(url, id) {
    console.log('Adding script: ' + url);

    const script = document.createElement('script');

    if (id == 'ltc2-deckgl') {
        script.onload = () => {
            console.log('Deck.gl script loaded');

            AddLayers();
        };
    }

    script.src = url;
    script.id = id;

    document.head.appendChild(script);
}

function Init() {
    console.log('Initializing RWGPS');

    window.checkedPlaces = ['noplaces'];
    window.checkedNewPlaces = ['nonewplaces'];

    var instance = window.rwgps.MapDelegate.getMapInstance();
    if (instance == null) {
        return '0';
    }

    console.log('RWGPS Map Instance found');

    window.checkedPlaces = ['noplaces'];
    window.checkedNewPlaces = ['nonewplaces'];

    window.mapInstance = instance;
    window.layervisible = true;
    window.currentvisibility = true;

    console.log('RWGPS Map Instance lyer: ' + window.mapInstance.getLayersOrder().length);

    if (!window.rwgps.MapDelegate.props.mapInstance.__gm) {
        instance.on('idle', () => {
            console.log('RWGPS Map idle');

            LayerControl();
        });

        instance.on('render', LayerControl);
    }

    setInterval(LayerControl, 200);

    window.addEventListener('message', (event) => {
        console.log('message!');

        if (event.data == 'toggleLayer') {
            console.log('toggleLayer message!');

            window.layervisible = !window.layervisible;

            setVisibility();
        } else if (event.data.command == 'toggleLayer') {
            console.log('toggleLayer message!');

            window.layervisible = event.data.visible;

            setVisibility();
        }
    });

    return '1';
}

function setVisibility(visible) {
    if (window.mapInstance != null) {
        if (window.rwgps.MapDelegate.props.mapInstance.__gm) {
            if (window.mapOverlay != null && window.currentvisibility != window.layervisible) {
                window.mapOverlay.setProps({
                    layers: [ GetGoogleMapsLayer() ]
                });
            }
        } else {
            const visibilty = window.layervisible ? 'visible' : 'none';
            const currentVisibility = window.mapInstance.getLayoutProperty('ltc2tiles', 'visibility');

            if (currentVisibility != visibilty) {
                console.log('setting layer visibility to: ' + visibilty);

                window.mapInstance.setLayoutProperty('ltc2tiles', 'visibility', visibilty);
                window.mapInstance.setLayoutProperty('fltc2tiles', 'visibility', visibilty);
            }
        }

        window.currentvisibility = visible;
    }
}

function AddLayers(){
    if (window.addinglayers) {
        return;
    }

    window.addinglayers = true;

    console.log('Adding layers to RWGPS Map');
    try {
        if (window.rwgps.MapDelegate.props.mapInstance.__gm) {
            console.log('RWGPS Map Instance is Google Maps, not supported yet');

            AddGoogleMapsLayer();

            return;
        } else {
            console.log('RWGPS Map Instance is MapLibre, adding layers');

            if (window.chrome && window.chrome.webview) {
                chrome.webview.postMessage(""----,----"");
            }

            AddLayersMapLibre();
        }
    } catch (error) {
        console.log('Error adding layers: ' + error);
    } finally {
        window.addinglayers = null;
    }
}

function AddGoogleMapsLayer() {

    const { GoogleMapsOverlay, MVTLayer } = deck;
    const mvtLayer = GetGoogleMapsLayer();

    const overlay = new GoogleMapsOverlay({
        layers: [mvtLayer]
    });

    overlay.setMap(window.mapInstance);

    window.mapInstance.addListener('maptypeid_changed', () => {
        console.log('Google Maps maptypeid changed, updating layer colors');
        window.currentStatus = Date.now();
        window.mapOverlay.setProps({
            layers: [ GetGoogleMapsLayer() ]
        });
    });

    window.currentStatus = Date.now();
    window.mapOverlay = overlay;
}

function GetGoogleMapsLayer() {
    const layer = new deck.MVTLayer({
        id: 'ltc2tiles',
        data: 'http://localhost:50000/api/Tiles/tile/{z}/{x}/{y}.pbf',

        getFillColor: feature => GetGoogleMapsFillColor(feature.properties.featurePointer),
        getLineColor: GetGoogleMapsColor(),

        lineWidthMinPixels: 2,
        pickable: true,
        visible: window.layervisible,

        updateTriggers: {
            getFillColor: [window.currentStatus]
        },

        onHover: info => {
            console.log('hover on tile layer');

            if (info.object) {
                const properties = info.object.properties;

                console.log(properties.popupContent);
                console.log(properties.featurePointer);

                const id = properties.featurePointer.split("":"")[0];

                const isChecked = GetVisitedAlltime().includes(id);
                const isCheckedYear = GetVisitedYear().includes(id);

                console.log(""isChecked: "" + isChecked);
                console.log(""isCheckedYear: "" + isCheckedYear);

                chrome.webview.postMessage(properties.popupContent + "","" + properties.featurePointer);
            } else {
                if (window.chrome && window.chrome.webview) {
                    chrome.webview.postMessage(""----,----"");
                }
            }
        }
    });

    return layer;
}

function AddLayersMapLibre()
{
    window.mapInstance.addSource('ltc2tiles', {
        'type': 'vector',
        'tiles': ['http://localhost:50000/api/Tiles/tile/{z}/{x}/{y}.pbf'],
        'minzoom': 3,
        'maxzoom': 17
    });

    window.mapInstance.addLayer({
        'id': 'fltc2tiles',
        'type': 'fill',
        'source': 'ltc2tiles',
        'source-layer': 'geojsonLayer',
        'paint': {
            'fill-opacity': 0.3,
            'fill-color': GetFillColor()
        }
    },
    'global_heatmap');

    window.mapInstance.addLayer({
        'id': 'ltc2tiles',
        'type': 'line',
        'source': 'ltc2tiles',
        'source-layer': 'geojsonLayer',
        'layout': {
            'line-cap': 'round',
            'line-join': 'round'
        },
        'paint': {
            'line-opacity': 0.5,
            'line-color': GetColor(),
            'line-width': 2
        }
    });

    window.mapInstance.on('mousemove', 'fltc2tiles', (event) => {
        console.log('mouse move on tile layer');

        if (event.features.length > 0) {
            console.log(event.features[0].properties.popupContent);
            console.log(event.features[0].properties.featurePointer);

            if (window.chrome && window.chrome.webview) {
                console.log('posting message to webview');

                const id = event.features[0].properties.featurePointer.split(':')[0];

                const isChecked = GetVisitedAlltime().includes(id);
                const isCheckedYear = GetVisitedYear().includes(id);

                console.log('isChecked: ' + isChecked);
                console.log('isCheckedYear: ' + isCheckedYear);

                chrome.webview.postMessage(event.features[0].properties.popupContent + ',' + event.features[0].properties.featurePointer);
            }
        }
    });


    const checkExprOpacity = window.createCheckExprOpacity();
    const checkExprColor = window.createCheckExprColor();

    window.mapInstance.setPaintProperty('fltc2tiles', 'fill-opacity', checkExprOpacity);
    window.mapInstance.setPaintProperty('fltc2tiles', 'fill-color', checkExprColor);

    window.mapInstance.on('mousemove', (e) => {
        const features = window.mapInstance.queryRenderedFeatures(e.point, {
            layers: ['fltc2tiles']
        });

        if (features.length <= 0) {
            if (window.chrome && window.chrome.webview) {
                chrome.webview.postMessage('----,----');
            }
        }
    });
}

window.createCheckExprOpacity = function() {
    var expr = [
        'match',
            ['slice', ['get', 'featurePointer'], 0, ['index-of', ':', ['get', 'featurePointer']]],
            GetVisitedAlltime(), 0.25,
            GetVisitedYear(), 0.45,
            GetCheckedPlaces(), 0.45,
            GetCheckedNewPlaces(), 0.45,
            0.0
    ];

    return expr;
}

window.createCheckExprColor = function() {
    var expr = [
        'match',
            ['slice', ['get', 'featurePointer'], 0, ['index-of', ':', ['get', 'featurePointer']]],
            GetVisitedAlltime(), GetFillColor(),
            GetVisitedYear(), GetFillColor(),
            GetCheckedPlaces(), GetFillColorOnTrack(),
            GetCheckedNewPlaces(), GetFillColorOnTrackNew(),
            GetFillColor()
    ];

    return expr;
}

function LayerControl()
{
    var instance = window.rwgps.MapDelegate.getMapInstance();

    if (instance != window.mapInstance) {

        console.log('RWGPS Map Instance changed');

        window.mapInstance = instance;

        if (window.rwgps.MapDelegate.props.mapInstance.__gm)
        {
            console.log('RWGPS Map Instance is now Google Maps');

            if (document.getElementById('ltc2-deckgl')) {
                console.log('Deck.gl script already added');

                AddLayers();
            } else {
                addScript('https://unpkg.com/deck.gl@9.2.11/dist.min.js', 'ltc2-deckgl');
            }

        } else {
            console.log('RWGPS Map Instance layers changed, adding layers');

            AddLayers();

            instance.on('idle', () => {
                console.log('RWGPS Map idle');

                LayerControl();
            });

            instance.on('render', LayerControl);
        }
    } else {
        if (!window.rwgps.MapDelegate.props.mapInstance.__gm) {
            var layers = window.mapInstance.getLayersOrder();

            if (layers.length >= 1 && !layers.includes('ltc2tiles')) {
                console.log('RWGPS Map Instance layers changed, re-adding layers');

                AddLayers();

                instance.on('idle', () => {
                    console.log('RWGPS Map idle');

                    LayerControl();
                });

                instance.on('render', LayerControl);
            }
        }
    }

    setVisibility();
}

function GetColor() {
    return 'rgb(0,0,0)';
}

function GetGoogleMapsColor() {
    const currentType = window.mapInstance.getMapTypeId();

    if (currentType === 'roadmap') {
        return [0,0,0, 128];
    } else if (currentType === 'satellite') {
        return [255,165,0, 128];
    } else if (currentType === 'hybrid') {
        return [255,165,0, 128];
    } else if (currentType === 'terrain') {
        return [0,0,0, 128];
    }
}

function GetGoogleMapsFillColor(featurePointer) {
    const id = featurePointer.split(':')[0];
    const isChecked = GetVisitedAlltime().includes(id);
    const isCheckedYear = GetVisitedYear().includes(id);
    const isCheckedOnTrack = GetCheckedPlaces().includes(id);
    const isCheckedOnTrackNew = GetCheckedNewPlaces().includes(id);

    const currentType = window.mapInstance.getMapTypeId();
    const isDarkType = (currentType === 'satellite' || currentType === 'hybrid');

    if (isCheckedYear) {
        return [255,165,0, 115];
    } else if (isChecked) {
        return [255,165,0, 65];
    } else if (isCheckedOnTrack) {
        return isDarkType ? [0, 0, 255, 115] : [0, 100, 0, 115];
    } else if (isCheckedOnTrackNew) {
        return isDarkType ? [0, 255, 255, 115] : [0, 255, 0, 115];
    } else {
        return [255,165,0,1];
    }
}

function GetFillColor() {
    return 'rgb(255,165,0)';
}

function GetFillColorOnTrack() {
    return 'rgb(0, 100, 0)';
}

function GetFillColorOnTrackNew() {
    return 'rgb(0, 255, 0)';
}

function GetCheckedPlaces() {
    console.log('Getting checked places');

    const filteredPlaces = window.checkedPlaces.filter(p => !window.checkedNewPlaces.includes(p));

    return filteredPlaces;
}

function GetCheckedNewPlaces() {
    return window.checkedNewPlaces;
}

function GetVisitedAlltimeUnfiltered() {
    return [""GetVisitedAlltime""];
}

function GetVisitedAlltime() {
    const alltime = GetVisitedAlltimeUnfiltered();

    const filteredAlltime = alltime.filter(at =>
        !window.checkedPlaces.includes(at) && !window.checkedNewPlaces.includes(at)
    );

    return filteredAlltime;
}

function GetVisitedYear() {
    const year = [""GetVisitedYear""];

    const filteredYear = year.filter(y =>
        !window.checkedPlaces.includes(y) && !window.checkedNewPlaces.includes(y)
    );

    return filteredYear;
}

Init();
            ";

            return script;
        }

        public static string GetStravaTrackRetrievalScript()
        {
            var script = @"

function GetLines() {
    console.log('Retrieving line coordinates');

    let lineCoordinates = [];

    const route = window.strava.maps.getCurrentRoute();

    if (route.features) {
        route.features.forEach(feature => {
            lineCoordinates.push(feature.geometry.coordinates);
        });
    }

    console.log('Retrieved line coordinates:', JSON.stringify(lineCoordinates));

    return lineCoordinates;

}

console.log('>>> Retrieving line coordinates');

try {
    GetLines();
} catch (error) {
    console.error(error);
}
            ";

            return script;
        }

        public static string GetStravaTrackUpdateScript(List<string> places)
        {
            var placesForScript = places.Select(p => $"'{p}'").ToList();
            var placesString = string.Join(",", placesForScript);

            var setPlacesScript = places.Count > 0 ? $"window.checkedPlacesTrack = [{placesString}];" : "window.checkedPlacesTrack = ['noplaces'];";

            var script = setPlacesScript + @"

function GetVisitedAlltimeForTrack () {
    return [""GetVisitedAlltime""];
}

function UpdateMap() {
    const checkExprOpacity = window.createCheckExprOpacity();
    const checkExprColor = window.createCheckExprColor();

    window.routeMap.setPaintProperty('fltc2tiles', 'fill-opacity', checkExprOpacity);
    window.routeMap.setPaintProperty('fltc2tiles', 'fill-color', checkExprColor);

    window.routeMap.triggerRepaint();

    console.log('Map updated with new places');
}

function UpdatePlacesOnTrack() {
    const allTimeResult = GetVisitedAlltimeForTrack();
    const newPlaces = window.checkedPlacesTrack.filter(p => !allTimeResult.includes(p));

    console.log('New places on track:', JSON.stringify(newPlaces));

    console.log('All time visited places:', JSON.stringify(allTimeResult));
    console.log('Checked places:', JSON.stringify(window.checkedPlacesTrack));
    console.log('New places:', JSON.stringify(newPlaces));

    window.checkedNewPlaces = newPlaces.length > 0 ? newPlaces : ['nonewplaces'];

    console.log('>>Checked new places set to:', JSON.stringify(window.checkedNewPlaces));

    const filterCheckedPlaces = window.checkedPlacesTrack.filter(p => !window.checkedNewPlaces.includes(p));
    console.log('>>Checked filterCheckedPlaces set to:', JSON.stringify(filterCheckedPlaces));

    window.checkedPlaces = filterCheckedPlaces.length > 0 ? filterCheckedPlaces : ['justaplace'];

    console.log('Places set to:', JSON.stringify(window.checkedPlaces));
    console.log('New Places set to:', JSON.stringify(window.checkedNewPlaces));

    UpdateMap();
}

UpdatePlacesOnTrack();
";

            return script;
        }

        public static string GetRwGpsTrackRetrievalScript()
        {
            var script = @"

function GetLines() {
    console.log('Retrieving line coordinates');

    let lineCoordinates = [];

    const mapCoordinatesSources = Routes.sampleGraph._data._points;

    mapCoordinatesSources.forEach(source => {
        if (source.point && source.point.lng && source.point.lat) {
            const coordinates = [ source.point.lng, source.point.lat];
            lineCoordinates.push(coordinates);
        }
    });

    console.log('Retrieved line coordinates:', JSON.stringify(lineCoordinates));

    return lineCoordinates;
}

GetLines();
            ";

            return script;
        }

        public static string GetStravaDrawRouteScript()
        {
            return "window.ltc2DrawRoute()";
        }

        public static string GetStravaClearRouteScript()
        {
            return "window.ltc2ClearRoute()";
        }

        public static string GetRwGpsTrackUpdateScript(List<string> places)
        {
            var placesForScript = places.Select(p => $"'{p}'").ToList();
            var placesString = string.Join(",", placesForScript);

            var setPlacesScript = places.Count > 0 ? $"window.checkedPlacesTrack = [{placesString}];" : "window.checkedPlacesTrack = ['noplaces'];";

            var script = setPlacesScript + @"

function GetVisitedAlltimeForTrack () {
    return [""GetVisitedAlltime""];
}

function UpdateMapMapLibre() {
    const checkExprOpacity = window.createCheckExprOpacity();
    const checkExprColor = window.createCheckExprColor();

    window.mapInstance.setPaintProperty('fltc2tiles', 'fill-opacity', checkExprOpacity);
    window.mapInstance.setPaintProperty('fltc2tiles', 'fill-color', checkExprColor);

    window.mapInstance.triggerRepaint();

    console.log('Map updated with new places');
}

function UpdateMapGoogleMaps() {
    console.log('Start Updating Google Maps layer with new places');

    if (window.mapOverlay != null) {
        console.log('Updating Google Maps layer with new places');
        window.currentStatus = Date.now(); // trigger updateTriggers in deck.gl layer
        window.mapOverlay.setProps({
            layers: [ GetGoogleMapsLayer() ]
        });
    }
}

function UpdatePlacesOnTrack() {
    console.log('RWGPS Map Instance is MapLibre, updating track places');

    const allTimeResult = GetVisitedAlltimeForTrack();
    const newPlaces = window.checkedPlacesTrack.filter(p => !allTimeResult.includes(p));

    console.log('New places on track:', JSON.stringify(newPlaces));

    console.log('All time visited places:', JSON.stringify(allTimeResult));
    console.log('Checked places:', JSON.stringify(window.checkedPlacesTrack));
    console.log('New places:', JSON.stringify(newPlaces));

    window.checkedNewPlaces = newPlaces.length > 0 ? newPlaces : ['nonewplaces'];

    console.log('>>Checked new places set to:', JSON.stringify(window.checkedNewPlaces));

    const filterCheckedPlaces = window.checkedPlacesTrack.filter(p => !window.checkedNewPlaces.includes(p));
    console.log('>>Checked filterCheckedPlaces set to:', JSON.stringify(filterCheckedPlaces));

    window.checkedPlaces = filterCheckedPlaces.length > 0 ? filterCheckedPlaces : ['justaplace'];

    console.log('Places set to:', JSON.stringify(window.checkedPlaces));
    console.log('New Places set to:', JSON.stringify(window.checkedNewPlaces));

    if (window.rwgps.MapDelegate.props.mapInstance.__gm) {
        UpdateMapGoogleMaps();
    } else {
        UpdateMapMapLibre();
    }
}

UpdatePlacesOnTrack();
";

            return script;
        }
    }
}
