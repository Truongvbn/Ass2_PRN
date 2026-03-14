import { useEffect, useMemo, useRef, useState } from "react";
import { Map, MapControls, MapMarker, MarkerContent, MarkerLabel, MarkerPopup, useMap } from "@/components/ui/map";
import { getHotels, getHotelRooms, getProvinces } from "@/api";
import type { HotelDto, ProvinceV2, RoomListDto } from "@/types";
import { cn } from "@/lib/utils";

type ViewState = {
  center: [number, number];
  zoom: number;
  bearing: number;
  pitch: number;
};

type Filters = {
  city: string;
  checkIn: string;
  checkOut: string;
  guests: number | null;
};

function StarRow({ value }: { value: number }) {
  const stars = Array.from({ length: 5 }, (_, i) => i < value);
  return (
    <div className="flex items-center gap-0.5">
      {stars.map((on, i) => (
        <span key={i} className={cn("text-xs", on ? "text-amber-500" : "text-muted-foreground/40")}>
          ★
        </span>
      ))}
    </div>
  );
}

function flyToVietnamCenterForProvince(name: string): { center: [number, number]; zoom: number } {
  const known: Record<string, { center: [number, number]; zoom: number }> = {
    "Thành phố Hà Nội": { center: [105.8542, 21.0285], zoom: 11 },
    "Thành phố Hồ Chí Minh": { center: [106.7009, 10.7769], zoom: 11 },
    "Thành phố Đà Nẵng": { center: [108.2022, 16.0544], zoom: 11 },
    "Thành phố Hải Phòng": { center: [106.6881, 20.8449], zoom: 11 },
    "Thành phố Cần Thơ": { center: [105.7469, 10.0452], zoom: 11 },
    "Thành phố Huế": { center: [107.5909, 16.4637], zoom: 12 },
  };
  return known[name] ?? { center: [106.0, 16.0], zoom: 5.5 };
}

function MapFlyTo({ view }: { view: Pick<ViewState, "center" | "zoom"> }) {
  const { map, isLoaded } = useMap();
  const last = useRef<string>("");

  useEffect(() => {
    if (!map || !isLoaded) return;
    const key = `${view.center[0]},${view.center[1]},${view.zoom}`;
    if (key === last.current) return;
    last.current = key;
    map.flyTo({ center: view.center, zoom: view.zoom, duration: 700 });
  }, [map, isLoaded, view.center, view.zoom]);

  return null;
}

function getPrimaryImage(h: HotelDto): string {
  if (h.gallery && h.gallery.length > 0) return h.gallery[0];
  if (h.imageUrl) return h.imageUrl;
  return "/images/hotel-default.jpg";
}

function getRoomImage(r: RoomListDto): string {
  if (r.gallery && r.gallery.length > 0) return r.gallery[0];
  if (r.imageUrl) return r.imageUrl;
  return "/images/hotel-default.jpg";
}

