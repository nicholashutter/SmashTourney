export interface ApplicationUser 
{
    id: string;
    userName?: string | null;
    normalizedUserName?: string | null;
    email?: string | null;
    normalizedEmail?: string | null;
    emailConfirmed: boolean;
    passwordHash?: string | null;
    securityStamp?: string | null;
    concurrencyStamp?: string | null;
    phoneNumber?: string | null;
    phoneNumberConfirmed: boolean;
    twoFactorEnabled: boolean;
    lockoutEnd?: Date | null;
    lockoutEnabled: boolean;
    accessFailedCount: number;

    registrationDate: Date;
    lastLoginDate?: Date | null;
    allTimeMatches: number;
    allTimeWins: number;
    allTimeLosses: number;
}