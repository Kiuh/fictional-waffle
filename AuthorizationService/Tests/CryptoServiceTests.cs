using Microsoft.Extensions.Options;

namespace Tests
{
    public class Tests
    {
        private CryptographyService cryptographyService;
        private readonly string encryptedString =
            "AAEAABAAAAB10jkgJ8E5OZzx1q8cj9Exrvf/a67HL52O7CdydkXmbjFncmRtKdpA54YSuiI+EO07rQkDI6f+gegILDDZVZ9fCVkDQuZ87JJL8uqD3ImmzGXX90O1O4jR11DHI3e+Tvjc6IMYmBWZ6BSCiSZUhCtfeFMHbj1FoE/+KEWpDDI+fbsMpaF4UitzbdoEuteSR66wBvRnvndXKLOanJS+WtLirORTU3NSGh9H+3qT6FJJFOuQcpncQmbP7Y1kHF3dAuFws5ra5beIRs59J8ugUGKJjWMTMdLsGJlgtQwRePRtVQmkfnYGaQsxMpLFkhQ82Kt8wUZndgXVzcrAe4qJkjyiX0VsoqGJhw7t4yGgnnxYzPXY/skRdNb8dWMyLXELeOW9W4bDoneAMTT0g5FY59Ry";
        private readonly string notEncryptedString = "NotEncryptedString";
        private readonly string privateKey =
            "<RSAKeyValue><Modulus>ti5zrmtBCsqgWppC44frnBJdQcUqWEcnveLt6+fQXbaFKBY3Ic6uTunyqfKAFVG2LSth2xYVkD7jZHkfcjYR9w23kpjjJMgy4+YHjuA4mBAmFxU9jnxOSStu8BV44K/VePrlrNosHROTNpUwK46WK3mwVc7PrDU7bdEMgBr0vK7sH9gfWIfn7/+D/Tz77G9VAjtbEo3SGC/M1NdSAoC1510jQ/wMjvGDJthvIl52GpW0M+mXTAiy638YPoyJeG19SaDnySM9euouvxLkKJFVPpwb1YUCR5fclxUgR0AKoJQL7zmLj1pDtHVSClZ82SFXGZmjzEVB8k5kF10K9NU+3Q==</Modulus><Exponent>AQAB</Exponent><P>xANj/qg75Ab3CRnN2EqnYLu/dUvZbgfFQcbsbr7vECUAdw/4ewVBu3sZ6o1x0WqHixHCClNnhggnCai0FAGv8xZ7mTB/J2Z6ss7X242Z7hRezfBm9WdklRWmk9HAvirkiI2iuQZS15e0+qMRoPNrgjA1EeIy8V8rKVoC5MBmZBc=</P><Q>7e9ptryB9P4DRQULjew5JZVYTug6vFqNWMklxzLmUm0VC0f128C2FPvnxmrwTAbbJ5Mhm0HkzZORYtW+pi9Fb/odM4rF3QEqTY3Mhu4Gxi7q6nIeTEobLvCEb27D7vWlbW+wwA0rGfE6Vxi+kVTzymk8mb6fLGZu/7Cts2wzaSs=</Q><DP>aCR1CGRSDcE1l3xCRdzEOT0HaEa4ayFtyJjsHZsMYUOHj7rIhnQwjG/HQTuNcXpsMZNpyRqvMiM6uoMLymVrGnijiqydok1se3wya9A8LKZeCITl7xTT9/Hc40TaZy9a5MJ75lpZwXo1CcWCJEWwpSb/y1SnRF8QC5RrXSZsQFk=</DP><DQ>w7RlnmGE/w8jf1S8ATsocgpZ/WU0UpKkqj+grCzolymT4piI1/y1h6L/LAvvpIkLH4z8WmijmpfjQIkQ7D37fjQaaredMb/wnKOic+U6ey5CGR5OX2+g1kYMmF9iF53DOLVii0UJi6gH1XXL4VwfvgA7UTYiVHHYoMUgynCYBMs=</DQ><InverseQ>lTfgIJ8kQzWo+Tf76lWGNRdOkPp6Wn/POse1q0X8bv7hShkVG1Ztaqps0BIoTRrRHF8ribXJxYEvypcz822Nds/Wb/SpvUe9EqEk1rVzei0bxoqB0Mcw4D6jumaaE+kuwkCgD1kIt0Mx65Q+pZeQyGbrkM7YPObxeYtBh1Qnwm8=</InverseQ><D>FRIyVw4Yq8Lb4R7OdbqWyapmFuFTEHrKYfAJTKu3Md+WbxiCr+pTxtTQOE/P+KdGxtqpqslXHSPo8QoBVRhj44s2nEIKo4p0OF+2qBPO9+eGQE04nW5LL7NSVXpmTLaJnkCGqANj7skA0eNXevI29DhS6NnOs0BE62Kua3Kys8/lVqhJsOTB7PThUiN6gVFPHrXbEUhI2YQH+GRD5rmqHcb3GdZMHD6lKClSPVDugENh/sTVPCl1bKix3Lofe0is38PMPUJin5P01XWqkfIS7H6O+hISmEmZLcmmKc0p91EzUJMnuCO3bUlGZ86oeS24QTJeRcg/etwtqD2Z2QbnEQ==</D></RSAKeyValue>";
        private readonly string publicKey =
            "<RSAKeyValue><Modulus>ti5zrmtBCsqgWppC44frnBJdQcUqWEcnveLt6+fQXbaFKBY3Ic6uTunyqfKAFVG2LSth2xYVkD7jZHkfcjYR9w23kpjjJMgy4+YHjuA4mBAmFxU9jnxOSStu8BV44K/VePrlrNosHROTNpUwK46WK3mwVc7PrDU7bdEMgBr0vK7sH9gfWIfn7/+D/Tz77G9VAjtbEo3SGC/M1NdSAoC1510jQ/wMjvGDJthvIl52GpW0M+mXTAiy638YPoyJeG19SaDnySM9euouvxLkKJFVPpwb1YUCR5fclxUgR0AKoJQL7zmLj1pDtHVSClZ82SFXGZmjzEVB8k5kF10K9NU+3Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        [SetUp]
        public void Setup()
        {
            CryptographyServiceSettings settings = new() { PrivateKey = privateKey };
            cryptographyService = new CryptographyService(Options.Create(settings));
        }

        [Test]
        public void HashStringTest()
        {
            string notHashedString = "NotHashedString";

            string hashedString = cryptographyService.HashString(notHashedString);

            Assert.That(
                hashedString,
                Is.EqualTo("cfb4a240297d3efcafd40732353fc6537710234fa335979119f3019f177106c2")
            );
        }

        [Test]
        public void DecryptStringTest()
        {
            string decryptedString = cryptographyService.DecryptString(encryptedString);
            Assert.That(decryptedString, Is.EqualTo(notEncryptedString));
        }

        [Test]
        public void GetPublicKeyTest()
        {
            Assert.That(cryptographyService.GetPublicKey(), Is.EqualTo(publicKey));
        }
    }
}
