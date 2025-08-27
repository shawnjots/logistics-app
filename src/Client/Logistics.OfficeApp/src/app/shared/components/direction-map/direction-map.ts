import {HttpClient} from "@angular/common/http";
import {Component, effect, inject, input, model, output, signal} from "@angular/core";
import {GeoJSONSourceSpecification, LngLatLike} from "mapbox-gl";
import {ImageComponent, LayerComponent, MapComponent} from "ngx-mapbox-gl";
import {firstValueFrom} from "rxjs";
import {environment} from "@/env";
import {GeoPoint, MapboxDirectionsResponse} from "@/shared/types/mapbox";

interface Segment {
  layerId: string;
  source: GeoJSONSourceSpecification;
}

@Component({
  selector: "app-direction-map",
  templateUrl: "./direction-map.html",
  imports: [MapComponent, ImageComponent, LayerComponent],
})
export class DirectionMap {
  protected readonly accessToken = environment.mapboxToken;
  private readonly defaultCenter: GeoPoint = [-95, 35];
  private readonly defaultZoom = 3;
  private renderId = 0;
  protected readonly segmentColors = [
    "#28a745", // green
    "#0074D9", // blue
    "#FF4136", // red
    "#FF851B", // orange
    "#B10DC9", // purple
  ];

  private readonly http = inject(HttpClient);

  //#region States

  protected readonly segments = signal<Segment[]>([]);
  protected readonly stopsSource = signal<GeoJSONSourceSpecification | null>(null);
  protected readonly bounds = signal<[LngLatLike, LngLatLike] | null>(null);
  protected readonly imageLoaded = signal(false);

  //#endregion

  //#region Inputs

  public readonly center = model<GeoPoint>(this.defaultCenter);
  public readonly zoom = model<number>(this.defaultZoom);
  public readonly stops = input<GeoPoint[]>([]);
  public readonly width = input<string>("100%");
  public readonly height = input<string>("100%");

  //#endregion

  //#region Outputs

  public readonly routeChanged = output<RouteChangedEvent>();

  //#endregion

  constructor() {
    effect(() => this.draw());
  }

  private async draw(): Promise<void> {
    const pts = this.stops();

    if (!pts || pts.length < 2 || !this.coordsAreValid(pts)) {
      return this.clear();
    }

    // build stop FeatureCollection with labels
    this.stopsSource.set({
      type: "geojson",
      data: {
        type: "FeatureCollection",
        features: pts.map((p, i) => ({
          type: "Feature",
          geometry: {type: "Point", coordinates: p},
          properties: {label: (i + 1).toString()},
        })),
      },
    });

    // build one LineString per leg
    const segSources: Segment[] = [];
    const cycle = ++this.renderId;

    for (let i = 0; i < pts.length - 1; i++) {
      const segment = await this.buildDirectionsSegment(pts[i], pts[i + 1]);

      segSources.push({
        layerId: `seg-${cycle}-${i}`,
        source: segment,
      });
    }
    this.segments.set(segSources);

    // fit bounds
    const minX = Math.min(...pts.map((p) => p[0]));
    const minY = Math.min(...pts.map((p) => p[1]));
    const maxX = Math.max(...pts.map((p) => p[0]));
    const maxY = Math.max(...pts.map((p) => p[1]));
    this.bounds.set([
      [minX, minY],
      [maxX, maxY],
    ]);
    this.center.set([(minX + maxX) / 2, (minY + maxY) / 2]);

    this.routeChanged.emit({origin: pts[0], destination: pts.at(-1)!, distance: 0});
  }

  /**
   * Builds a segment for the directions map using Mapbox Directions API.
   * @param a Starting point coordinates.
   * @param b Ending point coordinates.
   * @param index Index of the segment in the route.
   * @returns A GeoJSONSourceSpecification for the segment.
   */
  private async buildDirectionsSegment(
    a: GeoPoint,
    b: GeoPoint
  ): Promise<GeoJSONSourceSpecification> {
    const coords = `${a[0]},${a[1]};${b[0]},${b[1]}`;

    const url = `https://api.mapbox.com/directions/v5/mapbox/driving/${coords}?geometries=geojson&access_token=${this.accessToken}`;
    const response = await firstValueFrom(this.http.get<MapboxDirectionsResponse>(url));

    return {
      type: "geojson",
      data: {type: "Feature", geometry: response.routes[0].geometry, properties: {}},
    };
  }

  private clear(): void {
    this.segments.set([]);
    this.stopsSource.set(null);
    this.bounds.set(null);
    this.center.set(this.defaultCenter);
    this.zoom.set(this.defaultZoom);
  }

  /**
   * Validates if the given point is a valid GeoPoint.
   * A valid GeoPoint is an array of two numbers: [longitude, latitude].
   * Longitude must be between -180 and 180, latitude must be between -90 and 90.
   * @param point The point to validate.
   * @returns True if the point is valid, false otherwise.
   */
  private isValidPoint(point: GeoPoint): boolean {
    return (
      Array.isArray(point) &&
      point.length === 2 &&
      typeof point[0] === "number" &&
      typeof point[1] === "number" &&
      point[0] >= -180 && // longitude
      point[0] <= 180 &&
      point[0] !== 0 &&
      point[1] >= -90 && // latitude
      point[1] <= 90 &&
      point[1] !== 0
    );
  }

  /** Ensures every stop is within valid longitude/latitude bounds. */
  private coordsAreValid(pts: GeoPoint[]): boolean {
    return pts.every((pt) => this.isValidPoint(pt));
  }
}

export interface RouteChangedEvent {
  origin: number[];
  destination: number[];
  distance: number;
}
