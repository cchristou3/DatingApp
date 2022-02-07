import { HttpClient, HttpParams } from "@angular/common/http";
import { map, take } from "rxjs/operators";
import { PaginatedResult } from "../_models/pagination";

export function getPaginatedResult<T>(url: any, params: any, http: HttpClient) {
    const paginationResult: PaginatedResult<T> = new PaginatedResult<T>();
    return http.get<T>(url, { observe: 'response', params }).pipe(
        map(response => {
            paginationResult.result = response.body;
            if (response.headers.get('Pagination')) {
                paginationResult.pagination = JSON.parse(response.headers.get('Pagination'));
            }
            return paginationResult;
        })
    );
}

export function getPaginationHeaders(pageNumber: number, pageSize: number) {
    return new HttpParams()
        .append('pageNumber', pageNumber)
        .append('pageSize', pageSize);
}