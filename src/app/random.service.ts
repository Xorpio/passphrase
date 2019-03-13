import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { List } from 'immutable';

@Injectable({
  providedIn: 'root'
})
export class RandomService {

  constructor() { }

  public GenerateList(numberOfLists: number): Observable<string> {
    return of('abc', '123456');
  }
}