export function RoomsExplorer() {
  const [hotels, setHotels] = useState<HotelDto[]>([]);
  const [provinces, setProvinces] = useState<ProvinceV2[]>([]);
  const [selectedHotelId, setSelectedHotelId] = useState<number | null>(null);
  const [rooms, setRooms] = useState<RoomListDto[]>([]);
  const [filters, setFilters] = useState<Filters>({ city: "", checkIn: "", checkOut: "", guests: null });
  const [loadingHotels, setLoadingHotels] = useState(true);
  const [loadingRooms, setLoadingRooms] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [viewport, setViewport] = useState<ViewState>({
    center: [106.0, 16.0],
    zoom: 5.5,
    bearing: 0,
    pitch: 0,
  });

  const cities = useMemo(() => {
    const names = provinces.map((p) => p.name);
    names.sort((a, b) => a.localeCompare(b, "vi"));
    return names;
  }, [provinces]);

  // Load provinces + hotels
  useEffect(() => {
    const ac = new AbortController();
    setLoadingHotels(true);
    setError(null);
    Promise.all([getProvinces(ac.signal), getHotels(undefined, ac.signal)])
      .then(([p, h]) => {
        setProvinces(p);
        setHotels(h);
      })
      .catch((e) => setError(e instanceof Error ? e.message : "Failed to load data."))
      .finally(() => setLoadingHotels(false));
    return () => ac.abort();
  }, []);

  // Re-fetch hotels when city filter changes
  useEffect(() => {
    const ac = new AbortController();
    setError(null);
    setLoadingHotels(true);
    getHotels(filters.city || undefined, ac.signal)
      .then((h) => {
        setHotels(h);
      })
      .catch((e) => setError(e instanceof Error ? e.message : "Failed to load hotels."))
      .finally(() => setLoadingHotels(false));
    return () => ac.abort();
  }, [filters.city]);

  // Fetch rooms when a hotel is selected
  useEffect(() => {
    if (!selectedHotelId) {
      setRooms([]);
      return;
    }
    const ac = new AbortController();
    setLoadingRooms(true);
    setError(null);
    const opts: { checkIn?: string; checkOut?: string; guests?: number } = {};
    if (filters.checkIn) opts.checkIn = filters.checkIn;
    if (filters.checkOut) opts.checkOut = filters.checkOut;
    if (filters.guests != null) opts.guests = filters.guests;
    getHotelRooms(selectedHotelId, opts, ac.signal)
      .then(setRooms)
      .catch((e) => setError(e instanceof Error ? e.message : "Failed to load rooms."))
      .finally(() => setLoadingRooms(false));
    return () => ac.abort();
  }, [selectedHotelId, filters.checkIn, filters.checkOut, filters.guests]);

  const onPickCity = (city: string) => {
    setFilters((f) => ({ ...f, city }));
    const next = flyToVietnamCenterForProvince(city);
    setViewport((v) => ({ ...v, center: next.center, zoom: next.zoom }));
  };

  const onSelectHotel = (id: number, lat: number, lng: number) => {
    setSelectedHotelId(id);
    setViewport((v) => ({ ...v, center: [lng, lat], zoom: 13 }));
  };

  const selectedHotel = selectedHotelId ? hotels.find((h) => h.id === selectedHotelId) ?? null : null;

  return (
    <div className="w-full h-full">
      <div className="grid grid-cols-1 md:grid-cols-[380px_1fr] h-full">
        {/* Sidebar */}
        <aside className="border-b md:border-b-0 md:border-r border-border bg-background/90 backdrop-blur-sm flex flex-col">
          <div className="px-4 pt-4 pb-3 border-b border-border/60">
            <div className="text-xs uppercase tracking-wide text-muted-foreground mb-1">Hotel booking</div>
            <h1 className="text-lg font-semibold leading-snug">Find your stay across Vietnam</h1>
            <p className="mt-1 text-xs text-muted-foreground">
              Filter by city and dates, then choose a hotel on the map to see available rooms.
            </p>
          </div>

          {/* Filters */}
          <div className="px-4 py-3 space-y-3 border-b border-border/60">
            <div>
              <label className="text-xs font-medium text-muted-foreground">Province / City</label>
              <select
                className="mt-1 w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
                value={filters.city}
                onChange={(e) => onPickCity(e.target.value)}
              >
                <option value="">All Vietnam</option>
                {cities.map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </div>

            <div className="grid grid-cols-2 gap-2">
              <div>
                <label className="text-xs font-medium text-muted-foreground">Check-in</label>
                <input
                  type="date"
                  className="mt-1 w-full rounded-md border border-border bg-background px-2 py-1.5 text-xs"
                  value={filters.checkIn}
                  onChange={(e) => setFilters((f) => ({ ...f, checkIn: e.target.value }))}
                />
              </div>
              <div>
                <label className="text-xs font-medium text-muted-foreground">Check-out</label>
                <input
                  type="date"
                  className="mt-1 w-full rounded-md border border-border bg-background px-2 py-1.5 text-xs"
                  value={filters.checkOut}
                  onChange={(e) => setFilters((f) => ({ ...f, checkOut: e.target.value }))}
                />
              </div>
            </div>

            <div className="grid grid-cols-[1fr_auto] gap-2 items-end">
              <div>
                <label className="text-xs font-medium text-muted-foreground">Guests</label>
                <input
                  type="number"
                  min={1}
                  className="mt-1 w-full rounded-md border border-border bg-background px-2 py-1.5 text-xs"
                  value={filters.guests ?? ""}
                  onChange={(e) =>
                    setFilters((f) => ({
                      ...f,
                      guests: e.target.value ? Number(e.target.value) : null,
                    }))
                  }
                />
              </div>
              <button
                type="button"
                className="inline-flex items-center justify-center rounded-md border border-border bg-background px-3 py-1.5 text-xs font-medium hover:bg-accent/40"
                onClick={() =>
                  setFilters({ city: "", checkIn: "", checkOut: "", guests: null })
                }
              >
                Clear
              </button>
            </div>
          </div>

          {/* Content: hotels or rooms */}
          <div className="flex-1 overflow-y-auto px-3 py-3 space-y-2">
            <div className="flex items-center justify-between text-[11px] text-muted-foreground mb-1">
              <span>
                {selectedHotel
                  ? loadingRooms
                    ? "Loading rooms…"
                    : `${rooms.length} room(s) at ${selectedHotel.name}`
                  : loadingHotels
                    ? "Loading hotels…"
                    : `${hotels.length} hotel(s)`}
              </span>
            </div>

            {error ? (
              <div className="text-xs text-destructive">{error}</div>
            ) : null}

            {!selectedHotel && (
              <>
                {loadingHotels && hotels.length === 0 ? (
                  <div className="space-y-2">
                    {Array.from({ length: 3 }).map((_, i) => (
                      <div
                        key={i}
                        className="h-18 rounded-md border border-border bg-muted/60 animate-pulse"
                      />
                    ))}
                  </div>
                ) : (
                  <div className="space-y-2 transition-opacity duration-200">
                    {hotels.map((h) => {
                      const isSelected = selectedHotelId === h.id;
                      const hasPrice = h.minPricePerNight != null;
                      return (
                        <button
                          key={h.id}
                          type="button"
                          onClick={() => onSelectHotel(h.id, h.latitude, h.longitude)}
                          className={cn(
                            "w-full text-left rounded-md border bg-background p-3 hover:bg-accent/40 transition-colors flex gap-3",
                            "border-border",
                            isSelected && "border-primary/70 ring-1 ring-primary/60"
                          )}
                        >
                          <div className="relative h-14 w-14 flex-shrink-0 overflow-hidden rounded-md">
                            <img
                              src={getPrimaryImage(h)}
                              loading="lazy"
                              alt={h.name}
                              className="h-full w-full object-cover"
                              onError={(e) => {
                                e.currentTarget.src = "/images/hotel-default.jpg";
                              }}
                            />
                          </div>
                          <div className="min-w-0 flex-1">
                            <div className="flex items-start justify-between gap-1">
                              <div className="text-sm font-medium line-clamp-1">{h.name}</div>
                              <StarRow value={h.starRating} />
                            </div>
                            <div className="mt-0.5 text-xs text-muted-foreground line-clamp-1">
                              {h.city}
                            </div>
                            <div className="mt-1 flex items-center justify-between gap-2">
                              <div className="inline-flex items-center gap-1 rounded-full bg-secondary px-2 py-0.5 text-[11px] text-secondary-foreground">
                                <span className="h-1.5 w-1.5 rounded-full bg-primary" />
                                <span>
                                  {h.roomCount} room{h.roomCount === 1 ? "" : "s"}
                                </span>
                              </div>
                              <div className="text-[11px] text-muted-foreground text-right">
                                {hasPrice ? (
                                  <span className="font-medium text-foreground">
                                    từ ₫
                                    {h.minPricePerNight!.toLocaleString("vi-VN")}
                                    <span className="text-[10px] text-muted-foreground"> /đêm</span>
                                  </span>
                                ) : (
                                  <span className="italic">View details</span>
                                )}
                              </div>
                            </div>
                          </div>
                        </button>
                      );
                    })}
                    {hotels.length === 0 && !loadingHotels ? (
                      <div className="text-xs text-muted-foreground">
                        No hotels found. Try changing the filters.
                      </div>
                    ) : null}
                  </div>
                )}
              </>
            )}

            {selectedHotel && (
              <div className="space-y-2 transition-opacity duration-200">
                <button
                  type="button"
                  onClick={() => {
                    setSelectedHotelId(null);
                    setRooms([]);
                  }}
                  className="mb-1 inline-flex items-center gap-1 text-[11px] text-muted-foreground hover:text-foreground"
                >
                  ← All hotels
                </button>

                <div className="rounded-md border border-border bg-muted/40 p-3 flex gap-3">
                  <div className="relative h-12 w-12 flex-shrink-0 overflow-hidden rounded-md">
                    <img
                      src={getPrimaryImage(selectedHotel)}
                      loading="lazy"
                      alt={selectedHotel.name}
                      className="h-full w-full object-cover"
                      onError={(e) => {
                        e.currentTarget.src = "/images/hotel-default.jpg";
                      }}
                    />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <div className="text-sm font-semibold line-clamp-1">{selectedHotel.name}</div>
                        <div className="text-[11px] text-muted-foreground line-clamp-1">
                          {selectedHotel.city} · {selectedHotel.address}
                        </div>
                      </div>
                      <StarRow value={selectedHotel.starRating} />
                    </div>
                    <div className="mt-1 flex items-center justify-between gap-2">
                      <div className="inline-flex items-center gap-1 rounded-full bg-secondary px-2 py-0.5 text-[11px] text-secondary-foreground">
                        <span className="h-1.5 w-1.5 rounded-full bg-primary" />
                        <span>
                          {selectedHotel.roomCount} room
                          {selectedHotel.roomCount === 1 ? "" : "s"}
                        </span>
                      </div>
                      {selectedHotel.minPricePerNight != null && (
                        <div className="text-[11px] text-muted-foreground">
                          từ ₫
                          {selectedHotel.minPricePerNight.toLocaleString("vi-VN")}
                          <span className="text-[10px] text-muted-foreground"> /đêm</span>
                        </div>
                      )}
                    </div>
                  </div>
                </div>

                {loadingRooms && rooms.length === 0 ? (
                  <div className="space-y-2">
                    {Array.from({ length: 2 }).map((_, i) => (
                      <div
                        key={i}
                        className="h-20 rounded-md border border-border bg-muted/60 animate-pulse"
                      />
                    ))}
                  </div>
                ) : (
                  <div className="space-y-2">
                    {rooms.map((r) => (
                      <div
                        key={r.id}
                        className="rounded-md border border-border bg-background p-3 flex gap-3"
                      >
                        <div className="relative h-16 w-16 flex-shrink-0 overflow-hidden rounded-md">
                          <img
                            src={getRoomImage(r)}
                            loading="lazy"
                            alt={r.name}
                            className="h-full w-full object-cover"
                            onError={(e) => {
                              e.currentTarget.src = "/images/hotel-default.jpg";
                            }}
                          />
                        </div>
                        <div className="min-w-0 flex-1">
                          <div className="flex items-start justify-between gap-1">
                            <div>
                              <div className="text-sm font-medium line-clamp-1">{r.name}</div>
                              <div className="text-[11px] text-muted-foreground line-clamp-1">
                                {r.roomTypeName} · up to {r.maxOccupancy} guests
                              </div>
                            </div>
                            <div className="text-right">
                              <div className="text-xs font-semibold text-foreground">
                                ₫{r.pricePerNight.toLocaleString("vi-VN")}
                              </div>
                              <div className="text-[11px] text-muted-foreground">/đêm</div>
                            </div>
                          </div>
                          <div className="mt-1 flex items-center justify-between gap-2">
                            <div className="inline-flex items-center gap-1 text-[11px] text-muted-foreground">
                              <span
                                className={cn(
                                  "h-1.5 w-1.5 rounded-full",
                                  r.isAvailable ? "bg-emerald-500" : "bg-red-400"
                                )}
                              />
                              <span>{r.isAvailable ? "Available" : "Unavailable"}</span>
                            </div>
                            <div className="flex gap-1">
                              <a
                                href={`/Rooms/Detail?id=${r.id}`}
                                className="inline-flex items-center rounded-md border border-border px-2 py-1 text-[11px] hover:bg-accent/40"
                              >
                                Details
                              </a>
                              <a
                                href={`/Booking/Create?roomId=${r.id}`}
                                className="inline-flex items-center justify-center rounded-md bg-primary px-3 py-1.5 text-[11px] font-semibold text-primary-foreground hover:bg-primary/90 min-w-[80px]"
                              >
                                Book
                              </a>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                    {rooms.length === 0 && !loadingRooms ? (
                      <div className="text-xs text-muted-foreground">
                        No rooms available for the selected filters.
                      </div>
                    ) : null}
                  </div>
                )}
              </div>
            )}
          </div>
        </aside>

        {/* Map area */}
        <main className="relative min-h-[560px] md:min-h-0">
          <Map
            className="w-full h-full"
            center={viewport.center}
            zoom={viewport.zoom}
            attributionControl={true}
            theme="light"
          >
            <MapFlyTo view={viewport} />
            <MapControls position="bottom-right" showLocate showFullscreen showCompass />

            {hotels
              .filter((h) => Number.isFinite(h.latitude) && Number.isFinite(h.longitude))
              .map((h) => {
                const isSelected = selectedHotelId === h.id;
                const hasPrice = h.minPricePerNight != null;
                return (
                  <MapMarker
                    key={h.id}
                    longitude={h.longitude}
                    latitude={h.latitude}
                    onClick={() => onSelectHotel(h.id, h.latitude, h.longitude)}
                  >
                    <MarkerContent className="group">
                      <div className="relative">
                        <div
                          className={cn(
                            "inline-flex items-center gap-1 rounded-full border px-2 py-0.5 shadow-lg text-[11px] backdrop-blur-sm",
                            "bg-primary text-primary-foreground border-primary/80",
                            "group-hover:scale-105 transition-transform",
                            isSelected && "ring-2 ring-offset-2 ring-primary/70 ring-offset-background"
                          )}
                        >
                          {hasPrice ? (
                            <span>
                              ₫{h.minPricePerNight!.toLocaleString("vi-VN")}
                              <span className="text-[9px] opacity-80"> /đêm</span>
                            </span>
                          ) : (
                            <span>{h.name}</span>
                          )}
                        </div>
                        <MarkerLabel className="rounded bg-background/90 px-1.5 py-0.5 border border-border shadow-sm mt-1">
                          {h.name}
                        </MarkerLabel>
                      </div>
                    </MarkerContent>
                    <MarkerPopup className="w-[260px]" closeButton>
                      <div className="flex gap-3">
                        <img
                          src={getPrimaryImage(h)}
                          loading="lazy"
                          className="h-16 w-16 rounded object-cover border"
                          alt={h.name}
                          onError={(e) => {
                            e.currentTarget.src = "/images/hotel-default.jpg";
                          }}
                        />
                        <div className="min-w-0">
                          <div className="text-sm font-semibold line-clamp-1">{h.name}</div>
                          <div className="mt-1 text-xs text-muted-foreground line-clamp-1">
                            {h.city}
                          </div>
                          <div className="mt-1">
                            <StarRow value={h.starRating} />
                          </div>
                        </div>
                      </div>
                      <div className="mt-3 flex items-center justify-between">
                        <div className="text-xs text-muted-foreground">
                          {h.roomCount} room{h.roomCount === 1 ? "" : "s"}
                        </div>
                        <button
                          type="button"
                          onClick={() => onSelectHotel(h.id, h.latitude, h.longitude)}
                          className="text-xs font-medium underline underline-offset-4"
                        >
                          View rooms
                        </button>
                      </div>
                    </MarkerPopup>
                  </MapMarker>
                );
              })}
          </Map>

          {loadingHotels && hotels.length === 0 ? (
            <div className="absolute inset-0 flex items-center justify-center bg-background/60 backdrop-blur-sm">
              <div className="rounded-md border border-border bg-background px-3 py-2 text-sm">
                Loading map data…
              </div>
            </div>
          ) : null}
        </main>
      </div>
    </div>
  );
}

