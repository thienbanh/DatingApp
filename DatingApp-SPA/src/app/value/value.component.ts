import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-value',
  templateUrl: './value.component.html',
  styleUrls: ['./value.component.css']
})
export class ValueComponent implements OnInit {
  // tao values từ valuecomponent, any để lấy bất kỳ những gì có bên trong values
  values: any;
  // tao contructor gồm httpclient cẩn thận ko chọn cái từ selenium mà chọn cái từ request
  constructor(private http: HttpClient) { }

  ngOnInit() {
    // goi ham getvalue
    this.getValues();
  }
  // get api
  getValues() {
    this.http.get('http://localhost:5000/api/values').subscribe(response => {
      this.values = response;
    }, error => {
      console.log(error);
    });
  }
}
