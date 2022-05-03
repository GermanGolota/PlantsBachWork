import React, { useCallback, useState } from "react";
import ReactDOM from "react-dom";
import {
  Route,
  Link,
  BrowserRouter,
  Router,
  Routes,
  Navigate,
  useNavigate,
  useParams,
} from "react-router-dom";
import { Elm as StatsElm } from "./Elm/Pages/Stats";
import { Elm as LoginElm } from "./Elm/Pages/Login";
import { Elm as SearchElm } from "./Elm/Pages/Search";
import { Elm as PlantElm } from "./Elm/Pages/Plant";
import "./assets/tree.svg";
import "bootstrap/dist/css/bootstrap.min.css";
import "@fortawesome/fontawesome-free/css/all.min.css";
import "./main.css";
import { AuthResponse, retrieve, store } from "./Store";

const SearchPage = () => {
  const [app, setApp] = React.useState<
    SearchElm.Pages.Search.App | undefined
  >();
  const elmRef = React.useRef(null);

  const elmApp = () => {
    let model = retrieve();

    return SearchElm.Pages.Search.init({
      node: elmRef.current,
      flags: model,
    });
  };

  React.useEffect(() => {
    setApp(elmApp());
  }, []);

  return <div ref={elmRef}></div>;
};

const PlantPage = (props: { isOrder: boolean }) => {
  const [app, setApp] = React.useState<PlantElm.Pages.Plant.App | undefined>();
  const elmRef = React.useRef(null);

  const { plantId } = useParams();

  const elmApp = () => {
    let model = retrieve();
    let finalResult = {
      ...model,
      plantId: plantId,
      isOrder: props.isOrder,
    };
    return PlantElm.Pages.Plant.init({
      node: elmRef.current,
      flags: finalResult,
    });
  };

  React.useEffect(() => {
    setApp(elmApp());
  }, []);

  return <div ref={elmRef}></div>;
};

const StatsPage = () => {
  const [app, setApp] = React.useState<StatsElm.Pages.Stats.App | undefined>();
  const elmRef = React.useRef(null);

  const elmApp = () => {
    let model = retrieve();

    return StatsElm.Pages.Stats.init({
      node: elmRef.current,
      flags: model,
    });
  };

  React.useEffect(() => {
    setApp(elmApp());
  }, []);

  return <div ref={elmRef}></div>;
};

const LoginPage = () => {
  const [app, setApp] = React.useState<LoginElm.Pages.Login.App | undefined>();
  const elmRef = React.useRef(null);

  const elmApp = () =>
    LoginElm.Pages.Login.init({
      node: elmRef.current,
      flags: null,
    });

  React.useEffect(() => {
    if (retrieve()) {
      window.location.replace("/search");
    }
    setApp(elmApp());
  }, []);
  // Subscribe to state changes from Elm
  React.useEffect(() => {
    app &&
      app.ports.notifyLoggedIn.subscribe((userModel) => {
        let model = userModel as AuthResponse;
        store(model);
        window.location.replace("/search");
      });
  }, [app]);

  return <div ref={elmRef}></div>;
};

const NotFound = () => {
  return (
    <div>
      There is nothing at this url. Maybe you wanted to{" "}
      <a href="/login">log in</a>?
    </div>
  );
};

const App = () => (
  <BrowserRouter>
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/stats" element={<StatsPage />} />
      <Route path="/search" element={<SearchPage />} />
      <Route path="/plant/:plantId" element={<PlantPage isOrder={false} />} />
      <Route
        path="/plant/:plantId/order"
        element={<PlantPage isOrder={true} />}
      />
      <Route path="*" element={<NotFound />} />
    </Routes>
  </BrowserRouter>
);

ReactDOM.render(<App />, document.querySelector("#root"));
