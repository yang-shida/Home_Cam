import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'app-side-menu',
  templateUrl: './side-menu.component.html',
  styleUrls: ['./side-menu.component.css']
})
export class SideMenuComponent implements OnInit {

  menuItems: string[] = ["Camera Cards", "CCTV Style", "Cameras", "Settings"];
  camIdList: string[] = ["aa:aa:aa:aa:aa:aa", "bb:bb:bb:bb:bb:bb", "cc:cc:cc:cc:cc:cc"]
  showingCamIdList: boolean = false;
  selectedMenuItem: string;
  selectedCamId?: string;

  constructor() { 
    this.selectedMenuItem=this.menuItems[0];
  }

  ngOnInit(): void {
    
  }

  isString(val: any): boolean{
    
    return typeof(val)==='string';
  }

  toggleShowCamIdList(): void {
    this.showingCamIdList=!this.showingCamIdList;
  }

  onClickMenuItem(menuItem: string): void {
    this.selectedMenuItem=menuItem;
    if(menuItem==='Cameras'){
      this.toggleShowCamIdList();
    }
    else{
      if(this.showingCamIdList){
        this.toggleShowCamIdList();
      }
    }
  }

  onClickCamId(camId: string): void{
    this.selectedCamId=camId;
  }

}
