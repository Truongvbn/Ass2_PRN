import React from "react";
import { createRoot } from "react-dom/client";
import "./styles.css";
import { RoomsExplorer } from "@/components/RoomsExplorer";
import { HotelLocationMap } from "@/components/HotelLocationMap";

function mountIfPresent(elementId: string, render: (el: HTMLElement) => React.ReactNode) {
  const el = document.getElementById(elementId);
  if (!el) return;
  createRoot(el).render(<React.StrictMode>{render(el)}</React.StrictMode>);
}

mountIfPresent("rooms-explorer-root", () => <RoomsExplorer />);
mountIfPresent("hotel-location-root", (el) => {
  const lat = Number(el.getAttribute("data-lat") ?? "");
  const lng = Number(el.getAttribute("data-lng") ?? "");
  const name = el.getAttribute("data-name") ?? "Hotel";
  return <HotelLocationMap name={name} latitude={lat} longitude={lng} />;
});

