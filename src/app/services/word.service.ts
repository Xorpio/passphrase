import { Injectable } from '@angular/core';
import { Observable, of, range } from 'rxjs';
import { WordlistService } from './wordlist.service';
import { tap, map, mergeMap, reduce, toArray } from 'rxjs/operators';
import { SentenceSpec } from '../models/SentenceSpec';
import { List } from 'immutable';
import { RandomService } from './random.service';

@Injectable({
  providedIn: 'root'
})
export class WordService {

  private wordList: any[];

  constructor(private wordListService: WordlistService, private randomService: RandomService) {
    this.wordList = [];
  }

  getSentence(sentenceSpec: SentenceSpec): Observable<List<string>> {
    return this.randomService.GenerateList(sentenceSpec.Words)
    .pipe(
      mergeMap(key => this.getWord(sentenceSpec.Language, key)),
      toArray(),
      map(wordArray => List(wordArray))
    )
    ;
  }

  private getWord(language: string, key: string): Observable<string> {
    return this.getList(language)
    .pipe(
      map(wordList => wordList[key])
    );
  }

  private getList(language: string): Observable<Map<string, string>> {
    if (this.wordList[language] !== undefined) {
      return of(this.wordList[language]);
    }

    return this.wordListService.GetList(language)
    .pipe(
      tap(wordList => this.wordList[language] = wordList)
    );
  }
}
