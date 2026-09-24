export interface GuestFormInput {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

export interface GuestDto extends GuestFormInput {
  id: number;
}