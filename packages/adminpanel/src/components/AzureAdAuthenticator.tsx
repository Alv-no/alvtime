import { AccountInfo } from "@azure/msal-common";
import React, { createContext, FC, useEffect, useState } from "react";
import store from "store2";
import {
  createAdAuthenticatedFetch,
  getAccountByHomeId,
  getAllAccounts,
  handleRedirect,
  login,
  logout,
} from "./../services/azureAd";

//@ts-ignore
export const AlvtimeContext = createContext();

const AzureAdAuthenticator: FC<{
  render: (
    adAuthenticatedFetch: (path: string, options: any) => Promise<Response>
  ) => any;
}> = (props) => {
  const homeAccountId = store("AlvtimeAdminHomeAccountId");
  const accounts = getAllAccounts();

  const [selectedAccount, setSelectedAccount] = useState<AccountInfo | null>();
  const [error, setError] = useState("");
  const [notAdmins, setNotAdmins] = useState<string[]>([]);

  useEffect(() => {
    async function handleAzureRedirect() {
      const tokenResponse = await handleRedirect().catch((error) =>
        setError(error.message)
      );

      if (tokenResponse && (await isAdmin(tokenResponse.account))) {
        store("AlvtimeAdminHomeAccountId", tokenResponse.account.homeAccountId);
        setSelectedAccount(tokenResponse.account);
      } else if (await isAdmin(getAccountByHomeId(homeAccountId))) {
        setSelectedAccount(getAccountByHomeId(homeAccountId));
      } else if (!accounts.length) {
        login();
      }
    }
    handleAzureRedirect();
  }, []);

  async function chooseAccount(account: AccountInfo) {
    if (await isAdmin(account)) {
      store("AlvtimeAdminHomeAccountId", account.homeAccountId);
      setSelectedAccount(account);
    } else {
      setNotAdmins([...notAdmins, account.username]);
    }
  }

  if (selectedAccount)
    return (
      <div>
        <section>
          <button
            onClick={() => {
              store.remove("AlvtimeAdminHomeAccountId");
              logout();
            }}
          >
            Logg ut
          </button>
          {props.render(createAdAuthenticatedFetch(selectedAccount))}
        </section>
      </div>
    );

  if (accounts.length)
    return (
      <section>
        <h1>Select admin account</h1>
        {accounts.map((account) => {
          return (
            <div>
              <button
                key={account.username}
                onClick={() => chooseAccount(account)}
              >
                {account.username}
              </button>
              {notAdmins.some((username) => username === account.username)
                ? " - is not an admin account"
                : ""}
            </div>
          );
        })}
        <div>
          <button onClick={login}>Logg inn with a different account</button>
        </div>
      </section>
    );

  return (
    <section>
      <button onClick={login}>Logg inn</button>
      {error && <p className="error">Error: {error}</p>}
    </section>
  );
};

async function isAdmin(account: AccountInfo | null) {
  if (!account) return false;
  const path = "/api/admin/Tasks";
  const adAuthenticatedFetch = createAdAuthenticatedFetch(account);
  const res = await adAuthenticatedFetch(path).catch((error) =>
    console.error(error.message)
  );
  return res && res.ok;
}

export default AzureAdAuthenticator;
