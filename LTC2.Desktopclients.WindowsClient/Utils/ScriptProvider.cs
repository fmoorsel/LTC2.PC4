namespace LTC2.Desktopclients.WindowsClient.Utils
{
    public static class ScriptProvider
    {
        public static string GetStravaRouteBuilderScript()
        {
            var script = @"
function GetMapbox() {
    console.log('looking for mapbox element');

    var elements = document.getElementsByClassName('mapboxgl-map');
    if (elements[0] == null) {
        console.log('mapbox element not found');
    } else {
        var element = elements[0];
        Object.entries(element).find(([k, _]) => k.startsWith('__react'))[1].return.memoizedProps.mapboxRef((x) => (window.routeMap = x, x));

        if (window.routeMap != null) {
            console.log('route map found');

            console.log('try adding vector layer');

            window.routeMap.on('style.load', () => {
                console.log('changed layer style re-add layer to: ' + window.routeMap.getStyle().sprite);

                AddTileLayer();

                setVisibility(window.layervisible);
            });


            AddTileLayer();

            window.layervisible = true;           
            
            window.addEventListener('message', (event) => {
                console.log('message!');

                if (event.data == 'toggleLayer') {
                    console.log('toggleLayer message!');
                    
                    window.layervisible = !window.layervisible;

                    setVisibility();
                }
            });

            return '1';
        }

        console.log('route map is null try again');
    }

    return '0';
}

function setVisibility(visible) {
    if (window.routeMap != null) {
        const visibilty = window.layervisible ? 'visible' : 'none';
        
        window.routeMap.setLayoutProperty('ltc2tiles', 'visibility', visibilty);
        window.routeMap.setLayoutProperty('fltc2tiles', 'visibility', visibilty);
    }
}

function AddTileLayer() {
    if (window.routeMap != null) {
        console.log('adding vector layer');
        window.routeMap.addSource('ltc2tiles', {
            'type': 'vector',
            'tiles': [' http://localhost:50000/api/Tiles/tile/{z}/{x}/{y}.pbf'],
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
            }
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
                'line-opacity': 1.0,
                'line-color': GetColor(),
                'line-width': 2
            },
            slot: 'middle'
        });


        window.routeMap.on('mousemove', 'fltc2tiles', (event) => {
            console.log('mouse move on tile layer');

            if (event.features.length > 0) {
                console.log(event.features[0].properties.popupContent);

                if (window.chrome && window.chrome.webview) {
                    console.log(""posting message to webview"");
                    
                    const id = event.features[0].properties.featurePointer.split(':')[0];

                    const isChecked = GetVisitedAlltime().includes(id);
                    const isCheckedYear = GetVisitedYear().includes(id);

                    console.log('isChecked: ' + isChecked);
                    console.log('isCheckedYear: ' + isCheckedYear);

                    chrome.webview.postMessage(event.features[0].properties.popupContent + ',' + event.features[0].properties.featurePointer);
                }
            }
        });

        const checkExprVisitedAlltimeOpacity = [
                'match',
                    ['slice', ['get', 'featurePointer'], 0, ['index-of', ':', ['get', 'featurePointer']]],
                    GetVisitedAlltime(), 0.25, 
                    GetVisitedYear(), 0.45, 
                    0.0              
                ];

        window.routeMap.setPaintProperty('fltc2tiles', 'fill-opacity', checkExprVisitedAlltimeOpacity);

        window.routeMap.on('mousemove', (e) => {
            const features = window.routeMap.queryRenderedFeatures(e.point, {
                layers: ['fltc2tiles']
            });

            if (features.length <= 0) {
                if (window.chrome && window.chrome.webview) {
                    chrome.webview.postMessage(""----,----"");
                }
            }
        });
    } else {
        console.log('route map is null cannot add tile layer');
    }
}

function GetColor() {
    const stryle = window.routeMap.getStyle().sprite;

    if (stryle.includes('satellite')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('hybrid')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('dark-standard')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('winter')) {
        return 'rgb(0,0,0)';
    } else if (stryle.includes('light')) {
        return 'rgb(0,0,0)';
    } else if (stryle.includes('standard')) {
        return 'rgb(0,0,0)';
    }
}

function GetFillColor() {
    const stryle = window.routeMap.getStyle().sprite;

    if (stryle.includes('satellite')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('hybrid')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('dark-standard')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('winter')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('light')) {
        return 'rgb(255,165,0)';
    } else if (stryle.includes('standard')) {
        return 'rgb(255,165,0)';
    }
}

function GetVisitedAlltime() {
    return [""GetVisitedAlltime""];
}

function GetVisitedYear() {
    return [""GetVisitedYear""];
}

GetMapbox();
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

    var instance = window.rwgps.MapDelegate.getMapInstance();
    if (instance == null) {
        return '0';
    }

    console.log('RWGPS Map Instance found');

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

            setVisibility(window.layervisible);
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
    console.log('Adding layers to RWGPS Map');

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
        window.mapOverlay.setProps({
            layers: [ GetGoogleMapsLayer() ]
        });
    });

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

        onHover: info => {
            console.log('hover on tile layer');
            
            if (info.object) {
                const properties = info.object.properties;
      
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
        'tiles': [' http://localhost:50000/api/Tiles/tile/{z}/{x}/{y}.pbf'],
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
    });

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
            'line-opacity': 1.0,
            'line-color': GetColor(),
            'line-width': 2
        }
    });

    window.mapInstance.on('mousemove', 'fltc2tiles', (event) => {
        console.log('mouse move on tile layer');

        if (event.features.length > 0) {
            console.log(event.features[0].properties.popupContent);

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

    const checkExprVisitedAlltimeOpacity = [
            'match',
                ['slice', ['get', 'featurePointer'], 0, ['index-of', ':', ['get', 'featurePointer']]],
                GetVisitedAlltime(), 0.25, 
                GetVisitedYear(), 0.45, 
                0.0              
            ];

    window.mapInstance.setPaintProperty('fltc2tiles', 'fill-opacity', checkExprVisitedAlltimeOpacity);

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

            if (layers.length >= 1 && layers[layers.length-1] != 'ltc2tiles') {
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

    setVisibility(window.layervisible);
}

function GetColor() {
    return 'rgb(0,0,0)';
}

function GetGoogleMapsColor() {
    var currentType = window.mapInstance.getMapTypeId();

    if (currentType === 'roadmap') {
        return [0,0,0,255];
    } else if (currentType === 'satellite') {
        return [255,165,0, 255];
    } else if (currentType === 'hybrid') {
        return [255,165,0, 255];
    } else if (currentType === 'terrain') {
        return [0,0,0,255];
    }
}

function GetGoogleMapsFillColor(featurePointer) {
    const id = featurePointer.split(':')[0];
    const isChecked = GetVisitedAlltime().includes(id);
    const isCheckedYear = GetVisitedYear().includes(id);

    if (isCheckedYear) {
        return [255,165,0, 115];
    } else if (isChecked) {
        return [255,165,0, 65];
    } else {
        return [255,165,0,1];
    }
}

function GetFillColor() {
    return 'rgb(255,165,0)';
}

function GetVisitedAlltime() {
    return [""GetVisitedAlltime""];
}

function GetVisitedYear() {
    return [""GetVisitedYear""];
}

Init();            
            ";

            return script;
        }
    }
}
