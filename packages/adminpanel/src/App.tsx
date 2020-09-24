import { ThemeProvider } from "@material-ui/core";
import React from "react";
import Authentication from "./components/Login";
import Tables from "./components/Tables";
import theme from "./theme";

function App() {
  return (
    <ThemeProvider theme={theme}>
      <Authentication>
        <Tables />
      </Authentication>
    </ThemeProvider>
  );
}

export default App;
