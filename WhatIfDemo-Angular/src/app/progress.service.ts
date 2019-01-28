import { Injectable, ChangeDetectorRef } from '@angular/core';
import { Observer } from 'rxjs';

// A simple service to handle lengthy operations in UI
@Injectable({ providedIn: 'root' })
export class ProgressService  {

    inProgress: boolean;
    errorMessage: string = null;

    changeDetectorRef: ChangeDetectorRef;
    
    getObserver<TResult>(changeDetectorRef?: ChangeDetectorRef, successHandler?: (result: TResult) => void) {

        // capturing the changeDetectorRef, if it was passed
        if (!!changeDetectorRef) {
            this.changeDetectorRef = changeDetectorRef;
        }

        return new ProgressObserver(this, !!successHandler ? successHandler : () => { });
    }
}

class ProgressObserver<TResult> implements Observer<TResult> {

    constructor(private progressService: ProgressService, private successHandler: (result: TResult) => void) {
        progressService.inProgress = true;
        progressService.errorMessage = null;
        progressService.changeDetectorRef.detectChanges();
    }

    next(result: TResult) {
        this.progressService.inProgress = false;
        this.successHandler(result);
        this.progressService.changeDetectorRef.detectChanges();
    }

    error(err: any) {
        this.progressService.inProgress = false;
        this.progressService.errorMessage = err.statusText + ' ' + err.message
        this.progressService.changeDetectorRef.detectChanges();
    }

    complete() { 
    }
}