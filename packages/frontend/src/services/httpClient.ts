import axios from "axios";
import config from "@/config";
import authenticationService from "./auth";

const errorCallbacks: ((error: ErrorResponse) => void)[] = [];

export interface ErrorResponse {
  name: string;
  status: number;
  message: string;
}

export const registerErrorCallback: (
  callback: (error: ErrorResponse) => void
) => void = callback => {
  errorCallbacks.push(callback);
};

const protectedUrls = [config.API_HOST].map(url => {
  if (url) {
    const parsed = new URL(url);
    return `${parsed.protocol}//${parsed.hostname}:${parsed.port}`;
  }
  return "";
});

const isProtectedUrl: (url: string | undefined) => boolean = (
  url: string | undefined
) => {
  if (!url) return false;

  const parsedUrl = new URL(url);

  const complete = `${parsedUrl.protocol}//${parsedUrl.hostname}:${parsedUrl.port}`;
  return protectedUrls.some(item => item === complete);
};

// Define an interceptor that only adds an access-token whenever we target our API's
axios.interceptors.request.use(request => {
  return new Promise(resolve => {
    request.headers["Content-Type"] = "application/json";
    if (isProtectedUrl(request.url)) {
      authenticationService.getAccessToken().then(token => {
        request.headers["Authorization"] = `Bearer ${token}`;
        resolve(request);
      });
    } else {
      resolve(request);
    }
  });
});

axios.interceptors.response.use(
  response => {
    return response;
  },
  error => {
    const errorResponse = {
      status: error.response.status as number,
      name: "",
      message: "",
    };

    // We should never do a request without a token therefore a 401
    // means that we used a token and the token was not accepted
    errorResponse.status = error.response.status;

    if (error.response.status % 500 < 100) {
      errorResponse.name = error.response.data.title;
      errorResponse.message = error.response.data.detail || "";
    }

    if (error.response.status % 400 < 100) {
      errorResponse.name = `API returned a ${error.response.status}-response`;
      errorResponse.message = error.response.data.message || "";
    }

    if (
      error.response.status === 404 &&
      error.response.data === "User not found"
    ) {
      errorResponse.name = "User not found";
      errorResponse.message = "User not found";
    }

    errorCallbacks.forEach(callback => callback(errorResponse));
    return Promise.reject(errorResponse);
  }
);

export default axios;
