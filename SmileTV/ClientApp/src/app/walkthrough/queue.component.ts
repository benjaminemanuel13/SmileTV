import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'queue-component',
  templateUrl: './queue.component.html'
})

export class MyQueueComponent implements OnInit {
  @Output() static emit: EventEmitter<string> = new EventEmitter<string>();
  @Output() localemit: EventEmitter<string> = new EventEmitter<string>();

  connection: any;
  static message: boolean = true;
  static messages: Item[] = [{ data: "No Messages" }];
  localmessages: Item[] = [];
  ws: WebSocket = new WebSocket("wss://localhost:7147/ws/client");

  ngOnInit(): void {
    this.ws.binaryType = 'blob';
    this.ws.onopen = this.onOpen;
    this.ws.onmessage = this.onMessage;
    this.ws.onerror = this.onError;
    this.ws.onclose = this.onClose;

    this.localmessages = MyQueueComponent.messages;


    MyQueueComponent.emit = this.localemit;
    MyQueueComponent.emit.emit("Hello");
  }

  onOpen(): void { }

  static addMessage(msg: string): void {
    if (this.message) {
      this.messages.length = 0;
      this.message = false;
    }

    this.messages.push({ data: msg });
  }

  onMessage(evt: any) {
    if (evt.data.startsWith("[UPDATE]")) {
      MyQueueComponent.emit.emit(evt.data.substr(8, evt.data.length - 8));
    }
    else {
      MyQueueComponent.addMessage(evt.data);
    }
  }

  onError(err: any) {
    alert(JSON.stringify(err));
  }

  onClose(): void {
  }

  connect(): void {
    
  }

  send(msg: String) {
    //this.sock.next(msg);
  }
}

class Item {
  data: string = "";
}
