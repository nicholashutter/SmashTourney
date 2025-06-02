using System;
using Xunit;
using Validators;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace Tests;

public class UserRepositoryTest
{

    UserRepositoryTest()
    {

    }

    [Fact]
    public void testCreateUserAsync()
    {
        AppDBContext db = new AppDBContext();



        //  UserRepository userRepository = new UserRepository()
    }
}

