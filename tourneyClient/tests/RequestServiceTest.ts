import { test } from 'vitest';
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

test("RequestService submits a properly formatted http request", () =>
{
    RequestService("createUserSession",
        {
            body:
            {
                User
            }
        })

})