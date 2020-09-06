import { Account } from "@azure/msal-common";
import React, { useState } from "react";
import { login, logout } from "./../services/azureAd";

const Json = ({ data }: any) => <pre>{JSON.stringify(data, null, 4)}</pre>;

function Login() {
  const [account, setAccount] = useState({} as Account);
  const [error, setError] = useState("");

  const onSignInClick = async () => {
    const loginResponse = await login().catch((error) => {
      setError(error.message);
    });
    if (loginResponse && loginResponse.account) {
      setAccount(loginResponse.account);
    }
  };

  return (
    <div>
      <section>
        <h1>Login MVP</h1>
        {!account.name ? (
          <button onClick={onSignInClick}>Sign In</button>
        ) : (
          <button onClick={logout}>Sign Out</button>
        )}
        {error && <p className="error">Error: {error}</p>}
      </section>
      <section>
        {account.name && (
          <div>
            <h2>Session Account Data</h2>
            <Json data={account} />
          </div>
        )}
      </section>
    </div>
  );
}

export default Login;
