import { Injectable } from '@angular/core';
import { RandomService } from './random.service';
import { Observable } from 'rxjs';
import { mergeMap, toArray, map } from 'rxjs/operators';
import { WordService } from './word.service';
import { Passwordspec } from '../models/passwordspec';

@Injectable({
  providedIn: 'root'
})
export class PasswordService {

  constructor(private randomService: RandomService, private wordService: WordService) { }

  generatePassword(passwordspec: Passwordspec): Observable<string> {
    return this.randomService.GenerateList(passwordspec.Words)
    .pipe(
      mergeMap(key => this.wordService.getWord(passwordspec.Language, key)),
      toArray(),
      map(words => words.join(passwordspec.UseSpace ? ' ' : ''))
    )
    ;
  }
}
