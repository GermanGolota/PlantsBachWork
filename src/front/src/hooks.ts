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
        send(data: { command: { commandId: string; commandName: string; aggregate: { id: string; name: string } }; success: boolean }): void
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

  let resp = retrieve();

  if (resp) {
    const { events } = Connector(resp.token);

    useEffect(() => {
      events((message) => {
        if (resp) {
          resp.notifications = resp.notifications.concat(message);
          store(resp);
        }
        app?.ports.notificationReceived.send(message);
      });
    });
  }

  const elmApp = () => {
    return init({
      node: elmRef.current,
      flags: setFlags ? setFlags(resp!) : resp,
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
      app.ports.navigate?.subscribe((location) => {
        console.log("Navigating to ", location);
        navigate("/wrapper/" + encodeURIComponent(location));
      });

      app.ports.goBack?.subscribe((_) => {
        navigate("/wrapper/" + "-1");
      });

      if (additional) {
        additional(ports);
      }
    }
  }, [app]);

  return { elmRef, ports };
};

export { useElmApp };