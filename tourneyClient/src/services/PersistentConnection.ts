
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { HUB_URL } from "@/ServerConstants";
import { Player } from "@/models/entities/Player";


class SignalRService
{

    private connection: HubConnection | null = null;

    private onPlayersUpdated: ((players: Player[]) => void) | null;

    private gameId: string = "";

    constructor()
    {

    }
    /**
   * Establishes a SignalR connection and sets up event listeners.
   * - "Successfully Joined" triggers updateOthers(gameId)
   * - "PlayersUpdated" invokes the registered callback with updated player data
   */
    public createPlayerConnection()
    {
        this.connection = new HubConnectionBuilder()
            .withUrl(HUB_URL,
                {
                    withCredentials: true,
                }
            )
            .withAutomaticReconnect()
            .build();

        this.connection.on("Successfully Joined", () =>
        {
            this.updateOthers(this.gameId);

        });
        this.connection.on("PlayersUpdated", (players: Player[]) =>
        {
            if (this.onPlayersUpdated)
            {
                this.onPlayersUpdated(players);
            }
        });
        this.connection.start().catch(error => console.log(error));
    }
    /**
   * Registers a callback to handle incoming player updates from the hub.
   */
    public setOnPlayersUpdated(callback: (players: Player[]) => void)
    {
        this.onPlayersUpdated = callback;
    }
    /**
   * Stores the current game session ID used for hub communication.
   */
    public setGameId(gameId: string)
    {
        this.gameId = gameId;
    }
    /**
   * Invokes the "UpdatePlayers" method on the hub to notify other clients.
   */
    public async updateOthers(gameId: string)
    {
        try
        {
            await this.connection?.invoke("UpdatePlayers", gameId);
        }
        catch (err)
        {
            console.error(err);
        }
    }

}
export default SignalRService;