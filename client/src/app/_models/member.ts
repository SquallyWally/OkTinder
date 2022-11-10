import { Photo } from './photo';

// Generated from http://json2ts.com/ by using the response from
// https://localhost:5001/api/users
export interface Member {
  id: number;
  userName: string;
  photoUrl: string;
  age: number;
  knownAs: string;
  created: Date;
  lastActive: Date;
  gender: string;
  introduction: string;
  lookingFor: string;
  interests: string;
  city: string;
  country: string;
  photos: Photo[];
}
