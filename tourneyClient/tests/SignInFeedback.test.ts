import { expect, test } from "vitest";
import { describeSignInFailure } from "../src/services/signInFeedback";
import { describeRegistrationFailure } from "../src/services/registrationService";

// Verifies an unconfirmed account is told to go and read its email.
//
// This is the failure most likely to be mistaken for a wrong password, and the
// action needed is completely different.
test("an unconfirmed account is told to confirm, not to retype its password", () =>
{
    const message = describeSignInFailure("HTTP 403: {\"message\":\"Confirm your email\"}");

    expect(message).toContain("Confirm your email");
    expect(message).toContain("inbox");
});

// Verifies a locked-out account is told to wait.
test("a locked out account is told to wait rather than keep trying", () =>
{
    const message = describeSignInFailure("HTTP 423: locked");

    expect(message.toLowerCase()).toContain("wait");
});

// Verifies a rate-limited caller is told to wait.
test("a rate limited caller is told to wait", () =>
{
    const message = describeSignInFailure("HTTP 429: too many");

    expect(message.toLowerCase()).toContain("wait");
});

// Verifies bad credentials say so plainly.
test("bad credentials are reported as bad credentials", () =>
{
    const message = describeSignInFailure("HTTP 401");

    expect(message.toLowerCase()).toContain("not right");
});

// Verifies an unrecognised failure still says something useful.
test("an unknown failure falls back to a connection message", () =>
{
    const message = describeSignInFailure("something exploded");

    expect(message.toLowerCase()).toContain("connection");
});

// Verifies a 423 is not mistaken for a 429, or the reverse.
//
// The two are one digit apart and mean different things, so this pins down that
// the match is on the whole status rather than a loose substring.
test("lockout and rate limiting are not confused with each other", () =>
{
    expect(describeSignInFailure("HTTP 423")).not.toEqual(describeSignInFailure("HTTP 429"));
});

// Verifies the server's reasons for refusing a registration reach the user.
//
// Identity explains exactly why a password was rejected, and that explanation is
// the only thing that lets somebody fix it.
test("registration surfaces the server's stated reasons", () =>
{
    const message = describeRegistrationFailure(
        "HTTP 400: {\"message\":\"Could not create the account.\",\"reasons\":[\"Passwords must be at least 12 characters.\"]}");

    expect(message).toContain("at least 12 characters");
});

// Verifies a failed send is reported as such rather than as a bad password.
test("a failed confirmation send explains the account was not created", () =>
{
    const message = describeRegistrationFailure("HTTP 503: {\"detail\":\"nope\"}");

    expect(message).toContain("confirmation email");
    expect(message).toContain("not created");
});

// Verifies registration falls back sensibly with no parseable body.
test("registration falls back when the server said nothing useful", () =>
{
    const message = describeRegistrationFailure("HTTP 500");

    expect(message).toContain("could not create your account");
});

// Verifies a message-only body is still surfaced.
test("registration surfaces a message when there are no itemised reasons", () =>
{
    const message = describeRegistrationFailure("HTTP 400: {\"message\":\"That email is already taken.\"}");

    expect(message).toContain("already taken");
});
