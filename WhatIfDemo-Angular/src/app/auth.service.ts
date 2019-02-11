import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router, RouterEvent, NavigationEnd } from '@angular/router';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AppInsights } from "applicationinsights-js"

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

    constructor(private http: HttpClient, private router: Router) {

        // Also initializing AppInsights here
        AppInsights.downloadAndSetup({ instrumentationKey: environment.appInsightsInstrumentationKey });

        // And tracking page views
        router.events.subscribe((evt: RouterEvent) => {
            if (evt instanceof NavigationEnd) {
                AppInsights.trackPageView(evt.url);
            }
        });
    }

    // Turns Facebook's access token into Azure Functions session token
    login(accessToken: string): Observable<any> {

        var tokenValidationBody = {
            access_token: accessToken
        };

        return this.http.post(environment.backendBaseUri + '/.auth/login/facebook', tokenValidationBody).pipe(
            tap((validationResponse: any) => {
                
                this.userId = validationResponse.user.userId;

                // Binding AppInsights metrics to this userId
                // WARNING: make sure you're actually allowed to log these userIds first!
                AppInsights.setAuthenticatedUserContext(this.userId);
                // Also resubmitting the page view event, now for this specific user
                AppInsights.trackPageView(this.router.url);

                // storing session token in a class field
                this.backendHttpOptions = {
                    headers: new HttpHeaders({ 'X-ZUMO-AUTH': validationResponse.authenticationToken })
                };
            })
        );
    }

    // Gets SAS tokens for each file to be uploaded to Azure Blob. Each token fits the corresponding file and doesn't fit any other name.
    getBlobCredentialsForUpload(fileNames: string[]): Observable<any> {
        return this.http.post(environment.backendBaseUri + '/api/GetBlobSasTokens', fileNames, this.backendHttpOptions);
    }
}