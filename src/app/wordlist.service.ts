import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class WordlistService {

  private dutchList = 'http://theworld.com/%7Ereinhold/DicewareDutch.txt';

  constructor(private http: HttpClient) { }

  GetList(language: string): Observable<Map<number, string>> {

    // logic to check local storage

    let dat = this.http.get(this.dutchList, {responseType: 'text'})
    .pipe(
        tap( // Log the result or error
        data => this.log(data),
        error => this.logError(error)
      )
    );

    return of();
  }

  logError(error: any): any {
    console.log(error);
  }
  log(data: any): any {
    console.log(data);
  }
}
