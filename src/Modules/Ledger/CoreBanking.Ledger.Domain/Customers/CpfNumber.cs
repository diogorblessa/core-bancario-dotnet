namespace CoreBanking.Ledger.Domain.Customers;

public readonly record struct CpfNumber
{
    public string Value { get; }

    private CpfNumber(string value) => Value = value;

    public static CpfNumber Parse(string input)
    {
        var digits = input.Replace(".", string.Empty, StringComparison.Ordinal)
                          .Replace("-", string.Empty, StringComparison.Ordinal);

        if (digits.Length != 11
            || !digits.All(char.IsAsciiDigit)
            || digits.All(c => c == digits[0])
            || !HasValidCheckDigits(digits))
        {
            throw new DomainException("customer.invalid_document", "CPF is invalid.");
        }

        return new CpfNumber(digits);
    }

    private static bool HasValidCheckDigits(string digits) =>
        ToNumber(digits[9]) == CheckDigit(digits[..9])
        && ToNumber(digits[10]) == CheckDigit(digits[..10]);

    // Weights go from (length + 1) down to 2: 10..2 for the first digit, 11..2 for the second.
    private static int CheckDigit(string digits)
    {
        var sum = 0;

        for (var i = 0; i < digits.Length; i++)
        {
            sum += ToNumber(digits[i]) * (digits.Length + 1 - i);
        }

        var remainder = sum % 11;

        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static int ToNumber(char digit) => digit - '0';
}
