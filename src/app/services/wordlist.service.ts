import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class WordlistService {

  private dutchList = './assets/DicewareDutch.json';
  private englishList = './assets/diceware.wordlist.json';

  constructor(private http: HttpClient) { }

  GetList(language: string): Observable<any> {

    // logic to check local storage
    switch (language) {
      case 'EN':
        return this.getEnglishList();
      case 'NL':
        return this.getDutchList();
    }
  }

  logError(error: any): any {
    console.log(error);
  }
  log(data: any): any {
    console.log(data);
  }

  getDutchList(): Observable<any> {
    return this.http.get(this.dutchList);
  }

  getEnglishList(): Observable<any> {
    return this.http.get(this.englishList);
  }

}
