import config from "@/config";
import {
  Configuration,
  PublicClientApplication,
  AuthenticationResult,
  AccountInfo,
  EventMessage,
} from "@azure/msal-browser";
export class AuthService {
  private accountLoaded$: Promise<AccountInfo>;
  private msalManager: PublicClientApplication;
  private authResult: AuthenticationResult | null = null;
  private accountInfo: AccountInfo | null = null;
  private scopes = [config.ACCESS_SCOPE];

  constructor() {
    const msalConfig: Configuration = {
      auth: {
        clientId: config.CLIENT_ID,
        redirectUri: window.location.origin,
        authority: config.AUTHORITY + config.TENANT_ID,
      },
      cache: {
        storeAuthStateInCookie: false,
      },
    };

    this.msalManager = new PublicClientApplication(msalConfig);
    this.accountLoaded$ = new Promise(resolve => {
      if (this.isDevMode()) {
        resolve({
          localAccountId: "",
          tenantId: "",
          environment: "dev",
          homeAccountId: "",
          username: "",
          name: "Ahre-Ketil Lillehagen",
        });
        return;
      }

      this.msalManager.handleRedirectPromise().then(response => {
        this.accountInfo = response ? response.account : this.resolveAccount();
        if (!this.accountInfo) {
          this.msalManager.loginRedirect({
            scopes: this.scopes,
          });
        } else {
          resolve(this.accountInfo);
        }
      });
    });
  }

  private handleResponse(authentiCationResponse: AuthenticationResult | null) {
    if (authentiCationResponse) {
      this.authResult = authentiCationResponse;
      this.accountInfo = authentiCationResponse.account;
    } else {
      this.accountInfo = this.msalManager.getAllAccounts()[0];
    }
  }

  public async logout(): Promise<void> {
    return this.msalManager.logout();
  }

  public addCallback(callback: (message: EventMessage) => void) {
    this.msalManager.addEventCallback(callback);
  }

  public async loginMsal(): Promise<void> {
    const response = await this.msalManager.loginRedirect({
      scopes: this.scopes,
    });
    return response;
  }

  public async getAccessToken(): Promise<string | undefined> {
    if (this.isDevMode()) {
      return "5801gj90-jf39-5j30-fjk3-480fj39kl409";
    }
    const accountInfo = await this.getAccountAsync();

    const response = await this.msalManager.acquireTokenSilent({
      account: accountInfo,
      scopes: this.scopes,
    });
    this.handleResponse(response);
    return response.accessToken;
  }

  public getUser(): AuthenticationResult | null {
    return this.authResult;
  }

  public async requireLogin(): Promise<boolean> {
    if (this.isDevMode()) {
      return false;
    }
    return (await this.getAccountAsync()) === null;
  }

  public getAccount(): AccountInfo | null {
    return this.msalManager.getActiveAccount();
  }

  public async getAccountAsync(): Promise<AccountInfo> {
    return this.resolveAccount() || (await this.accountLoaded$);
  }

  private resolveAccount(): AccountInfo | null {
    if (this.accountInfo) return this.accountInfo;

    const accounts = this.msalManager.getAllAccounts();
    if (accounts && accounts.length > 0) return accounts[0];
    return null;
  }

  private isDevMode(): boolean {
    return false;
    //return process.env.NODE_ENV === "development";
  }
}

export default new AuthService();
