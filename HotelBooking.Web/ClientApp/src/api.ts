import type { HotelDto, ProvinceV2, RoomListDto } from "@/types";

async function fetchJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  const res = await fetch(url, { signal, headers: { Accept: "application/json" } });
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`Request failed (${res.status}): ${text || res.statusText}`);
  }
  return (await res.json()) as T;
}

export function getHotels(city?: string, signal?: AbortSignal) {
  const url = city ? `/api/hotels?city=${encodeURIComponent(city)}` : "/api/hotels";
  return fetchJson<HotelDto[]>(url, signal);
}

export function getHotel(id: number, signal?: AbortSignal) {
  return fetchJson<HotelDto>(`/api/hotels/${id}`, signal);
}

export function getHotelRooms(
  id: number,
  opts?: { checkIn?: string; checkOut?: string; guests?: number },
  signal?: AbortSignal
) {
  const qs = new URLSearchParams();
  if (opts?.checkIn) qs.set("checkIn", opts.checkIn);
  if (opts?.checkOut) qs.set("checkOut", opts.checkOut);
  if (opts?.guests) qs.set("guests", String(opts.guests));
  const url = qs.toString() ? `/api/hotels/${id}/rooms?${qs}` : `/api/hotels/${id}/rooms`;
  return fetchJson<RoomListDto[]>(url, signal);
}

export function getProvinces(signal?: AbortSignal) {
  return fetchJson<ProvinceV2[]>("/api/geo/provinces", signal);
}

