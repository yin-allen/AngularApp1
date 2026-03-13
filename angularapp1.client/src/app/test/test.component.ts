import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http'; // 1. 匯入工具

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrl: './test.component.css'
})
export class TestComponent implements OnInit {

  forecasts: any[] = []; // 用來存放抓回來的資料
  constructor(private http: HttpClient) { }
  ngOnInit(): void {
    this.getWeather();
  }

  getWeather() {
    // 5. 呼叫 API，路徑直接寫 /weatherforecast，Proxy 會幫你轉給 5019
    this.http.get<any[]>('/test/Weatherforecast').subscribe({
      next: (result) => {
        this.forecasts = result;
        console.log('Test 畫面抓到資料了:', this.forecasts);
      },
      error: (err) => {
        console.error('抓取失敗:', err);
      }
    });
  }
}
