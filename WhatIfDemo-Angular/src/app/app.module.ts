import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { QuotesComponent } from './quotes/quotes.component';

const appRoutes: Routes = [
    { path: 'quotes', component: QuotesComponent },
    { path: '**', component: QuotesComponent }
];

@NgModule({
    declarations: [
        AppComponent,
        QuotesComponent
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
