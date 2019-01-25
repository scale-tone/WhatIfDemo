import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { AuthService } from '../auth.service';
import { ProgressService } from '../progress.service';

// Change Facebook AppId and other settings inside this file
import * as config from '../../config.json';

@Component({
    selector: 'app-quotes',
    templateUrl: './quotes.component.html',
    styleUrls: ['./quotes.component.css']
})
export class QuotesComponent implements OnInit {

    quotes = null;

    constructor(private authService: AuthService,
        private progressService: ProgressService,
        private http: HttpClient)
    {}

    ngOnInit() {
        this.reloadQuotes();
    }
    
    // Loads the latest quotes from server
    reloadQuotes() {
        this.http.get(config.backendBaseUri + '/GetQuotes?userId=' + this.authService.userId, this.authService.backendHttpOptions)
            .subscribe(this.progressService.getObserver(null, (response: any) => {
                this.quotes = response;
            }));
    }

    // Purchases the selected policy and reloads the list
    buyPolicy(policy: any) {

        this.http.post(config.backendBaseUri + '/Purchase?userId=' + this.authService.userId, policy, this.authService.backendHttpOptions)
            .subscribe(this.progressService.getObserver(null, () => {

                alert('Thank you for purchasing ' + policy.title + '! You will be charged daily!');
            }));
    }
}