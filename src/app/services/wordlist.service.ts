import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})

export class WordlistService {

  private defaultLang = 'EN';
  private languageDict = { 'EN': './assets/diceware.wordlist.json', 'NL': './assets/DicewareDutch.json' };

  constructor(private http: HttpClient) { }

  GetList(language: string): Observable<any> {
    return this.http.get(this.languageDict[language] || this.languageDict[this.defaultLang]);
  }
}
