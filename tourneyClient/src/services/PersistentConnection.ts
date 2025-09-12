
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { HUB_URL } from "@/ServerConstants";


class SignalRService
{

    private connection: HubConnection | null = null;
    constructor(private username: string)
    {

    }
    public createUserRoomConnection()
    {
        this.connection = new HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect()
            .build();
    }
}
export default SignalRService;