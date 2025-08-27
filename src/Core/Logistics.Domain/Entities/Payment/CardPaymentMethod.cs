using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class CardPaymentMethod : PaymentMethod
{
    public override PaymentMethodType Type { get; protected set; } = PaymentMethodType.Card;
    public required string CardHolderName { get; set; }
    public required string CardNumber { get; set; }
    public required string Cvc { get; set; }
    public required int ExpMonth { get; set; }
    public required int ExpYear { get; set; }
}
