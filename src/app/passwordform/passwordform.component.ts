import { Component, OnInit } from '@angular/core';
import { PasswordService } from '../password.service';
import { Passwordspec } from './passwordspec';

@Component({
  selector: 'app-passwordform',
  templateUrl: './passwordform.component.html',
  styleUrls: ['./passwordform.component.scss']
})
export class PasswordformComponent implements OnInit {

  password: string;
  passwordSpec = new Passwordspec();

  langString: string;
  constructor(private passwordService: PasswordService) {
  }

  ngOnInit() {
  }

  generatePassword() {
    this.passwordService.generatePassword(this.passwordSpec)
    .subscribe(password => this.password = password);
  }

}
