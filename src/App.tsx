import React, { useState } from "react";
import "./App.css";
import { getAccount, login, logout } from "./services/azureAd";
import { Account } from "@azure/msal-common";

const Json = ({ data }: any) => <pre>{JSON.stringify(data, null, 4)}</pre>;

function App() {
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

  const onGetAccountClick = () => {
    const account = getAccount();
    setAccount(account);
  };

  return (
    <div>
      <section>
        <h1>
          Welcome to the Microsoft Authentication Library For Javascript - React
          Quickstart
        </h1>
        {!account.name ? (
          <>
            <button onClick={onSignInClick}>Sign In</button>
          </>
        ) : (
          <>
            <button onClick={logout}>Sign Out</button>
            <button onClick={onGetAccountClick}>View account</button>
          </>
        )}
        {error && <p className="error">Error: {error}</p>}
      </section>
      <section className="data">
        {account.name && (
          <div className="data-account">
            <h2>Session Account Data</h2>
            <Json data={account} />
          </div>
        )}
      </section>
    </div>
  );
}

export default App;
