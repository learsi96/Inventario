import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  imports: [RouterOutlet],
  selector: 'inv-root',
  template: '<router-outlet></router-outlet>',
})
export class App {}
