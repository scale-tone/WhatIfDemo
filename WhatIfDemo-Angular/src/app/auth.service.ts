import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

// Change Facebook AppId and other settings inside ../environments/environment.ts file
import { environment } from '../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    // Detects whether the user has logged in
    get isLoggedIn(): boolean{
        return this.userId !== null;
    }

    // Here the user's id will be stored after a successful login.
    userId: string = null;
    // These are HttpOptions to be used for communicating with Azure Functions backend. Will contain the session token, after a successful login.
    backendHttpOptions = null;

    constructor(private http: HttpClient) { 
    }

    // Turns Facebook's access token into Azure Functions session token
    login(accessToken: string): Observable<any> {

        var tokenValidationBody = {
            access_token: accessToken
        };

        return this.http.post(environment.backendBaseUri + '/.auth/login/facebook', tokenValidationBody).pipe(
            tap((validationResponse: any) => {
                
                this.userId = validationResponse.user.userId;

                // storing session token in a class field
                this.backendHttpOptions = {
                    headers: new HttpHeaders({ 'X-ZUMO-AUTH': validationResponse.authenticationToken })
                };
            })
        );
    }

    // Gets a SAS token for uploading files to Azure Blob
    getBlobCredentialsForUpload(): Observable<any> {
        return this.http.get(environment.backendBaseUri + '/api/GetBlobSasToken', this.backendHttpOptions);
    }
}