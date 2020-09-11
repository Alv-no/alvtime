import { AccountInfo } from "@azure/msal-common";
import React, { useState, useEffect } from "react";
import { getAllAccounts, login, logout } from "./../services/azureAd";

const Json = ({ data }: any) => <pre>{JSON.stringify(data, null, 4)}</pre>;

function Login() {
  const [accounts, setAccounts] = useState<AccountInfo[]>([]);
  const [account, setAccount] = useState<AccountInfo>();
  const [error, setError] = useState("");

  const signIn = async () => {
    const accounts = getAllAccounts();
    if (accounts.length) {
      setAccounts(accounts);
      return;
    }

    const loginResponse = await login().catch((error) => {
      setError(error.message);
    });

    if (loginResponse) {
      setAccounts(getAllAccounts());
    }
  };

  useEffect(() => {
    signIn();
  }, []);

  if (!accounts.length)
    return (
      <section>
        <h1>Login MVP</h1>
        {!accounts.length ? (
          <button onClick={signIn}>Sign In</button>
        ) : (
          <button onClick={logout}>Sign Out</button>
        )}
        {error && <p className="error">Error: {error}</p>}
      </section>
    );

  if (!account)
    return (
      <section>
        <h1>Select Account</h1>
        {accounts.map((account) => {
          return (
            <button key={account.username} onClick={() => setAccount(account)}>
              {account.username}
            </button>
          );
        })}
      </section>
    );

  return (
    <div>
      <section>
        <button onClick={logout}>Sign Out</button>
        <div>
          <h2>Session Account Data</h2>
          <Json data={accounts} />
        </div>
      </section>
    </div>
  );
}

export default Login;
