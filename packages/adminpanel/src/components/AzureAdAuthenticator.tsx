import { AccountInfo } from "@azure/msal-common";
import React, { useState, useEffect, FC } from "react";
import {
  getAllAccounts,
  login,
  logout,
  getAccountByHomeId,
  handleRedirect,
  acquireTokenSilent,
} from "./../services/azureAd";
import store from "store2";

const AzureAdAuthenticator: FC = (props) => {
  const [accounts, setAccounts] = useState<AccountInfo[]>([]);
  const [account, setAccount] = useState<AccountInfo>();
  const [error, setError] = useState("");
  const homeAccountId = store("AlvtimeAdminHomeAccountId");

  async function setAccountOrAccounts() {
    if (homeAccountId) {
      const account = getAccountByHomeId(homeAccountId);
      if (account) {
        setAccount(account);
        return true;
      }
    }

    const accounts = getAllAccounts();
    if (accounts.length) {
      if (accounts.length === 1) {
        const path = "/api/admin/Tasks";
        const accessTokenResponse = await acquireTokenSilent(accounts[0]);
        const res = await fetch(path, {
          headers: {
            Authorization: `Bearer ${
              accessTokenResponse ? accessTokenResponse.accessToken : ""
            }`,
          },
        }).catch((error) => console.error(error.message));
        if (res && res.ok) {
          setAccount(accounts[0]);
          return true;
        }
      }
      setAccounts(accounts);
      return true;
    }

    return;
  }

  const signIn = async () => {
    if (await setAccountOrAccounts()) return;
    const tokenResponse = await handleRedirect().catch((error) =>
      setError(error.message)
    );

    if (tokenResponse) {
      if (await setAccountOrAccounts()) return;
    } else {
      login();
    }
  };

  useEffect(() => {
    signIn();
  }, []);

  if (account)
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
          <div>{props.children}</div>
        </section>
      </div>
    );

  if (accounts.length)
    return (
      <section>
        <h1>Select Account</h1>
        {accounts.map((account) => {
          return (
            <button
              key={account.username}
              onClick={() => {
                store("AlvtimeAdminHomeAccountId", account.homeAccountId);
                setAccount(account);
              }}
            >
              {account.username}
            </button>
          );
        })}
      </section>
    );

  return (
    <section>
      <button onClick={signIn}>Logg inn</button>
      {error && <p className="error">Error: {error}</p>}
    </section>
  );
};

export default AzureAdAuthenticator;
