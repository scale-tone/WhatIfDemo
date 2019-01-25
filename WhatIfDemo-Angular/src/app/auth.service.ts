import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

// Change Facebook AppId and other settings inside this file
import * as config from '../config.json';

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    // Here the user's id will be stored after a successful login.
    userId: string = null;
    // These are HttpOptions to be used for communicating with Azure Functions backend. Will contain the session token, after a successful login.
    backendHttpOptions = null;

    constructor(private http: HttpClient) { 
    }

    // Turns Facebook's access token into Azure Functions session token
    login(accessToken: string): Observable<Object> {

        var tokenValidationBody = {
            access_token: accessToken
        };

        return this.http.post(config.tokenValidationUri, tokenValidationBody).pipe(
            tap((validationResponse: any) => {
                
                this.userId = validationResponse.user.userId;

                // storing session token in a class field
                this.backendHttpOptions = {
                    headers: new HttpHeaders({ 'X-ZUMO-AUTH': validationResponse.authenticationToken })
                };
            })
        );
    }    
}