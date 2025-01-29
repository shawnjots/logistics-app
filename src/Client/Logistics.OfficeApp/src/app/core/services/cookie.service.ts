import {Injectable} from "@angular/core";

@Injectable({providedIn: "root"})
export class CookieService {
  getCookie(name: string): string {
    const cookies = document.cookie.split(";");
    const cookieName = `${name}=`;

    for (const cookie of cookies) {
      const cookieValue = cookie.replace(/^\s+/g, "");
      if (cookieValue.indexOf(cookieName) == 0) {
        return cookieValue.substring(cookieName.length, cookieValue.length);
      }
    }
    return "";
  }

  deleteCookie(cookieName: string) {
    this.setCookie({name: cookieName, value: "", expireDays: -1});
  }

  /**
   * Expires default 1 day
   * If params.session is set and true expires is not added
   * If params.path is not set or value is not greater than 0 its default value will be root "/"
   * Secure flag can be activated only with https implemented
   * Examples of usage:
   * {service instance}.setCookie({name:'token',value:'abcd12345', session:true }); <- This cookie will not expire
   * {service instance}.setCookie({name:'userName',value:'John Doe', secure:true }); <- If page is not https then secure will not apply
   * {service instance}.setCookie({name:'niceCar', value:'red', expireDays:10 }); <- For all this examples if path is not provided default will be root
   */
  setCookie(cookie: Cookie) {
    const date = new Date();
    date.setTime(
      date.getTime() + (cookie.expireDays ? cookie.expireDays : 1) * 24 * 60 * 60 * 1000
    );
    const path = cookie.path && cookie.path.length > 0 ? cookie.path : "/";
    const expires =
      cookie.session && cookie.session == true ? "" : `expires=${date.toUTCString()};`;
    const secure =
      location.protocol === "https:" && cookie.secure && cookie.secure == true ? "secure" : "";
    document.cookie = `${cookie.name}=${cookie.value};path=${path};${expires}${secure}`;
  }
}

interface Cookie {
  name: string;
  value: string;
  path?: string;
  expireDays?: number;
  session?: boolean;
  secure?: boolean;
}
