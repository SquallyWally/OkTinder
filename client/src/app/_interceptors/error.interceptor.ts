import {Injectable} from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import {Observable} from 'rxjs';
import {NavigationExtras, Router} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {catchError} from 'rxjs/operators';
import {throwError} from 'rxjs';
import {TestErrorsComponent} from "../errors/test-errors/test-errors.component";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {
  }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {
        if (error) {
          switch (error.status) {
            case 400:
              const errors = error.error.errors
              if (errors) {
                const modalStateErrors = [];
                for (const key in errors) {
                  if (errors[key]) {
                    modalStateErrors.push(errors[key])
                  }
                }
                throw modalStateErrors.flat(); // Concates the arrays
              } else {
                this.toastr.error(error.statusText, error.status);
              }
              break;
            case 401:
              this.toastr.error(error.statusText, error.status);
              break;
            case 404:
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              const navigationExtras: NavigationExtras = {state: {error: error.error}}
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error("Something unexpected went wrong");
              console.log(error)
              break;
          }
        }
        return throwError(error);
      })
    );
  }
}
