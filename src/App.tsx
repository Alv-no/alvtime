import React from "react";
import { ThemeProvider } from "@material-ui/core";
import theme from "./theme";

// Views
import MenuAppBar from "./components/MenuAppBar";
import Login from "./components/Login";


function App() {
  return (
    <ThemeProvider theme={theme}>
      <MenuAppBar />
      <Login />
    </ThemeProvider>
  );
}

export default App;
