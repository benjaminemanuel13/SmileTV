import { Component, EventEmitter, Inject, Input, OnChanges, Output, SimpleChange, SimpleChanges } from '@angular/core';
import { NavButton } from './nav-button';
import { MenuUpdate } from './menu-update';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'navlist-component',
  templateUrl: './navlist.component.html'
})
export class NavListComponent implements OnChanges {
  public buttons: NavButton[] = [];
  private baseUrl: string = "";
  private http: HttpClient;
  private menuNew: boolean = true;
  public currentMenu: string = "root";

  @Output() srcemit: EventEmitter<string> = new EventEmitter<string>();
  @Input() data: MenuUpdate = new MenuUpdate();

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
    this.clickButton("root", "https://localhost:44448/walkthroughapi/getvideo?videoName=Welcome.mp4")
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (this.menuNew) {
      this.menuNew = false;
      return;
    }

    let change: SimpleChange = changes["data"];

    if (change != null && change.currentValue.menu === this.currentMenu) {
      this.buttons[parseInt(change.currentValue.pos)].caption = change.currentValue.newCaption;
    }
  }

  public clickButton(menuName: string = "", src: string = "") {
    this.http.get<NavButton[]>(this.baseUrl + 'walkthroughapi/getmenu?menuName=' + menuName).subscribe(result => {
      this.buttons = result;
      this.currentMenu = menuName;
    }, error => console.error(error));

    this.srcemit.emit(src);
  }
}
