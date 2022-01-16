import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of, Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  members: Member[] = [];
  memberCache = new Map();
  user: User;
  private _userParams: UserParams;

  get userParams() { return this._userParams; }
  set userParams(userParams: UserParams) { this._userParams = userParams; }
  resetUserParams() { this._userParams = new UserParams(this.user); return this.userParams; }

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this._userParams = new UserParams(this.user)
    })
  }

  getMembers(userParams: UserParams) {
    const response = this.memberCache.get(this.getUserParamsKey(userParams));
    if (response) return of(response);

    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.getPaginatedResult<Member[]>('users', params)
      .pipe(
        map(response => {
          this.memberCache.set(this.getUserParamsKey(userParams), response)
          return response;
        })
      );
  }

  private getPaginatedResult<T>(url: any, params: any) {
    const paginationResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        paginationResult.result = response.body;
        if (response.headers.get('Pagination')) {
          paginationResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginationResult;
      })
    );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    return new HttpParams()
      .append('pageNumber', pageNumber)
      .append('pageSize', pageSize);
  }

  getMember(username: string) {
    const member: Member = this.getLatestCachedMemberByUsername(username);
    if (member) {
      return of(member);
    }

    return this.http.get<Member>('users/' + username)
  }

  updateMember(member: Member) {
    return this.http.put('users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  addLike(username: string) {
    return this.http.post(`likes/${username}`, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = this.getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    return this.getPaginatedResult<Partial<Member[]>>('likes', params);
  }

  setMainPhoto(photoId: number) {
    return this.http.put('users/set-main-photo/' + photoId, {});
  }

  removePhoto(photoId: number) {
    return this.http.delete('users/delete-photo/' + photoId);
  }

  private getUserParamsKey(userParams: UserParams): string {
    return Object.values(userParams).join('-');
  }

  private getLatestCachedMemberByUsername(username: string): Member {
    // Get all member objects with username matching the given one.
    const members: Member[] = [...this.memberCache.values()]
      .reduce((arr, currentPaginationResult) =>
        arr.concat(currentPaginationResult.result), []
      ).filter(x => x.username == username);

    // sort based on the last active
    members.sort((first, second) => 0 - (second.lastActive > first.lastActive ? -1 : 1));
    if (members.length > 0) {
      // Get the first one -> the most recent version of it
      return members[0];
    }
    return null;
  }
}
