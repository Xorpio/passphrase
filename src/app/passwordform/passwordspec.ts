export class Passwordspec {
  Language: string;
  Words: number;
  UseSpecialCharacters: boolean;
  UseSpace: boolean;
  UseNumbers: boolean;
  UseCapital: boolean;

  constructor() {
    this.Words = 5;
    this.Language = 'NL';
    this.UseNumbers = false;
    this.UseSpace = true;
    this.UseSpecialCharacters = true;
    this.UseCapital = true;
  }
}
