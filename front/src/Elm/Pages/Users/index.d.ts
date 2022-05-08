// WARNING: Do not manually modify this file. It was generated using:
// https://github.com/dillonkearns/elm-typescript-interop
// Type definitions for Elm ports

export namespace Elm {
  namespace Pages.Users {
    export interface App {
      ports: {
        notifyLoggedIn: {
          subscribe(callback: (data: unknown) => void): void
        }
      };
    }
    export function init(options: {
      node?: HTMLElement | null;
      flags: any;
    }): Elm.Pages.Users.App;
  }
}