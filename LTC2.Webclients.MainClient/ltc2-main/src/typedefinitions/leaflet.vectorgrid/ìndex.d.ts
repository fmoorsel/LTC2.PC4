import * as L from "leaflet";

declare module "leaflet" {
  namespace vectorGrid {
    export function protobuf(url: string, options?: any): any;
    export function slicer(data: any, options?: any): any;
  }

  namespace canvas {
    export function tile(tileCoord: any, tileSize: any, opts: any): any
  }

  namespace DomEvent {
    function fakeStop(): boolean;
  }
}