export interface Photo {
    id: number;
    url: string;
    isMain: boolean;
    isApproved: boolean;
    username?: string;
}

export interface PhotoForApprovalDto {
    photoId: number;
    url: string;
    isApproved: boolean;
    username?: string;
}