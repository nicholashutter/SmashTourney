import { BASE_URL } from "./ServerConstants";
// define interface for http methods used below
export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

// define interface for object that provides centralized wrapper for endpoint urls and http methods
export interface ApiEndpoint
{
  method: HttpMethod;
  path: string;
}

// define enum like object with all url paths and respective http methods
const RequestBuilder = {
  getAllGames: { method: "GET", path: "/Games/getAllGames" },
  createGame: { method: "POST", path: "/Games/CreateGame" },
  getGameById: { method: "GET", path: "/Games/GetGameById/{gameId}" },
  endGame: { method: "GET", path: "/Games/EndGame/{gameId}" },
  addPlayers: { method: "POST", path: "/Games/AddPlayers/{gameId}" },
  createUserSession: { method: "POST", path: "/Games/CreateUserSession" },
  startGame: { method: "POST", path: "/Games/StartGame/{gameId}" },
  loadGame: { method: "POST", path: "/Games/LoadGame/{gameId}" },
  saveGame: { method: "POST", path: "/Games/SaveGame/{gameId}" },
  startRound: { method: "POST", path: "/Games/StartRound/{gameId}" },
  startMatch: { method: "POST", path: "/Games/StartMatch/{gameId}" },
  endMatch: { method: "POST", path: "/Games/EndMatch" },

  register: { method: "POST", path: "/Users/Register" },
  login: { method: "POST", path: "/Users/Login" },
  profile: { method: "GET", path: "/Users/Profile/{userId}" },
  updateProfile: { method: "PUT", path: "/Users/UpdateProfile" },
  logout: { method: "POST", path: "/Users/logout" },

  getAllUsers: { method: "GET", path: "/users/GetAllUsers" },
  getUserById: { method: "GET", path: "/users/GetById/{Id}" },
  getUserByUserName: { method: "GET", path: "/users/GetByUserName/{UserName}" },
  updateUser: { method: "PUT", path: "/users/UpdateUser" },
  deleteUser: { method: "DELETE", path: "/users/{Id}" }
} as const satisfies Record<string, ApiEndpoint>;

// 🔹 5. Generate Request object with full URLs
export const Request = Object.fromEntries(
  Object.entries(RequestBuilder).map(([key, { method, path }]) => [
    key,
    {
      method,
      path,
      url: `${BASE_URL}${path}`
    }
  ])
) as {
    [K in keyof typeof RequestBuilder]: ApiEndpoint & { url: string }
  };

//Function to wrap fetch api with type safety, expecting Request object
export const RequestService =
  <Key extends keyof typeof Request, Req, Res>(
    endpoint: Key,
    options?: Omit<RequestInit, "body" | "method"> & { body?: Req }
  ): Promise<Res> =>
  {
    const { url, method } = Request[endpoint];

    return fetch(url, {
      method,
      headers: { "Content-Type": "application/json" },
      ...options,
      body: options?.body ? JSON.stringify(options.body) : undefined
    }).then((res) =>
    {
      if (!res.ok)
      {
        throw new Error(`HTTP ${res.status}`);
      }
      return res.json() as Promise<Res>;
    });
  };