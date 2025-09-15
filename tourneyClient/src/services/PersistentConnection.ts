
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { HUB_URL } from "@/ServerConstants";


class SignalRService
{

    private connection: HubConnection | null = null;
    constructor()
    {

    }
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
            console.log("Server called here");

        });
        this.connection.start().catch(error => console.log(error));
    }
    public async notifyOthers(gameId: string)
    {
        try
        {
            await this.connection?.invoke("NotifyOthers", gameId);
        }
        catch (err)
        {
            console.error(err);
        }
    }
    public async updateOthers(playerName: string)
    {
        try
        {
            await this.connection?.invoke("UpdatePlayers", playerName);
        }
        catch (err)
        {
            console.error(err);
        }
    }

}
export default SignalRService;