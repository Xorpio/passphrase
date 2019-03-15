import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { WordlistService } from './wordlist.service';
import { tap, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class WordService {

  private wordList: any[];

  constructor(private wordListService: WordlistService) {
    this.wordList = [];
  }

  getWord(language: string, key: string): Observable<string> {
    return this.getList(language)
    .pipe(
      map(wordList => wordList[key])
    );
  }

  getList(language: string): Observable<Map<string, string>> {
    if (this.wordList[language] !== undefined) {
      return of(this.wordList[language]);
    }

    return this.wordListService.GetList(language)
    .pipe(
      tap(wordList => this.wordList[language] = wordList)
    );
  }
}
