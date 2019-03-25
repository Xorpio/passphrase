export class PasswordSpec {
  UseSpecialCharacters: boolean;
  UseSpace: boolean;
  UseNumbers: boolean;
  UseCapital: boolean;

  constructor() {
    this.UseNumbers = false;
    this.UseSpace = true;
    this.UseSpecialCharacters = true;
    this.UseCapital = true;
  }
}
