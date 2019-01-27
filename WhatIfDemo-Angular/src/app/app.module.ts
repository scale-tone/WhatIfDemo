import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { QuotesComponent } from './quotes/quotes.component';
import { ClaimsComponent } from './claims/claims.component';

const appRoutes: Routes = [
    { path: 'quotes', component: QuotesComponent },
    { path: 'claims', component: ClaimsComponent },
    { path: '**', component: QuotesComponent }
];

@NgModule({
    declarations: [
        AppComponent,
        QuotesComponent,
        ClaimsComponent
    ],
    imports: [
        BrowserModule,
        HttpClientModule,
        RouterModule.forRoot(appRoutes, { enableTracing: true })
    ],
    providers: [],
    bootstrap: [AppComponent]
})
export class AppModule { }
