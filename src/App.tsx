import React from "react";
import {
  BrowserRouter as Router,
  Switch,
  Route
} from "react-router-dom";
import { ThemeProvider } from "@material-ui/core";
import theme from "./theme";

// Views
import MenuAppBar from "./components/MenuAppBar";
import Login from "./components/Login";


function App() {
  return (
    <ThemeProvider theme={theme}>
    <Router>
    <MenuAppBar/>
      <Switch>
        <Route exact path="/" component={Login} />
      </Switch>
    </Router>
    </ThemeProvider>
  );
}

export default App;
