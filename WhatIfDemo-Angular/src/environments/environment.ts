// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
	production: false,
	
	// Register your Facebook app at and specify it's appId here:
	facebookAppId: '',
	
	// Deploy Azure Functions project to Azure and specify it's base URI (like 'https://myazurefunctionapp.azurewebsites.net') here:
    backendBaseUri: '',
    
    // Put your Application Insights instrumentation key here:
    appInsightsInstrumentationKey: ''
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
