import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AuthResponse, retrieve, store } from "./Store";
import Connector from "./signalr-connection";

const useElmApp = <
  TApp extends {
    ports: {
      navigate: {
        subscribe(callback: (data: string) => void): void;
      };
      goBack: {
        subscribe(callback: (data: null) => void): void;
      };
      notificationReceived: {
        send(data: { commandName: string; success: boolean; aggregate: { id: string; name: string } }): void
      };
    };
  }
>(
  init: (options: { node?: HTMLElement | null; flags: any }) => TApp,
  config: {
    additional?: (app: Omit<TApp["ports"], "navigate" | "goBack">) => void;
    onSetApp?: () => void;
    setFlags?: (auth: AuthResponse) => any;
  }
) => {
  const { additional, onSetApp, setFlags } = config;

  const [app, setApp] = React.useState<TApp | undefined>();
  const elmRef = React.useRef(null);
  const navigate = useNavigate();

  const elmApp = () => {
    let model = retrieve();
    return init({
      node: elmRef.current,
      flags: setFlags ? setFlags(model) : model,
    });
  };

  React.useEffect(() => {
    if (onSetApp) {
      onSetApp();
    }
    setApp(elmApp());
  }, []);
  // Subscribe to state changes from Elm
  let ports = app?.ports as Omit<TApp["ports"], "navigate" | "goBack">;
  React.useEffect(() => {
    if (app) {
      app.ports.navigate.subscribe((location) => {
        console.log("Navigating to ", location);
        navigate("/wrapper/" + encodeURIComponent(location));
      });

      app.ports.goBack.subscribe((_) => {
        navigate("/wrapper/" + "-1");
      });

      if (additional) {
        additional(ports);
      }
    }
  }, [app]);

  let resp = retrieve();

  if (resp) {
    const { events } = Connector(resp.token);

    useEffect(() => {
      events((message) => {
        resp.notifications.concat(message);
        store(resp);
        app?.ports.notificationReceived.send(message);
      });
    });
  }

  return { elmRef, ports };
};

export { useElmApp };