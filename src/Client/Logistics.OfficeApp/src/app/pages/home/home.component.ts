import {CurrencyPipe} from "@angular/common";
import {Component, OnInit} from "@angular/core";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ChartModule} from "primeng/chart";
import {SkeletonModule} from "primeng/skeleton";
import {TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {TrucksMapComponent} from "@/components";
import {globalConfig} from "@/configs";
import {ApiService} from "@/core/api";
import {DailyGrossesDto, LoadDto} from "@/core/api/models";
import {AddressPipe, DistanceUnitPipe} from "@/core/pipes";
import {Converters, DateUtils} from "@/core/utilities";
import {NotificationsPanelComponent} from "./components";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.scss"],
  standalone: true,
  imports: [
    CardModule,
    SharedModule,
    TableModule,
    RouterLink,
    TooltipModule,
    ButtonModule,
    SkeletonModule,
    ChartModule,
    CurrencyPipe,
    DistanceUnitPipe,
    TrucksMapComponent,
    NotificationsPanelComponent,
    AddressPipe,
  ],
})
export class HomeComponent implements OnInit {
  public readonly accessToken = globalConfig.mapboxToken;
  public todayGross = 0;
  public weeklyGross = 0;
  public weeklyDistance = 0;
  public weeklyRpm = 0;
  public isLoadingLoadsData = false;
  public isLoadingChartData = false;
  public loads: LoadDto[] = [];
  public chartData: unknown;
  public chartOptions: unknown;

  constructor(private readonly apiService: ApiService) {
    this.chartData = {
      labels: [],
      datasets: [
        {
          label: "Daily Gross",
          data: [],
        },
      ],
    };
    this.chartOptions = {
      plugins: {
        legend: {
          display: false,
        },
      },
    };
  }

  ngOnInit() {
    this.fetchActiveLoads();
    this.fetchLastTenDaysGross();
  }

  private fetchActiveLoads() {
    this.isLoadingLoadsData = true;

    this.apiService.getLoads({orderBy: "-dispatchedDate"}, true).subscribe((result) => {
      if (result.success && result.data) {
        this.loads = result.data;
      }

      this.isLoadingLoadsData = false;
    });
  }

  private fetchLastTenDaysGross() {
    this.isLoadingChartData = true;
    const oneWeekAgo = DateUtils.daysAgo(7);

    this.apiService.getDailyGrosses(oneWeekAgo).subscribe((result) => {
      if (result.success && result.data) {
        const grosses = result.data;

        this.weeklyGross = grosses.totalGross;
        this.weeklyDistance = grosses.totalDistance;
        this.weeklyRpm = this.weeklyGross / Converters.metersTo(this.weeklyDistance, "mi");
        this.drawChart(grosses);
        this.calcTodayGross(grosses);
      }

      this.isLoadingChartData = false;
    });
  }

  private drawChart(grosses: DailyGrossesDto) {
    const labels: string[] = [];
    const data: number[] = [];

    grosses.data.forEach((i) => {
      labels.push(DateUtils.toLocaleDate(i.date));
      data.push(i.gross);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: "Daily Gross",
          data: data,
          fill: true,
          tension: 0.4,
          borderColor: "#405a83",
          backgroundColor: "#88a5d3",
        },
      ],
    };
  }

  private calcTodayGross(grosses: DailyGrossesDto) {
    const today = new Date();
    let totalGross = 0;

    grosses.data
      .filter((i) => DateUtils.dayOfMonth(i.date) === today.getDate())
      .forEach((i) => (totalGross += i.gross));

    this.todayGross = totalGross;
  }
}
