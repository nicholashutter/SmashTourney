
import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { SERVER_HOST } from '../ServerConstants';


const PersistentConnection = () =>
{
    const SIGNAL_HUB = "";
    const PERSISTENT_CONNECTION = `${SERVER_HOST}/${SIGNAL_HUB}`;
    const [connection, setConnection] = useState(null);

    useEffect(() => 
    {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(PERSISTENT_CONNECTION)
    }
    )

    return (<div></div>);
}

export default PersistentConnection; 