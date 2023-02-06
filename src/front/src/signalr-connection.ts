import * as signalR from "@microsoft/signalr";

const URL = process.env.HUB_ADDRESS ?? "https://localhost:5001/hub/notifications"; //or whatever your backend port is
class Connector {

  private connection: signalR.HubConnection;
  public events: (onCommandFinished: (notificationName: string, success: boolean) => void) => void;
  static instance: Connector;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(URL)
      .withAutomaticReconnect()
      .build();
    this.connection.start().catch(err => document.write(err));
    this.events = (onCommandFinished) => {
      this.connection.on("CommandFinished", (notificationName, success) => {
        onCommandFinished(notificationName, success);
      });
    };
  }

  public static getInstance(): Connector {
    if (!Connector.instance)
      Connector.instance = new Connector();
    return Connector.instance;
  }
}
export default Connector.getInstance;