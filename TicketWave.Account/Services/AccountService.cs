using System.Text;
using TicketWaveAccountApi.Dtos;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using System.Security.Cryptography;
using System.Net;

namespace TicketWaveAccountApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;

        private readonly ITenantService _tenantService;

        public AccountService(ITenantService tenantService)
        {
            _cognitoClient = new AmazonCognitoIdentityProviderClient(
                            "",
                            "",
                            Amazon.RegionEndpoint.USEast1);
            _tenantService = tenantService;
        }
        public async Task<string> RegisterUserAsync(RegisterDto dto)
        {
            var signUpRequest = new SignUpRequest
            {
                ClientId = "",
                SecretHash= GenerateSecretHash(dto.Email, "", ""),
                Username = dto.Email,
                Password = dto.Password,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = dto.Email },
                    new AttributeType { Name = "name", Value = dto.UserName }
                }
            };

            var response = await _cognitoClient.SignUpAsync(signUpRequest);
            
            await _tenantService.CreateAsync("organização" + dto.UserName, dto.Email, Guid.Parse(response.UserSub));

            return response.UserConfirmed ? "Usuário confirmado!" : "Usuário registrado, confirmação pendente!";
        }

        public async Task<object> LoginAsync(LoginDto dto)
        {
            var authParameters = new Dictionary<string, string>
            {
                { "PASSWORD", dto.Password },
                { "USERNAME", dto.Email },
                { "SECRET_HASH", GenerateSecretHash(dto.Email, "", "") }
            };

            var authRequest = new InitiateAuthRequest
            {
                ClientId = "",
                AuthParameters = authParameters,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH
            };

            var response = await _cognitoClient.InitiateAuthAsync(authRequest);
            return response.AuthenticationResult;
        }

        private static string GenerateSecretHash(string username, string clientId, string clientSecret)
        {
            string message = username + clientId;
            byte[] keyBytes = Encoding.UTF8.GetBytes(clientSecret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashMessage = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashMessage);
            }
        }

        public async Task VerifyCodeAsync(VerifyEmailDto dto)
        {
            var confirmRequest = new ConfirmSignUpRequest
            {
                ClientId = "",
                SecretHash = GenerateSecretHash(dto.Email, "", ""),
                Username = dto.Email,
                ConfirmationCode = dto.Code
            };

            var response = await _cognitoClient.ConfirmSignUpAsync(confirmRequest);

            if (!response.HttpStatusCode.Equals(HttpStatusCode.OK))
            {
                throw new Exception("Código de confirmação inválido!");
            }
        }
    }
}
