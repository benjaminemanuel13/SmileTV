import { AfterViewChecked, Component } from '@angular/core';
import { MenuUpdate } from './menu-update';
import { VideoUpdate } from './video-update';

@Component({
  selector: 'walkthrough-component',
  templateUrl: './walkthrough.component.html'
})
export class WalkthroughComponent {
  public pageTitle = "Walkthrough - Introduction";
  public inputToChild: MenuUpdate = new MenuUpdate();
  public inputSrc: VideoUpdate = new VideoUpdate();

  onNotify(data: string): void {
    var args = data.split(',');
    var menu = args[0];
    var pos = args[1];
    var newcaption = args[2];

    this.inputToChild = {
      menu: menu,
      pos: pos,
      newCaption: newcaption
    };
  }

  newSource(data: string): void {
    this.inputSrc = {
      src: data
    }
  }
}
