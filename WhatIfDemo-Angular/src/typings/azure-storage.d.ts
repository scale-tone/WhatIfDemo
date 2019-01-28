export interface AzureStorage {
    Blob: BlobStorage;
}

export interface BlobStorage {
    createBlobServiceWithSas: (uri: string, sharedAccessToken: string) => BlobService;
}

export interface BlobService {
    withFilter: (filter: any) => BlobService;
    createBlockBlobFromBrowserFile: (
        container: string,
        filename: string,
        file: File,
        options: any,
        callback: (error: any, response: any) => void
    ) => any;
}