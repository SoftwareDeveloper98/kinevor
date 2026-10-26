/**
 * Trainer employed by the gym.
 */
export interface TrainerSummary {
  /** Stable identifier of the trainer. */
  id: string;
  /** Full display name. */
  fullName: string;
  /** Email address. */
  email: string;
  /** Phone number in international format. */
  phone: string;
  /** Short biographical text shown on profile cards. */
  bio: string;
  /** Localized specializations, for example `yoga` or `strength`. */
  specializations: string[];
  /** Average class rating from 1 to 5. */
  rating: number;
  /** Number of ratings contributing to the average. */
  totalRatings: number;
  /** Years of coaching experience. */
  yearsExperience: number;
  /** Employment status, for example `Active`. */
  status: string;
  /** Absolute or app-relative URL of the profile photo, when any. */
  photoUrl?: string;
  /** Certification names displayed on the profile. */
  certifications?: string[];
}
