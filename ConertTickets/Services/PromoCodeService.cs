using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.DTO;

namespace ConcertTickets_API.Services;

public class PromoCodeService
{
    private readonly IPromoCodeRepository _promoCodes;

    public PromoCodeService(IPromoCodeRepository promoCodes)
    {
        _promoCodes = promoCodes;
    }

    public async Task<ValidatePromoCodeResponse> ValidateAsync(string code, CancellationToken ct = default)
    {
        code = (code ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(code))
        {
            return new ValidatePromoCodeResponse
            {
                IsValid = false,
                Message = "Promo code is required."
            };
        }

        var promo = await _promoCodes.GetByCodeAsync(code, ct);

        if (promo is null)
        {
            return new ValidatePromoCodeResponse
            {
                IsValid = false,
                Message = "Promo code does not exist."
            };
        }

        if (!promo.Status.Equals("Active"))
        {
            return new ValidatePromoCodeResponse
            {
                IsValid = false,
                Message = "Promo code is not active."
            };
        }

        return new ValidatePromoCodeResponse
        {
            IsValid = true,
            Message = "Promo code is valid.",
            Code = promo.Code,
            DiscountPercent = 5
        };
    }
}