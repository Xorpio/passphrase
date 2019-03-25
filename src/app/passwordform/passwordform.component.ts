import { Component, OnInit } from '@angular/core';
import { faAngleLeft, faAngleRight, faSyncAlt } from '@fortawesome/free-solid-svg-icons';
import { SentenceSpec } from '../models/SentenceSpec';
import { List } from 'immutable';
import { PasswordSpec } from '../models/passwordSpec';
import { tap, mergeMap } from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { WordService } from '../services/word.service';
import { PasswordService } from '../services/password.service';

@Component({
  selector: 'app-passwordform',
  templateUrl: './passwordform.component.html',
  styleUrls: ['./passwordform.component.scss']
})

export class PasswordformComponent implements OnInit {

  passwordSpec = new PasswordSpec();
  password: string;

  sentenceSpec = new SentenceSpec();
  wordList: List<string>;
  sentence: string;

  faAngleLeft = faAngleLeft;
  faAngleRight = faAngleRight;
  faSyncAlt = faSyncAlt;

  constructor(private wordService: WordService, private passwordService: PasswordService) {
  }

  ngOnInit() {
    this.sentenceSpec.Words = 6;
  }

  generatePassword() {
    this.getSentence()
    .pipe( mergeMap(wordList => this.passwordService.generatePassword(this.passwordSpec, wordList)))
    .subscribe(password => this.password = password);
  }

  generateSentence() {
    this.wordService.getSentence(this.sentenceSpec)
    .pipe(
      tap(wordList => this.wordList = wordList),
      tap(wordList => this.sentence = wordList.toArray().join(' '))
    ).subscribe();
  }

  private getSentence(): Observable<List<string>>{
    if (this.wordList.isEmpty()) {
      this.generateSentence();
    }

    return of(this.wordList);
  }

  wordsDown() {
    this.sentenceSpec.Words--;
  }

  wordsUp() {
    this.sentenceSpec.Words++;
  }
}
