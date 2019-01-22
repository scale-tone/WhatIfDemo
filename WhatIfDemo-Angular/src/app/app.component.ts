import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient, HttpHeaders }    from '@angular/common/http';

// Change Facebook AppId and other settings inside this file
import * as config from './config.json';

// Facebook JavaScript SDK's URI
const facebookScriptBaseUri = 'https://connect.facebook.net/en_US/sdk.js#xfbml=1&version=v3.2&appId=';

// using Facebook JavaScript SDK as JavaScript (since wasn't able to find suitable typings for it)
declare var FB: any;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

	inProgress = false;
	userId = null;
	quotes = null;
	errorMessage = null;
	backendHttpOptions = null;

	constructor(private http: HttpClient, private changeDetectorRef: ChangeDetectorRef) { }
  
	ngOnInit() { 
	
		const facebookScriptTagId = 'facebook-jssdk';
		if (document.getElementById(facebookScriptTagId)) return;
		
		var facebookScriptTag = document.createElement('script'); 
		facebookScriptTag.id = facebookScriptTagId;
		facebookScriptTag.src = facebookScriptBaseUri + config.facebookAppId;
		
		var allScriptTags = document.getElementsByTagName('script');
		allScriptTags[0].parentNode.insertBefore(facebookScriptTag, allScriptTags[0]);
		
		// handling the Facebook login event with an instance method
		var onFacebookLoginCallback = (fbResponse) => {
			if(fbResponse.status === 'connected'){
				this.onFacebookLogin(fbResponse.authResponse);
			}
		}
		
		// asynchronously subscribing to 'auth.statusChange' event
		setTimeout(function(){
			FB.Event.subscribe('auth.statusChange', onFacebookLoginCallback);
		}, 500);
	}
	
	// this happens when the user successfully logs in with their Facebook account
	onFacebookLogin(authResponse){
		this.errorMessage = null;
		this.inProgress = true;

		// For some reason, at this stage Angular's automatic change detection doesn't happen, and we need to trigger it manually
		this.changeDetectorRef.detectChanges();

		var tokenValidationBody = {
			access_token: authResponse.accessToken
		};
		
		this.http.post(config.tokenValidationUri, tokenValidationBody).subscribe((validationResponse: any) => {

			this.userId = validationResponse.user.userId;
			this.changeDetectorRef.detectChanges();
			
			// storing session token in a class field
			this.backendHttpOptions = {
				headers: new HttpHeaders({ 'X-ZUMO-AUTH': validationResponse.authenticationToken })
			};
			
			this.http.get(config.backendBaseUri + '/GetQuotes?userId=' + this.userId, this.backendHttpOptions).subscribe((response: any) => {

				this.inProgress = false;
				this.quotes = response;

				this.changeDetectorRef.detectChanges();
			}, (err: any) => {
					
				this.inProgress = false;
				this.errorMessage = err.statusText + ' ' + err.message;	
				this.changeDetectorRef.detectChanges();
			});
			
		}, (err: any) => { 
				
			this.inProgress = false;
			this.errorMessage = err.statusText + ' ' + err.message;
			this.changeDetectorRef.detectChanges();
		});	
	}

	buyPolicy(policy) {

		this.errorMessage = null;
		this.inProgress = true;

		this.http.post(config.backendBaseUri + '/Purchase?userId=' + this.userId, policy, this.backendHttpOptions).subscribe((response: any) => {

			alert('Thank you for purchasing ' + policy.title + '! You will be charged daily!');

			this.inProgress = false;
		}, (err: any) => {

			this.errorMessage = err.statusText + ' ' + err.message;
			this.inProgress = false;
		});
	}
}