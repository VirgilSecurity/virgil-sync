namespace Virgil.SDK.Domain
{
    using System;
    using Identities;

    public static class EnumParseExtensions
    {
        public static VerifiableIdentityType ToIdentityType(this string source)
        {
            switch (source)
            {
                case "email":
                    return VerifiableIdentityType.Email;
            }
           
            throw new InvalidOperationException("Can't verify this identity type");
        }
    }
}