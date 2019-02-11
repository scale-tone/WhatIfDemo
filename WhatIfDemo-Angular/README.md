# WhatIfDemo-Angular

A demo Single-Page Application created with [Angular CLI](https://cli.angular.io/) and TypeScript. Allows you to login with your Facebook account and send some requests to your [WhatIfDemo-Functions](https://github.com/scale-tone/WhatIfDemo/tree/master/WhatIfDemo-Functions) deployment.

Configuration settings should be specified in [environment.ts](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Angular/src/environments/environment.ts) file. **backendBaseUri** parameter should point to your Function App deployment and **facebookAppId** should be the same that you used for configuring Facebook authentication in that deployment.

For running the app locally, you'll need [Angular CLI](https://cli.angular.io/) installed. Then specify local configuration settings via **environment.local.ts** file (that file is deliberately excluded from commits with .gitignore) and run:
```
  ng serve -c local
```
inside your **WhatIfDemo-Angular/src** folder. 
