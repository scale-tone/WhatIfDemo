import { AzureFunction, Context, HttpRequest } from "@azure/functions"
import { ManagedIdentityCredential } from "@azure/identity"
import axios from 'axios';

const WhatIfDemoFunctionAppUrl = "https://whatifdemofunctionapp.azurewebsites.net";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {

    try {

        // Getting a token for calling https://whatifdemofunctionapp.azurewebsites.net
        // using current Managed Identity
        const credential = new ManagedIdentityCredential();
        const token = (await credential.getToken(WhatIfDemoFunctionAppUrl)).token;

        // Logging the token for testing purposes.
        // NOTE: NEVER DO THIS IN PRODUCTION
        console.log(`>>> Got an access token: ${token}`);

        // Now making the actual call and returning results
        const response = await axios.get(`${WhatIfDemoFunctionAppUrl}/api/GetAllProducts`, {
            headers: {
                Authorization: `Bearer ${token}`
            }
        });

        context.res = { body: response.data };
        
    } catch (err) {
        context.res = { body: err };
    }
};

export default httpTrigger;