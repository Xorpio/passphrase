import { Component, OnInit } from '@angular/core';
import { PasswordService } from '../services/password.service';
import { Passwordspec } from '../models/passwordspec';
import { faAngleLeft, faAngleRight } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-passwordform',
  templateUrl: './passwordform.component.html',
  styleUrls: ['./passwordform.component.scss']
})
export class PasswordformComponent implements OnInit {

  password: string;
  passwordSpec = new Passwordspec();

  faAngleLeft = faAngleLeft;
  faAngleRight = faAngleRight;

  constructor(private passwordService: PasswordService) {
  }

  ngOnInit() {
  }

  generatePassword() {
    this.passwordService.generatePassword(this.passwordSpec)
    .subscribe(password => this.password = password);
  }

  wordsDown() {
    this.passwordSpec.Words--;
  }

  wordsUp() {
    this.passwordSpec.Words++;
  }
}
