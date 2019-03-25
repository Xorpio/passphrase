import { Injectable } from '@angular/core';
import { RandomService } from './random.service';
import { Observable, of } from 'rxjs';
import { PasswordSpec } from '../models/passwordSpec';
import { List } from 'immutable';
import { toArray, map, mergeMap, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PasswordService {

  charList = [
    '!',
    '@',
    '#',
    '$',
    '%',
    '^',
    '&',
    '*',
    '(',
    ')',
    '_',
    '-',
    '+',
    '=',
    '`',
    '~',
    '<',
    '>',
    ',',
    '.',
    '/',
    '?',
    ':',
    ';',
    '"',
    "'",
    '{',
    '}',
    '[',
    ']',
    '\\',
    '|'
  ];

  constructor(private randomService: RandomService) { }

  generatePassword(passwordspec: PasswordSpec, wordList: List<string>): Observable<string> {
    return of(wordList)
    .pipe(
      map(wordList => wordList.toArray()),
      mergeMap(wordList => this.addCapital(passwordspec, wordList)),
      mergeMap(wordList => this.addNumber(passwordspec, wordList)),
      mergeMap(wordList => this.addSpecial(passwordspec, wordList)),
      map(wordArray => wordArray.join(passwordspec.UseSpace ? ' ' : ''))
    );
  }

  private addCapital(passwordSpec: PasswordSpec, wordList: string[]): Observable<string[]>{
    console.log(passwordSpec);
    if (passwordSpec.UseCapital) {
      return this.randomService.generateRandomNumber(2)
      .pipe(
        toArray(),
        map(numbers => [
          numbers[0] % wordList.length,
          numbers[1]
        ]),
        map(numbers => [
          numbers[0],
          numbers[1] % wordList[numbers[0]].length
        ]),
        tap(numbers => wordList[numbers[0]] = this.changeChar(wordList[numbers[0]], numbers[1], wordList[numbers[0]][numbers[1]].toUpperCase())),
        map(_ => wordList)
      );
      // const word = wordList[(this.randomService.generateRandomNumber() % wordList.length) -1]
    }

    return of(wordList);
  }
  private addNumber(passwordSpec: PasswordSpec, wordList: string[]): Observable<string[]>{
    console.log(passwordSpec);
    if (passwordSpec.UseNumbers) {
      return this.randomService.generateRandomNumber(3)
      .pipe(
        toArray(),
        map(numbers => [
          numbers[0] % wordList.length,
          numbers[1],
          numbers[2] % 10
        ]),
        map(numbers => [
          numbers[0],
          numbers[1] % wordList[numbers[0]].length,
          numbers[2]
        ]),
        tap(numbers => wordList[numbers[0]] = this.changeChar(wordList[numbers[0]], numbers[1], numbers[2].toString())),
        map(_ => wordList)
      );
      // const word = wordList[(this.randomService.generateRandomNumber() % wordList.length) -1]
    }

    return of(wordList);
  }
  private addSpecial(passwordSpec: PasswordSpec, wordList: string[]): Observable<string[]>{
    console.log(passwordSpec);
    if (passwordSpec.UseSpecialCharacters) {
      return this.randomService.generateRandomNumber(3)
      .pipe(
        toArray(),
        map(numbers => [
          numbers[0] % wordList.length,
          numbers[1],
          numbers[2] % this.charList.length
        ]),
        map(numbers => [
          numbers[0],
          numbers[1] % wordList[numbers[0]].length,
          numbers[2]
        ]),
        tap(numbers => wordList[numbers[0]] = this.changeChar(wordList[numbers[0]], numbers[1], this.charList[numbers[2]])),
        map(_ => wordList)
      );
      // const word = wordList[(this.randomService.generateRandomNumber() % wordList.length) -1]
    }

String.fromCharCode()

    return of(wordList);
  }

  private changeChar(word: string, position: number, character: string) {
    return word.slice(0, position) + character + word.slice(position + 1);
  }
}
