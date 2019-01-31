import { Component, ChangeDetectorRef } from '@angular/core';
import { AzureStorage } from '../../typings/azure-storage';

import { AuthService } from '../auth.service';
import { ProgressService } from '../progress.service';

declare var AzureStorage: AzureStorage;

const uploadContainerName: string = 'claims';
const driversLicenseFileName: string = 'drivers-license';

@Component({
    selector: 'app-claims',
    templateUrl: './claims.component.html',
    styleUrls: ['./claims.component.css']
})
export class ClaimsComponent {

    constructor(private authService: AuthService,
        private progressService: ProgressService,
        private changeDetectorRef: ChangeDetectorRef)
    {}

    private filesToUpload: any = {};
    private driversLicenseFileInput: HTMLInputElement;
    private otherFilesInput: HTMLInputElement;

    get submitButtonDisabled(): boolean {
        // User should at least upload their driver's license
        return !this.filesToUpload[driversLicenseFileName];
    }

    // When user hits submit
    submitClaim() {

        this.authService.getBlobCredentialsForUpload(Object.keys(this.filesToUpload)).subscribe(this.progressService.getObserver(null, (blobCredentials: any) => {

            // Uploading all the files in parallel
            var progress = this.progressService.getObserver(null);
            var errorResult = null;
            var count = 0;
            for (var fileName in this.filesToUpload) {

                var file = this.filesToUpload[fileName];

                var blobService = AzureStorage.Blob.createBlobServiceWithSas(blobCredentials.blobUri, blobCredentials.sasTokens[count++]);

                // Along with SAS tokens, the service returns us the folderName. SAS tokens won't fit any other folder.
                blobService.createBlockBlobFromBrowserFile(blobCredentials.containerName, blobCredentials.folderName + "/" + fileName, file, {}, (err) => {

                    errorResult = err;
                    count--; if (count > 0) return;

                    // Now when all uploads have finished, cleaning up the form
                    if (!!this.driversLicenseFileInput) this.driversLicenseFileInput.value = '';
                    if (!!this.otherFilesInput) this.otherFilesInput.value = '';
                    this.filesToUpload = {};

                    if (!errorResult) {
                        alert('Your claim was submitted successfully!');
                        progress.next(null);
                    }
                    else {
                        progress.error(errorResult);
                    }
                });
            }
        }));
    }

    // When user selects a driver's license image file
    onDriversLicenseFileChange(event: any) {
        // Need to remember the input element, to clean it up later
        this.driversLicenseFileInput = event.target;

        var file = (event.target.files as FileList)[0];
        this.filesToUpload[driversLicenseFileName] = file;

        this.changeDetectorRef.detectChanges();
    }

    // When user selects other files
    onOtherFilesChange(event: any) {
        // Need to remember the input element, to clean it up later
        this.otherFilesInput = event.target;

        var fileList = (event.target.files as FileList);
        for (var i = 0; i < fileList.length; i++) {

            var file = fileList[i];
            this.filesToUpload[file.name] = file;
        }
    }
}
