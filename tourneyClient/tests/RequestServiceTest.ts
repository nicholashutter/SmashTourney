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

})