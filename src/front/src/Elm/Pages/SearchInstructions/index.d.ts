// WARNING: Do not manually modify this file. It was generated using:
// https://github.com/dillonkearns/elm-typescript-interop
// Type definitions for Elm ports

export namespace Elm {
  namespace Pages.SearchInstructions {
    export interface App {
      ports: {
        navigate: {
          subscribe(callback: (data: string) => void): void
        }
        openEditor: {
          subscribe(callback: (data: string) => void): void
        }
        editorChanged: {
          send(data: string): void
        }
        notifyLoggedIn: {
          subscribe(callback: (data: unknown) => void): void
        }
      };
    }
    export function init(options: {
      node?: HTMLElement | null;
      flags: any;
    }): Elm.Pages.SearchInstructions.App;
  }
}