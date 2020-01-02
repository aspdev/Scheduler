using IdentityModel;
using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserRepository _userRepository;

        public CustomResourceOwnerPasswordValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (await _userRepository.ValidateCredentials(context.UserName, context.Password, context.Request.Client.ClientId))
            {
                var user = await _userRepository.FindByUsernameAndClientName(context.UserName, context.Request.Client.ClientId);
                context.Result = new GrantValidationResult(user.Id, OidcConstants.AuthenticationMethods.Password);
            }
                        
        }
    }
}
