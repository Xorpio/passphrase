import { Injectable } from '@angular/core';
import { Observable, range, of } from 'rxjs';
import { map, toArray, mergeMap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class RandomService {

  constructor() { }

  public GenerateList(numberOfLists: number): Observable<string> {
    return range(1, numberOfLists)
    .pipe(mergeMap(num => this.generateKey()));
  }

  private generateKey(): Observable<string> {
    return range(1, 5)
      .pipe(
        map(x => ( Math.ceil(Math.random() * 100 % 6)) // dice roll
        ),
        map(generatedNumber => generatedNumber.toString()),
        toArray(),
        map(numberArray => numberArray.join(''))
      );
  }

  generateRandomNumber(numOfNumbers: number): Observable<number>{
    return range(1, numOfNumbers)
    .pipe(map(_ => Math.ceil(Math.random() * 1000)));
  }
}
