import { ThemeProvider } from "@material-ui/core";
import React from "react";
import { BrowserRouter as Router } from "react-router-dom";
// Views
import MenuAppBar from "./components/MenuAppBar";
import Tables from "./components/Tables";
import Authentication from "./components/Login";
import theme from "./theme";

function App() {
  return (
    <ThemeProvider theme={theme}>
      <Router>
        <Authentication>
          <MenuAppBar />
          <Tables />
        </Authentication>
      </Router>
    </ThemeProvider>
  );
}

export default App;
