import {HttpClient} from "@angular/common/http";
import {inject} from "@angular/core";
import {Observable, catchError, of} from "rxjs";
import {ToastService} from "../services";
import {PagedIntervalQuery, SearchableQuery} from "./models";

export abstract class ApiBase {
  private readonly headers = {"content-type": "application/json"};
  private readonly toastService = inject(ToastService);
  protected readonly http: HttpClient;
  protected readonly apiUrl: string;

  constructor(apiUrl: string, http: HttpClient) {
    this.apiUrl = apiUrl;
    this.http = http;
  }

  /**
   * Utility function to parse the sort property from the query string.
   * @param sortField Sort field name
   * @param sortOrder Sort order (1 for ascending, -1 for descending)
   * @returns The parsed sort property string.
   * @example
    ```typescript
      const sortProperty = this.parseSortProperty("name", -1); // returns "-name"
    ``` 
   */
  parseSortProperty(sortField?: string | null, sortOrder?: number | null): string {
    if (!sortOrder) {
      sortOrder = 1;
    }

    if (!sortField) {
      sortField = "";
    }

    return sortOrder <= -1 ? `-${sortField}` : sortField;
  }

  protected get<TResponse>(endpoint: string): Observable<TResponse> {
    return this.http
      .get<TResponse>(this.apiUrl + endpoint)
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected post<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.http
      .post<TResponse>(this.apiUrl + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected put<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.http
      .put<TResponse>(this.apiUrl + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected delete<TResponse>(endpoint: string): Observable<TResponse> {
    return this.http
      .delete<TResponse>(this.apiUrl + endpoint)
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected stringfySearchableQuery(query?: SearchableQuery): string {
    const {search = "", orderBy = "", page = 1, pageSize = 10} = query || {};
    return new URLSearchParams({
      search,
      orderBy,
      page: page.toString(),
      pageSize: pageSize.toString(),
    }).toString();
  }

  protected stringfyPagedIntervalQuery(
    query?: PagedIntervalQuery,
    additionalParams: Record<string, string | undefined> = {}
  ): string {
    const {startDate = new Date(), endDate, orderBy = "", page = 1, pageSize = 10} = query || {};

    // Filter out undefined values from additionalParams
    const filteredAdditionalParams = Object.fromEntries(
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      Object.entries(additionalParams).filter(([_, value]) => value !== undefined)
    );

    const params = new URLSearchParams({
      startDate: startDate.toJSON(),
      orderBy,
      page: page.toString(),
      pageSize: pageSize.toString(),
      ...filteredAdditionalParams,
    });

    if (endDate) params.set("endDate", endDate.toJSON());
    return params.toString();
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  protected handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error?.error ?? responseData.error;

    this.toastService.showError(errorMessage);
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
