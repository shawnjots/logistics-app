import {Component, Input, OnDestroy, OnInit} from "@angular/core";
import {CommonModule} from "@angular/common";
import {NotificationService, ToastService} from "@/core/services";
import {NotificationDto} from "@/core/models";
import {TimeAgoPipe} from "@/core/pipes";
import {CardModule} from "primeng/card";
import {ButtonModule} from "primeng/button";
import {BadgeModule} from "primeng/badge";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {DialogModule} from "primeng/dialog";

@Component({
  selector: "app-notifications-panel",
  standalone: true,
  templateUrl: "./notifications-panel.component.html",
  styleUrls: ["./notifications-panel.component.scss"],
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    TimeAgoPipe,
    BadgeModule,
    ProgressSpinnerModule,
    DialogModule,
  ],
})
export class NotificationsPanelComponent implements OnInit, OnDestroy {
  public notifications: NotificationDto[];
  public isLoading: boolean;
  public displayDialog: boolean;
  public selectedNotification?: NotificationDto;

  @Input() height: string;

  constructor(
    private notificationService: NotificationService,
    private toastSerice: ToastService
  ) {
    this.notifications = [];
    this.isLoading = false;
    this.displayDialog = false;
    this.height = "100%";
  }

  ngOnInit(): void {
    this.fetchNotifications();
    this.notificationService.connect();
    this.notificationService.onReceiveNotification = (notification) => {
      this.toastSerice.showSuccess(notification.message, notification.title);
      this.notifications.push(notification);
    };

    // Generating 15 fake notifications
    // this.notifications = Array.from({length: 15}, (_, index) => {
    //   const date = new Date();
    //   date.setDate(date.getDate() - index); // Subtract 'index' days from the current date

    //   return {
    //     id: index.toString(),
    //     title: `Fake Title ${index + 1}`,
    //     message: `This is a fake message number ${index + 1}.`,
    //     isRead: index % 2 === 0, // Alternate read status
    //     created: date.toISOString(),
    //   };
    // });
  }

  ngOnDestroy(): void {
    this.notificationService.disconnect();
  }

  fetchNotifications() {
    this.isLoading = true;

    this.notificationService.getPastTwoWeeksNotifications().subscribe((result) => {
      if (result.data) {
        this.notifications = result.data;
      }

      this.isLoading = false;
    });
  }

  showNotification(notification: NotificationDto): void {
    this.selectedNotification = notification;
    this.displayDialog = true;

    if (!notification.isRead) {
      this.markAsRead(notification);
    }
  }

  closeDialog(): void {
    this.displayDialog = false;
  }

  markAsRead(notification: NotificationDto): void {
    // Update frontend state
    notification.isRead = true;
    this.notificationService.markAsRead(notification.id).subscribe((_) => _);
  }

  getUnreadNotificationsCount(): string {
    return this.notifications.filter((i) => !i.isRead).length.toString();
  }
}
