import React from "react";
import ReactDOM from "react-dom";
import { Route, Link, BrowserRouter, Router, Routes } from "react-router-dom";
import { Elm as StatsElm } from "./Elm/Pages/Stats";
import { Elm as LoginElm } from "./Elm/Pages/Login";
import "./assets/tree.svg";
import "bootstrap/dist/css/bootstrap.min.css";
import "@fortawesome/fontawesome-free/css/all.min.css";

const tokenName = "PlantAuthToken";

const StatsPage = () => {
  const [app, setApp] = React.useState<StatsElm.Pages.Stats.App | undefined>();
  const elmRef = React.useRef(null);

  const elmApp = () =>
    StatsElm.Pages.Stats.init({ node: elmRef.current, flags: 12 });

  React.useEffect(() => {
    setApp(elmApp());
  }, []);

  /*
  // Subscribe to state changes from Elm
  React.useEffect(() => {
    app &&
      app.ports.updateCountInReact.subscribe((newCount) => {
        setCount(newCount);
      });
  }, [app]);*/

  return <div ref={elmRef}></div>;
};

const LoginPage = () => {
  const [app, setApp] = React.useState<LoginElm.Pages.Login.App | undefined>();
  const elmRef = React.useRef(null);

  const elmApp = () =>
    LoginElm.Pages.Login.init({
      node: elmRef.current,
      flags: localStorage.getItem(tokenName),
    });

  React.useEffect(() => {
    setApp(elmApp());
  }, []);

  /*
  // Subscribe to state changes from Elm
  React.useEffect(() => {
    app &&
      app.ports.updateCountInReact.subscribe((newCount) => {
        setCount(newCount);
      });
  }, [app]);*/

  return <div ref={elmRef}></div>;
};

const NotFound = () => {
  return (
    <div>
      There is nothing at this url. Maybe you wanted to{" "}
      <a href="/login">log in</a>
    </div>
  );
};

const App = () => (
  <BrowserRouter>
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/stats" element={<StatsPage />} />
    </Routes>
  </BrowserRouter>
);

ReactDOM.render(<App />, document.querySelector("#root"));
