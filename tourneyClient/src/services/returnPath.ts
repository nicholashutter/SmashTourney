// Carries the page someone was trying to reach across a trip through sign-in.
//
// Without this, a player whose cookie had expired clicked their tournament link,
// got bounced to sign-in, and arrived at the menu with the destination thrown
// away — which defeats the point of the URL being durable in the first place.

const RETURN_PARAM = "next";

// Builds the sign-in path that remembers where the player was headed.
export const signInPathReturningTo = (destination: string): string =>
{
    return `/?${RETURN_PARAM}=${encodeURIComponent(destination)}`;
};

// Reads a safe destination out of a query string, or null when there is none.
//
// Only same-site absolute paths are accepted. A value starting with "//" is a
// protocol-relative URL, so it would send a freshly signed-in player to another
// origin entirely — the classic open-redirect shape, and worth refusing even
// though nothing here is currently generating one.
export const readReturnPath = (search: string): string | null =>
{
    const destination = new URLSearchParams(search).get(RETURN_PARAM);

    if (!destination)
    {
        return null;
    }

    if (!destination.startsWith("/"))
    {
        return null;
    }

    if (destination.startsWith("//"))
    {
        return null;
    }

    return destination;
};
