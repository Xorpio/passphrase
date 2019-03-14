export class Passwordspec {
  Language: string;
  Words: number;
  UseSpecialCharacters: boolean;
  UseSpace: boolean;
  UseNumbers: boolean;

  constructor() {
    this.Words = 5;
  }
}
