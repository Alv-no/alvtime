import { ThemeProvider } from "@material-ui/core";
import React from "react";
import AzureAdAuthenticator from "./components/AzureAdAuthenticator";
import Tables from "./components/Tables";
import theme from "./theme";

function App() {
  return (
    <ThemeProvider theme={theme}>
      <AzureAdAuthenticator>
        <Tables />
      </AzureAdAuthenticator>
    </ThemeProvider>
  );
}

export default App;
