
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

        this.connection.on("Player Joined", () =>
        {
            console.log("Server called here");
        });
        this.connection.start().catch(error => console.log(error));
    }
}
export default SignalRService;