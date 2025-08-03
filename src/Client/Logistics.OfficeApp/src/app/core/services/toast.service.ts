import {Injectable} from "@angular/core";
import {ConfirmationService, MessageService} from "primeng/api";

@Injectable({providedIn: "root"})
export class ToastService {
  constructor(
    private readonly messageService: MessageService,
    private readonly confirmService: ConfirmationService
  ) {}

  /**
   * Displays a success message toast notification with the given message and title.
   * @param message The message to be displayed in the toast notification.
   * @param title The title of the toast notification. Defaults to "Notification".
   */
  showSuccess(message: string, title = "Notification") {
    this.messageService.add({
      key: "notification",
      severity: "success",
      summary: title,
      detail: message,
    });
  }

  /**
   * Displays an error message toast notification with the given message.
   * @param message The message to be displayed in the toast notification.
   */
  showError(message: string) {
    this.messageService.add({
      key: "notification",
      severity: "error",
      summary: "Error",
      detail: message,
    });
  }

  /**
   * Displays a confirmation dialog with the given message and actions.
   * @param message The message to be displayed in the confirmation dialog.
   * @param onAccept The callback function to be executed when the accept button is clicked.
   * @param onReject The callback function to be executed when the reject button is clicked.
   */
  confrimAction(message: string, onAccept: () => void, onReject?: () => void) {
    this.confirmService.confirm({
      key: "confirmDialog",
      message,
      accept: onAccept,
      reject: onReject,
    });
  }
}
