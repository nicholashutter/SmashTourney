// Defines backend base URLs used by API and SignalR clients.
export const SERVER_HOST = "http://localhost";
export const SERVER_PORT = 5280;
export const BASE_URL = `${SERVER_HOST}:${SERVER_PORT}`;
export const HUB_URL = `${BASE_URL}/hubs/GameServiceHub`
