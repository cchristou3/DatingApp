import { environment } from "src/environments/environment";
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from "../_models/user";

/**
 * Base class for all the services that contain logic related to SignalR's hubs.
 */
export class HubService {
    hubUrl = environment.hubUrl;
    private _hubConnection: HubConnection;

    public get hubConnection(): HubConnection {
        return this._hubConnection;
    }

    /** Creates a connection with the Hub based on the given endpoint.
     * 
     * !Any subclasses that wish to override it, require to call it first before any subclass related logic.
     * 
     * @param {User} user the user tries to connect to the SignalR hub
     * @param {string} endpoint the endpoint to call (which specifies which Hub to use (PreseneceHub, MessageHub, etc.))
     */
    createHubConnection(user: User, endpoint: string) {
        this._hubConnection = new HubConnectionBuilder().withUrl(this.hubUrl + endpoint, {
            accessTokenFactory: () => user.token
        })
            .withAutomaticReconnect()
            .build();

        this._hubConnection
            .start()
            .catch(error => console.log(error));
    }

    /** Stops the connection.
     */
    stopHubConnection() {
        if (this._hubConnection)
            this._hubConnection
                .stop()
                .catch(error => console.log(error));
    }
}