import { Component, OnInit, inject } from '@angular/core';
import { SignalrService } from './core/signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
})
export class App implements OnInit {
  private readonly signalr = inject(SignalrService);

  ngOnInit(): void {
    this.signalr.init();
  }
}
