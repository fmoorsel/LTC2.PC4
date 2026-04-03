namespace LTC2.Desktopclients.WindowsClient.Utils
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

            window.routeMap.on('idle', () => {
                EnsureLayerOrder();        
            });


            AddTileLayer();

            window.layervisible = true;           
            
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

        console.log('route map is null try again');
    }

    return '0';
}

function EnsureLayerOrder() {
    const layers = window.routeMap.getStyle().layers;

    if (layers && layers.map(l => l.id).includes('fltc2tiles') && layers.map(l => l.id).includes('z-index-1'))
    {    
        window.routeMap.moveLayer('fltc2tiles', 'z-index-1');
    }
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


        window.routeMap.on('mousemove', 'fltc2tiles', (event) => {
            console.log('mouse move on tile layer');

            if (event.features.length > 0) {
                console.log(event.features[0].properties.popupContent);
                console.log(event.features[0].properties.featurePointer);

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

        const checkExprOpacity = window.createCheckExprOpacity();        
        const checkExprColor = window.createCheckExprColor();

        window.routeMap.setPaintProperty('fltc2tiles', 'fill-opacity', checkExprOpacity);
        window.routeMap.setPaintProperty('fltc2tiles', 'fill-color', checkExprColor);

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

function GetFillColorOnTrack() {
    const stryle = window.routeMap.getStyle().sprite;

    if (stryle.includes('satellite')) {
        return 'rgb(0, 0, 255)';
    } else if (stryle.includes('hybrid')) {
        return 'rgb(0, 0, 255)';
    } else if (stryle.includes('dark-standard')) {
        return 'rgb(0, 0, 255)';
    } else if (stryle.includes('winter')) {
        return 'rgb(0, 100, 0)';
    } else if (stryle.includes('light')) {
        return 'rgb(0, 100, 0)';
    } else if (stryle.includes('standard')) {
        return 'rgb(0, 100, 0)';
    }
}


function GetFillColorOnTrackNew() {
    const stryle = window.routeMap.getStyle().sprite;

    if (stryle.includes('satellite')) {
        return 'rgb(0, 255, 255)';
    } else if (stryle.includes('hybrid')) {
        return 'rgb(0, 255, 255)';
    } else if (stryle.includes('dark-standard')) {
        return 'rgb(0, 255, 255)';
    } else if (stryle.includes('winter')) {
        return 'rgb(0, 255, 0)';
    } else if (stryle.includes('light')) {
        return 'rgb(0, 255, 0)';
    } else if (stryle.includes('standard')) {
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

Init();
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

    setVisibility(window.layervisible);
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

    const layers = window.routeMap.getStyle().layers.filter(function (layer) {
        return layer.id.match(/^route-.*-polyline/);
    });

    let coordinateSources = new Set();

    layers.forEach(layer => {
        if (!coordinateSources.has(layer.source)) {
            coordinateSources.add(layer.source);
        }
    });


    coordinateSources.forEach(source => {
        const sourceData = window.routeMap.getSource(source)._data;

        if (sourceData.features) {
            sourceData.features.forEach(feature => {
                if (feature.geometry.type == 'LineString') {
                    lineCoordinates.push(feature.geometry.coordinates);
                }
            });
        }
    });


    console.log('Retrieved line coordinates:', JSON.stringify(lineCoordinates));

    return lineCoordinates;

}

console.log('>>> Retrieving line coordinates');

GetLines();
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
