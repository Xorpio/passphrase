import { Injectable } from '@angular/core';
import { RandomService } from './random.service';
import { of, Observable } from 'rxjs';
import { reduce, mergeMap, toArray, map } from 'rxjs/operators';
import { Passwordspec } from './passwordform/passwordspec';
import { WordService } from './word.service';

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

    // return of('Mijn wachtwoord');
  }
}
