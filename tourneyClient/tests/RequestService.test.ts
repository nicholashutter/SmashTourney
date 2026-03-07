import { beforeEach, expect, test, vi } from "vitest";
import { RequestService } from "../src/services/RequestService";

type MockFetchTextResponse = {
    ok: boolean;
    status: number;
    headers: {
        get: (key: string) => string | null;
    };
    text: () => Promise<string>;
};

const queueJsonResponse = (payload: unknown, status = 200, ok = true) =>
{
    const response: MockFetchTextResponse = {
        ok,
        status,
        headers: {
            get: () => "application/json"
        },
        text: async () => JSON.stringify(payload)
    };

    (fetch as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(response);
};

const getFetchCall = (callIndex: number) =>
{
    return (fetch as unknown as ReturnType<typeof vi.fn>).mock.calls[callIndex] as [string, RequestInit];
};

beforeEach(() =>
{
    global.fetch = vi.fn();
});

// Verifies login requests call the expected auth route.
test("login calls auth login route", async () =>
{
    queueJsonResponse({ message: "ok" });

    await RequestService("login", {
        body: {
            email: "user@example.com",
            password: "password"
        }
    });

    expect(getFetchCall(0)[0]).toContain("/users/login");
});

// Verifies register requests use POST method.
test("register uses POST method", async () =>
{
    queueJsonResponse({ message: "ok" });

    await RequestService("register", {
        body: {
            email: "user@example.com",
            password: "password"
        }
    });

    expect(getFetchCall(0)[1].method).toBe("POST");
});

// Verifies session status requests use GET method.
test("sessionStatus uses GET method", async () =>
{
    queueJsonResponse({ isAuthenticated: true });

    await RequestService("sessionStatus");

    expect(getFetchCall(0)[1].method).toBe("GET");
});

// Verifies logout requests include credentials for cookie auth.
test("logout includes credentials", async () =>
{
    queueJsonResponse({ message: "ok" });

    await RequestService("logout", {
        body: {}
    });

    expect(getFetchCall(0)[1].credentials).toBe("include");
});

// Verifies non-success responses throw an HTTP-formatted error.
test("RequestService throws HTTP error for non-success response", async () =>
{
    queueJsonResponse({ message: "Unauthorized" }, 401, false);

    await expect(
        RequestService("sessionStatus")
    ).rejects.toThrow("HTTP 401");
});
