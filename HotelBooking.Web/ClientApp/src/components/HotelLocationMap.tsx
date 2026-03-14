import { Map, MapControls, MapMarker, MarkerContent, MarkerLabel } from "@/components/ui/map";
import { cn } from "@/lib/utils";

export function HotelLocationMap(props: { name: string; latitude: number; longitude: number }) {
  const { name, latitude, longitude } = props;
  const hasCoords = Number.isFinite(latitude) && Number.isFinite(longitude);

  if (!hasCoords) {
    return (
      <div className="react-root w-full h-full flex items-center justify-center rounded-md border border-border bg-muted/20">
        <div className="text-sm text-muted-foreground">Location not available.</div>
      </div>
    );
  }

  return (
    <div className="react-root w-full h-full">
      <Map className="w-full h-full" center={[longitude, latitude]} zoom={14}>
        <MapControls position="bottom-right" showZoom showFullscreen showCompass />
        <MapMarker longitude={longitude} latitude={latitude}>
          <MarkerContent className={cn("group")}>
            <div className="size-4 rounded-full bg-blue-600 border-2 border-white shadow-lg group-hover:scale-110 transition-transform" />
            <MarkerLabel className="rounded bg-background/90 px-1.5 py-0.5 border border-border shadow-sm">
              {name}
            </MarkerLabel>
          </MarkerContent>
        </MapMarker>
      </Map>
    </div>
  );
}

