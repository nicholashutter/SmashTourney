/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */

import { test, expect, vi, beforeEach } from 'vitest';
import { RequestService } from '../src/utilities/RequestService';
import { ApplicationUser } from '../src/models/entities/ApplicationUser';

const UserName = "TestUser";
const Password = "P@SSW0RD123!"
const Email = `${UserName}@email.com`

const User: ApplicationUser =
{
    UserName,
    Password,
    Email
}

beforeEach(
    () => 
    {
        global.fetch = vi.fn();
    }
)

//unit test level fetchService test mocking request formatting
test("RequestService submits a properly formatted http request", async () =>
{

    (fetch as any).mockResolvedValueOnce(
        {
            ok: true,
            json: (async: any) => (
                {
                    success: true
                }
            )
        }
    );

    //RequestService is a custom api to wrap fetch that includes type safety for back end
    const result = await RequestService("createUserSession",
        {
            body:
            {
                User
            }
        });
    expect(result).toEqual(
        {
            success: true
        }
    )

    //Using mock provided fetch test that shape of data is correct and reponse returns success
    const [url, options] = (fetch as any).mock.calls[0];

    expect(url).toContain("createUserSession");

    expect(options.method).toBe("POST"); 
    expect(options.headers["Content-Type"]).toBe("application/json");

    const parsedBody = JSON.parse(options.body);
    expect(parsedBody.User).toEqual(User);

})