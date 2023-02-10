import * as signalR from "@microsoft/signalr";

const URL = "http://localhost:5000/commandsnotifications"; //or whatever your backend port is
class Connector {

  private connection: signalR.HubConnection;
  private token: string;
  public events: (onCommandFinished: (message: NotificationMessage) => void) => void;
  static instance: Connector;

  constructor(token: string) {
    this.token = token;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(URL, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory: async () => { return this.token }
      })
      .withAutomaticReconnect()
      .build();
    this.connection.start().catch(err => { console.log(err); });
    this.events = (onCommandFinished) => {
      this.connection.on("CommandFinished", (message) => {
        onCommandFinished(message);
      });
    };
  }

  public static getInstance(token: string): Connector {
    if (!Connector.instance)
      Connector.instance = new Connector(token);
    return Connector.instance;
  }
}

export type NotificationMessage = {
  command: {
    id: string,
    name: string,
    startedTime: string,
    aggregate: {
      id: string,
      name: string
    }
  },
  success: boolean
}


export default Connector.getInstance;