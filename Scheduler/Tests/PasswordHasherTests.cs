using System;
using Common;
using Xunit;

namespace Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashesPassword()
        {
            var salt = Convert.FromBase64String("AD/aANhlLPua8OEdMlsvFA==");
            var hashedPassword = PasswordHasher.HashPassword("password", salt);

            Assert.Equal("OYouVG00pm0DsBfeeZFSF9QD8s1uDyYMAcuEplu43jc=", hashedPassword);
        }
    }
}