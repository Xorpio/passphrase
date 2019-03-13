import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WordService {

  constructor() { }

  getWord(language: string, key: string): Observable<string> {
    console.log('returning: ' + key);
    switch (language) {
      case 'EN':
      return of('Passwoprd');
      default:
    return of('wachtwoord');
    }
  }
}
