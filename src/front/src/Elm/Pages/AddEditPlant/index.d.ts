// WARNING: Do not manually modify this file. It was generated using:
// https://github.com/dillonkearns/elm-typescript-interop
// Type definitions for Elm ports

export namespace Elm {
  namespace Pages.AddEditPlant {
    export interface App {
      ports: {
        navigate: {
          subscribe(callback: (data: string) => void): void
        }
        goBack: {
          subscribe(callback: (data: null) => void): void
        }
        notificationReceived: {
          send(data: { commandName: string; success: boolean; aggregate: { id: string; name: string } }): void
        }
        openEditor: {
          subscribe(callback: (data: string) => void): void
        }
        editorChanged: {
          send(data: string): void
        }
        resizeAggregates: {
          subscribe(callback: (data: null) => void): void
        }
        notifyLoggedIn: {
          subscribe(callback: (data: unknown) => void): void
        }
      };
    }
    export function init(options: {
      node?: HTMLElement | null;
      flags: any;
    }): Elm.Pages.AddEditPlant.App;
  }
}