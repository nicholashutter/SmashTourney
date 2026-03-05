import { BASE_URL } from "@/ServerConstants";

// Defines supported HTTP verbs for API requests.
export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

// Defines a single API endpoint mapping.
export interface ApiEndpoint
{
  method: HttpMethod;
  path: string;
  params?: string[];
}

// Stores API endpoint metadata used by request helpers.
const RequestBuilder = {
  createGameWithMode: { method: "POST", path: "/Games/CreateGameWithMode" },
  getBracket: {
    method: "GET",
    path: "/Games/GetBracket/{gameId}",
    params: ["gameId"]
  },
  getCurrentMatch: {
    method: "GET",
    path: "/Games/GetCurrentMatch/{gameId}",
    params: ["gameId"]
  },
  getFlowState: {
    method: "GET",
    path: "/Games/GetFlowState/{gameId}",
    params: ["gameId"]
  },
  submitMatchVote: {
    method: "POST",
    path: "/Games/SubmitMatchVote/{gameId}",
    params: ["gameId"]
  },
  addPlayers: {
    method: "POST",
    path: "/Games/AddPlayer/{gameId}",
    params: ["gameId"]
  },
  getPlayersInGame: {
    method: "POST",
    path: "/Games/GetPlayersInGame/{gameId}",
    params: ["gameId"]
  },
  startGame: {
    method: "POST",
    path: "/Games/StartGame/{gameId}",
    params: ["gameId"]
  },

  register: { method: "POST", path: "/Users/Register" },
  login: { method: "POST", path: "/users/login" },
  sessionStatus: { method: "GET", path: "/users/session" },
  logout: { method: "POST", path: "/users/logout" }
} as const satisfies Record<string, ApiEndpoint>;

// Builds request definitions with fully qualified URLs.
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

// Sends a typed API request for the selected endpoint.
export const RequestService =
  <Key extends keyof typeof Request, Req, Res>(
    endpoint: Key,
    options?: Omit<RequestInit, "body" | "method"> & {
      body?: Req;
      routeParams?: Record<string, string>;
    }
  ): Promise<Res> =>
  {
    const { path, method } = Request[endpoint];
    let interpolatedPath = path;
    if (options && options.routeParams)
    {
      interpolatedPath = insertParams(path, options.routeParams);
    }

    const url = `${BASE_URL}${interpolatedPath}`;

    let requestBody: string | undefined = undefined;
    if (options && options.body)
    {
      requestBody = JSON.stringify(options.body);
    }

    return fetch(url, {
      method,
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      ...options,
      body: requestBody
    }).then(parseResponse<Res>);
  };

// Parses an HTTP response into a typed result payload.
const parseResponse = async <Res>(res: Response): Promise<Res> =>
{
  if (!res.ok)
  {
    const errorBody = await safelyReadResponseBody(res);
    if (errorBody)
    {
      throw new Error(`HTTP ${res.status}: ${errorBody}`);
    }

    throw new Error(`HTTP ${res.status}`);
  }

  if (res.status === 204)
  {
    return undefined as Res;
  }

  let contentType = "";
  if (res.headers && typeof res.headers.get === "function")
  {
    const contentTypeHeader = res.headers.get("content-type");
    if (contentTypeHeader)
    {
      contentType = contentTypeHeader;
    }
  }
  const textBody = typeof res.text === "function" ? await res.text() : "";

  if (!textBody)
  {
    if (typeof res.json === "function")
    {
      return res.json() as Promise<Res>;
    }

    return undefined as Res;
  }

  return parseResponseBody<Res>(textBody, contentType);
};

// Reads response text safely for error construction.
const safelyReadResponseBody = async (res: Response): Promise<string> =>
{
  try
  {
    if (typeof res.text !== "function")
    {
      return "";
    }

    const body = await res.text();
    return body || "";
  }
  catch
  {
    return "";
  }
};

// Parses text response content as JSON when possible.
const parseResponseBody = <Res>(textBody: string, contentType: string): Res =>
{
  if (contentType.includes("application/json"))
  {
    return JSON.parse(textBody) as Res;
  }

  try
  {
    return JSON.parse(textBody) as Res;
  }
  catch
  {
    return textBody as Res;
  }
};

// Replaces route placeholders with encoded path parameter values.
const insertParams = (
  path: string,
  params: Record<string, string>
): string =>
{
  return path.replace(/{(\w+)}/g, (_, key) =>
  {
    if (!(key in params))
    {
      throw new Error(`Missing route param: ${key}`);
    }
    return encodeURIComponent(params[key]);
  });
};

