import { Component, Input, SimpleChange, SimpleChanges } from '@angular/core';
import { VideoUpdate } from './video-update';

@Component({
  selector: 'video-component',
  templateUrl: './video.component.html'
})
export class VideoComponent {
  public videoWidth = 320;
  public videoHeight = 240;

  public source = "https://localhost:44448/walkthroughapi/getvideo?videoName=Welcome.mp4";

  @Input() data: VideoUpdate = new VideoUpdate();

  ngOnChanges(changes: SimpleChanges): void {
    let change: SimpleChange = changes["data"];

    if (change.currentValue.src != "") {
      var myVideo = document.getElementsByTagName('video')[0];
      myVideo.src = change.currentValue.src;
      myVideo.load();
      myVideo.play();
    }
  }
}
