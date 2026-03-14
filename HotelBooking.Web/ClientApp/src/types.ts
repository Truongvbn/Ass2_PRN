export type HotelDto = {
  id: number;
  name: string;
  description: string;
  city: string;
  address: string;
  latitude: number;
  longitude: number;
  phoneNumber: string;
  email: string;
  imageUrl: string;
  gallery: string[];
  starRating: number;
  isActive: boolean;
  roomCount: number;
  minPricePerNight: number | null;
};

export type RoomListDto = {
  id: number;
  hotelId: number;
  hotelName: string;
  name: string;
  roomTypeName: string;
  pricePerNight: number;
  maxOccupancy: number;
  imageUrl: string;
  gallery: string[];
  isAvailable: boolean;
  averageRating: number;
};

export type ProvinceV2 = {
  name: string;
  code: number;
  division_type: string;
  codename: string;
  phone_code: number;
};

