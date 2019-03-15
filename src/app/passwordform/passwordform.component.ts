import { Component, OnInit } from '@angular/core';
import { PasswordService } from '../services/password.service';
import { Passwordspec } from '../models/passwordspec';
import { faAngleLeft, faAngleRight, faSyncAlt } from '@fortawesome/free-solid-svg-icons';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-passwordform',
  templateUrl: './passwordform.component.html',
  styleUrls: ['./passwordform.component.scss']
})
export class PasswordformComponent implements OnInit {

  sentence: string;

  password: string;
  passwordSpec = new Passwordspec();

  faAngleLeft = faAngleLeft;
  faAngleRight = faAngleRight;
  faSyncAlt = faSyncAlt;

  constructor(private passwordService: PasswordService) {
  }

  ngOnInit() {
  }

  generatePassword() {
    this.passwordService.generatePassword(this.passwordSpec)
    .subscribe(passwordTuple => {
      this.password = passwordTuple[0];
      this.sentence = passwordTuple[1];
    });
  }

  wordsDown() {
    this.passwordSpec.Words--;
  }

  wordsUp() {
    this.passwordSpec.Words++;
  }
}
