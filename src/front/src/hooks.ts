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
        send(data: { command: { id: string; name: string; aggregate: { id: string; name: string } }; success: boolean }): void
      };
      resizeAccordions: {
        subscribe(callback: (data: null) => void): void
      };
      dismissNotification: {
        subscribe(callback: (data: { command: { id: string; name: string; startedTime: string; aggregate: { id: string; name: string } }; success: boolean }) => void): void
      };
    };
  }
>(
  init: (options: { node?: HTMLElement | null; flags: any }) => TApp,
  config: {
    additional?: (app: Omit<TApp["ports"], "navigate" | "goBack" | "resizeAccordions" | "dismissNotification">) => void;
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
          resp.notifications = [message].concat(resp.notifications);
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
  let ports = app?.ports as Omit<TApp["ports"], "navigate" | "goBack" | "resizeAccordions" | "dismissNotification">;
  React.useEffect(() => {
    if (app) {
      app.ports.navigate?.subscribe((location) => {
        console.log("Navigating to ", location);
        navigate("/wrapper/" + encodeURIComponent(location));
      });

      app.ports.goBack?.subscribe((_) => {
        navigate("/wrapper/" + "-1");
      });

      app.ports.dismissNotification?.subscribe((notification) => {
        if (resp) {
          resp.notifications = resp.notifications.filter(n => n.command.id != notification.command.id);
          store(resp);
        }
      });

      app.ports.resizeAccordions?.subscribe((_) => {
        const accordions = document.getElementsByClassName("accordion");
        for (let index = 0; index < accordions.length; index++) {
          const accordion = accordions[index];
          if (accordion) {
            for (let index = 0; index < accordion.childNodes.length; index++) {
              const cards = accordion.childNodes[index];
              for (let index2 = 0; index2 < cards.childNodes.length; index2++) {
                const body = cards.childNodes[index2];
                if (body instanceof HTMLElement && body.id) {
                  if (body.style.height == "0px") {
                    //element.style.removeProperty("height");
                    //element.style.height = "0%";
                  } else {
                    body.style.removeProperty("height");
                    body.style.height = "100%";
                  }
                }
              }
            }
          }
        }
      });

      if (additional) {
        additional(ports);
      }
    }
  }, [app]);

  return { elmRef, ports };
};

export { useElmApp };